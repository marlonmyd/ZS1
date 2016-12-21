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
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[ExecuteInEditMode]
public class BuildrEditMode : MonoBehaviour
{

    public enum stages
    {
        start,
        floorplan,
        textures,
        facades,
        roofs,
        details,
        interior,
        building,
        options,
        export,
        generate
    }

    public enum modes
    {
        floorplan,
        splitwall,
        removewall,
        extrudewallselect,
        extrudewallmove,
        height,
        addNewVolume,
        addNewVolumeByDraw,
        addNewVolumeByPoints,
        addVolumeByPoint,
        addVolumeByWall,
        addVolumeByNewWall,
        removeVolume,
        mergeVolumes,
        splitVolume,
        addPointToVolume,
        addNewCore,
        removeCore
    }

    public enum renderModes
    {
        full,
        lowDetail,
        box
    }

    [SerializeField]
    private stages _stage = stages.start;
    private modes _mode = modes.floorplan;
    private renderModes _renderMode;
    private BuildrData _data = null;

    public bool alwaysSnap = false;
    public bool snapFloorplanToGrid = false;
    public float floorplanGridSize = 1.0f;

    public List<int> selectedPoints = new List<int>();
    public Vector3 startVolumeDraw = Vector3.zero;
    public List<Vector3> volumeDrawPoints = new List<Vector3>();
    public int selectedPoint = -1;

    public GameObject model = null;
    public DynamicMeshGeneric mesh = null;
    public DynamicMeshGenericMultiMaterialMesh colliderMesh = null;
    public DynamicMeshGenericMultiMaterialMesh fullMesh = null;
    public DynamicMeshGenericMultiMaterialMesh detailMesh = null;
    public List<DynamicMeshGenericMultiMaterialMesh> interiorMeshes = new List<DynamicMeshGenericMultiMaterialMesh>();
    public List<GameObject> bayModels = new List<GameObject>();

    public List<GameObject> colliderHolders = new List<GameObject>();
    public List<GameObject> meshHolders = new List<GameObject>();
    public List<GameObject> interiorMeshHolders = new List<GameObject>();
    public GameObject bayModelHolder;
    public MeshFilter meshFilt = null;
    public MeshRenderer meshRend = null;
    private Material blueMat;
    private Material lowDetailMat;
    private Color blueprintColour = BuildrColours.BLUE;
    public List<Material> materials;
    public GameObject[] details;

    public bool showDimensionLines = true;
    public bool showFacadeMarkers = true;
    public bool showWireframe = true;

    void Awake()
    {
        if (model != null)
        {
            DestroyImmediate(model);
        }

        if(bayModelHolder == null)
        {
            bayModelHolder = new GameObject("Bay Models");
            bayModelHolder.transform.parent = transform;
            bayModelHolder.transform.localPosition = Vector3.zero;
        }

        if (blueMat == null)
            blueMat = new Material(Shader.Find("Self-Illumin/Diffuse"));
        if (lowDetailMat == null)
            lowDetailMat = new Material(Shader.Find("Diffuse"));
        materials = new List<Material>();
    }

    void Start()
    {
        if (!Application.isPlaying)
        {
            //only update the models when we're editing.
            if(data!=null)
                BuildrUpgrader.UpgradeData(data);
            UpdateRender();
        }
    }

    public modes mode
    {
        get { return _mode; }
        set { _mode = value; }
    }

    public stages stage
    {
        get { return _stage; }
        set { _stage = value; }
    }

    public BuildrData data
    {
        get
        {
            if (_data == null)
                _data = gameObject.GetComponent<BuildrData>();
            return _data;
        }
    }

    public renderModes renderMode
    {
        get
        {
            return _renderMode;
        } 
        set
        {
            _renderMode = value;
        }
    }

    public void StartBuilding()
    {
        gameObject.AddComponent<BuildrData>().Init();
        SetStage(stages.floorplan);
    }

    public void SetStage(stages newStage)
    {
        switch (newStage)
        {
            case stages.floorplan:
                _stage = stages.floorplan;
                break;

            case stages.building:
                _stage = stages.building;
                break;

            case stages.textures:
                _stage = stages.textures;
                break;

            case stages.roofs:
                _stage = stages.roofs;
                break;

            case stages.export:
                _stage = stages.export;
                break;
        }
    }

