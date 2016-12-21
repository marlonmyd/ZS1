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

[CanEditMultipleObjects]
[CustomEditor(typeof(BuildrEditMode))]
public class BuildrEditModeEditor : Editor
{
    private BuildrEditMode _editMode;
    private BuildrData _data;

    private GUIStyle textStyle;
    private Texture2D[] _stageToolbarTextures;
    private readonly string[] _stageToolbar = new[] { "Floorplan Design", "Texture Designs", "Facade Designs", "Roof Designs", "Building Details", "Interior", "Building Design", "Generation Options", "Export Options" };

    private Vector3 _position;
    private float _handleSize;
    private bool _shouldSnap;

    private Rect _splashRect;
    private Texture2D _facadeTexture;

    void Awake()
    {
        textStyle = new GUIStyle();
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.onHover.textColor = Color.yellow;

        _stageToolbarTextures = new Texture2D[9];
        _stageToolbarTextures[0] = (Texture2D)Resources.Load("GUI/floorplan");
        _stageToolbarTextures[1] = (Texture2D)Resources.Load("GUI/textures");
        _stageToolbarTextures[2] = (Texture2D)Resources.Load("GUI/facades");
        _stageToolbarTextures[3] = (Texture2D)Resources.Load("GUI/roofs");
        _stageToolbarTextures[4] = (Texture2D)Resources.Load("GUI/details");
        _stageToolbarTextures[5] = (Texture2D)Resources.Load("GUI/interiors");
        _stageToolbarTextures[6] = (Texture2D)Resources.Load("GUI/building");
        _stageToolbarTextures[7] = (Texture2D)Resources.Load("GUI/options");
        _stageToolbarTextures[8] = (Texture2D)Resources.Load("GUI/export");

        _facadeTexture = new Texture2D(1, 1);
        _facadeTexture.SetPixel(0, 0, BuildrColours.BLUE);
        _facadeTexture.Apply();
    }

    void OnEnable()
    {
        if (target != null)
        {
            _editMode = (BuildrEditMode)target;
            _data = _editMode.data;
        }

        
        bool editing = (_data != null) ? _data.editing : true;
        if (!editing)
        {
            BuildrGenerateModeEditor.OnEnable();
        }
    }

    void OnSceneGUI()
    {
        if (SceneView.focusedWindow == null)
            return;

        if (_data == null)
        {
            _data = _editMode.data;
            return;
        }

        _position = _editMode.transform.position;
        _handleSize = HandleUtility.GetHandleSize(_position);

        if (_editMode.alwaysSnap)
            _shouldSnap = true;
        else
            _shouldSnap = Event.current.shift;

        if(_data.editing)
        {
            switch(_editMode.stage)
            {
                case BuildrEditMode.stages.floorplan:
                    BuildrEditModeFloorplan.SceneGUI(_editMode, _data.plan, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.building:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    BuildrEditModeBuilding.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.facades:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.textures:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    BuildrEditModeTextures.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.roofs:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.details:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    BuildrEditModeDetails.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.interior:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    BuildrEditModeInterior.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.options:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;

                case BuildrEditMode.stages.export:
                    BuildrEditModeHUD.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
                    break;
            }
        }else
        {
            BuildrGenerateModeEditor.SceneGUI(_editMode, _data, _shouldSnap, _handleSize);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_editMode);
            EditorUtility.SetDirty(_data);
            _editMode.UpdateRender();
        }
    }

