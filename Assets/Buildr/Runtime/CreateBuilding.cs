// BuildR
// Available on the Unity3D Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;

public class CreateBuilding : MonoBehaviour 
{
    public enum renderModes
    {
        full,
        lowDetail,
        box
    }

    public BuildrData data;
    public BuildrRuntimeConstraints constraints;

    public GameObject model = null;
    public DynamicMeshGenericMultiMaterialMesh colliderMesh = null;
    public DynamicMeshGenericMultiMaterialMesh fullMesh = null;
    public List<DynamicMeshGenericMultiMaterialMesh> interiorMeshes = new List<DynamicMeshGenericMultiMaterialMesh>();

    public List<GameObject> colliderHolders = new List<GameObject>();
    public List<GameObject> meshHolders = new List<GameObject>();
    public List<GameObject> interiorMeshHolders = new List<GameObject>();
    public MeshFilter meshFilt = null;
    public MeshRenderer meshRend = null;
    private Material lowDetailMat = null;
    public List<Material> materials;
    public bool includeCollider = true;

    private Camera cam;
    private Vector3 camPos;
    Bounds buildingBounds = new Bounds();

    void Awake()
    {
        data = gameObject.AddComponent<BuildrData>();
        data.Init();
        constraints = ScriptableObject.CreateInstance<BuildrRuntimeConstraints>();

    }

	void Start () 
    {
        materials = new List<Material>();
        cam = Camera.main;

        CreateBuildingNow();
	}

    void Update()
    {
        cam.transform.position = Vector3.Lerp(cam.transform.position, camPos, 0.5f);
        cam.transform.LookAt(buildingBounds.center);
    }