    public void SetMode(modes newMode)
    {
        selectedPoints.Clear();
        startVolumeDraw = Vector3.zero;
        volumeDrawPoints.Clear();
        selectedPoint = -1;

        data.plan.CheckPlan();

        _mode = newMode;
        RenderFloorPlan();
    }

    public void UpdateRender()
    {
        switch (_stage)
        {
            case stages.start:
                //DO NOTHING
                break;

            case stages.generate:
                //DO NOTHING
                break;

            case stages.floorplan:
                RenderFloorPlan();
                break;

            default:
                UpdateRender(renderMode);
                UpdateCollider();
                break;
        }
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            child.gameObject.isStatic = gameObject.isStatic;
        }
    }

    public void RenderFloorPlan()
    {
        if (data.plan == null)
            return;
        int numberOfPoints = data.plan.points.Count;
        if (mesh == null)
            mesh = new DynamicMeshGeneric();
        if (mesh.size != numberOfPoints)
            mesh.Clear();

        mesh.vertices.Clear();
        mesh.uv.Clear();
        mesh.triangles.Clear();
        mesh.vertices.AddRange(data.plan.GetPointsAsVector3());

        for (int i = 0; i < numberOfPoints; i++)
            mesh.uv.Add(Vector2.zero);

        mesh.triangles.AddRange(data.plan.triangles);

        mesh.Build();

        while (meshHolders.Count > 0)
        {
            GameObject destroyOld = meshHolders[0];
            meshHolders.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }

        while (colliderHolders.Count > 0)
        {
            GameObject destroyOld = colliderHolders[0];
            colliderHolders.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }

        while (interiorMeshHolders.Count > 0)
        {
            GameObject destroyOld = interiorMeshHolders[0];
            interiorMeshHolders.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }

        while (bayModels.Count > 0)
        {
            GameObject destroyOld = bayModels[0];
            bayModels.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }
        

        int numberOfDetails = 0;
        if (details != null)
            numberOfDetails = details.Length;
        for (int i = 0; i < numberOfDetails; i++)
            DestroyImmediate(details[i]);

        GameObject newMeshHolder = new GameObject("floorplan");
        newMeshHolder.transform.parent = transform;
        newMeshHolder.transform.localPosition = Vector3.zero;
        meshFilt = newMeshHolder.AddComponent<MeshFilter>();
        meshRend = newMeshHolder.AddComponent<MeshRenderer>();
        meshFilt.mesh = mesh.mesh;
        meshRend.sharedMaterials = new Material[0];
        meshRend.sharedMaterial = blueMat;
        blueMat.color = blueprintColour;
        meshHolders.Add(newMeshHolder);

#if UNITY_EDITOR
        EditorUtility.UnloadUnusedAssets();
#endif
    }

    public void UpdateRender(renderModes _mode)
    {
        if(data.plan==null)
            return;
        if (data.floorHeight == 0)
            return;
        if (fullMesh == null)
            fullMesh = new DynamicMeshGenericMultiMaterialMesh();

        fullMesh.Clear();
        fullMesh.subMeshCount = data.textures.Count;

        foreach(DynamicMeshGenericMultiMaterialMesh intMesh in interiorMeshes)
        {
            intMesh.Clear();
        }

        switch(_mode)
        {
                case renderModes.full:
                    if(data.oneDrawCall)
                    {
                        BuildrBuildingOneDrawCall.Build(fullMesh, data);
                    }
                    else
                    {
                        BuildrBuilding.Build(fullMesh, data);
                        BuildrRoof.Build(fullMesh, data);
                    }
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
        while (colliderHolders.Count > 0)
        {
            GameObject destroyOld = colliderHolders[0];
            colliderHolders.RemoveAt(0);
            DestroyImmediate(destroyOld);
        }

        int numberOfMeshes = fullMesh.meshCount;
        for(int i = 0; i < numberOfMeshes; i++)
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
                renderMode = renderModes.full;
                UpdateInteriors();
                UpdateTextures();
                UpdateDetails();
                UpdateBayModels();
                break;

            case renderModes.lowDetail:
                renderMode = renderModes.lowDetail;
                meshRend.sharedMaterials = new Material[0];
                lowDetailMat.mainTexture = data.LODTextureAtlas;
                meshRend.sharedMaterial = lowDetailMat;
                UpdateDetails();
                break;

            case renderModes.box:
                renderMode = renderModes.box;
                meshRend.sharedMaterials = new Material[0];
                lowDetailMat.mainTexture = data.textures[0].texture;
                meshRend.sharedMaterial = lowDetailMat;
                UpdateDetails();
                break;
        }

#if UNITY_EDITOR
        EditorUtility.UnloadUnusedAssets();
#endif
    }

    public void UpdateTextures()
    {

        int numberOfMeshes = fullMesh.meshCount;
        int numberOfMaterials = data.textures.Count;
        List<Material> buildMaterials = new List<Material>(numberOfMaterials);
        for(int i = 0; i < numberOfMeshes; i++)
        {
            buildMaterials.Clear();
            for (int m = 0; m < numberOfMaterials; m++)
            {
                if (!fullMesh[i].SubmeshUsed(m))
                    continue;//skip, unused
                BuildrTexture bTexture = data.textures[m];
                buildMaterials.Add(bTexture.usedMaterial);

            }
            meshHolders[i].GetComponent<MeshRenderer>().sharedMaterials = buildMaterials.ToArray();
        }
    }

    public void UpdateCollider()
    {
        if(data.generateCollider != BuildrData.ColliderGenerationModes.None)
        {
            if (data.floorHeight == 0)
                return;
            if (colliderMesh == null)
                colliderMesh = new DynamicMeshGenericMultiMaterialMesh();
            colliderMesh.Clear();
            colliderMesh.subMeshCount = data.textures.Count;
            BuildrBuildingCollider.Build(colliderMesh, data);
            colliderMesh.Build(false);

            int numberOfStairMeshes = colliderMesh.meshCount;
            for (int i = 0; i < numberOfStairMeshes; i++)
            {
                string meshName = "collider";
                if (numberOfStairMeshes > 1) meshName += " mesh " + (i + 1);
                GameObject newMeshHolder = new GameObject(meshName);
                newMeshHolder.transform.parent = transform;
                newMeshHolder.transform.localPosition = Vector3.zero;
                MeshCollider meshCollider = newMeshHolder.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = colliderMesh[i].mesh;
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
            for(int v = 0; v < numberOfVolumes; v++)
            {
                DynamicMeshGenericMultiMaterialMesh interiorMesh = new DynamicMeshGenericMultiMaterialMesh();
                interiorMesh.subMeshCount = data.textures.Count;
                BuildrVolume volume = _data.plan.volumes[v];
                BuildrInteriors.Build(interiorMesh, data, v);
                interiorMesh.Build(false);

                List<int> unusedInteriorTextures = interiorMesh.unusedSubmeshes;
                int numberOfInteriorMaterials = data.textures.Count;
                List<Material> interiorMaterials = new List<Material>();
                for (int m = 0; m < numberOfInteriorMaterials; m++)
                {
                    if (unusedInteriorTextures.Contains(m))
                        continue;//skip, unused
                    BuildrTexture bTexture = data.textures[m];
                    interiorMaterials.Add(bTexture.usedMaterial);
                }

                int numberOfInteriorMeshes = interiorMesh.meshCount;
                for (int i = 0; i < numberOfInteriorMeshes; i++)
                {
                    string meshName = "model interior";
                    if (numberOfVolumes > 0) meshName += " volume " + (v + 1);
                    if(numberOfInteriorMeshes>1)meshName += " mesh " + (i + 1);
                    GameObject newMeshHolder = new GameObject(meshName);
                    newMeshHolder.transform.parent = transform;
                    newMeshHolder.transform.localPosition = Vector3.zero;
                    meshFilt = newMeshHolder.AddComponent<MeshFilter>();
                    meshRend = newMeshHolder.AddComponent<MeshRenderer>();
                    meshFilt.mesh = interiorMesh[i].mesh;
                    interiorMeshHolders.Add(newMeshHolder);

                    int numberOfInterior = interiorMeshHolders.Count;
                    for (int m = 0; m < numberOfInterior; m++)
                        meshRend.sharedMaterials = interiorMaterials.ToArray();
                }
                interiorMeshes.Add(interiorMesh);

                if(!volume.generateStairs) continue;

                DynamicMeshGenericMultiMaterialMesh stairwellMesh = new DynamicMeshGenericMultiMaterialMesh();
                stairwellMesh.subMeshCount = data.textures.Count;
                BuildrStairs.Build(stairwellMesh, data, v, BuildrStairs.StairModes.Stepped, true);
                stairwellMesh.Build(false);


                List<int> unusedStairTextures = stairwellMesh.unusedSubmeshes;
                int numberOfStairMaterials = data.textures.Count;
                List<Material> stairMaterials = new List<Material>();
                for (int m = 0; m < numberOfStairMaterials; m++)
                {
                    if (unusedStairTextures.Contains(m))
                        continue;//skip, unused
                    BuildrTexture bTexture = data.textures[m];
                    stairMaterials.Add(bTexture.usedMaterial);
                }

                int numberOfStairMeshes = stairwellMesh.meshCount;
                for (int i = 0; i < numberOfStairMeshes; i++)
                {
                    string meshName = "model stairs";
                    if (numberOfVolumes > 0) meshName += " volume " + (v + 1);
                    if (numberOfStairMeshes > 1) meshName += " mesh " + (i + 1);
                    GameObject newMeshHolder = new GameObject(meshName);
                    newMeshHolder.transform.parent = transform;
                    newMeshHolder.transform.localPosition = volume.stairBaseVector[i];
                    meshFilt = newMeshHolder.AddComponent<MeshFilter>();
                    meshRend = newMeshHolder.AddComponent<MeshRenderer>();
                    meshFilt.mesh = stairwellMesh[i].mesh;
                    interiorMeshHolders.Add(newMeshHolder);
                    meshRend.sharedMaterials = stairMaterials.ToArray();
                }
                interiorMeshes.Add(stairwellMesh);
            }
        }
    }

    public void UpdateDetails()
    {
        int numberOfDetails = 0;
        if (details != null)
            numberOfDetails = details.Length;
        for (int i = 0; i < numberOfDetails; i++)
            DestroyImmediate(details[i]);

        if (data.plan == null)
            return;
        if (data.floorHeight == 0)
            return;
        if (data.details.Count == 0)
            return;
        if (detailMesh == null)
            detailMesh = new DynamicMeshGenericMultiMaterialMesh();

        if (renderMode != renderModes.full)
            return;//once data is cleared - asses if we want to rerender the details

        details = BuildrBuildingDetails.Render(detailMesh,data);
        numberOfDetails = details.Length;

        for (int i = 0; i < numberOfDetails; i++)
        {
           details[i].transform.parent = transform;
           details[i].transform.localPosition = Vector3.zero;
           details[i].transform.localRotation = Quaternion.identity;
        }
    }

    public void UpdateBayModels()
    {
        while(bayModels.Count > 0)
        {
            DestroyImmediate(bayModels[0]);
            bayModels.RemoveAt(0);
        }
        GameObject[] newModels = BuildrBuildingBayModels.Place(data);
        foreach(GameObject newModel in newModels)
        {
            newModel.transform.parent = bayModelHolder.transform;
            newModel.transform.position += transform.position;
        }
        bayModels.AddRange(newModels);
    }

    public Object[] GetUndoObjects()
    {
        List<Object> returnObjects = new List<Object>();

        returnObjects.Add(this);
        returnObjects.Add(transform);
        returnObjects.Add(gameObject);
        if(details!=null)
        {
            foreach(GameObject detail in details)
            {
                returnObjects.Add(detail);
                returnObjects.Add(detail.transform);
            }
        }
        returnObjects.AddRange(colliderHolders.ToArray());
        returnObjects.AddRange(interiorMeshHolders.ToArray());
        returnObjects.AddRange(meshHolders.ToArray());
        if(data != null)
            returnObjects.AddRange(data.GetUndoObjects());

        return returnObjects.ToArray();
    }
}
