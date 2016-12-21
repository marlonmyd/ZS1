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
using System.IO;
using System.Xml;
using System.Text;

[CanEditMultipleObjects]
public class BuildrGenerateModeEditor
{
    private enum EditModes
    {
        floorplan,
        general
    }

    public static Color BUILDR_COLOUR = new Color(0.13f, 0.3f, 0.72f);
    private static BuildrEditMode editMode;
    private static BuildrData data;
    private static BuildrGenerateConstraints constraints;

    private static EditModes mode = EditModes.general;
    private static string dataFilePath = "Assets/Buildr/XML/none.xml";
    private static char[] filenameDelimiters = new[] { '\\', '/' };
    private static List<string> xmlfilelist = new List<string>();
    private static int selectedFile;
    private static List<string> xmltexturefilelist = new List<string>();
    private static int selectedTextureFile;

    private static bool showFacadeConstraints = true;
    private static bool showRoofConstraints = true;

    public static void OnEnable()
    {
        ScrapeXMLFilenames();
        ScrapeXMLTextureFilenames();
        mode = EditModes.general;
    }

    public static void SceneGUI(BuildrEditMode editMode, BuildrData data, bool shouldSnap, float handleSize)
    {
        switch(mode)
        {
                case EditModes.general:
                //No scene actions
                break;

                case EditModes.floorplan:
                BuildrEditModeFloorplan.SceneGUI(editMode, data.plan, false, handleSize);
                break;
        }

        if(constraints.constrainPlanByArea)
        {
            Rect area = constraints.area;
            Vector3 position = editMode.transform.position;

            Vector3 areaLeft = new Vector3(area.xMin, 0, (area.yMin + area.yMax) / 2);
            Vector3 areaRight = new Vector3(area.xMax, 0, (area.yMin + area.yMax) / 2);
            Vector3 areaBottom = new Vector3((area.xMin + area.xMax) / 2, 0, area.yMin);
            Vector3 areaTop = new Vector3((area.xMin + area.xMax) / 2, 0, area.yMax);

            Vector3 newAreaLeft = Handles.Slider(areaLeft + position, Vector3.left, HandleUtility.GetHandleSize(areaLeft) * 0.666f, Handles.ArrowCap, 0.0f);
            Vector3 newAreaRight = Handles.Slider(areaRight + position, Vector3.right, HandleUtility.GetHandleSize(areaLeft) * 0.666f, Handles.ArrowCap, 0.0f);
            Vector3 newAreaBottom = Handles.Slider(areaBottom + position, Vector3.back, HandleUtility.GetHandleSize(areaLeft) * 0.666f, Handles.ArrowCap, 0.0f);
            Vector3 newAreaTop = Handles.Slider(areaTop + position, Vector3.forward, HandleUtility.GetHandleSize(areaLeft) * 0.666f, Handles.ArrowCap, 0.0f);

            newAreaLeft -= position;
            newAreaRight -= position;
            newAreaBottom -= position;
            newAreaTop -= position;

            if (areaLeft != newAreaLeft)
                area.xMin = Mathf.Min(newAreaLeft.x, area.xMax - 1.0f);
            if (areaRight != newAreaRight)
                area.xMax = Mathf.Max(newAreaRight.x, area.xMin + 1.0f);
            if (areaBottom != newAreaBottom)
                area.yMin = Mathf.Min(newAreaBottom.z, area.yMax - 1.0f);
            if (areaTop != newAreaTop)
                area.yMax = Mathf.Max(newAreaTop.z, area.yMin + 1.0f);

            constraints.area = area;

            Vector3 coreBL = new Vector3(area.xMin, 0, area.yMin) + position;
            Vector3 coreBR = new Vector3(area.xMax, 0, area.yMin) + position;
            Vector3 coreTL = new Vector3(area.xMin, 0, area.yMax) + position;
            Vector3 coreTR = new Vector3(area.xMax, 0, area.yMax) + position;
            Handles.DrawLine(coreBL, coreBR);
            Handles.DrawLine(coreBR, coreTR);
            Handles.DrawLine(coreTR, coreTL);
            Handles.DrawLine(coreTL, coreBL);
        }
    }