    void OnGUI()
    {
        if(GUILayout.Button("Create New Building"))
        {
            CreateBuildingNow();
        }
//
//        EditorGUILayout.BeginVertical(GUILayout.Width(300));
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Seed", GUILayout.Width(100));
//        constraints.useSeed = EditorGUILayout.Toggle(constraints.useSeed);
//        EditorGUI.BeginDisabledGroup(!constraints.useSeed);
//        constraints.seed = EditorGUILayout.IntField(constraints.seed);
//        EditorGUI.EndDisabledGroup();
//        EditorGUILayout.EndHorizontal();
//
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Floor Height", GUILayout.Width(140));
//        EditorGUILayout.LabelField(constraints.minimumFloorHeight.ToString(), GUILayout.Width(35));
//        EditorGUILayout.MinMaxSlider(ref constraints.minimumFloorHeight, ref constraints.maximumFloorHeight, 2.0f, 3.8f);
//        EditorGUILayout.LabelField(constraints.maximumFloorHeight.ToString(), GUILayout.Width(35));
//        EditorGUILayout.EndHorizontal();
//
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Constrain Maximum Height", GUILayout.Width(170));
//        constraints.constrainHeight = EditorGUILayout.Toggle(constraints.constrainHeight);
//        EditorGUI.BeginDisabledGroup(!constraints.constrainHeight);
//        EditorGUILayout.BeginVertical();
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Minimum:", GUILayout.Width(70));
//        constraints.minimumHeight = EditorGUILayout.FloatField(constraints.minimumHeight, GUILayout.Width(30));
//        EditorGUILayout.LabelField("metres", GUILayout.Width(53));
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Maximum:", GUILayout.Width(70));
//        constraints.maximumHeight = EditorGUILayout.FloatField(constraints.maximumHeight, GUILayout.Width(30));
//        EditorGUILayout.LabelField("metres", GUILayout.Width(53));
//        EditorGUILayout.EndHorizontal();
//        EditorGUI.EndDisabledGroup();
//        EditorGUILayout.EndVertical();
//        EditorGUILayout.EndHorizontal();
//
//        EditorGUILayout.Space();
//
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.BeginVertical();
//        EditorGUILayout.LabelField("Design Choices", GUILayout.Width(100));
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.BeginVertical();
//        constraints.rowStyled = EditorGUILayout.Toggle("Row Styles", constraints.rowStyled);
//        constraints.columnStyled = EditorGUILayout.Toggle("Column Styles", constraints.columnStyled);
//        //        constraints.externalAirConUnits = EditorGUILayout.Toggle("External Air Conditioner Units", constraints.rowStyled);
//        constraints.splitLevel = EditorGUILayout.Toggle("Split Level Volume", constraints.splitLevel);
//        constraints.taperedLevels = EditorGUILayout.Toggle("Tapered Volume", constraints.taperedLevels);
//        EditorGUILayout.EndVertical();
//        EditorGUILayout.BeginVertical();
//        constraints.singleLevel = EditorGUILayout.Toggle("Single Volume Level", constraints.singleLevel);
//        constraints.atticDesign = EditorGUILayout.Toggle("Attic Design", constraints.atticDesign);
//        constraints.shopGroundFloor = EditorGUILayout.Toggle("Shop Design", constraints.shopGroundFloor);
//        EditorGUILayout.EndVertical();
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.EndVertical();
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.Space();
//
//        //FACADE CONSTRAINTS
//        int styleLabelSize = 130;
//        EditorGUILayout.BeginVertical("box");
//        showFacadeConstraints = EditorGUILayout.Foldout(showFacadeConstraints, "Facade Constraints");
//        if (showFacadeConstraints)
//        {
//            EditorGUILayout.Space();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Bay Width", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.openingMinimumWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.openingMinimumWidth, ref constraints.openingMaximumWidth, 0.5f, 2.0f);
//            EditorGUILayout.LabelField(constraints.openingMaximumWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Bay Height", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.openingMinimumHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.openingMinimumHeight, ref constraints.openingMaximumHeight, 0.5f, constraints.maximumFloorHeight);
//            EditorGUILayout.LabelField(constraints.openingMaximumHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Bay Spacing", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.minimumBayMinimumWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.minimumBayMinimumWidth, ref constraints.minimumBayMaximumWidth, 0.125f, 2.0f);
//            EditorGUILayout.LabelField(constraints.minimumBayMaximumWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Bay Opening Depth", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.openingMinimumDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.openingMinimumDepth, ref constraints.openingMaximumDepth, -0.70f, 0.70f);
//            EditorGUILayout.LabelField(constraints.openingMaximumDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Facade Depth", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.facadeMinimumDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.facadeMinimumDepth, ref constraints.facadeMaximumDepth, -0.5f, 0.5f);
//            EditorGUILayout.LabelField(constraints.facadeMaximumDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//        }
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.Space();
//
//        //ROOF CONSTRAINTS
//
//        EditorGUILayout.BeginVertical("box");
//        showRoofConstraints = EditorGUILayout.Foldout(showRoofConstraints, "Roof Constraints");
//        if (showRoofConstraints)
//        {
//
//
//            EditorGUILayout.Space();
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Height", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.minimumRoofHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.minimumRoofHeight, ref constraints.maximumRoofHeight, 1.0f, constraints.maximumFloorHeight);
//            EditorGUILayout.LabelField(constraints.maximumRoofHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Mansard Face Depth", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.minimumRoofDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.minimumRoofDepth, ref constraints.maximumRoofDepth, 0.0f, 1.0f);
//            EditorGUILayout.LabelField(constraints.maximumRoofDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Mansard Floor Depth", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.minimumRoofFloorDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.minimumRoofFloorDepth, ref constraints.maximumRoofFloorDepth, 0.0f, 1.0f);
//            EditorGUILayout.LabelField(constraints.maximumRoofFloorDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.Space();
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.BeginVertical();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Flat Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleFlat = EditorGUILayout.Toggle(constraints.roofStyleFlat);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Mansard Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleMansard = EditorGUILayout.Toggle(constraints.roofStyleMansard);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Barrel Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleBarrel = EditorGUILayout.Toggle(constraints.roofStyleBarrel);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Gabled Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleGabled = EditorGUILayout.Toggle(constraints.roofStyleGabled);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.EndVertical();
//            EditorGUILayout.BeginVertical();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Hipped Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleHipped = EditorGUILayout.Toggle(constraints.roofStyleHipped);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Lean To Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleLeanto = EditorGUILayout.Toggle(constraints.roofStyleLeanto);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Steepled Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleSteepled = EditorGUILayout.Toggle(constraints.roofStyleSteepled);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Sawtooth Roof Styles", GUILayout.Width(styleLabelSize));
//            constraints.roofStyleSawtooth = EditorGUILayout.Toggle(constraints.roofStyleSawtooth);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.EndVertical();
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.Space();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Allow Dormers", GUILayout.Width(styleLabelSize));
//            constraints.allowDormers = EditorGUILayout.Toggle(constraints.allowDormers, GUILayout.Width(30));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.Space();
//
//            EditorGUI.BeginDisabledGroup(!constraints.allowDormers);
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Dormer Chance", GUILayout.Width(150));
//            constraints.dormerChance = EditorGUILayout.FloatField(constraints.dormerChance, GUILayout.Width(30));
//            EditorGUILayout.EndHorizontal();
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Dormer Width", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.dormerMinimumWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumWidth, ref constraints.dormerMaximumWidth, 0.5f, 2.0f);
//            EditorGUILayout.LabelField(constraints.dormerMaximumWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Dormer Height", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.dormerMinimumHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumHeight, ref constraints.dormerMaximumHeight, 0.5f, 2.0f);
//            EditorGUILayout.LabelField(constraints.dormerMaximumHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Dormer Roof Height", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.dormerMinimumRoofHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumRoofHeight, ref constraints.dormerMaximumRoofHeight, 0.5f, 2.0f);
//            EditorGUILayout.LabelField(constraints.dormerMaximumRoofHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Dormer Spacing", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.dormerMinimumSpacing.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumSpacing, ref constraints.dormerMaximumSpacing, 0.5f, 3.0f);
//            EditorGUILayout.LabelField(constraints.dormerMaximumSpacing.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//            EditorGUI.EndDisabledGroup();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Allow Parapets", GUILayout.Width(styleLabelSize));
//            constraints.allowParapet = EditorGUILayout.Toggle(constraints.allowParapet, GUILayout.Width(30));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUI.BeginDisabledGroup(!constraints.allowParapet);
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Parapet Render Chance", GUILayout.Width(150));
//            constraints.parapetChance = EditorGUILayout.FloatField(constraints.parapetChance, GUILayout.Width(30));
//            EditorGUILayout.EndHorizontal();
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Parapet Width", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.parapetMinimumDesignWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumDesignWidth, ref constraints.parapetMaximumDesignWidth, 0.0f, 1.0f);
//            EditorGUILayout.LabelField(constraints.parapetMaximumDesignWidth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Parapet Height", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.parapetMinimumHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumHeight, ref constraints.parapetMaximumHeight, 0.0f, 1.0f);
//            EditorGUILayout.LabelField(constraints.parapetMaximumHeight.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Parapet Front Depth", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.parapetMinimumFrontDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumFrontDepth, ref constraints.parapetMaximumFrontDepth, -1.0f, 1.0f);
//            EditorGUILayout.LabelField(constraints.parapetMaximumFrontDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Parapet Back Depth", GUILayout.Width(styleLabelSize));
//            EditorGUILayout.LabelField(constraints.parapetMinimumBackDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumBackDepth, ref constraints.parapetMaximumBackDepth, -1.0f, 1.0f);
//            EditorGUILayout.LabelField(constraints.parapetMaximumBackDepth.ToString(), GUILayout.Width(35));
//            EditorGUILayout.EndHorizontal();
//            EditorGUI.EndDisabledGroup();
//        }
//
//
//        EditorGUILayout.EndVertical();
    }

