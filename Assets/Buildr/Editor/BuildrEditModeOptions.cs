// BuildR
// Available on the Unity3D Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEditor;
using UnityEngine;
using System.Collections;

public class BuildrEditModeOptions
{
    //private static BuildrData data;

    public static void InspectorGUI(BuildrEditMode editMode, BuildrData _data)
    {
        //ensure that the model exists in the scene
        if (editMode.fullMesh == null)
            editMode.UpdateRender();

        Undo.RecordObject(_data, "Building Modified");
        //texture library string array
        BuildrTexture[] textures = _data.textures.ToArray();
        int numberOfTextures = textures.Length;
        string[] textureNames = new string[numberOfTextures];
        for (int t = 0; t < numberOfTextures; t++)
            textureNames[t] = textures[t].name;

        //Render a full version version
        EditorGUILayout.LabelField("Building Generation Types");
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUI.BeginDisabledGroup(editMode.renderMode == BuildrEditMode.renderModes.full);
        if (GUILayout.Button("Full Detail", GUILayout.Height(28)))
        {
            editMode.UpdateRender(BuildrEditMode.renderModes.full);
        }
        EditorGUI.EndDisabledGroup();

        //Render a low detail version
        EditorGUI.BeginDisabledGroup(editMode.renderMode == BuildrEditMode.renderModes.lowDetail);
        if (GUILayout.Button("Low Detail", GUILayout.Height(28)))
        {
            editMode.UpdateRender(BuildrEditMode.renderModes.lowDetail);
        }
        EditorGUI.EndDisabledGroup();

        //Render a box version
        EditorGUI.BeginDisabledGroup(editMode.renderMode == BuildrEditMode.renderModes.box);
        if (GUILayout.Button("Box Outline", GUILayout.Height(28)))
        {
            editMode.UpdateRender(BuildrEditMode.renderModes.box);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        //Toggle showing the wireframe when we have selected the model.
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUILayout.LabelField("Show Wireframe");
        editMode.showWireframe = EditorGUILayout.Toggle(editMode.showWireframe, GUILayout.Width(15));
        EditorGUILayout.EndHorizontal();

        //Tangent calculation
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUI.BeginDisabledGroup(editMode.fullMesh.hasTangents);
        if (GUILayout.Button("Build Tangents", GUILayout.Height(38)))
        {
            if (editMode.fullMesh != null)
                editMode.fullMesh.SolveTangents();
            if(editMode.detailMesh != null)
                editMode.detailMesh.SolveTangents();
            GUI.changed = false;
        }
        EditorGUI.EndDisabledGroup();
        if (!editMode.fullMesh.hasTangents)
            EditorGUILayout.HelpBox("The model doesn't have tangents", MessageType.Warning);
        EditorGUILayout.EndHorizontal();

        //Lightmap rendering
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUI.BeginDisabledGroup(editMode.fullMesh.lightmapUvsCalculated);
        if (GUILayout.Button("Build Lightmap UVs", GUILayout.Height(38)))
        {
//            Undo.RegisterSceneUndo("Build Lightmap UVs");
            if(editMode.fullMesh != null)
            {
                for (int i = 0; i < editMode.fullMesh.meshCount; i++)
                    Unwrapping.GenerateSecondaryUVSet(editMode.fullMesh[i].mesh);}
                editMode.fullMesh.lightmapUvsCalculated = true;
            if(editMode.detailMesh != null)
            {
                for (int i = 0; i < editMode.detailMesh.meshCount; i++)
                    Unwrapping.GenerateSecondaryUVSet(editMode.detailMesh[i].mesh);
                editMode.detailMesh.lightmapUvsCalculated = true;
            }
            int numberOfInteriors = editMode.interiorMeshes.Count;
            for(int i = 0; i < numberOfInteriors; i++)
            {
                DynamicMeshGenericMultiMaterialMesh interiorMesh = editMode.interiorMeshes[i];
                for (int j = 0; j < interiorMesh.meshCount; j++)
                    Unwrapping.GenerateSecondaryUVSet(interiorMesh[j].mesh);
                interiorMesh.lightmapUvsCalculated = true;
            }
            GUI.changed = false;
        }
        EditorGUI.EndDisabledGroup();
        if (!editMode.fullMesh.lightmapUvsCalculated)
            EditorGUILayout.HelpBox("The model doesn't have lightmap UVs", MessageType.Warning);
        EditorGUILayout.EndHorizontal();

        //Mesh Optimisation
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUI.BeginDisabledGroup(editMode.fullMesh.optimised);
        if (GUILayout.Button("Optimise Mesh For Runtime", GUILayout.Height(38)))
        {
//            Undo.RegisterSceneUndo("Optimise Mesh");
            if (editMode.fullMesh != null)
            {
                for (int i = 0; i < editMode.fullMesh.meshCount; i++)
                    MeshUtility.Optimize(editMode.fullMesh[i].mesh);
                editMode.fullMesh.optimised = true;
            }

            if (editMode.detailMesh != null)
            {
                for(int i = 0; i < editMode.detailMesh.meshCount; i++)
                    MeshUtility.Optimize(editMode.detailMesh[i].mesh);
                editMode.detailMesh.optimised = true;
            }
            GUI.changed = false;
        }
        EditorGUI.EndDisabledGroup();
        if (!editMode.fullMesh.optimised)
            EditorGUILayout.HelpBox("The model is currently fully optimised for runtime", MessageType.Warning);
        EditorGUILayout.EndHorizontal();

        //Underside render toggle
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUILayout.LabelField("Render Underside of Building");
        bool drawUnderside = EditorGUILayout.Toggle(_data.drawUnderside, GUILayout.Width(15));
        if(drawUnderside != _data.drawUnderside)
        {
            _data.drawUnderside = drawUnderside;
        }
        EditorGUILayout.EndHorizontal();

        //Define the height of foundations if you need them
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUILayout.LabelField("Foundation Height", GUILayout.Width(120));
        _data.foundationHeight = EditorGUILayout.Slider(_data.foundationHeight, 0, 25);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Foundation Texture", GUILayout.Width(120));
        _data.foundationTexture = EditorGUILayout.Popup(_data.foundationTexture, textureNames, GUILayout.Width(260));
        EditorGUILayout.EndHorizontal();

        //Collider mesh
        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUILayout.LabelField("Generate Collider");
        _data.generateCollider = (BuildrData.ColliderGenerationModes)EditorGUILayout.EnumPopup(_data.generateCollider);
        EditorGUILayout.EndHorizontal();

        //One draw call mesh  -TODO
//        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
//        EditorGUILayout.LabelField("One Draw Call");
//        _data.oneDrawCall = EditorGUILayout.Toggle(_data.oneDrawCall);
//        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < editMode.fullMesh.meshCount; i++)
            EditorUtility.SetSelectedWireframeHidden(editMode.meshHolders[i].GetComponent<Renderer>(), !editMode.showWireframe);
    }
}
