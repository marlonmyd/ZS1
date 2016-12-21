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

public class BuildrEditModeBuilding
{
    private static BuildrData data;
    private static int selectedVolume = 0;
    private static int selectedFacade = 0;
    private static int selectedPoint = 0;
    private static int selectedStyle = 0;
    private static int selectedRoofVolume = 0;
    private static int selectedFloorNumberVolume = 0;

    public static void SceneGUI(BuildrEditMode editMode, BuildrData _data, bool shouldSnap, float handleSize)
    {
        data = _data;
        Vector3 camDirection = Camera.current.transform.forward;
        Vector3 camPosition = Camera.current.transform.position;

        int numberOfFacades = data.facades.Count;
        int numberOfRoofs = data.roofs.Count;
        int numberOfTextures = data.textures.Count;

        if (numberOfFacades == 0 || numberOfRoofs == 0 || numberOfTextures == 0)
            return;

        Vector3 position = editMode.transform.position;
        float floorHeight = data.floorHeight;

        BuildrPlan area = data.plan;
        int numberOfVolumes = area.numberOfVolumes;

        int facadeCounter = 0;
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volume = data.plan.volumes[s];
            int volumeSize = volume.Count;
            Vector3 floorCentre = Vector3.zero;
            Handles.color = Color.white;

            for (int p = 0; p < volumeSize; p++)
            {
                int point = volume.points[p];
                Vector3 pointPos = area.points[point].vector3;
                floorCentre += pointPos;

                List<Vector3> verts = new List<Vector3>();
                int indexB = (p < volumeSize - 1) ? p + 1 : 0;
                Vector3 volumeHeight = Vector3.up * (volume.numberOfFloors * data.floorHeight);
                verts.Add(pointPos + position);
                verts.Add(area.points[volume.points[indexB]].vector3 + position);
                verts.Add(verts[1] + volumeHeight);
                verts.Add(verts[0] + volumeHeight);
                if (s == selectedVolume && point == selectedPoint)
                    //display something to highlight this facade
                    Handles.DrawSolidRectangleWithOutline(verts.ToArray(), Color.clear, BuildrColours.MAGENTA);
                Handles.color = BuildrColours.CYAN;
                if (s == selectedRoofVolume)
                    Handles.DrawLine(verts[2], verts[3]);
                if (editMode.showFacadeMarkers)
                {
                    Handles.color = Color.white;
                    Vector3 facadeDirection = Vector3.Cross((verts[0] - verts[1]), Vector3.up);
                    GUIStyle facadeLabelStyle = new GUIStyle();
                    facadeLabelStyle.normal.textColor = Color.white;
                    facadeLabelStyle.alignment = TextAnchor.MiddleCenter;
                    facadeLabelStyle.fixedWidth = 75.0f;
                    Vector3 centerPos = (verts[0] + verts[1]) * 0.5f;
                    bool camVisible = Vector3.Dot(camDirection, centerPos - camPosition) > 0;
                    bool facadeVisible = Vector3.Dot(camDirection, facadeDirection) < 0;
                    if (camVisible && facadeVisible)//only display label when facade is facing camera and is in camera view
                    {
                        Vector3 labelPos = centerPos + facadeDirection.normalized;
                        Handles.Label(labelPos, "facade " + facadeCounter, facadeLabelStyle);
                        Handles.DrawLine(centerPos, labelPos);
                    }
                }
                facadeCounter++;
            }
            floorCentre /= volumeSize;

            //Volume height/floor number slider
            Vector3 volumeHeightDir = Vector3.up * (volume.numberOfFloors * data.floorHeight);
            Vector3 volumePosition = floorCentre + position + volumeHeightDir;
            if (Vector3.Dot(camDirection, volumePosition - camPosition) > 0)//only display label when facade is facing camera
            {
                Handles.Label(volumePosition + (Vector3.up * (handleSize * 0.1f)), "volume " + s);
                Handles.Label(volumePosition, "number of floors " + volume.numberOfFloors);
            }
            Handles.color = Color.green;
            volume.height = Handles.Slider(volumePosition, Vector3.up).y - position.y;
            if (volume.height < data.floorHeight)
                volume.height = data.floorHeight;

            volume.numberOfFloors = Mathf.RoundToInt(volume.height / floorHeight);
        }
    }

    public static void InspectorGUI(BuildrEditMode editMode, BuildrData _data)
    {

        data = _data;
        Undo.RecordObject(data, "Building Modified");
        BuildrPlan plan = data.plan;
        int numberOfFacadeFaces = 0;
        int numberOfVolumes = plan.numberOfVolumes;
        int numberOfFacadeDesigns = data.facades.Count;

        if (numberOfVolumes == 0)
        {
            EditorGUILayout.HelpBox("There are no defined volumes, go to Floorplan and define one", MessageType.Error);
            return;
        }

        int numberOfFacades = data.facades.Count;
        int numberOfRoofs = data.roofs.Count;
        int numberOfTextures = data.textures.Count;

        bool legalBuilding = true;
        if (numberOfFacades == 0)
        {
            EditorGUILayout.HelpBox("There are no facade designs to render building, go to Facades to define a default one", MessageType.Error);
            legalBuilding = false;
        }
        if (numberOfRoofs == 0)
        {
            EditorGUILayout.HelpBox("There are no roof designs to render building, go to Facades to define a default one", MessageType.Error);
            legalBuilding = false;
        }
        if (numberOfTextures == 0)
        {
            EditorGUILayout.HelpBox("There are no textures to render building, go to Textures to define a default one", MessageType.Error);
            legalBuilding = false;
        }

        if (!legalBuilding)
            return;
        
        //Building Name
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", GUILayout.Width(200));
        data.name = EditorGUILayout.TextField(data.name, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        //Floor Height
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Floor height", GUILayout.Width(200));
        data.floorHeight = EditorGUILayout.FloatField(data.floorHeight, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Number of Floors", GUILayout.Width(200));
        int[] volumeSeletionsList = new int[numberOfVolumes];
        string[] volumeSeletionsStringList = new string[numberOfVolumes];
        for (int s = 0; s < numberOfVolumes; s++)
        {
            volumeSeletionsStringList[s] = ("volume " + s);
            volumeSeletionsList[s] = (s);
        }

        selectedFloorNumberVolume = EditorGUILayout.IntPopup(selectedFloorNumberVolume, volumeSeletionsStringList, volumeSeletionsList, GUILayout.Width(100));
        int numberOfFloors = EditorGUILayout.IntField(data.plan.volumes[selectedFloorNumberVolume].numberOfFloors);
        if (GUILayout.Button("^"))
            numberOfFloors++; 
        if (GUILayout.Button("v"))
            numberOfFloors--;
        data.plan.volumes[selectedFloorNumberVolume].numberOfFloors = Mathf.Max(numberOfFloors, 1);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Show Facade Markers", GUILayout.Width(200));
        editMode.showFacadeMarkers = EditorGUILayout.Toggle(editMode.showFacadeMarkers, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUIStyle titlesyle = new GUIStyle(GUI.skin.label);
        titlesyle.fixedHeight = 60;
        titlesyle.fixedWidth = 400;
        titlesyle.alignment = TextAnchor.UpperCenter;
        titlesyle.fontStyle = FontStyle.Bold;
        titlesyle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("Facade Design", titlesyle);
        Texture2D facadeTexture = new Texture2D(1, 1);
        facadeTexture.SetPixel(0, 0, BuildrColours.MAGENTA);
        facadeTexture.Apply();
        Rect sqrPos = new Rect(0, 0, 0, 0);
        if (Event.current.type == EventType.Repaint)
            sqrPos = GUILayoutUtility.GetLastRect();
        GUI.DrawTexture(sqrPos, facadeTexture);
        EditorGUI.LabelField(sqrPos, "Facade Design", titlesyle);

        //create/display the facade selector
        List<int> facadeSeletionsList = new List<int>();
        List<int> facadeRenderList = new List<int>();
        List<string> facadeSeletionsStringList = new List<string>();
        for (int s = 0; s < numberOfVolumes; s++)
        {
            int numberOfPoints = plan.volumes[s].Count;
            numberOfFacadeFaces += numberOfPoints;
            for (int p = 0; p < numberOfPoints; p++)
            {
                int index = facadeSeletionsList.Count;
                facadeSeletionsStringList.Add("facade " + index);
                facadeSeletionsList.Add(index);
                facadeRenderList.Add(p);
            }
        }
        int[] facadeSelections = facadeSeletionsList.ToArray();
        string[] facadeSelectionString = facadeSeletionsStringList.ToArray();

        selectedFacade = EditorGUILayout.IntPopup("Selected Facade", selectedFacade, facadeSelectionString, facadeSelections, GUILayout.Width(400));

        //grab the selected facade
        int facadeCount = 0;
        int selectedVolumePoint = 0;
        for (int s = 0; s < numberOfVolumes; s++)
        {
            int numberOfPoints = plan.volumes[s].Count;
            for (int p = 0; p < numberOfPoints; p++)
            {
                if (selectedFacade == facadeCount)
                {
                    selectedVolume = s;
                    selectedVolumePoint = p;
                    selectedPoint = plan.volumes[s].points[p];
                }
                facadeCount++;
            }
        }

        BuildrVolume volume = plan.volumes[selectedVolume];
        BuildrVolumeStylesUnit[] styles = volume.styles.GetContents();

        bool renderFacade = volume.renderFacade[selectedVolumePoint];
        volume.renderFacade[selectedVolumePoint] = EditorGUILayout.Toggle("Render Facade", renderFacade);

        //ensure the selected style isn't out of bounds
        int numberOfStyles = styles.Length;
        if (selectedStyle >= numberOfStyles)
            selectedStyle = 0;

        //compose a list of style ids from the volume style library
        List<int> entryNum = new List<int>();
        for (int s = 0; s < numberOfStyles; s++)
        {
            if (selectedPoint == styles[s].facadeID)
                entryNum.Add(s);
        }

        int numberOfFacadeStyles = entryNum.Count;

        if (GUILayout.Button("Add style to facade", GUILayout.Width(400)))
            volume.styles.AddStyle(0, selectedPoint, 1);

        GUILayout.BeginHorizontal("box");
        GUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Style", GUILayout.Width(160));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Floors", GUILayout.Width(78));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Position", GUILayout.Width(54));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label(" ", GUILayout.Width(55));
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();

        for (int s = 0; s < numberOfFacadeStyles; s++)
        {
            int index = entryNum[s];
            BuildrVolumeStylesUnit styleUnit = styles[index];

            GUILayout.BeginHorizontal("box");
            GUILayout.BeginHorizontal("box");
            string[] facadeNames = new string[numberOfFacadeDesigns];
            for (int f = 0; f < numberOfFacadeDesigns; f++)
                facadeNames[f] = data.facades[f].name;
            int selectedFacadeDesign = EditorGUILayout.Popup(styleUnit.styleID, facadeNames, GUILayout.Width(160));
            if (selectedFacadeDesign != styleUnit.styleID)
            {
                volume.styles.ModifyStyle(index, selectedFacadeDesign);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            int currentFloors = styleUnit.floors;

            currentFloors = EditorGUILayout.IntField(currentFloors, GUILayout.Width(20));
            if (GUILayout.Button("+", GUILayout.Width(25)))
                currentFloors++;
            EditorGUI.BeginDisabledGroup(currentFloors < 2);
            if (GUILayout.Button("-", GUILayout.Width(25)))
                currentFloors--;
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUI.BeginDisabledGroup((s < 1));
            {
                if (GUILayout.Button("^", GUILayout.Width(25)))
                {
                    volume.styles.MoveEntry(entryNum[s], entryNum[s - 1] + 1);
                    GUI.changed = true;
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup((s > numberOfFacadeStyles - 2));
            {
                if (GUILayout.Button("v", GUILayout.Width(25)))
                {
                    volume.styles.MoveEntry(entryNum[s], entryNum[s + 1] + 1);
                    GUI.changed = true;
                }
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("remove", GUILayout.Width(55)))
            {
                volume.styles.RemoveStyle(index);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            if (currentFloors != styleUnit.floors)
            {
                volume.styles.ModifyFloors(index, currentFloors);
                GUI.changed = true;
            }
        }
        EditorGUILayout.Space();

        titlesyle = new GUIStyle(GUI.skin.label);
        titlesyle.fixedHeight = 60;
        titlesyle.fixedWidth = 400;
        titlesyle.alignment = TextAnchor.UpperCenter;
        titlesyle.fontStyle = FontStyle.Bold;
        titlesyle.normal.textColor = Color.black;

        EditorGUILayout.LabelField("Roof Design", titlesyle);
        facadeTexture = new Texture2D(1, 1);
        facadeTexture.SetPixel(0, 0, BuildrColours.CYAN);
        facadeTexture.Apply();
        sqrPos = new Rect(0, 0, 0, 0);
        if (Event.current.type == EventType.Repaint)
            sqrPos = GUILayoutUtility.GetLastRect();
        GUI.DrawTexture(sqrPos, facadeTexture);
        EditorGUI.LabelField(sqrPos, "Roof Design", titlesyle);



        //create/display the roof selector
        volumeSeletionsList = new int[numberOfVolumes];
        volumeSeletionsStringList = new string[numberOfVolumes];
        for (int s = 0; s < numberOfVolumes; s++)
        {
            volumeSeletionsStringList[s] = ("volume " + s);
            volumeSeletionsList[s] = (s);
        }

        selectedRoofVolume = EditorGUILayout.IntPopup("Selected Volume", selectedRoofVolume, volumeSeletionsStringList, volumeSeletionsList, GUILayout.Width(400));

        string[] roofNames = new string[numberOfRoofs];
        int[] roofList = new int[numberOfRoofs];
        for (int r = 0; r < numberOfRoofs; r++)
        {
            roofList[r] = r;
            roofNames[r] = data.roofs[r].name;
        }

        volume = data.plan.volumes[selectedRoofVolume];
        volume.roofDesignID = EditorGUILayout.IntPopup("Selected Roof Design", volume.roofDesignID, roofNames, roofList, GUILayout.Width(400));
    }
}