    private void CreateBuildingNow()
    {
        BuildrRuntimeGenerator.Generate(data,constraints);
        UpdateRender(renderModes.full);

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        buildingBounds = new Bounds();
        foreach(Renderer rend in renderers)
        {
            buildingBounds.Encapsulate(rend.bounds);
        }

        Vector3 max = buildingBounds.size;
        float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
        float dist = radius / (Mathf.Sin(cam.fieldOfView * Mathf.Deg2Rad));
        camPos = buildingBounds.center + new Vector3(1, 1, -1) * dist;
    }

    public void UpdateRender(renderModes _mode)
    {
        if (data.plan == null)
            return;
        if (data.floorHeight == 0)
            return;
        if (fullMesh == null)
            fullMesh = new DynamicMeshGenericMultiMaterialMesh();

        fullMesh.Clear();
        fullMesh.subMeshCount = data.textures.Count;

        foreach (DynamicMeshGenericMultiMaterialMesh intMesh in interiorMeshes)
        {
            intMesh.Clear();
        }

        switch (_mode)
        {
            case renderModes.full:
                BuildrBuilding.Build(fullMesh, data);
                BuildrRoof.Build(fullMesh, data);
                break;

            case renderModes.lowDetail:
                BuildrBuildingLowDetail2.Build(fullMesh, data);
                fullMesh.CollapseSubmeshes();
                break;

            case renderModes.box:
                BuildrBuildingBox.Build(fullMesh, data);
                break;
        }

        fullMesh.Build(false);

        while (meshHolders.Count > 0)
        {
            GameObject destroyOld = meshHolders[0];
            meshHolders.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }

        int numberOfMeshes = fullMesh.meshCount;
        for (int i = 0; i < numberOfMeshes; i++)
        {
            GameObject newMeshHolder = new GameObject("model " + (i + 1));
            newMeshHolder.transform.parent = transform;
            newMeshHolder.transform.localPosition = Vector3.zero;
            meshFilt = newMeshHolder.AddComponent<MeshFilter>();
            meshRend = newMeshHolder.AddComponent<MeshRenderer>();
            meshFilt.mesh = fullMesh[i].mesh;
            meshHolders.Add(newMeshHolder);
        }

        while (interiorMeshHolders.Count > 0)
        {
            GameObject destroyOld = interiorMeshHolders[0];
            interiorMeshHolders.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }

        switch (_mode)
        {
            case renderModes.full:
                UpdateInteriors();
                UpdateTextures();
                break;

            case renderModes.lowDetail:
                meshRend.sharedMaterials = new Material[0];
                lowDetailMat.mainTexture = data.LODTextureAtlas;
                meshRend.sharedMaterial = lowDetailMat;
                break;

            case renderModes.box:
                meshRend.sharedMaterials = new Material[0];
                lowDetailMat.mainTexture = data.textures[0].texture;
                meshRend.sharedMaterial = lowDetailMat;
                break;
        }
    }

