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
using System.Collections.Generic;

public class BuildrEditModeInterior
{
    private static BuildrData data;
    private static int selectedVolumeIndex = 0;
    private static int selectedFloor = 0;

    public static void SceneGUI(BuildrEditMode _editMode, BuildrData _data, bool shouldSnap, float handleSize)
    {
        Vector3 camDirection = Camera.current.transform.forward;
        Vector3 camPosition = Camera.current.transform.position;
        Vector3 position = _editMode.transform.position;
        data = _data;
        BuildrPlan plan = data.plan;
        BuildrVolume selectedVolume = plan.volumes[selectedVolumeIndex];
        int selectedVolumeSize = selectedVolume.Count;
        Handles.color = Color.white;
        int facadeCounter = 0;

        GUIStyle whiteLabelStyle = new GUIStyle();
        whiteLabelStyle.normal.textColor = Color.white;
        whiteLabelStyle.alignment = TextAnchor.MiddleCenter;
        whiteLabelStyle.fixedWidth = 75.0f;

        int numberOfVolumes = plan.numberOfVolumes;
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volume = data.plan.volumes[s];
            int volumeSize = volume.Count;
            Vector3 floorCentre = Vector3.zero;

            for (int p = 0; p < volumeSize; p++)
            {
                int point = volume.points[p];
                Vector3 pointPos = plan.points[point].vector3;
                floorCentre += pointPos;
            }
            floorCentre /= volumeSize;

            if(s==selectedVolumeIndex)
            {
                whiteLabelStyle.normal.textColor = BuildrColours.RED;
                whiteLabelStyle.fontStyle = FontStyle.Bold;
            }
            else
            {
                whiteLabelStyle.normal.textColor = Color.white;
                whiteLabelStyle.fontStyle = FontStyle.Normal;
            }

            //Volume height/floor number slider
            Vector3 volumeHeightDir = Vector3.up * (volume.numberOfFloors * data.floorHeight);
            Vector3 volumePosition = floorCentre + position + volumeHeightDir;
            Handles.color = Color.white;
            if (Vector3.Dot(camDirection, volumePosition - camPosition) > 0)//only display label when facade is facing camera
            {
                Handles.Label(volumePosition + (Vector3.up * (handleSize * 0.1f)), "volume " + s, whiteLabelStyle);
            }
        }
        whiteLabelStyle.normal.textColor = Color.white;
        whiteLabelStyle.fontStyle = FontStyle.Normal;