    public override void OnInspectorGUI()
    {
        bool editing = (_data != null) ? _data.editing : true;

        EditorGUILayout.Space();

        GUILayout.BeginVertical(GUILayout.Width(400));
        if (editing)
        {
            if(_editMode.stage == BuildrEditMode.stages.start)
            {
                GUILayout.Space(10);
                EditorGUILayout.Space();
                if(Event.current.type == EventType.Repaint)
                    _splashRect = GUILayoutUtility.GetLastRect();
                _splashRect.width = 300;
                _splashRect.height = 194;
                GUI.DrawTexture(_splashRect, (Texture2D)Resources.Load("splash"));
                GUILayout.Space(_splashRect.height);

                EditorGUILayout.LabelField("Welcome to BuildR.\nSelect from the following menu to begin a new building.", GUILayout.Height(30));

                if(GUILayout.Button("Start floorplan with basic square"))
                {
                    _editMode.StartBuilding();
                    _editMode.SetMode(BuildrEditMode.modes.addNewVolume);
                }

                if(GUILayout.Button("Start floorplan by drawing square"))
                {
                    _editMode.StartBuilding();
                    _editMode.SetMode(BuildrEditMode.modes.addNewVolumeByDraw);
                }

                if(GUILayout.Button("Start floorplan by drawing walls"))
                {
                    _editMode.StartBuilding();
                    _editMode.SetMode(BuildrEditMode.modes.addNewVolumeByPoints);
                }

                if(GUILayout.Button("Procedurally Generate Building"))
                {
                    _editMode.StartBuilding();
                    _editMode.SetStage(BuildrEditMode.stages.building);
                    _editMode.data.editing = false;
                }

                if(GUILayout.Button("Start floorplan from XML"))
                {
                    string xmlPath = EditorUtility.OpenFilePanel("Select the XML file...", "Assets/BuildR/Exported/", "xml");
                    if(xmlPath == "")
                        return;
                    BuildrData buildData = _editMode.gameObject.AddComponent<BuildrData>();
                    buildData.plan = ScriptableObject.CreateInstance<BuildrPlan>();
                    BuildrXMLImporter.Import(xmlPath, buildData);
                    _editMode.SetStage(BuildrEditMode.stages.building);
                }

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Online Documentation"))
                {
                    Help.BrowseURL("http://buildr.jasperstocker.com/documentation/");
                }

                if(GUILayout.Button("Contact"))
                {
                    Help.BrowseURL("mailto:email@jasperstocker.com");
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                //TITLE
                GUIStyle title = new GUIStyle(GUI.skin.label);
                title.fixedHeight = 60;
                title.fixedWidth = 223;
                title.alignment = TextAnchor.UpperCenter;
                title.fontStyle = FontStyle.Bold;
                title.normal.textColor = Color.white;
                EditorGUILayout.LabelField("Edit Mode", title);
                Texture2D facadeTexture = new Texture2D(1, 1);
                facadeTexture.SetPixel(0, 0, BuildrColours.BLUE);
                facadeTexture.Apply();
                Rect sqrPos = new Rect(0, 0, 0, 0);
                if (Event.current.type == EventType.Repaint)
                    sqrPos = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(sqrPos, facadeTexture);
                EditorGUI.LabelField(sqrPos, "Edit Mode", title);

                if (GUILayout.Button("Switch to Generate Mode", GUILayout.Width(165)))
                {
                    _editMode.stage = BuildrEditMode.stages.building;
                    _data.editing = false;
                }
                EditorGUILayout.EndHorizontal();

                bool isLegal = _data.plan != null;
                if(isLegal) isLegal = !(_data.plan.illegalPoints.Length > 0);
                if(isLegal) isLegal = _editMode.transform.localScale == Vector3.one;

                EditorGUILayout.Space();
                
                GUILayout.BeginHorizontal();
                int currentStage = (int)_editMode.stage - 1;

                GUIContent[] guiContent = new GUIContent[9];
                for(int i = 0; i < 9; i++)
                    guiContent[i] = new GUIContent(_stageToolbarTextures[i], _stageToolbar[i]);
                int newStage = GUILayout.Toolbar(currentStage, guiContent, GUILayout.Width(400), GUILayout.Height(50));
                if(newStage != currentStage)
                {
                    _editMode.stage = (BuildrEditMode.stages)(newStage + 1);
                    _editMode.mode = BuildrEditMode.modes.floorplan;//reset the floorplan mode
                    UpdateGui();
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                if(_editMode.transform.localScale != Vector3.one)
                {
                    EditorGUILayout.HelpBox("The scale is not set to (1,1,1)!", MessageType.Error);
                    return;
                }

                if(_data.plan!=null)
                {   
                if(_data.plan.illegalPoints.Length > 0)
                {
                    EditorGUILayout.HelpBox("Your floorplan contains walls that intersect one another. " + "\nEnsure that no walls intersect another. " + "\nThe intersecting walls are highlighted red.", MessageType.Error);
                }
                }

                switch(_editMode.stage)
                {
                    case BuildrEditMode.stages.floorplan:
                        RenderTitle(_stageToolbar[0]);
                        BuildrEditModeFloorplan.InspectorGUI(_editMode, _data.plan);
                        break;

                    case BuildrEditMode.stages.textures:
                        RenderTitle(_stageToolbar[1]);
                        BuildrEditModeTextures.InspectorGUI(_editMode, _data);
                        break;

                    case BuildrEditMode.stages.facades:
                        RenderTitle(_stageToolbar[2]);
                        BuildrEditModeFacades.InspectorGUI(_editMode, _data);
                        break;

                    case BuildrEditMode.stages.roofs:
                        RenderTitle(_stageToolbar[3]);
                        BuildrEditModeRoofs.InspectorGUI(_editMode, _data);
                        break;

                    case BuildrEditMode.stages.details:
                        RenderTitle(_stageToolbar[4]);
                        BuildrEditModeDetails.InspectorGUI(_editMode, _data);
                        break;

                    case BuildrEditMode.stages.interior:
                        RenderTitle(_stageToolbar[5]);
                        BuildrEditModeInterior.InspectorGUI(_editMode, _data);
                        break;

                    case BuildrEditMode.stages.building:
                        RenderTitle(_stageToolbar[6]);
                        BuildrEditModeBuilding.InspectorGUI(_editMode, _data);
                        break;

                    case BuildrEditMode.stages.options:
                        RenderTitle(_stageToolbar[7]);
                        BuildrEditModeOptions.InspectorGUI(_editMode, _data);
                        break;

                    case BuildrEditMode.stages.export:
                        RenderTitle(_stageToolbar[8]);
                        BuildrEditModeExport.InspectorGUI(_editMode, _data);
                        break;
                }
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();

            //TITLE
            GUIStyle title = new GUIStyle(GUI.skin.label);
            title.fixedHeight = 60;
            title.fixedWidth = 223;
            title.alignment = TextAnchor.UpperCenter;
            title.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("", title);
            Texture2D facadeTexture = new Texture2D(1, 1);
            facadeTexture.SetPixel(0, 0, BuildrColours.BLUE);
            facadeTexture.Apply();
            Rect sqrPos = new Rect(0, 0, 0, 0);
            if (Event.current.type == EventType.Repaint)
                sqrPos = GUILayoutUtility.GetLastRect();
            GUI.DrawTexture(sqrPos, facadeTexture);
            EditorGUI.LabelField(sqrPos, "Genereate Mode (beta)", title);

            if (GUILayout.Button("Switch to Edit Mode", GUILayout.Width(165)))
            {
                _editMode.stage = BuildrEditMode.stages.generate;
                _data.editing = true;
            }
            EditorGUILayout.EndHorizontal();

            //generating
            BuildrGenerateModeEditor.InspectorGUI(_editMode,_data);
        }
        GUILayout.EndVertical();

        if (Event.current.type == EventType.ValidateCommand)
        {
            switch (Event.current.commandName)
            {
                case "UndoRedoPerformed":
                    GUI.changed = true;
                    break;
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_editMode);
            if(_data!=null)
                EditorUtility.SetDirty(_data);//TODO: LOOK INTO ERROR HERE
            _editMode.UpdateRender();
            UpdateGui();
            //Undo.RegisterSceneUndo("Building Modified");
        }
    }

    private void RenderTitle(string title)
    {
        GUIStyle titlesyle = new GUIStyle(GUI.skin.label);
        titlesyle.fixedHeight = 60;
        titlesyle.fixedWidth = 400;
        titlesyle.alignment = TextAnchor.UpperCenter;
        titlesyle.fontStyle = FontStyle.Bold;
        titlesyle.normal.textColor = Color.white;
        EditorGUILayout.LabelField(title, titlesyle);
        Texture2D facadeTexture = new Texture2D(1, 1);
        facadeTexture.SetPixel(0, 0, BuildrColours.BLUE);
        facadeTexture.Apply();
        Rect sqrPos = new Rect(0, 0, 0, 0);
        if (Event.current.type == EventType.Repaint)
            sqrPos = GUILayoutUtility.GetLastRect();
        GUI.DrawTexture(sqrPos, facadeTexture);
        EditorGUI.LabelField(sqrPos, title, titlesyle);
    }

    private void UpdateGui()
    {
        Repaint();
        HandleUtility.Repaint();
        SceneView.RepaintAll();
        GUI.changed = true;
    }
}
