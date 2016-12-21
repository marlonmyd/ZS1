// BuildR
// Available on the Unity3D Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildrData : MonoBehaviour
{

    public float versionNumber = BuildrVersion.NUMBER;//log the version number this was created with

    public bool editing = true;
    public BuildrGenerateConstraints generatorConstraints = null;

    //this is the floor plan area that defines the footprint of the building
    public BuildrPlan plan = null;//ScriptableObject.CreateInstance<BuildrPlan>();

    //this is a library of all the facade designs available to be used on this building
    public List<BuildrFacadeDesign> facades = new List<BuildrFacadeDesign>();

    //this is a library of all the roof designs available to be used on this building
    public List<BuildrRoofDesign> roofs = new List<BuildrRoofDesign>();

    //this is a library of all the textures available to be used in this building
    public List<BuildrTexture> textures = new List<BuildrTexture>();

    //this is a library of all the bay designs available to be used on building facades
    public List<BuildrBay> bays = new List<BuildrBay>();

    public List<BuildrDetail> details = new List<BuildrDetail>();

    //should BuildR add polys to fill the underside of the building - useful for some shadow rendering programs
    public bool drawUnderside = true;

    //the amount to drop the foundations
    public float foundationHeight = 0;
    public int foundationTexture = 0;

    public bool renderInteriors = false;
    public bool cullBays = false;
    private float _interiorCeilingHeight = 0.0f;

    public bool illegal = false;

    //EXPORT SETTINGS
    public bool fullmesh = true;
//    public bool packed = false;
    public bool placeIntoScene = true;
    public bool copyTexturesIntoExportFolder = true;
    public bool includeTangents = false;
    public bool oneDrawCall = false;

    public enum filetypes
    {
        fbx,
        obj
    }

    public filetypes filetype = filetypes.obj;
    public string exportFilename = "ExportedModel";
//    public bool exportSimpleCollider = false;
    public bool createPrefabOnExport = false;
    public bool exportLowLOD = false;

    public enum ColliderGenerationModes
    {
        None,
        Simple,
        Complex
    }
    public ColliderGenerationModes generateCollider = ColliderGenerationModes.None;

    //this is the texture that will be populated if we choose to pack the textures into a single megatexture for a single draw call
    public Texture2D textureAtlas;
    public Rect[] atlasCoords = null;
    //this is a list of the submesh indexes packed
    public List<int> texturesPacked = new List<int>();
    //if we have packed textures, we can use the following list to denote textures we ignored
    public List<int> texturesNotPacked = new List<int>();

    //this is the generated texture atlas for the low LOD model
    public Texture2D LODTextureAtlas = null;
    //this is the generated texture atlas for the one draw call mode
    public Texture2D OneDrawCallTexture = null;

    //the floor height for the entire building
    [SerializeField]
    private float _floorHeight = BuildrMeasurements.FLOOR_HEIGHT_MIN;

    public float floorHeight
    {
        get
        {
            return _floorHeight;
        }
        set
        {
            _floorHeight = value;

            if (_floorHeight > 0)
            {
                foreach (BuildrFacadeDesign facadeDesign in facades)
                {
                    facadeDesign.doorHeight = Mathf.Min(facadeDesign.doorHeight, _floorHeight);
                    facadeDesign.windowHeight = Mathf.Min(facadeDesign.windowHeight, _floorHeight);
                }
            }
        }
    }

    public float interiorCeilingHeight {get {return _interiorCeilingHeight;} 
        set
        {
            _interiorCeilingHeight = Mathf.Clamp01(value);
        }}

    //DEBUG
    [SerializeField]
    private bool shouldDebug = false;
    [SerializeField]
    private float timer = 0;

    public void Init()
    {
        DestroyImmediate(plan);
        plan = ScriptableObject.CreateInstance<BuildrPlan>();
        facades.Add(new BuildrFacadeDesign("default"));

        //set up two basic textures to use
        textures.Add(new BuildrTexture("bricks"));
        textures.Add(new BuildrTexture("window"));
        textures.Add(new BuildrTexture("roof"));

        roofs.Add(new BuildrRoofDesign("default"));

        bays.Add(new BuildrBay("default"));

        DestroyImmediate(generatorConstraints);
        generatorConstraints = ScriptableObject.CreateInstance<BuildrGenerateConstraints>();
        generatorConstraints.Init();
    }

    public void RemoveTexture(BuildrTexture bTexture)
    {
        int bTextureIndex = textures.IndexOf(bTexture);
        textures.Remove(bTexture);

        foreach (BuildrFacadeDesign facade in facades)
        {
            int numberOfFacadeTextures = facade.textureValues.Length;
            for (int i = 0; i < numberOfFacadeTextures; i++)
            {
                if (facade.textureValues[i] >= bTextureIndex && facade.textureValues[i] > 0)
                    facade.textureValues[i]--;
            }

            BuildrBay bay = facade.simpleBay;
            int numberOfBayTextures = bay.textureValues.Length;
            for (int i = 0; i < numberOfBayTextures; i++)
            {
                if (bay.textureValues[i] >= bTextureIndex && bay.textureValues[i] > 0)
                    bay.textureValues[i]--;
            }
        }
        foreach (BuildrRoofDesign roof in roofs)
        {
            int numberOfRoofTextures = roof.textureValues.Length;
            for (int i = 0; i < numberOfRoofTextures; i++)
            {
                if (roof.textureValues[i] >= bTextureIndex && roof.textureValues[i] > 0)
                    roof.textureValues[i]--;
            }
        }
        foreach (BuildrBay bay in bays)
        {
            int numberOfBayTextures = bay.textureValues.Length;
            for (int i = 0; i < numberOfBayTextures; i++)
            {
                if (bay.textureValues[i] >= bTextureIndex && bay.textureValues[i] > 0)
                    bay.textureValues[i]--;
            }
        }
    }

    public void RemoveFacadeDesign(BuildrFacadeDesign facadeDesign)
    {
        int bFacadeIndex = facades.IndexOf(facadeDesign);//the the index of the facade BEFORE we delete it
        facades.Remove(facadeDesign);
        int numberOfVolumes = plan.numberOfVolumes;
        for (int s = 0; s < numberOfVolumes; s++)
            plan.volumes[s].styles.CheckRemovedStyle(bFacadeIndex);
    }

    public void RemoveRoofDesign(BuildrRoofDesign roof)
    {
        roofs.Remove(roof);
        int bRoofIndex = roofs.IndexOf(roof);
        int numberOfVolumes = plan.numberOfVolumes;
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volume = plan.volumes[s];
            if (volume.roofDesignID >= bRoofIndex && volume.roofDesignID > 0)
                volume.roofDesignID--;
        }
    }

    public Object[] GetUndoObjects()
    {
        if (plan == null)
            return new Object[0];
        Object[] planUndoObjects = plan.GetUndoObjects();
        Object[] returnObjects = new Object[planUndoObjects.Length + 1];
        returnObjects[0] = this;
        for (int i = 1; i < planUndoObjects.Length + 1; i++)
            returnObjects[i] = planUndoObjects[i - 1];
        return returnObjects;
    }

    public void StartLog()
    {
        timer = Time.realtimeSinceStartup;
    }

    public void Log(string Output)
    {
        if (shouldDebug)
        {
            float currenttime = Time.realtimeSinceStartup;
            float elapsedTime = currenttime - timer;
            Debug.Log(Output + " : " + elapsedTime + " sec");
        }
    }

    public void ResetData()
    {
        ResetData(false);
    }

    public void ResetData(bool keepPlan)
    {
        if(!keepPlan)
        {
            DestroyImmediate(plan);
            plan = ScriptableObject.CreateInstance<BuildrPlan>();
        }
        facades.Clear();
        roofs.Clear();
        textures.Clear();
        bays.Clear();
        details.Clear();
    }
}