        for (int p = 0; p < selectedVolumeSize; p++)
        {
            int point = selectedVolume.points[p];
            Vector3 pointPos = plan.points[point].vector3;

            List<Vector3> verts = new List<Vector3>();
            int indexB = (p < selectedVolumeSize - 1) ? p + 1 : 0;
            Vector3 floorHeightVector = Vector3.up * (selectedFloor * data.floorHeight);
            Vector3 ceilingHeightVector = Vector3.up * ((selectedFloor+1) * data.floorHeight);
            Vector3 p0 = pointPos + position;
            Vector3 p1 = plan.points[selectedVolume.points[indexB]].vector3 + position;
            verts.Add(pointPos + position);
            verts.Add(plan.points[selectedVolume.points[indexB]].vector3 + position);
            verts.Add(verts[1] + floorHeightVector);
            verts.Add(verts[0] + floorHeightVector);
            Handles.color = BuildrColours.RED;
            Handles.DrawLine(p0 + floorHeightVector, p1 + floorHeightVector);
            Handles.DrawLine(p0 + ceilingHeightVector, p1 + ceilingHeightVector);
            Handles.DrawLine(p0 + floorHeightVector, p0 + ceilingHeightVector);
            if (_editMode.showFacadeMarkers)
            {
                Handles.color = Color.white;
                Vector3 facadeDirection = Vector3.Cross((verts[0] - verts[1]), Vector3.up);
                Vector3 centerPos = (verts[0] + verts[1]) * 0.5f;
                bool camVisible = Vector3.Dot(camDirection, centerPos - camPosition) > 0;
                bool facadeVisible = Vector3.Dot(camDirection, facadeDirection) < 0;
                if (camVisible && facadeVisible)//only display label when facade is facing camera and is in camera view
                {
                    Vector3 labelPos = centerPos + facadeDirection.normalized;
                    Handles.Label(labelPos, "facade " + facadeCounter, whiteLabelStyle);
                    Handles.DrawLine(centerPos, labelPos);
                }
            }
            facadeCounter++;
        }
    }


    public static void InspectorGUI(BuildrEditMode editMode, BuildrData _data)
    {

        data = _data;
        Undo.RecordObject(data, "Interior Modified");

        BuildrTexture[] textures = data.textures.ToArray();
        int numberOfTextures = textures.Length;
        string[] textureNames = new string[numberOfTextures];
        for (int t = 0; t < numberOfTextures; t++)
            textureNames[t] = textures[t].name;

        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUILayout.LabelField("Render Interior of Building");
        bool renderInterior = EditorGUILayout.Toggle(_data.renderInteriors, GUILayout.Width(15));
        if (renderInterior != _data.renderInteriors)
        {
            _data.renderInteriors = renderInterior;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUILayout.LabelField("Cull All Building Bays");
        bool cullBays = EditorGUILayout.Toggle(_data.cullBays, GUILayout.Width(15));
        if (cullBays != _data.cullBays)
        {
            _data.cullBays = cullBays;
        }
        EditorGUILayout.EndHorizontal();

        //Floor Height
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Floor height", GUILayout.Width(200));
        float newFloorHeight = EditorGUILayout.FloatField(data.floorHeight, GUILayout.Width(50));
        if (newFloorHeight != data.floorHeight)
        {
            data.floorHeight = newFloorHeight;
        }
        EditorGUILayout.LabelField("metres", GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(!renderInterior);

        //Ceiling Height
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interior Ceiling Height", GUILayout.Width(200));
        float newCeilingHeight = EditorGUILayout.Slider(data.interiorCeilingHeight, 0, 1);
        if (newCeilingHeight != data.interiorCeilingHeight)
        {
            data.interiorCeilingHeight = newCeilingHeight;
        }
        EditorGUILayout.EndHorizontal();

        BuildrPlan plan = data.plan;
        int numberOfVolumes = plan.numberOfVolumes;
        int[] volumeSeletionsList = new int[numberOfVolumes];
        string[] volumeSeletionsStringList = new string[numberOfVolumes];
        for (int s = 0; s < numberOfVolumes; s++)
        {
            volumeSeletionsStringList[s] = ("volume " + s);
            volumeSeletionsList[s] = (s);
        }
        selectedVolumeIndex = EditorGUILayout.IntPopup("Selected Volume", selectedVolumeIndex, volumeSeletionsStringList, volumeSeletionsList, GUILayout.Width(400));
        BuildrVolume volume = plan.volumes[selectedVolumeIndex];


        //Stairs
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Generate Stairs", GUILayout.Width(110));
        volume.generateStairs = EditorGUILayout.Toggle(volume.generateStairs);
        EditorGUILayout.EndHorizontal();

        if(plan.cores.Count == 0)
            EditorGUILayout.HelpBox("There are no building cores defined. Go to floorplan to define one so you can generate a stairwell", MessageType.Error);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stair Width", GUILayout.Width(110));
        volume.staircaseWidth = EditorGUILayout.Slider(volume.staircaseWidth, 0.5f, 5.0f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Minimum Step Riser Value", GUILayout.Width(150));
        volume.stepHeight = EditorGUILayout.Slider(volume.stepHeight, 0.05f, 0.5f);
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(!volume.generateStairs);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Stairwell Textures");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Wall Texture", GUILayout.Width(120));
        volume.stairwellWallTexture = EditorGUILayout.Popup(volume.stairwellWallTexture, textureNames);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Floor Texture", GUILayout.Width(120));
        volume.stairwellFloorTexture = EditorGUILayout.Popup(volume.stairwellFloorTexture, textureNames);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Ceiling Texture", GUILayout.Width(120));
        volume.stairwellCeilingTexture = EditorGUILayout.Popup(volume.stairwellCeilingTexture, textureNames);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Step Texture", GUILayout.Width(120));
        volume.stairwellStepTexture = EditorGUILayout.Popup(volume.stairwellStepTexture, textureNames);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();

        //Basement floors
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Basement Floors", GUILayout.Width(110));
        EditorGUILayout.LabelField(volume.numberOfBasementFloors.ToString("F0"), GUILayout.Width(40));
        EditorGUI.BeginDisabledGroup(volume.numberOfBasementFloors < 1);
        if (GUILayout.Button("-"))
            volume.numberOfBasementFloors--;
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("+"))
            volume.numberOfBasementFloors++;
        EditorGUILayout.EndHorizontal();

        int numberOfFloors = volume.numberOfFloors;
        int numberOfBasementFloors = volume.numberOfBasementFloors;
        int totalNumberOfFloors = numberOfBasementFloors + numberOfFloors;
        int[] floorSeletionsList = new int[totalNumberOfFloors];
        string[] floorSeletionsStringList = new string[totalNumberOfFloors];
        for (int f = -numberOfBasementFloors; f < numberOfFloors; f++)
        {
            int index = f + numberOfBasementFloors;
            if(f>0)
                floorSeletionsStringList[index] = "floor " + f;
            else if(f<0)
                floorSeletionsStringList[index] = "basement " + -f;
            else
                floorSeletionsStringList[index] = "ground floor ";
            floorSeletionsList[index] = (f);
        }
        selectedFloor = EditorGUILayout.IntPopup("Selected Floor", selectedFloor, floorSeletionsStringList, floorSeletionsList, GUILayout.Width(400));
        
        EditorGUILayout.LabelField("Interior Textures");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Floor Texture", GUILayout.Width(120));
        volume.FloorTexture(selectedFloor, EditorGUILayout.Popup(volume.FloorTexture(selectedFloor), textureNames));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Wall Texture", GUILayout.Width(120));
        volume.WallTexture(selectedFloor, EditorGUILayout.Popup(volume.WallTexture(selectedFloor), textureNames));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Ceiling Texture", GUILayout.Width(120));
        volume.CeilingTexture(selectedFloor, EditorGUILayout.Popup(volume.CeilingTexture(selectedFloor), textureNames));
        EditorGUILayout.EndHorizontal();

        if(GUILayout.Button("Use Values for All Floors"))
        {
            int useFloorTextureIndex = volume.FloorTexture(selectedFloor);
            int useWallTextureIndex = volume.WallTexture(selectedFloor);
            int useCeilingTextureIndex = volume.CeilingTexture(selectedFloor);
            for(int f = 0; f < numberOfFloors; f++)
            {
                volume.FloorTexture(f, useFloorTextureIndex);
                volume.WallTexture(f, useWallTextureIndex);
                volume.CeilingTexture(f, useCeilingTextureIndex);
            }
        }

        if (GUILayout.Button("Use Values for Entire Building"))
        {
            int useFloorTextureIndex = volume.FloorTexture(selectedFloor);
            int useWallTextureIndex = volume.WallTexture(selectedFloor);
            int useCeilingTextureIndex = volume.CeilingTexture(selectedFloor);

            for (int v = 0; v < numberOfVolumes; v++)
            {
                BuildrVolume thisvolume = plan.volumes[v];
                int numberOfFloorsToModifiy = thisvolume.numberOfFloors;
                for (int f = 0; f < numberOfFloorsToModifiy; f++)
                {
                    thisvolume.FloorTexture(f, useFloorTextureIndex);
                    thisvolume.WallTexture(f, useWallTextureIndex);
                    thisvolume.CeilingTexture(f, useCeilingTextureIndex);
                }
            }
        }

        EditorGUI.EndDisabledGroup();
    }
}
