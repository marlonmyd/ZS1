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
using System.Collections;
using System.Collections.Generic;

public class BuildrEditModeRoofs
{
    private static BuildrData data;
    private static int selectedRoof = 0;
    private static int editTextureOnRoof = 0;

    public static void SceneGUI(BuildrEditMode editMode, BuildrData data, bool shouldSnap, float handleSize)
    {

    }

    public static void InspectorGUI(BuildrEditMode editMode, BuildrData _data)
    {

        data = _data;
        Undo.RecordObject(data, "Roof Modified");

        BuildrRoofDesign[] roofs = data.roofs.ToArray();
        int numberOfRoofs = roofs.Length;
        selectedRoof = Mathf.Clamp(selectedRoof, 0, numberOfRoofs - 1);

        if (GUILayout.Button("Add new roof design"))
        {
            data.roofs.Add(new BuildrRoofDesign("new roof " + numberOfRoofs));
            roofs = data.roofs.ToArray();
            numberOfRoofs++;
            selectedRoof = numberOfRoofs - 1;

        }
        if (numberOfRoofs == 0)
        {
            EditorGUILayout.HelpBox("There are no roof designs to show", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Design: ", GUILayout.Width(75));
        string[] roofNames = new string[numberOfRoofs];
        for (int f = 0; f < numberOfRoofs; f++)
            roofNames[f] = roofs[f].name;
        selectedRoof = EditorGUILayout.Popup(selectedRoof, roofNames);

        BuildrRoofDesign bRoof = roofs[selectedRoof];
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Delete", GUILayout.Width(75)))
        {
            if (EditorUtility.DisplayDialog("Deleting Roof Design Entry", "Are you sure you want to delete this roof?", "Delete", "Cancel"))
            {
                data.RemoveRoofDesign(bRoof);
                selectedRoof = 0;
                GUI.changed = true;

                return;
            }
        }

        if (GUILayout.Button("Import", GUILayout.Width(71)))
        {
            string xmlPath = EditorUtility.OpenFilePanel("Select the XML file...", "Assets/BuildR/Exported/", "xml");
            if (xmlPath == "")
                return;
            BuildrXMLImporter.ImportRoofs(xmlPath, _data);
            GUI.changed = true;
        }

        if (GUILayout.Button("Export", GUILayout.Width(71)))
        {
            string xmlPath = EditorUtility.SaveFilePanel("Export as...", "Assets/BuildR/Exported/", _data.name + "_roofLibrary", "xml");
            if (xmlPath == "")
                return;
            BuildrXMLExporter.ExportRoofs(xmlPath, _data);
            GUI.changed = true;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name: ", GUILayout.Width(75));
        bRoof.name = EditorGUILayout.TextField(bRoof.name);
        EditorGUILayout.EndHorizontal();

        //ROOF STYLE
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Style: ", GUILayout.Width(75));
        BuildrRoofDesign.styles bRoofstyle = (BuildrRoofDesign.styles)EditorGUILayout.EnumPopup(bRoof.style);
        if(bRoofstyle != bRoof.style)
        {
            bRoof.style = bRoofstyle;
        }
        EditorGUILayout.EndHorizontal();

        if (bRoof.style != BuildrRoofDesign.styles.flat && bRoof.style != BuildrRoofDesign.styles.mansard && bRoof.style != BuildrRoofDesign.styles.steepled && bRoof.style != BuildrRoofDesign.styles.hipped)
        {
            EditorGUILayout.HelpBox("Please note that this design can only be used on sections with 4 points." +
                "\nComplex sections can only use the Flat, Mansard and Steeple designs.", MessageType.Warning);
        }

        if (bRoof.style != BuildrRoofDesign.styles.flat)
            bRoof.height = Mathf.Max(EditorGUILayout.FloatField("Height", bRoof.height), 0);

        if (bRoof.style == BuildrRoofDesign.styles.mansard)
        {
            bRoof.floorDepth = Mathf.Max(EditorGUILayout.FloatField("Base Depth", bRoof.floorDepth), 0);
            bRoof.depth = Mathf.Max(EditorGUILayout.FloatField("Top Depth", bRoof.depth), 0);
        }

        if (bRoof.style == BuildrRoofDesign.styles.barrel)
            bRoof.barrelSegments = Mathf.Max(EditorGUILayout.IntField("Barrel Segments", bRoof.barrelSegments), 3);

        if (bRoof.style == BuildrRoofDesign.styles.barrel || bRoof.style == BuildrRoofDesign.styles.gabled || bRoof.style == BuildrRoofDesign.styles.hipped)
        {
            //two directions of the ridge
            bRoof.direction = Mathf.Clamp(bRoof.direction, 0, 1);
            string[] options = new string[2] { "short", "long" };
            bRoof.direction = EditorGUILayout.Popup(bRoof.direction, options);
        }

        if (bRoof.style == BuildrRoofDesign.styles.leanto || bRoof.style == BuildrRoofDesign.styles.sawtooth)
        {
            //four directions of the ridge
            bRoof.direction = Mathf.Clamp(bRoof.direction, 0, 3);
            string[] options = new string[4] { "left", "up", "right", "down" };
            bRoof.direction = EditorGUILayout.Popup(bRoof.direction, options);
        }

        if (bRoof.style == BuildrRoofDesign.styles.sawtooth)
            bRoof.sawtoothTeeth = Mathf.Max(EditorGUILayout.IntField("Number of 'teeth'", bRoof.sawtoothTeeth), 2);

        //PARAPET
        bool bRoofparapet = EditorGUILayout.Toggle("Has Parapet", bRoof.parapet);
        if(bRoofparapet != bRoof.parapet)
        {
            bRoof.parapet = bRoofparapet;
        }
        if (bRoof.parapet)
        {
            float bRoofparapetHeight = Mathf.Max(EditorGUILayout.FloatField("Parapet Width", bRoof.parapetHeight), 0);
            if(bRoofparapetHeight != bRoof.parapetHeight)
            {
                bRoof.parapetHeight = bRoofparapetHeight;
            }
            float bRoofparapetFrontDepth = Mathf.Max(EditorGUILayout.FloatField("Parapet Front Depth", bRoof.parapetFrontDepth), 0);
            if (bRoofparapetFrontDepth != bRoof.parapetFrontDepth)
            {
                bRoof.parapetFrontDepth = bRoofparapetFrontDepth;
            }
            float bRoofparapetBackDepth = Mathf.Max(EditorGUILayout.FloatField("Parapet Back Depth", bRoof.parapetBackDepth), 0);
            if (bRoofparapetBackDepth != bRoof.parapetBackDepth)
            {
                bRoof.parapetBackDepth = bRoofparapetBackDepth;
            }

            if (bRoof.parapetStyle == BuildrRoofDesign.parapetStyles.fancy)//NOT IMPLMENTED...YET...
            {
                EditorGUILayout.HelpBox("This allows you to specify a model mesh that will be used to create a parapet." +
                    "\nIt should not repeat as Buildr will attempt to repeat the style to fit the length of the facade.", MessageType.Info);
                bRoof.parapetDesign = (Mesh)EditorGUILayout.ObjectField("Parapet Mesh", bRoof.parapetDesign, typeof(Mesh), false);
                bRoof.parapetDesignWidth = Mathf.Max(EditorGUILayout.FloatField("Parapet Design Width", bRoof.parapetDesignWidth), 0);
            }
        }

        //DORMERS
        if (bRoof.style == BuildrRoofDesign.styles.mansard)
        {
            bool bRoofhasDormers = EditorGUILayout.Toggle("Has Dormers", bRoof.hasDormers);
            if(bRoofhasDormers != bRoof.hasDormers)
            {
                bRoof.hasDormers = bRoofhasDormers;
            }
            if (bRoof.hasDormers)
            {
                float bRoofdormerWidth = Mathf.Max(EditorGUILayout.FloatField("Dormer Width", bRoof.dormerWidth), 0);
                if (bRoofdormerWidth != bRoof.dormerWidth)
                {
                    bRoof.dormerWidth = bRoofdormerWidth;
                }
                float bRoofdormerHeight = Mathf.Clamp(EditorGUILayout.FloatField("Dormer Height", bRoof.dormerHeight), 0, bRoof.height);
                if (bRoofdormerHeight != bRoof.dormerHeight)
                {
                    bRoof.dormerHeight = bRoofdormerHeight;
                }
                float bRoofdormerRoofHeight = Mathf.Clamp(EditorGUILayout.FloatField("Dormer Roof Height", bRoof.dormerRoofHeight), 0, bRoof.dormerHeight);
                if (bRoofdormerRoofHeight != bRoof.dormerRoofHeight)
                {
                    bRoof.dormerRoofHeight = bRoofdormerRoofHeight;
                }
                float bRoofminimumDormerSpacing = Mathf.Max(EditorGUILayout.FloatField("Dormer Minimum Spacing", bRoof.minimumDormerSpacing), 0);
                if (bRoofminimumDormerSpacing != bRoof.minimumDormerSpacing)
                {
                    bRoof.minimumDormerSpacing = bRoofminimumDormerSpacing;
                }
                float bRoofdormerHeightRatio = EditorGUILayout.Slider("Dormer Height Ratio", bRoof.dormerHeightRatio, 0, 1);
                if (bRoofdormerHeightRatio != bRoof.dormerHeightRatio)
                {
                    bRoof.dormerHeightRatio = bRoofdormerHeightRatio;
                }
            }
        }

        //TEXTURES
        int numberOfTextures = data.textures.Count;
        string[] textureNames = new string[numberOfTextures];
        for (int t = 0; t < numberOfTextures; t++)
            textureNames[t] = data.textures[t].name;

        int numberOfTextureSlots = bRoof.numberOfTextures;
        string[] titles = new string[numberOfTextureSlots];
        for (int brt = 0; brt < numberOfTextureSlots; brt++)
        {
            titles[brt] = ((BuildrRoofDesign.textureNames)(brt)).ToString();
        }

        editTextureOnRoof = EditorGUILayout.Popup("Texture Surface:", editTextureOnRoof, titles);

        int selectedRoofTexture = EditorGUILayout.Popup("Selected Texture:", bRoof.textureValues[editTextureOnRoof], textureNames);
        if(selectedRoofTexture != bRoof.textureValues[editTextureOnRoof])
        {
            bRoof.textureValues[editTextureOnRoof] = selectedRoofTexture;
        }
        BuildrTexture bTexture = data.textures[bRoof.textureValues[editTextureOnRoof]];
        Texture2D texture = bTexture.texture;
        EditorGUILayout.BeginHorizontal();

        if (texture != null)
            GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
        else
            EditorGUILayout.HelpBox("No texture assigned for '" + textureNames[bRoof.textureValues[editTextureOnRoof]] + "', assign one in the Textures menu above", MessageType.Warning);

        bRoof.flipValues[editTextureOnRoof] = EditorGUILayout.Toggle("Flip 90\u00B0", bRoof.flipValues[editTextureOnRoof]);

        EditorGUILayout.EndHorizontal();
    }
}