    public static void InspectorGUI(BuildrEditMode _editMode, BuildrData _data)
    {
        editMode = _editMode;
        data = _data;
        constraints = data.generatorConstraints;

        EditModes newmode = (EditModes)EditorGUILayout.EnumPopup(mode);

        if(newmode != mode)
        {
            mode = newmode;
            switch (mode)
            {
                case EditModes.general:
                    editMode.stage = BuildrEditMode.stages.building;
                    break;

                case EditModes.floorplan:
                    editMode.stage = BuildrEditMode.stages.floorplan;
                    editMode.SetMode(BuildrEditMode.modes.floorplan);
                    break;
            }
        }

        switch (mode)
        {
            case EditModes.general:
                GeneralOptionsInspector();
                break;

            case EditModes.floorplan:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Constrain Building Generation to Floorplan", GUILayout.Width(280));
                constraints.constrainPlanByPlan = EditorGUILayout.Toggle(constraints.constrainPlanByPlan);
                EditorGUILayout.EndHorizontal();
                EditorGUI.BeginDisabledGroup(!constraints.constrainPlanByPlan);
                BuildrEditModeFloorplan.InspectorGUI(editMode,_data.plan);
                EditorGUI.EndDisabledGroup();

                if(data.plan != null)
                    constraints.plan = data.plan.Duplicate();
                break;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_editMode);
            EditorUtility.SetDirty(_data);
            _editMode.UpdateRender();
        }
    }

    public static void GeneralOptionsInspector() 
    {
        if(constraints==null)
        {
            data.generatorConstraints = ScriptableObject.CreateInstance<BuildrGenerateConstraints>();
            data.generatorConstraints.Init();
            constraints = data.generatorConstraints;
        }
        
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Saved constraints", GUILayout.Width(110));
        int numberOfFiles = xmlfilelist.Count;
        string[] fileNames = new string[numberOfFiles];
        for (int t = 0; t < numberOfFiles; t++)
        {
            string filepath = xmlfilelist[t];
            string[] filepathsplit = filepath.Split(filenameDelimiters);
            string displayPath = filepathsplit[filepathsplit.Length - 1];
            fileNames[t] = displayPath;
        }
        int newSelectedFile = EditorGUILayout.Popup(selectedFile, fileNames);
        if (newSelectedFile != selectedFile)
        {
            if(EditorUtility.DisplayDialog("Load Constraints","Are you sure you want to load a set of constraints from file?","Yes","Mmm, no."))
            {
                selectedFile = newSelectedFile;
                dataFilePath = xmlfilelist[selectedFile];
                LoadConstraints();
            }
        }

        if(GUILayout.Button("Load"))
        {
            LoadConstraints();
        }
        if (GUILayout.Button("Save"))
        {
            SaveConstraints();
        }
        if (GUILayout.Button("Save As"))
        {
            SaveConstraintsAs();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Seed", GUILayout.Width(100));
        constraints.useSeed = EditorGUILayout.Toggle(constraints.useSeed);
        EditorGUI.BeginDisabledGroup(!constraints.useSeed);
        constraints.seed = EditorGUILayout.IntField(constraints.seed);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Floor Height", GUILayout.Width(140));
        EditorGUILayout.LabelField(constraints.minimumFloorHeight.ToString(), GUILayout.Width(35));
        EditorGUILayout.MinMaxSlider(ref constraints.minimumFloorHeight, ref constraints.maximumFloorHeight, 2.0f, 3.8f);
        EditorGUILayout.LabelField(constraints.maximumFloorHeight.ToString(), GUILayout.Width(35));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Constrain Maximum Height", GUILayout.Width(170));
        constraints.constrainHeight = EditorGUILayout.Toggle(constraints.constrainHeight);
        EditorGUI.BeginDisabledGroup(!constraints.constrainHeight);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Minimum:", GUILayout.Width(70));
        constraints.minimumHeight = EditorGUILayout.FloatField(constraints.minimumHeight, GUILayout.Width(30));
        EditorGUILayout.LabelField("metres", GUILayout.Width(53));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Maximum:", GUILayout.Width(70));
        constraints.maximumHeight = EditorGUILayout.FloatField(constraints.maximumHeight, GUILayout.Width(30));
        EditorGUILayout.LabelField("metres",GUILayout.Width(53));
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Constrain Generated Floorplan", GUILayout.Width(200));
        constraints.constrainPlanByArea = EditorGUILayout.Toggle(constraints.constrainPlanByArea);
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginDisabledGroup(!constraints.constrainPlanByArea);
        EditorGUILayout.LabelField("Contraint Area");
        constraints.area = EditorGUILayout.RectField(constraints.area);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Texture Pack",GUILayout.Width(100));
        //Debug.Log("InspectorGUI " + constraints.texturePackXML + " " + xmltexturefilelist.IndexOf(constraints.texturePackXML));
        selectedTextureFile = xmltexturefilelist.IndexOf(constraints.texturePackXML);
        int numberTextureOfFiles = xmltexturefilelist.Count;
        string[] textureFileNames = new string[numberTextureOfFiles];
        for (int t = 0; t < numberTextureOfFiles; t++)
        {
            string filepath = xmltexturefilelist[t];
            string[] filepathsplit = filepath.Split(filenameDelimiters);
            string displayPath = filepathsplit[filepathsplit.Length - 1];
            textureFileNames[t] = displayPath;
        }
        int newSelectedTextureFile = EditorGUILayout.Popup(selectedTextureFile, textureFileNames);
        if (newSelectedTextureFile != selectedTextureFile)
        {
            selectedTextureFile = newSelectedTextureFile;
            constraints.texturePackXML = xmltexturefilelist[selectedTextureFile];
            BuildrBuildingGenerator.RefreshTextures(data);
        }

        if (GUILayout.Button("Edit Texture Packs"))
        {
            EditorWindow textureEditor = EditorWindow.GetWindow<BuildrBuiltInTextureEditor>(true);
            textureEditor.minSize = new Vector2(280f, 490f);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Design Choices", GUILayout.Width(100));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        constraints.rowStyled = EditorGUILayout.Toggle("Row Styles", constraints.rowStyled);
        constraints.columnStyled = EditorGUILayout.Toggle("Column Styles", constraints.columnStyled);
//        constraints.externalAirConUnits = EditorGUILayout.Toggle("External Air Conditioner Units", constraints.rowStyled);
        constraints.splitLevel = EditorGUILayout.Toggle("Split Level Volume", constraints.splitLevel);
        constraints.taperedLevels = EditorGUILayout.Toggle("Tapered Volume", constraints.taperedLevels);
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        constraints.singleLevel = EditorGUILayout.Toggle("Single Volume Level", constraints.singleLevel);
        constraints.atticDesign = EditorGUILayout.Toggle("Attic Design", constraints.atticDesign);
        constraints.shopGroundFloor = EditorGUILayout.Toggle("Shop Design", constraints.shopGroundFloor);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //FACADE CONSTRAINTS
        int styleLabelSize = 130;
        EditorGUILayout.BeginVertical("box");
        showFacadeConstraints = EditorGUILayout.Foldout(showFacadeConstraints, "Facade Constraints");
        if (showFacadeConstraints)
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bay Width", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.openingMinimumWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.openingMinimumWidth, ref constraints.openingMaximumWidth, 0.5f, 2.0f);
            EditorGUILayout.LabelField(constraints.openingMaximumWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bay Height", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.openingMinimumHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.openingMinimumHeight, ref constraints.openingMaximumHeight, 0.5f, constraints.maximumFloorHeight);
            EditorGUILayout.LabelField(constraints.openingMaximumHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bay Spacing", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.minimumBayMinimumWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.minimumBayMinimumWidth, ref constraints.minimumBayMaximumWidth, 0.125f, 2.0f);
            EditorGUILayout.LabelField(constraints.minimumBayMaximumWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bay Opening Depth", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.openingMinimumDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.openingMinimumDepth, ref constraints.openingMaximumDepth, -0.70f, 0.70f);
            EditorGUILayout.LabelField(constraints.openingMaximumDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Facade Depth", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.facadeMinimumDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.facadeMinimumDepth, ref constraints.facadeMaximumDepth, -0.5f, 0.5f);
            EditorGUILayout.LabelField(constraints.facadeMaximumDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //ROOF CONSTRAINTS

        EditorGUILayout.BeginVertical("box");
        showRoofConstraints = EditorGUILayout.Foldout(showRoofConstraints, "Roof Constraints");
        if (showRoofConstraints)
        {

            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.minimumRoofHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.minimumRoofHeight, ref constraints.maximumRoofHeight, 1.0f, constraints.maximumFloorHeight);
            EditorGUILayout.LabelField(constraints.maximumRoofHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mansard Face Depth", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.minimumRoofDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.minimumRoofDepth, ref constraints.maximumRoofDepth, 0.0f, 1.0f);
            EditorGUILayout.LabelField(constraints.maximumRoofDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mansard Floor Depth", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.minimumRoofFloorDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.minimumRoofFloorDepth, ref constraints.maximumRoofFloorDepth, 0.0f, 1.0f);
            EditorGUILayout.LabelField(constraints.maximumRoofFloorDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Flat Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleFlat = EditorGUILayout.Toggle(constraints.roofStyleFlat);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mansard Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleMansard = EditorGUILayout.Toggle(constraints.roofStyleMansard);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Barrel Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleBarrel = EditorGUILayout.Toggle(constraints.roofStyleBarrel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gabled Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleGabled = EditorGUILayout.Toggle(constraints.roofStyleGabled);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hipped Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleHipped = EditorGUILayout.Toggle(constraints.roofStyleHipped);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Lean To Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleLeanto = EditorGUILayout.Toggle(constraints.roofStyleLeanto);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Steepled Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleSteepled = EditorGUILayout.Toggle(constraints.roofStyleSteepled);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sawtooth Roof Styles", GUILayout.Width(styleLabelSize));
            constraints.roofStyleSawtooth = EditorGUILayout.Toggle(constraints.roofStyleSawtooth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Allow Dormers", GUILayout.Width(styleLabelSize));
            constraints.allowDormers = EditorGUILayout.Toggle(constraints.allowDormers, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!constraints.allowDormers);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dormer Chance", GUILayout.Width(150));
            constraints.dormerChance = EditorGUILayout.FloatField(constraints.dormerChance, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dormer Width", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.dormerMinimumWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumWidth, ref constraints.dormerMaximumWidth, 0.5f, 2.0f);
            EditorGUILayout.LabelField(constraints.dormerMaximumWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dormer Height", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.dormerMinimumHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumHeight, ref constraints.dormerMaximumHeight, 0.5f, 2.0f);
            EditorGUILayout.LabelField(constraints.dormerMaximumHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dormer Roof Height", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.dormerMinimumRoofHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumRoofHeight, ref constraints.dormerMaximumRoofHeight, 0.5f, 2.0f);
            EditorGUILayout.LabelField(constraints.dormerMaximumRoofHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dormer Spacing", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.dormerMinimumSpacing.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.dormerMinimumSpacing, ref constraints.dormerMaximumSpacing, 0.5f, 3.0f);
            EditorGUILayout.LabelField(constraints.dormerMaximumSpacing.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Allow Parapets", GUILayout.Width(styleLabelSize));
            constraints.allowParapet = EditorGUILayout.Toggle(constraints.allowParapet, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!constraints.allowParapet);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parapet Render Chance", GUILayout.Width(150));
            constraints.parapetChance = EditorGUILayout.FloatField(constraints.parapetChance, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parapet Width", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.parapetMinimumDesignWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumDesignWidth, ref constraints.parapetMaximumDesignWidth, 0.0f, 1.0f);
            EditorGUILayout.LabelField(constraints.parapetMaximumDesignWidth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parapet Height", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.parapetMinimumHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumHeight, ref constraints.parapetMaximumHeight, 0.0f, 1.0f);
            EditorGUILayout.LabelField(constraints.parapetMaximumHeight.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parapet Front Depth", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.parapetMinimumFrontDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumFrontDepth, ref constraints.parapetMaximumFrontDepth, -1.0f, 1.0f);
            EditorGUILayout.LabelField(constraints.parapetMaximumFrontDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parapet Back Depth", GUILayout.Width(styleLabelSize));
            EditorGUILayout.LabelField(constraints.parapetMinimumBackDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.MinMaxSlider(ref constraints.parapetMinimumBackDepth, ref constraints.parapetMaximumBackDepth, -1.0f, 1.0f);
            EditorGUILayout.LabelField(constraints.parapetMaximumBackDepth.ToString(), GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }


        EditorGUILayout.EndVertical();

        if(GUILayout.Button("Generate",GUILayout.Height(40)))
        {
            BuildrBuildingGenerator.Generate(data);
            editMode.UpdateRender(BuildrEditMode.renderModes.full);
        }
    }

    private static void ScrapeXMLFilenames()
    {
        string[] paths = Directory.GetFiles("Assets/Buildr/XML");
        xmlfilelist.Clear();
        XmlNodeList xmlData = null;
        foreach (string path in paths)
        {
            if (path.Contains(".meta")) continue;
            if (!path.Contains(".xml")) continue;

            XmlDocument xml = new XmlDocument();
            StreamReader sr = new StreamReader(path);
            xml.LoadXml(sr.ReadToEnd());
            sr.Close();
            xmlData = xml.SelectNodes("data/datatype");
            if (xmlData.Count > 0)
            {
                if (xmlData[0].FirstChild.Value == "ProGen")
                {
                    xmlfilelist.Add(path);
                }
            }

        }
    }

    private static void ScrapeXMLTextureFilenames()
    {
        string[] paths = Directory.GetFiles("Assets/Buildr/XML");
        xmltexturefilelist.Clear();
        XmlNodeList xmlData;
        foreach (string path in paths)
        {
            if (path.Contains(".meta")) continue;
            if (!path.Contains(".xml")) continue;

            XmlDocument xml = new XmlDocument();
            StreamReader sr = new StreamReader(path);
            xml.LoadXml(sr.ReadToEnd());
            sr.Close();
            xmlData = xml.SelectNodes("data/datatype");
            if (xmlData.Count > 0)
            {
                if (xmlData[0].FirstChild.Value == "TexturePack")
                {
                    xmltexturefilelist.Add(path);
                }
            }
        }
    }

    private static void SaveConstraintsAs()
    {
        dataFilePath = EditorUtility.SaveFilePanel(
            "Save Data to XML",
            "Assets/Buildr/XML",
            "data.xml",
            "xml");
        SaveConstraints();
        xmlfilelist.Add(dataFilePath);
        selectedFile = xmlfilelist.Count - 1;
    }


    private static void SaveConstraints()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version='1.0' encoding='ISO-8859-15'?>");
        sb.AppendLine("<!-- Unity3D Asset Buildr ProGen Constraint http://buildr.jasperstocker.com -->");
        sb.AppendLine("<data>");
        sb.AppendLine("<datatype>ProGen</datatype>");
        sb.AppendLine("<constraint>");

        sb.AppendLine("<constrainType>" + constraints.constrainType + "</constrainType>");
        sb.AppendLine("<type>" + constraints.type + "</type>");
        sb.AppendLine("<useSeed>" + constraints.useSeed + "</useSeed>");
        sb.AppendLine("<seed>" + constraints.seed + "</seed>");

        sb.AppendLine("<minimumFloorHeight>" + constraints.minimumFloorHeight + "</minimumFloorHeight>");
        sb.AppendLine("<maximumFloorHeight>" + constraints.maximumFloorHeight + "</maximumFloorHeight>");
        sb.AppendLine("<constrainHeight>" + constraints.constrainHeight + "</constrainHeight>");
        sb.AppendLine("<minimumHeight>" + constraints.minimumHeight + "</minimumHeight>");
        sb.AppendLine("<maximumHeight>" + constraints.maximumHeight + "</maximumHeight>");
        sb.AppendLine("<constrainFloorNumber>" + constraints.constrainFloorNumber + "</constrainFloorNumber>");
        sb.AppendLine("<floorNumber>" + constraints.floorNumber + "</floorNumber>");
        sb.AppendLine("<constrainPlanByArea>" + constraints.constrainPlanByArea + "</constrainPlanByArea>");
        sb.AppendLine("<areax>" + constraints.area.x + "</areax>");
        sb.AppendLine("<areay>" + constraints.area.y + "</areay>");
        sb.AppendLine("<areawidth>" + constraints.area.width + "</areawidth>");
        sb.AppendLine("<areaheight>" + constraints.area.height + "</areaheight>");
        sb.AppendLine("<constrainPlanByPlan>" + constraints.constrainPlanByPlan + "</constrainPlanByPlan>");
        //TODO support plans
        sb.AppendLine("<constrainDesign>" + constraints.constrainDesign + "</constrainDesign>");
        sb.AppendLine("<texturePackXML>" + constraints.texturePackXML + "</texturePackXML>");

        sb.AppendLine("<openingMinimumWidth>" + constraints.openingMinimumWidth + "</openingMinimumWidth>");
        sb.AppendLine("<openingMaximumWidth>" + constraints.openingMaximumWidth + "</openingMaximumWidth>");
        sb.AppendLine("<openingMinimumHeight>" + constraints.openingMinimumHeight + "</openingMinimumHeight>");
        sb.AppendLine("<openingMaximumHeight>" + constraints.openingMaximumHeight + "</openingMaximumHeight>");
        sb.AppendLine("<minimumBayMinimumWidth>" + constraints.minimumBayMinimumWidth + "</minimumBayMinimumWidth>");
        sb.AppendLine("<minimumBayMaximumWidth>" + constraints.minimumBayMaximumWidth + "</minimumBayMaximumWidth>");
        sb.AppendLine("<openingMinimumDepth>" + constraints.openingMinimumDepth + "</openingMinimumDepth>");
        sb.AppendLine("<openingMaximumDepth>" + constraints.openingMaximumDepth + "</openingMaximumDepth>");
        sb.AppendLine("<facadeMinimumDepth>" + constraints.facadeMinimumDepth + "</facadeMinimumDepth>");
        sb.AppendLine("<facadeMaximumDepth>" + constraints.facadeMaximumDepth + "</facadeMaximumDepth>");


        sb.AppendLine("<minimumRoofHeight>" + constraints.minimumRoofHeight + "</minimumRoofHeight>");
        sb.AppendLine("<maximumRoofHeight>" + constraints.maximumRoofHeight + "</maximumRoofHeight>");
        sb.AppendLine("<minimumRoofDepth>" + constraints.minimumRoofDepth + "</minimumRoofDepth>");
        sb.AppendLine("<maximumRoofDepth>" + constraints.maximumRoofDepth + "</maximumRoofDepth>");
        sb.AppendLine("<minimumRoofFloorDepth>" + constraints.minimumRoofFloorDepth + "</minimumRoofFloorDepth>");
        sb.AppendLine("<maximumRoofFloorDepth>" + constraints.maximumRoofFloorDepth + "</maximumRoofFloorDepth>");
        sb.AppendLine("<roofStyleFlat>" + constraints.roofStyleFlat + "</roofStyleFlat>");
        sb.AppendLine("<roofStyleMansard>" + constraints.roofStyleMansard + "</roofStyleMansard>");
        sb.AppendLine("<roofStyleBarrel>" + constraints.roofStyleBarrel + "</roofStyleBarrel>");
        sb.AppendLine("<roofStyleGabled>" + constraints.roofStyleGabled + "</roofStyleGabled>");
        sb.AppendLine("<roofStyleHipped>" + constraints.roofStyleHipped + "</roofStyleHipped>");
        sb.AppendLine("<roofStyleLeanto>" + constraints.roofStyleLeanto + "</roofStyleLeanto>");
        sb.AppendLine("<roofStyleSteepled>" + constraints.roofStyleSteepled + "</roofStyleSteepled>");
        sb.AppendLine("<roofStyleSawtooth>" + constraints.roofStyleSawtooth + "</roofStyleSawtooth>");
        sb.AppendLine("<allowParapet>" + constraints.allowParapet + "</allowParapet>");
        sb.AppendLine("<parapetChance>" + constraints.parapetChance + "</parapetChance>");
        sb.AppendLine("<allowDormers>" + constraints.allowDormers + "</allowDormers>");
        sb.AppendLine("<dormerChance>" + constraints.dormerChance + "</dormerChance>");
        sb.AppendLine("<dormerMinimumWidth>" + constraints.dormerMinimumWidth + "</dormerMinimumWidth>");
        sb.AppendLine("<dormerMaximumWidth>" + constraints.dormerMaximumWidth + "</dormerMaximumWidth>");
        sb.AppendLine("<dormerMinimumHeight>" + constraints.dormerMinimumHeight + "</dormerMinimumHeight>");
        sb.AppendLine("<dormerMaximumHeight>" + constraints.dormerMaximumHeight + "</dormerMaximumHeight>");
        sb.AppendLine("<dormerMinimumRoofHeight>" + constraints.dormerMinimumRoofHeight + "</dormerMinimumRoofHeight>");
        sb.AppendLine("<dormerMaximumRoofHeight>" + constraints.dormerMaximumRoofHeight + "</dormerMaximumRoofHeight>");
        sb.AppendLine("<dormerMinimumSpacing>" + constraints.dormerMinimumSpacing + "</dormerMinimumSpacing>");
        sb.AppendLine("<dormerMaximumSpacing>" + constraints.dormerMaximumSpacing + "</dormerMaximumSpacing>");

        sb.AppendLine("<parapetMinimumDesignWidth>" + constraints.parapetMinimumDesignWidth + "</parapetMinimumDesignWidth>");
        sb.AppendLine("<parapetMaximumDesignWidth>" + constraints.parapetMaximumDesignWidth + "</parapetMaximumDesignWidth>");
        sb.AppendLine("<parapetMinimumHeight>" + constraints.parapetMinimumHeight + "</parapetMinimumHeight>");
        sb.AppendLine("<parapetMaximumHeight>" + constraints.parapetMaximumHeight + "</parapetMaximumHeight>");
        sb.AppendLine("<parapetMinimumFrontDepth>" + constraints.parapetMinimumFrontDepth + "</parapetMinimumFrontDepth>");
        sb.AppendLine("<parapetMaximumFrontDepth>" + constraints.parapetMaximumFrontDepth + "</parapetMaximumFrontDepth>");
        sb.AppendLine("<parapetMinimumBackDepth>" + constraints.parapetMinimumBackDepth + "</parapetMinimumBackDepth>");
        sb.AppendLine("<parapetMaximumBackDepth>" + constraints.parapetMaximumBackDepth + "</parapetMaximumBackDepth>");

        sb.AppendLine("<rowStyled>" + constraints.rowStyled + "</rowStyled>");
        sb.AppendLine("<columnStyled>" + constraints.columnStyled + "</columnStyled>");
        sb.AppendLine("<externalAirConUnits>" + constraints.externalAirConUnits + "</externalAirConUnits>");
        sb.AppendLine("<splitLevel>" + constraints.splitLevel + "</splitLevel>");
        sb.AppendLine("<taperedLevels>" + constraints.taperedLevels + "</taperedLevels>");
        sb.AppendLine("<singleLevel>" + constraints.singleLevel + "</singleLevel>");
        sb.AppendLine("<atticDesign>" + constraints.atticDesign + "</atticDesign>");
        sb.AppendLine("<shopGroundFloor>" + constraints.shopGroundFloor + "</shopGroundFloor>");

        sb.AppendLine("</constraint>");
        sb.AppendLine("</data>");

        StreamWriter sw = new StreamWriter(dataFilePath);
        sw.Write(sb.ToString());//write out contents of data to XML
        sw.Close();
    }

    private static void LoadConstraints()
    {
        XmlNodeList xmlData = null;

        if (File.Exists(dataFilePath))
        {
            XmlDocument xml = new XmlDocument();
            StreamReader sr = new StreamReader(dataFilePath);
            xml.LoadXml(sr.ReadToEnd());
            sr.Close();
            xmlData = xml.SelectNodes("data");
            if (xmlData.Count > 0)
            {
                if (xmlData[0]["datatype"].FirstChild.Value != "ProGen")
                    xmlData = null;
            }
        }

        if (xmlData != null)
        {
            XmlNode node = (xmlData[0]["constraint"]);

            constraints.constrainType = node["constrainType"].FirstChild.Value == "True";
            constraints.type = (BuildrGenerateConstraints.buildingTypes)System.Enum.Parse(typeof(BuildrGenerateConstraints.buildingTypes), node["type"].FirstChild.Value);
            constraints.useSeed = node["useSeed"].FirstChild.Value == "True";
            constraints.seed = int.Parse(node["seed"].FirstChild.Value);
            constraints.minimumFloorHeight = float.Parse(node["minimumFloorHeight"].FirstChild.Value);
            constraints.maximumFloorHeight = float.Parse(node["maximumFloorHeight"].FirstChild.Value);
            constraints.constrainHeight = node["constrainHeight"].FirstChild.Value == "True";
            constraints.minimumHeight = float.Parse(node["minimumHeight"].FirstChild.Value);
            constraints.maximumHeight = float.Parse(node["maximumHeight"].FirstChild.Value);
            constraints.constrainFloorNumber = node["constrainFloorNumber"].FirstChild.Value == "True";
            constraints.floorNumber = int.Parse(node["floorNumber"].FirstChild.Value);
            constraints.constrainPlanByArea = node["constrainPlanByArea"].FirstChild.Value == "True";
            constraints.area.x = float.Parse(node["areax"].FirstChild.Value);
            constraints.area.y = float.Parse(node["areay"].FirstChild.Value);
            constraints.area.width = float.Parse(node["areawidth"].FirstChild.Value);
            constraints.area.height = float.Parse(node["areaheight"].FirstChild.Value);
            constraints.constrainPlanByPlan = node["constrainPlanByPlan"].FirstChild.Value == "True";
            //TODO support plans
            constraints.constrainDesign = node["constrainDesign"].FirstChild.Value == "True";
            constraints.texturePackXML = node["texturePackXML"].FirstChild.Value;

            constraints.openingMinimumWidth = float.Parse(node["openingMinimumWidth"].FirstChild.Value);
            constraints.openingMaximumWidth = float.Parse(node["openingMaximumWidth"].FirstChild.Value);
            constraints.openingMinimumHeight = float.Parse(node["openingMinimumHeight"].FirstChild.Value);
            constraints.openingMaximumHeight = float.Parse(node["openingMaximumHeight"].FirstChild.Value);
            constraints.minimumBayMinimumWidth = float.Parse(node["minimumBayMinimumWidth"].FirstChild.Value);
            constraints.minimumBayMaximumWidth = float.Parse(node["minimumBayMaximumWidth"].FirstChild.Value);
            constraints.openingMinimumDepth = float.Parse(node["openingMinimumDepth"].FirstChild.Value);
            constraints.openingMaximumDepth = float.Parse(node["openingMaximumDepth"].FirstChild.Value);
            constraints.facadeMinimumDepth = float.Parse(node["facadeMinimumDepth"].FirstChild.Value);
            constraints.facadeMaximumDepth = float.Parse(node["facadeMaximumDepth"].FirstChild.Value);

            constraints.minimumRoofHeight = float.Parse(node["minimumRoofHeight"].FirstChild.Value);
            constraints.maximumRoofHeight = float.Parse(node["maximumRoofHeight"].FirstChild.Value);
            constraints.minimumRoofDepth = float.Parse(node["minimumRoofDepth"].FirstChild.Value);
            constraints.maximumRoofDepth = float.Parse(node["maximumRoofDepth"].FirstChild.Value);
            constraints.minimumRoofFloorDepth = float.Parse(node["minimumRoofFloorDepth"].FirstChild.Value);
            constraints.maximumRoofFloorDepth = float.Parse(node["maximumRoofFloorDepth"].FirstChild.Value);
            constraints.roofStyleFlat = node["roofStyleFlat"].FirstChild.Value == "True";
            constraints.roofStyleMansard = node["roofStyleMansard"].FirstChild.Value == "True";
            constraints.roofStyleBarrel = node["roofStyleBarrel"].FirstChild.Value == "True";
            constraints.roofStyleGabled = node["roofStyleGabled"].FirstChild.Value == "True";
            constraints.roofStyleHipped = node["roofStyleHipped"].FirstChild.Value == "True";
            constraints.roofStyleLeanto = node["roofStyleLeanto"].FirstChild.Value == "True";
            constraints.roofStyleSteepled = node["roofStyleSteepled"].FirstChild.Value == "True";
            constraints.roofStyleSawtooth = node["roofStyleSawtooth"].FirstChild.Value == "True";
            constraints.allowParapet = node["allowParapet"].FirstChild.Value == "True";
            constraints.parapetChance = float.Parse(node["parapetChance"].FirstChild.Value);
            constraints.allowDormers = node["allowDormers"].FirstChild.Value == "True";
            constraints.dormerChance = float.Parse(node["dormerChance"].FirstChild.Value);
            constraints.dormerMinimumWidth = float.Parse(node["dormerMinimumWidth"].FirstChild.Value);
            constraints.dormerMaximumWidth = float.Parse(node["dormerMaximumWidth"].FirstChild.Value);
            constraints.dormerMinimumHeight = float.Parse(node["dormerMinimumHeight"].FirstChild.Value);
            constraints.dormerMaximumHeight = float.Parse(node["dormerMaximumHeight"].FirstChild.Value);
            constraints.dormerMinimumRoofHeight = float.Parse(node["dormerMinimumRoofHeight"].FirstChild.Value);
            constraints.dormerMaximumRoofHeight = float.Parse(node["dormerMaximumRoofHeight"].FirstChild.Value);
            constraints.dormerMinimumSpacing = float.Parse(node["dormerMinimumSpacing"].FirstChild.Value);
            constraints.dormerMaximumSpacing = float.Parse(node["dormerMaximumSpacing"].FirstChild.Value);


            constraints.parapetMinimumDesignWidth = float.Parse(node["parapetMinimumDesignWidth"].FirstChild.Value);
            constraints.parapetMaximumDesignWidth = float.Parse(node["parapetMaximumDesignWidth"].FirstChild.Value);
            constraints.parapetMinimumHeight = float.Parse(node["parapetMinimumHeight"].FirstChild.Value);
            constraints.parapetMaximumHeight = float.Parse(node["parapetMaximumHeight"].FirstChild.Value);
            constraints.parapetMinimumFrontDepth = float.Parse(node["parapetMinimumFrontDepth"].FirstChild.Value);
            constraints.parapetMaximumFrontDepth = float.Parse(node["parapetMaximumFrontDepth"].FirstChild.Value);
            constraints.parapetMinimumBackDepth = float.Parse(node["parapetMinimumBackDepth"].FirstChild.Value);
            constraints.parapetMaximumBackDepth = float.Parse(node["parapetMaximumBackDepth"].FirstChild.Value);

            constraints.rowStyled = node["rowStyled"].FirstChild.Value == "True";
            constraints.columnStyled = node["columnStyled"].FirstChild.Value == "True";
            constraints.externalAirConUnits = node["externalAirConUnits"].FirstChild.Value == "True";
            constraints.splitLevel = node["splitLevel"].FirstChild.Value == "True";
            constraints.taperedLevels = node["taperedLevels"].FirstChild.Value == "True";
            constraints.singleLevel = node["singleLevel"].FirstChild.Value == "True";
            constraints.atticDesign = node["atticDesign"].FirstChild.Value == "True";
            constraints.shopGroundFloor = node["shopGroundFloor"].FirstChild.Value == "True";
            

            ScrapeXMLTextureFilenames();
            selectedTextureFile = xmltexturefilelist.IndexOf(constraints.texturePackXML);
        }
    }
}
