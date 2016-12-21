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
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BuildrEditModeExport
{

    private static DynamicMeshGenericMultiMaterialMesh DYN_MESH;
    private static Mesh EXPORT_MESH;
    private static Transform CURRENT_TRANSFORM;
    private static string FILE_EXTENTION = ".obj";
    private const string LOD_SUFFIX = "_LowDetail";
    private const string COLLIDER_SUFFIX = "_Collider";
    private const string ATLASED_SUFFIX = "Atlased";
    private const string DETAIL_SUFFIX = "_Detail";
    private const string INTERIOR_SUFFIX = "_Interior";
    private const string STAIR_SUFFIX = "_Stairwell";

    private const string PROGRESSBAR_TEXT = "Exporting Building";

    private const string ROOT_FOLDER = "Assets/Buildr/Exported/";

    public static void InspectorGUI(BuildrEditMode editMode, BuildrData data)
    {
        if (data.plan==null || data.plan.numberOfVolumes == 0)
        {
            EditorGUILayout.HelpBox("There are no defined volumes, go to Floorplan and define one", MessageType.Error);
            return;
        }

        const int guiWidth = 400;
        const int textWidth = 348;
        const int toggleWidth = 25;
        const int helpWidth = 20;

        CURRENT_TRANSFORM = editMode.transform;
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filename", GUILayout.Width(225));
        data.exportFilename = EditorGUILayout.TextField(data.exportFilename, GUILayout.Width(175));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filetype", GUILayout.Width(350));
        data.filetype = (BuildrData.filetypes)EditorGUILayout.EnumPopup(data.filetype, GUILayout.Width(50));
        switch (data.filetype)
        {
            case BuildrData.filetypes.obj:
                FILE_EXTENTION = ".obj";
                break;
            case BuildrData.filetypes.fbx:
                FILE_EXTENTION = ".fbx";
                break;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export Full Mesh", GUILayout.Width(textWidth));
        data.fullmesh = EditorGUILayout.Toggle(data.fullmesh, GUILayout.Width(toggleWidth));
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - Export Full Mesh";
            string helpBody = "Select this checkbox if you want your export the full detail model.";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Place Exported Model Into Scene", GUILayout.Width(textWidth));
        data.placeIntoScene = EditorGUILayout.Toggle(data.placeIntoScene, GUILayout.Width(toggleWidth));
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - Place Exported Model Into Scene";
            string helpBody = "Select this checkbox if you want your exported models to be copied into your scene." +
                "\nThese will be positioned correctly and will include colliders and LOD models if you opt to export them also.";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Copy Textures into Export Folder", GUILayout.Width(textWidth));
        data.copyTexturesIntoExportFolder = EditorGUILayout.Toggle(data.copyTexturesIntoExportFolder, GUILayout.Width(toggleWidth));
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - Copy Textures into Export Folder";
            string helpBody = "Check this box if you want to copy the textures you are using into the export folder." +
                "\nThis is useful if you plan to use the exported model elsewhere. Having the model and the textures in one folder will allow you to move this model with ease.";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export Collider");
        data.generateCollider = (BuildrData.ColliderGenerationModes)EditorGUILayout.EnumPopup(data.generateCollider, GUILayout.Width(80));
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - Export Collider Mesh";
            string helpBody = "Check this box if you wish to generate a collider mesh for your model." +
                "\nThis will generate a mesh to be used with colliders.";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export as Prefab", GUILayout.Width(textWidth));
        data.createPrefabOnExport = EditorGUILayout.Toggle(data.createPrefabOnExport, GUILayout.Width(toggleWidth));
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - Export as Prefab";
            string helpBody = "Select this if you wish to create a prefab of your model." +
                "\nThis is recommended if you're exporting a collider so they will get packaged together.";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export a Low Detail Version", GUILayout.Width(textWidth));
        data.exportLowLOD = EditorGUILayout.Toggle(data.exportLowLOD, GUILayout.Width(toggleWidth));
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - Export a Low Detail Version";
            string helpBody = "Check this box to export a simplified model of your building." +
                "\nIdeal to use as a low level of detail version of your model." +
                "\nGeometry will be significantly reduced." +
                "\nFacades will flat and exported as a single atlased texture" +
                "\nThe model will use 1 draw call and will have around 10% of the triangles and verticies";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export with tangents", GUILayout.Width(textWidth));
        data.includeTangents = EditorGUILayout.Toggle(data.includeTangents, GUILayout.Width(toggleWidth));
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - with tangents";
            string helpBody = "Export the models with calculated tangents." + 
                "\nSome shaders require tangents to be calculated on the model." + 
                "\nUnity will do this automatically on all imported meshes so it's not neccessary here." + 
                "/nBut you might want them if you're taking them to another program.";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        bool unreadableTextures = false;
        bool nullTextures = false;
        string unreadableTextureList = "";
        foreach (BuildrTexture bTexture in data.textures)//check texture readablility
        {
            if(bTexture.type == BuildrTexture.Types.Substance)
                continue;//substances are preset in BuildrTexture class
            if (bTexture.texture != null)
            {
                string texturePath = AssetDatabase.GetAssetPath(bTexture.texture);
                TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                if (!textureImporter.isReadable)
                {
                    unreadableTextures = true;
                    if (unreadableTextureList.Length > 0)
                        unreadableTextureList += ", ";
                    unreadableTextureList += "'" + bTexture.name + "'";
                }
            }
            else
            {
                nullTextures = true;
            }
        }

        if (unreadableTextures)
        {
            EditorGUILayout.HelpBox("Unreadable Texture Error." +
                "\nThe following textures you are useing are not readable." +
                "\n" + unreadableTextureList + "." +
                "\nPlease select the readable checkbox under advanced texture settings." +
                "\nOr move this texture to the BuildR texture folder and reimport.",
                MessageType.Error);
        }

        if (nullTextures)
        {
            EditorGUILayout.HelpBox("Null Texture Error" +
                "\nSome of the textures have not been set" +
                "\nEnsure you are not using null textures to proceed.",
                MessageType.Error);
        }

        bool usingSubstances = false;
        foreach(BuildrTexture bTexture in data.textures)
        {
            if(bTexture.type == BuildrTexture.Types.Substance)
            {
                usingSubstances = true;
                break;
            }
        }
        if (usingSubstances)
        {
            EditorGUILayout.HelpBox("Model uses Substance textures." +
                "\nExporting model to " + data.filetype + " will lose references to this texture and it will be rendered white.",
                MessageType.Warning);
        }

        EditorGUI.BeginDisabledGroup(unreadableTextures || nullTextures);

        if (GUILayout.Button("Export", GUILayout.Width(guiWidth), GUILayout.Height(40)))
        {
            ExportModel(data);
        }

        if (GUILayout.Button("Export Data to XML", GUILayout.Width(guiWidth)))
        {
            BuildrXMLExporter.Export(ROOT_FOLDER + data.exportFilename + "/", data.exportFilename, data);
            AssetDatabase.Refresh();
        }

        EditorGUI.EndDisabledGroup();

        CURRENT_TRANSFORM = null;
    }

    private static void ExportModel(BuildrData data)
    {
        try
        {
            EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.0f);

            //check overwrites...
            string newDirectory = ROOT_FOLDER + data.exportFilename;
            if(!CreateFolder(newDirectory))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.05f);
            if(data.fullmesh)
            {
                //export unpacked model
                DYN_MESH = new DynamicMeshGenericMultiMaterialMesh();
                DYN_MESH.subMeshCount = data.textures.Count;
                BuildrBuilding.Build(DYN_MESH, data);
                EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.30f);
                BuildrRoof.Build(DYN_MESH, data);
                EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.60f);
                DYN_MESH.Build(data.includeTangents);
                int meshCount = DYN_MESH.meshCount;


                List<int> unusedTextures = DYN_MESH.unusedSubmeshes;
                int numberOfUnpackedTextures = data.textures.Count;
                List<ExportMaterial> exportTextureList = new List<ExportMaterial>();
                for (int t = 0; t < numberOfUnpackedTextures; t++)
                {
                    if (unusedTextures.Contains(t))
                        continue;//skip, unused
                    ExportMaterial newTexture = new ExportMaterial();
                    newTexture.name = data.textures[t].name;
                    newTexture.material = data.textures[t].material;
                    newTexture.generated = false;
                    newTexture.filepath = data.textures[t].filePath;
                    exportTextureList.Add(newTexture);
                }
                for(int i = 0; i < meshCount; i++)
                {
                    EXPORT_MESH = DYN_MESH[i].mesh;
                    MeshUtility.Optimize(EXPORT_MESH);
                    Export(data, EXPORT_MESH, exportTextureList.ToArray());
                    string filenameSuffix = (meshCount>1)? i.ToString() : "";
                    string filename = data.exportFilename + filenameSuffix;
                    Export(filename, ROOT_FOLDER + data.exportFilename + "/", data, EXPORT_MESH, exportTextureList.ToArray());
                }
            }

            //Export Collider
            if(data.generateCollider != BuildrData.ColliderGenerationModes.None)
                ExportCollider(data);

            int[] numberOfInteriorMeshes = new int[data.plan.numberOfVolumes];
            if(data.renderInteriors && data.fullmesh)
                numberOfInteriorMeshes = ExportInteriors(data);

            int[] numberOfStairwells = new int[data.plan.numberOfVolumes];
            if (data.renderInteriors && data.fullmesh)
                numberOfStairwells = ExportStairwells(data);

            int numberOfDetailMeshes = 0;
            if(data.fullmesh)
                numberOfDetailMeshes = ExportDetails(data);

            EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.70f);

            //Place exported version into scene
            if(data.fullmesh)
            {
                AssetDatabase.Refresh();//ensure the database is up to date...
                GameObject baseObject = new GameObject(data.exportFilename);
                if((data.createPrefabOnExport || data.placeIntoScene))
                {
                    baseObject.transform.position = CURRENT_TRANSFORM.position;
                    baseObject.transform.rotation = CURRENT_TRANSFORM.rotation;

                    string modelFilePath = ROOT_FOLDER + data.exportFilename + "/" + data.exportFilename + FILE_EXTENTION;
                    GameObject newModel = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(modelFilePath));
                    newModel.name = "model";
                    newModel.transform.parent = baseObject.transform;
                    newModel.transform.localPosition = Vector3.zero;
                    newModel.transform.localRotation = Quaternion.identity;
                    if(data.generateCollider != BuildrData.ColliderGenerationModes.None)
                    {
                        GameObject colliderObject = new GameObject("collider");
                        string colliderFilePath = ROOT_FOLDER + data.exportFilename + "/" + data.exportFilename + COLLIDER_SUFFIX + FILE_EXTENTION;
                        colliderObject.AddComponent<MeshCollider>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(colliderFilePath, typeof(Mesh));
                        colliderObject.transform.parent = baseObject.transform;
                        colliderObject.transform.localPosition = Vector3.zero;
                        colliderObject.transform.localRotation = Quaternion.identity;
                    }

                    for (int i = 0; i < numberOfDetailMeshes; i++)
                    {
                        string detailSuffixIndex = ((numberOfDetailMeshes > 1) ? "_" + i : "");
                        string detailFileName = data.exportFilename + DETAIL_SUFFIX + detailSuffixIndex;
                        string detailFolder = ROOT_FOLDER + data.exportFilename + "/";
                        string detailFilepath = detailFolder + detailFileName + FILE_EXTENTION;
                        GameObject detailObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(detailFilepath));
                        detailObject.name = "details";
                        detailObject.transform.parent = baseObject.transform;
                        detailObject.transform.localPosition = Vector3.zero;
                        detailObject.transform.localRotation = Quaternion.identity;
                    }

                    int numberOfVolumes = data.plan.numberOfVolumes;
                    GameObject interiorHolder = new GameObject("interiors");
                    interiorHolder.transform.parent = baseObject.transform;
                    interiorHolder.transform.localPosition = Vector3.zero;
                    interiorHolder.transform.localRotation = Quaternion.identity;
                    for (int v = 0; v < numberOfInteriorMeshes.Length; v++)
                    {
                        int numMeshes = numberOfInteriorMeshes[v];
                        for (int i = 0; i < numMeshes; i++)
                        {
                            string VolumeSuffix = ((numberOfVolumes > 1) ? "_" + v : "");
                            string DetailSuffixIndex = ((numMeshes > 1) ? "_" + i : "");
                            string DetailFileName = data.exportFilename + INTERIOR_SUFFIX + VolumeSuffix + DetailSuffixIndex;
                            string DetailFolder = ROOT_FOLDER + data.exportFilename + "/";
                            string filePath = DetailFolder + DetailFileName + FILE_EXTENTION;
                            GameObject interiorObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(filePath));
                            interiorObject.name = INTERIOR_SUFFIX + VolumeSuffix + DetailSuffixIndex;
                            interiorObject.transform.parent = interiorHolder.transform;
                            interiorObject.transform.localPosition = Vector3.zero;
                            interiorObject.transform.localRotation = Quaternion.identity;
                        }
                    }

                    for(int v = 0; v < numberOfStairwells.Length; v++)
                    {
                        int numMeshes = numberOfStairwells[v];
                        for (int i = 0; i < numMeshes; i++)
                        {
                            string VolumeSuffix = ((numberOfVolumes > 1) ? "_" + v : "");
                            string DetailSuffixIndex = ((numMeshes > 1) ? "_" + i : "");
                            string DetailFileName = data.exportFilename + STAIR_SUFFIX + VolumeSuffix + DetailSuffixIndex;
                            string DetailFolder = ROOT_FOLDER + data.exportFilename + "/";
                            string filePath = DetailFolder + DetailFileName + FILE_EXTENTION;
                            GameObject interiorObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(filePath));
                            interiorObject.name = STAIR_SUFFIX + VolumeSuffix + DetailSuffixIndex;
                            interiorObject.transform.parent = interiorHolder.transform;
                            interiorObject.transform.localPosition = data.plan.volumes[v].stairBaseVector[i];
                            interiorObject.transform.localRotation = Quaternion.identity;
                        }
                    }
                }

                if(data.createPrefabOnExport)
                {
                    string prefabPath = ROOT_FOLDER + data.exportFilename + "/" + data.exportFilename + ".prefab";
                    Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                    if(prefab == null)
                        prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
                    PrefabUtility.ReplacePrefab(baseObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
                }

                if(!data.placeIntoScene)
                    Object.DestroyImmediate(baseObject);
            }

            if(data.exportLowLOD)
            {
                ExportLowLOD(data);
            }

            DYN_MESH = null;
            EXPORT_MESH = null;

            EditorUtility.ClearProgressBar();
            EditorUtility.UnloadUnusedAssets();

            AssetDatabase.Refresh();
        }catch(System.Exception e)
        {
            Debug.LogError("BuildR Export Error: "+e);
            EditorUtility.ClearProgressBar();
        }
    }

    private static void ExportLowLOD(BuildrData data)
    {
        DynamicMeshGenericMultiMaterialMesh dynLODMesh = new DynamicMeshGenericMultiMaterialMesh();
        dynLODMesh.subMeshCount = data.textures.Count;
        BuildrBuildingLowDetail2.Build(dynLODMesh, data);
        dynLODMesh.CollapseSubmeshes();
        EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.80f);
        dynLODMesh.Build(data.includeTangents);
        Mesh LODMesh = dynLODMesh[0].mesh;//TODO: support many meshes
        MeshUtility.Optimize(LODMesh);
        EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.90f);

        string textureName = data.exportFilename + ATLASED_SUFFIX + LOD_SUFFIX;
        string textureFileName = textureName + ".png";
        string newDirectory = ROOT_FOLDER + data.exportFilename;

        File.WriteAllBytes(newDirectory + "/" + textureFileName, data.LODTextureAtlas.EncodeToPNG());
        ExportMaterial[] exportTextures = new ExportMaterial[1];
        ExportMaterial newTexture = new ExportMaterial();
        newTexture.name = textureName;
        newTexture.filepath = textureFileName;
        newTexture.generated = true;
        exportTextures[0] = newTexture;
        string LODFileName = data.exportFilename + LOD_SUFFIX;
        string LODFolder = ROOT_FOLDER + data.exportFilename + "/";
        Export(LODFileName, LODFolder, data, LODMesh, exportTextures);


        if (data.placeIntoScene)
        {
            AssetDatabase.Refresh();//ensure the database is up to date...
            string filePath = LODFolder + LODFileName + FILE_EXTENTION;
            GameObject newModel = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(filePath));
            newModel.transform.position = CURRENT_TRANSFORM.position;
            newModel.transform.rotation = CURRENT_TRANSFORM.rotation;
        }

        Texture2D.DestroyImmediate(data.textureAtlas);
        Texture2D.DestroyImmediate(data.LODTextureAtlas);
    }

    private static void ExportCollider(BuildrData data)
    {
        DynamicMeshGenericMultiMaterialMesh COL_MESH = new DynamicMeshGenericMultiMaterialMesh();
        COL_MESH.subMeshCount = data.textures.Count;
        BuildrBuildingCollider.Build(COL_MESH, data);
//        COL_MESH.CollapseSubmeshes();
        COL_MESH.Build(false);

        ExportMaterial[] exportTextures = new ExportMaterial[1];
        ExportMaterial newTexture = new ExportMaterial();
        newTexture.name = "blank";
        newTexture.filepath = "";
        newTexture.generated = true;
        exportTextures[0] = newTexture;

        int numberOfColliderMeshes = COL_MESH.meshCount;
        for (int i = 0; i < numberOfColliderMeshes; i++)
        {
            MeshUtility.Optimize(COL_MESH[i].mesh);
            string ColliderSuffixIndex = ((numberOfColliderMeshes > 1) ? "_" + i : "");
            string ColliderFileName = data.exportFilename + COLLIDER_SUFFIX + ColliderSuffixIndex;
            string ColliderFolder = ROOT_FOLDER + data.exportFilename + "/";
            Export(ColliderFileName, ColliderFolder, data, COL_MESH[i].mesh, exportTextures);
        }
    }

    //returns the number of meshes
    private static int ExportDetails(BuildrData data)
    {
        DynamicMeshGenericMultiMaterialMesh DET_MESH = new DynamicMeshGenericMultiMaterialMesh();
        BuildrDetailExportObject exportObject = BuildrBuildingDetails.Build(DET_MESH, data);

        int numberOfMeshes = exportObject.detailMeshes.Length;
        if (numberOfMeshes == 0)
            return 0;

        string textureName = data.exportFilename + ATLASED_SUFFIX + DETAIL_SUFFIX;
        string textureFileName = textureName + ".png";
        string newDirectory = ROOT_FOLDER + data.exportFilename;

        File.WriteAllBytes(newDirectory + "/" + textureFileName, exportObject.texture.EncodeToPNG());
        ExportMaterial[] exportTextures = new ExportMaterial[1];
        ExportMaterial newTexture = new ExportMaterial();
        newTexture.name = textureName;
        newTexture.filepath = textureFileName;
        newTexture.generated = true;
        exportTextures[0] = newTexture;
        for(int i = 0; i < numberOfMeshes; i++)
        {
            string DetailSuffixIndex = ((numberOfMeshes > 1) ? "_"+i : "");
            string DetailFileName = data.exportFilename + DETAIL_SUFFIX + DetailSuffixIndex;
            string DetailFolder = ROOT_FOLDER + data.exportFilename + "/";
            Export(DetailFileName, DetailFolder, data, exportObject.detailMeshes[i], exportTextures);
        }

        Texture2D.DestroyImmediate(exportObject.texture);

        return numberOfMeshes;
    }

    private static int[] ExportInteriors(BuildrData data)
    {
        int numberOfVolumes = data.plan.numberOfVolumes;
        int[] returnNumberOfMeshes = new int[numberOfVolumes];

        for (int v = 0; v < numberOfVolumes; v++)
        {
            DynamicMeshGenericMultiMaterialMesh INT_MESH = new DynamicMeshGenericMultiMaterialMesh();
            INT_MESH.subMeshCount = data.textures.Count;
            BuildrInteriors.Build(INT_MESH, data, v);

            INT_MESH.Build(data.includeTangents);

            List<int> unusedTextures = INT_MESH.unusedSubmeshes;
            int numberOfUnpackedTextures = data.textures.Count;
            List<ExportMaterial> exportTextures = new List<ExportMaterial>();
            for (int t = 0; t < numberOfUnpackedTextures; t++)
            {
                if (unusedTextures.Contains(t))
                    continue;//skip, unused
                ExportMaterial newTexture = new ExportMaterial();
                newTexture.name = data.textures[t].name;
                newTexture.material = data.textures[t].material;
                newTexture.generated = false;
                newTexture.filepath = data.textures[t].filePath;
                exportTextures.Add(newTexture);
            }

            int numberOfMeshes = INT_MESH.meshCount;
            for (int i = 0; i < numberOfMeshes; i++)
            {
                MeshUtility.Optimize(INT_MESH[i].mesh);
                string VolumeSuffix = ((numberOfVolumes > 1) ? "_" + v : "");
                string DetailSuffixIndex = ((numberOfMeshes > 1) ? "_" + i : "");
                string DetailFileName = data.exportFilename + INTERIOR_SUFFIX + VolumeSuffix + DetailSuffixIndex;
                string DetailFolder = ROOT_FOLDER + data.exportFilename + "/";
                Export(DetailFileName, DetailFolder, data, INT_MESH[i].mesh, exportTextures.ToArray());
            }

            returnNumberOfMeshes[v] = numberOfMeshes;
        }

        return returnNumberOfMeshes;
    }

    private static int[] ExportStairwells(BuildrData data)
    {
        int numberOfVolumes = data.plan.numberOfVolumes;
        int[] returnNumberOfMeshes = new int[numberOfVolumes];

        for (int v = 0; v < numberOfVolumes; v++)
        {
            BuildrVolume volume = data.plan.volumes[v];

            int numberOfUnpackedTextures = data.textures.Count;
            List<ExportMaterial> exportTextures = new List<ExportMaterial>();

            if (!volume.generateStairs) continue;
            DynamicMeshGenericMultiMaterialMesh INT_STAIRWELL = new DynamicMeshGenericMultiMaterialMesh();
            INT_STAIRWELL.subMeshCount = data.textures.Count;
            BuildrStairs.Build(INT_STAIRWELL, data, v, BuildrStairs.StairModes.Stepped, true);
            INT_STAIRWELL.Build(data.includeTangents);

            List<int> unusedStairTextures = INT_STAIRWELL.unusedSubmeshes;
            numberOfUnpackedTextures = data.textures.Count;
            for (int t = 0; t < numberOfUnpackedTextures; t++)
            {
                if (unusedStairTextures.Contains(t))
                    continue;//skip, unused
                ExportMaterial newTexture = new ExportMaterial();
                newTexture.name = data.textures[t].name;
                newTexture.material = data.textures[t].material;
                newTexture.generated = false;
                newTexture.filepath = data.textures[t].filePath;
                exportTextures.Add(newTexture);
            }

            int numberOfStairMeshes = INT_STAIRWELL.meshCount;
            for (int i = 0; i < numberOfStairMeshes; i++)
            {
                MeshUtility.Optimize(INT_STAIRWELL[i].mesh);
                string VolumeSuffix = ((numberOfVolumes > 1) ? "_" + v : "");
                string DetailSuffixIndex = ((numberOfStairMeshes > 1) ? "_" + i : "");
                string DetailFileName = data.exportFilename + STAIR_SUFFIX + VolumeSuffix + DetailSuffixIndex;
                string DetailFolder = ROOT_FOLDER + data.exportFilename + "/";
                Export(DetailFileName, DetailFolder, data, INT_STAIRWELL[i].mesh, exportTextures.ToArray());
            }

            returnNumberOfMeshes[v] = numberOfStairMeshes;
        }

        return returnNumberOfMeshes;
    }

    private static void Export(BuildrData data, Mesh exportMesh, ExportMaterial[] exportTextures)
    {
        Export(data.exportFilename, ROOT_FOLDER + data.exportFilename + "/", data, exportMesh, exportTextures);
    }

    private static void Export(string filename, string folder, BuildrData data, Mesh exportMesh, ExportMaterial[] exportTextures)
    {
        switch (data.filetype)
        {
            case BuildrData.filetypes.obj:
                OBJExporter.Export(folder, filename, exportMesh, exportTextures, data.copyTexturesIntoExportFolder);
                break;
            case BuildrData.filetypes.fbx:
                FBXExporter.Export(folder, filename, exportMesh, exportTextures, data.copyTexturesIntoExportFolder);
                break;
        }
    }

    private static bool CreateFolder(string newDirectory)
    {
        if (Directory.Exists(newDirectory))
        {
            if (EditorUtility.DisplayDialog("File directory exists", "Are you sure you want to overwrite the contents of this file?", "Cancel", "Overwrite"))
            {
                return false;
            }
        }

        try
        {
            Directory.CreateDirectory(newDirectory);
        }
        catch
        {
            EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
            return false;
        }

        return true;
    }
}