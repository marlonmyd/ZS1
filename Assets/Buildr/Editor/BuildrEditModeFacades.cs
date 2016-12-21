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

public class BuildrEditModeFacades
{

    private static float depth = 1;
    private static BuildrData data;
    private static int selectedFacade = 0;
    private static int selectedBayPatternIndex = 0;
    private static int editTextureOnFacade = 0;
    private static int selectedBayDesign = 0;
    private static Vector2 bayDesignPatternScrollView;

    public static void SceneGUI(BuildrEditMode editMode, BuildrData data, bool shouldSnap, float handleSize)
    {

    }

    public static void InspectorGUI(BuildrEditMode editMode, BuildrData _data)
    {
        int helpWidth = 20;

        data = _data;

        Undo.RecordObject(data, "Facade Modified");

        BuildrFacadeDesign[] facades = data.facades.ToArray();
        int numberOfFacades = facades.Length;
        selectedFacade = Mathf.Clamp(selectedFacade, 0, numberOfFacades - 1);

        if (numberOfFacades == 0)
        {
            EditorGUILayout.HelpBox("There are no facade designs to show", MessageType.Info);
            return;
        }

        bool hasUnusedFacades = false;
        int unusedIndex = 0;
        //Check all facades have een used and warn if there are unused ones
        for(int i = 0; i < numberOfFacades; i++)
        {
            bool facadeUnused = true;
            foreach(BuildrVolume volume in data.plan.volumes)
            {
                if(volume.ContainsFacade(i))
                {
                    facadeUnused = false;
                    break;
                }
            }
            if(facadeUnused)
            {
                hasUnusedFacades = true;
                unusedIndex = i;
                break;
            }
        }
        if (hasUnusedFacades)
            EditorGUILayout.HelpBox("There are facade designs that are not applied to your building and are unused.\nGo to the Building section to apply them to a facade.\nCheck facade design \""+facades[unusedIndex].name+"\"", MessageType.Warning);

        //Facade Selector
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Facade Design:", GUILayout.Width(145));
        string[] facadeNames = new string[numberOfFacades];
        for (int f = 0; f < numberOfFacades; f++)
            facadeNames[f] = facades[f].name;
        selectedFacade = EditorGUILayout.Popup(selectedFacade, facadeNames);
        BuildrFacadeDesign bFacade = facades[selectedFacade];
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add", GUILayout.Width(60)))
        {
            data.facades.Add(new BuildrFacadeDesign("new facade " + numberOfFacades));
            facades = data.facades.ToArray();
            numberOfFacades++;
            selectedFacade = numberOfFacades - 1;
        }

        if (GUILayout.Button("Duplicate", GUILayout.Width(90)))
        {
            data.facades.Add(bFacade.Duplicate());
            facades = data.facades.ToArray();
            numberOfFacades++;
            selectedFacade = numberOfFacades - 1;
        }

        if (GUILayout.Button("Delete", GUILayout.Width(70)))
        {
            if (EditorUtility.DisplayDialog("Deleting Facade Design Entry", "Are you sure you want to delete this facade?", "Delete", "Cancel"))
            {
                data.RemoveFacadeDesign(bFacade);
                selectedFacade = 0;
                GUI.changed = true;

                return;
            }
        }

        if (GUILayout.Button("Import", GUILayout.Width(71)))
        {
            string xmlPath = EditorUtility.OpenFilePanel("Select the XML file...", "Assets/BuildR/Exported/", "xml");
            if (xmlPath == "")
                return;
            BuildrXMLImporter.ImportFacades(xmlPath, _data);
            facades = _data.facades.ToArray();
            selectedFacade = 0;
            GUI.changed = true;
        }

        if (GUILayout.Button("Export", GUILayout.Width(71)))
        {
            string xmlPath = EditorUtility.SaveFilePanel("Export as...", "Assets/BuildR/Exported/", _data.name + "_facadeLibrary", "xml");
            if (xmlPath == "")
                return;
            BuildrXMLExporter.ExportFacades(xmlPath, _data);
            GUI.changed = true;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        bFacade = facades[selectedFacade];//reassign
        bFacade.name = EditorGUILayout.TextField("Facade Name: ", bFacade.name);


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Facade Design Type:", GUILayout.Width(145));
        bFacade.type = (BuildrFacadeDesign.types)EditorGUILayout.EnumPopup(bFacade.type);
        if (GUILayout.Button("?", GUILayout.Width(helpWidth)))
        {
            string helpTitle = "Help - Design Type";
            string helpBody = "This allows you to select the type of design you're using.\n" +
                "Simple - the facade openings will be uniform\n" +
                "Patterned - the facade openings will follow a pattern of defined dimensions and textures\n";
            EditorUtility.DisplayDialog(helpTitle, helpBody, "close");
        }
        EditorGUILayout.EndHorizontal();

        int numberOfTextures = data.textures.Count;
        string[] textureNames = new string[numberOfTextures];
        for (int t = 0; t < numberOfTextures; t++)
            textureNames[t] = data.textures[t].name;

        bFacade.hasWindows = EditorGUILayout.Toggle("Facade Has Bays", bFacade.hasWindows);

        if (bFacade.hasWindows)
        {
            if (bFacade.type == BuildrFacadeDesign.types.simple)
            {
                BuildrBay bbay = bFacade.simpleBay;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Render Bay", GUILayout.Width(146));
                bool renderBayBack = EditorGUILayout.Toggle(bbay.renderBack);
                if (renderBayBack != bbay.renderBack)
                {
                    bbay.renderBack = renderBayBack;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Bay Model", GUILayout.Width(146));
                bbay.bayModel = (GameObject)EditorGUILayout.ObjectField(bbay.bayModel, typeof(GameObject), false);
                if (GUILayout.Button("Clear", GUILayout.Width(70)))
                    bbay.bayModel = null;
                EditorGUILayout.EndHorizontal();

                float bbayopeningWidth = Mathf.Max(EditorGUILayout.FloatField("Opening Width", bbay.openingWidth), 0);
                if (bbayopeningWidth != bbay.openingWidth)
                {
                    bbay.openingWidth = bbayopeningWidth;
                }
                float bbayopeningHeight = Mathf.Max(EditorGUILayout.FloatField("Opening Height", bbay.openingHeight), 0);
                if (bbayopeningHeight > data.floorHeight)
                    bbayopeningHeight = data.floorHeight;
                if (bbayopeningHeight != bbay.openingHeight)
                {
                    bbay.openingHeight = bbayopeningHeight;
                }
                float bbayminimumBayWidth = Mathf.Max(EditorGUILayout.FloatField("Min. Spacing", bbay.minimumBayWidth), 0);
                if (bbayminimumBayWidth != bbay.minimumBayWidth)
                {
                    bbay.minimumBayWidth = bbayminimumBayWidth;
                }

                float bbayopeningWidthRatio = EditorGUILayout.Slider("Horizontal Space Ratio", bbay.openingWidthRatio, 0, 1);
                if(bbayopeningWidthRatio != bbay.openingWidthRatio)
                {
                    bbay.openingWidthRatio = bbayopeningWidthRatio;
                }
                float bbayopeningHeightRatio = EditorGUILayout.Slider("Vertical Space Ratio", bbay.openingHeightRatio, 0, 1);
                if (bbayopeningHeightRatio != bbay.openingHeightRatio)
                {
                    bbay.openingHeightRatio = bbayopeningHeightRatio;
                }

                float bbayopeningDepth = EditorGUILayout.Slider("Opening Depth", bbay.openingDepth, -depth, depth);
                if (bbayopeningDepth != bbay.openingDepth)
                {
                    bbay.openingDepth = bbayopeningDepth;
                }
                float bbaycolumnDepth = EditorGUILayout.Slider("Column Depth", bbay.columnDepth, -depth, depth);
                if (bbaycolumnDepth != bbay.columnDepth)
                {
                    bbay.columnDepth = bbaycolumnDepth;
                }
                float bbayrowDepth = EditorGUILayout.Slider("Row Depth", bbay.rowDepth, -depth, depth);
                if (bbayrowDepth != bbay.rowDepth)
                {
                    bbay.rowDepth = bbayrowDepth;
                }
                float bbaycrossDepth = EditorGUILayout.Slider("Cross Depth", bbay.crossDepth, -depth, depth);
                if (bbaycrossDepth != bbay.crossDepth)
                {
                    bbay.crossDepth = bbaycrossDepth;
                }

                int numberOfTextureSlots = bbay.numberOfTextures;
                string[] titles = new string[numberOfTextureSlots];
                for (int bft = 0; bft < numberOfTextureSlots; bft++)
                {
                    titles[bft] = ((BuildrBay.TextureNames)(bft)).ToString();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Wall Surface:", GUILayout.Width(75));
                editTextureOnFacade = EditorGUILayout.Popup(editTextureOnFacade, titles);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Wall Texture:", GUILayout.Width(75));
                int newFacadeTextureID = EditorGUILayout.Popup(bbay.textureValues[editTextureOnFacade], textureNames);
                if (newFacadeTextureID != bbay.textureValues[editTextureOnFacade])
                {
                    bbay.textureValues[editTextureOnFacade] = newFacadeTextureID;
                }
                EditorGUILayout.EndHorizontal();
                BuildrTexture bTexture = data.textures[bbay.textureValues[editTextureOnFacade]];
                Texture2D texture = bTexture.texture;
                EditorGUILayout.BeginHorizontal();

                if (texture != null)
                    GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
                else
                    EditorGUILayout.HelpBox("No texture assigned for '" + textureNames[bbay.textureValues[editTextureOnFacade]] + "', assign one in the Textures menu above", MessageType.Warning);

                bbay.flipValues[editTextureOnFacade] = EditorGUILayout.Toggle("Flip 90\u00B0", bbay.flipValues[editTextureOnFacade]);

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //Patterned design GUI

                int numberOfBays = bFacade.bayPattern.Count;
                int numberOfBayDesigns = data.bays.Count;

                EditorGUILayout.BeginHorizontal();
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Add New Bay Design"))
                {
                    BuildrBay newBay = new BuildrBay("new bay design " + (numberOfBayDesigns + 1));
                    data.bays.Add(newBay);
                    bFacade.bayPattern.Add(numberOfBayDesigns);
                    numberOfBays++;
                    selectedBayPatternIndex = numberOfBays - 1;
                    numberOfBayDesigns++;
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();
                if (numberOfBays == 0 || data.bays.Count == 0)
                {
                    EditorGUILayout.HelpBox("There are no bay designs to show", MessageType.Info);
//                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }

                BuildrBay[] bays = new BuildrBay[numberOfBays];
                for (int i = 0; i < numberOfBays; i++)
                {
                    bays[i] = data.bays[bFacade.bayPattern[i]];
                }
                selectedBayPatternIndex = Mathf.Clamp(selectedBayPatternIndex, 0, numberOfBays - 1);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.BeginHorizontal("box");
                string[] bayDesignNames = new string[data.bays.Count];
                for (int i = 0; i < numberOfBayDesigns; i++)
                {
                    bayDesignNames[i] = data.bays[i].name;
                }
                selectedBayDesign = EditorGUILayout.Popup(selectedBayDesign, bayDesignNames);
                if (GUILayout.Button("Add Selected"))
                {
                    bFacade.bayPattern.Add(selectedBayDesign);
                    GUI.changed = true;
                }
                if (GUILayout.Button("Duplicate Selected"))
                {
                    BuildrBay newBay = data.bays[selectedBayDesign].Duplicate();
                    data.bays.Add(newBay);
                    bFacade.bayPattern.Add(numberOfBayDesigns);
                    numberOfBays++;
                    selectedBayPatternIndex = numberOfBays - 1;
                    numberOfBayDesigns++;
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Bay Design Order:");
                var scrollbarHStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
                var scrollbarBackStyle = new GUIStyle();
                var scrollbarVStyle = new GUIStyle(GUI.skin.verticalScrollbar);
                scrollbarVStyle.fixedHeight = scrollbarVStyle.fixedWidth = 0;
                bayDesignPatternScrollView = EditorGUILayout.BeginScrollView(bayDesignPatternScrollView, false, false, scrollbarHStyle, scrollbarVStyle, scrollbarBackStyle, GUILayout.Height(40));
                List<string> bayNames = new List<string>();
                foreach (int bayIndex in bFacade.bayPattern)
                {
                    bayNames.Add(data.bays[bayIndex].name);
                }
                selectedBayPatternIndex = GUILayout.Toolbar(selectedBayPatternIndex, bayNames.ToArray());
                EditorGUILayout.EndScrollView();
                BuildrBay bBay = data.bays[bFacade.bayPattern[selectedBayPatternIndex]];

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(selectedBayPatternIndex == 0);
                if (GUILayout.Button("<<", GUILayout.Width(40)))
                {
                    int bayDesignIndex = bFacade.bayPattern[selectedBayPatternIndex];
                    bFacade.bayPattern.RemoveAt(selectedBayPatternIndex);
                    bFacade.bayPattern.Insert(selectedBayPatternIndex - 1, bayDesignIndex);
                    selectedBayPatternIndex--;
                    GUI.changed = true;
                }
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Remove"))
                {
                    bFacade.bayPattern.RemoveAt(selectedBayPatternIndex);
                    GUI.changed = true;
                }
                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Deleting Bay Design Entry", "Are you sure you want to delete this bay?", "Delete", "Cancel"))
                    {
                        int deletedBayDesignIndex = bFacade.bayPattern[selectedBayPatternIndex];
                        Debug.Log("Delete Bay Design " + deletedBayDesignIndex);
                        Debug.Log("Delete Bay Design " + data.bays[deletedBayDesignIndex].name);
                        data.bays.RemoveAt(deletedBayDesignIndex);
                        int numberOfFacadeDesigns = data.facades.Count;
                        for (int i = 0; i < numberOfFacadeDesigns; i++)
                        {
                            BuildrFacadeDesign checkFacade = data.facades[i];
                            int bayPatternSize = checkFacade.bayPattern.Count;
                            for (int j = 0; j < bayPatternSize; j++)
                            {
                                if (checkFacade.bayPattern[j] == deletedBayDesignIndex)
                                {
                                    checkFacade.bayPattern.RemoveAt(j);
                                    j--;
                                    bayPatternSize--;
                                }
                                else if (checkFacade.bayPattern[j] > deletedBayDesignIndex)
                                    checkFacade.bayPattern[j]--;
                            }
                        }
                        GUI.changed = true;
                    }
                }
                EditorGUI.BeginDisabledGroup(selectedBayPatternIndex == numberOfBays - 1);
                if (GUILayout.Button(">>", GUILayout.Width(40)))
                {
                    int bayDesignIndex = bFacade.bayPattern[selectedBayPatternIndex];
                    bFacade.bayPattern.Insert(selectedBayPatternIndex + 2, bayDesignIndex);
                    bFacade.bayPattern.RemoveAt(selectedBayPatternIndex);
                    selectedBayPatternIndex++;
                    GUI.changed = true;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");
                bBay.name = EditorGUILayout.TextField("Name: ", bBay.name);
                bool bBayisOpening = EditorGUILayout.Toggle("Has Opening", bBay.isOpening);
                if(bBayisOpening != bBay.isOpening)
                {
                    bBay.isOpening = bBayisOpening;
                }

                bool bBayRenderBack = EditorGUILayout.Toggle("Render Back", bBay.renderBack);
                if (bBayRenderBack != bBay.renderBack)
                {
                    bBay.renderBack = bBayRenderBack;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Bay Model", GUILayout.Width(146));
                bBay.bayModel = (GameObject)EditorGUILayout.ObjectField(bBay.bayModel, typeof(GameObject), false);
                if (GUILayout.Button("Clear", GUILayout.Width(70)))
                    bBay.bayModel = null;
                EditorGUILayout.EndHorizontal();

                float bBayopeningWidth = Mathf.Max(EditorGUILayout.FloatField("Opening Width", bBay.openingWidth), 0);
                if (bBayopeningWidth != bBay.openingWidth)
                {
                    bBay.openingWidth = bBayopeningWidth;
                }
                float bBayopeningHeight = Mathf.Clamp(EditorGUILayout.FloatField("Opening Height", bBay.openingHeight), 0, data.floorHeight);
                if (bBayopeningHeight != bBay.openingHeight)
                {
                    bBay.openingHeight = bBayopeningHeight;
                }

                float bBayminimumBayWidth = Mathf.Max(EditorGUILayout.FloatField("Bay Spacing Width", bBay.minimumBayWidth), 0);
                if (bBayminimumBayWidth != bBay.minimumBayWidth)
                {
                    bBay.minimumBayWidth = bBayminimumBayWidth;
                }

                float bBayopeningWidthRatio = EditorGUILayout.Slider("Horizontal Space Ratio", bBay.openingWidthRatio, 0, 1);
                if (bBayopeningWidthRatio != bBay.openingWidthRatio)
                {
                    bBay.openingWidthRatio = bBayopeningWidthRatio;
                }
                float bBayopeningHeightRatio = EditorGUILayout.Slider("Vertical Space Ratio", bBay.openingHeightRatio, 0, 1);
                if (bBayopeningHeightRatio != bBay.openingHeightRatio)
                {
                    bBay.openingHeightRatio = bBayopeningHeightRatio;
                }

                float bBayopeningDepth = EditorGUILayout.Slider("Opening Depth", bBay.openingDepth, -depth, depth);
                if (bBayopeningDepth != bBay.openingDepth)
                {
                    bBay.openingDepth = bBayopeningDepth;
                }
                float bBaycolumnDepth = EditorGUILayout.Slider("Column depth", bBay.columnDepth, -depth, depth);
                if (bBaycolumnDepth != bBay.columnDepth)
                {
                    bBay.columnDepth = bBaycolumnDepth;
                }
                float bBayrowDepth = EditorGUILayout.Slider("Row depth", bBay.rowDepth, -depth, depth);
                if (bBayrowDepth != bBay.rowDepth)
                {
                    bBay.rowDepth = bBayrowDepth;
                }
                float bBaycrossDepth = EditorGUILayout.Slider("Cross depth", bBay.crossDepth, -depth, depth);
                if (bBaycrossDepth != bBay.crossDepth)
                {
                    bBay.crossDepth = bBaycrossDepth;
                }

                //BAY TEXTURES

                int numberOfTextureSlots = bBay.numberOfTextures;
                string[] titles = new string[numberOfTextureSlots];
                for (int bft = 0; bft < numberOfTextureSlots; bft++)
                {
                    titles[bft] = ((BuildrBay.TextureNames)(bft)).ToString();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Surface:", GUILayout.Width(75));
                editTextureOnFacade = EditorGUILayout.Popup(editTextureOnFacade, titles);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Texture:", GUILayout.Width(75));
                bBay.textureValues[editTextureOnFacade] = EditorGUILayout.Popup(bBay.textureValues[editTextureOnFacade], textureNames);
                EditorGUILayout.EndHorizontal();
                BuildrTexture bTexture = data.textures[bBay.textureValues[editTextureOnFacade]];
                Texture2D texture = bTexture.texture;
                EditorGUILayout.BeginHorizontal();

                if (texture != null)
                    GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
                else
                    EditorGUILayout.HelpBox("No texture assigned for '" + textureNames[bBay.textureValues[editTextureOnFacade]] + "', assign one in the Textures menu above", MessageType.Warning);

                bFacade.flipValues[editTextureOnFacade] = EditorGUILayout.Toggle("Flip 90\u00B0", bFacade.flipValues[editTextureOnFacade]);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            editTextureOnFacade = 7;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Surface:", GUILayout.Width(75));
            EditorGUILayout.LabelField("Wall");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Texture:", GUILayout.Width(75));
            int newFacadeTexture = EditorGUILayout.Popup(bFacade.simpleBay.textureValues[editTextureOnFacade], textureNames);
            if (newFacadeTexture != bFacade.simpleBay.textureValues[editTextureOnFacade])
            {
                bFacade.simpleBay.textureValues[editTextureOnFacade] = newFacadeTexture;
            }
            EditorGUILayout.EndHorizontal();
            BuildrTexture bTexture = data.textures[bFacade.simpleBay.textureValues[editTextureOnFacade]];
            Texture2D texture = bTexture.texture;
            EditorGUILayout.BeginHorizontal();

            if (texture != null)
                GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
            else
                EditorGUILayout.HelpBox("No texture assigned for '" + textureNames[bFacade.simpleBay.textureValues[editTextureOnFacade]] + "', assign one in the Textures menu above", MessageType.Warning);

            bFacade.simpleBay.flipValues[editTextureOnFacade] = EditorGUILayout.Toggle("Flip 90\u00B0", bFacade.simpleBay.flipValues[editTextureOnFacade]);

            EditorGUILayout.EndHorizontal();
        }
    }
}