    public void UpdateTextures()
    {
        int numberOfMaterials = data.textures.Count;
        if (materials == null)
            materials = new List<Material>(numberOfMaterials);
        materials.Clear();
        for (int m = 0; m < numberOfMaterials; m++)
        {
            materials.Add(data.textures[m].material);
            materials[m].name = data.textures[m].name;
            materials[m].mainTexture = data.textures[m].texture;
        }
        //meshRend.sharedMaterials = materials.ToArray();

        int numberOfMeshes = fullMesh.meshCount;
        for (int i = 0; i < numberOfMeshes; i++)
            meshHolders[i].GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();

        int numberOfInterior = interiorMeshHolders.Count;
        for (int i = 0; i < numberOfInterior; i++)
            interiorMeshHolders[i].GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
    }

    public void UpdateCollider()
    {
        if (data.generateCollider != BuildrData.ColliderGenerationModes.None)
        {
            if (data.floorHeight == 0)
                return;
            if (colliderMesh == null)
                colliderMesh = new DynamicMeshGenericMultiMaterialMesh();

            colliderMesh.Clear();
            colliderMesh.subMeshCount = 1;
            BuildrBuildingCollider.Build(colliderMesh, data);
            colliderMesh.Build(false);

            int numberOfStairMeshes = colliderMesh.meshCount;
            for (int i = 0; i < numberOfStairMeshes; i++)
            {
                string meshName = "collider";
                if (numberOfStairMeshes > 1) meshName += " mesh " + (i + 1);
                GameObject newMeshHolder = new GameObject(meshName);
                newMeshHolder.transform.parent = transform;
                meshFilt = newMeshHolder.AddComponent<MeshFilter>();
                meshRend = newMeshHolder.AddComponent<MeshRenderer>();
                meshFilt.mesh = colliderMesh[i].mesh;
                colliderHolders.Add(newMeshHolder);
            }
        }
    }

    public void UpdateInteriors()
    {
        while (interiorMeshHolders.Count > 0)
        {
            GameObject destroyOld = interiorMeshHolders[0];
            interiorMeshHolders.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }

        interiorMeshes.Clear();

        if (data.renderInteriors)
        {
            int numberOfVolumes = data.plan.numberOfVolumes;
            for (int v = 0; v < numberOfVolumes; v++)
            {
                DynamicMeshGenericMultiMaterialMesh interiorMesh = new DynamicMeshGenericMultiMaterialMesh();
                interiorMesh.subMeshCount = data.textures.Count;
                BuildrInteriors.Build(interiorMesh, data, v);
                interiorMesh.Build(false);

                int numberOfInteriorMeshes = interiorMesh.meshCount;
                for (int i = 0; i < numberOfInteriorMeshes; i++)
                {
                    string meshName = "model interior";
                    if (numberOfVolumes > 0) meshName += " volume " + (v + 1);
                    if (numberOfInteriorMeshes > 1) meshName += " mesh " + (i + 1);
                    GameObject newMeshHolder = new GameObject(meshName);
                    newMeshHolder.transform.parent = transform;
                    newMeshHolder.transform.localPosition = Vector3.zero;
                    meshFilt = newMeshHolder.AddComponent<MeshFilter>();
                    meshRend = newMeshHolder.AddComponent<MeshRenderer>();
                    meshFilt.mesh = interiorMesh[i].mesh;
                    interiorMeshHolders.Add(newMeshHolder);
                }
            }
        }
    }
}
