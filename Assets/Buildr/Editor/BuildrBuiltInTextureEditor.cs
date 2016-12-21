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
using System.Collections.Generic;
using UnityEditor;
using System.Xml;
using System.IO;
using System.Text;

public class BuildrBuiltInTextureEditor : EditorWindow
{

    private static string textureFilePath = "Assets/Buildr/Textures/textures.xml";
    private static Rect texturePreviewPostion = new Rect(0, 0, 0, 0);
    private List<BuildrTexture> textures = new List<BuildrTexture>();
    private List<string> xmlfilelist = new List<string>();
    private int selectedTexture = 0;
    private int selectedFile = 0;
    private int addSelectedTexture = 0;
    private static char[] filenameDelimiters = new[] { '\\', '/' };

    private void OnEnable()
    {
        ScrapeXMLFilenames();
        CheckData();
    }

    private void OnGUI()
    {

        string asterisk = GUI.changed ? "*" : "";

        EditorGUILayout.LabelField("Texture Designs" + asterisk, GUILayout.Height(12), GUILayout.Width(280));
        Texture2D facadeTexture = new Texture2D(1, 1);
        facadeTexture.SetPixel(0, 0, BuildrColours.BLUE);
        facadeTexture.Apply();
        Rect sqrPos = new Rect(0, 0, 0, 0);
        if (Event.current.type == EventType.Repaint)
            sqrPos = GUILayoutUtility.GetLastRect();
        GUI.DrawTexture(sqrPos, facadeTexture);
        EditorGUI.DropShadowLabel(sqrPos, "Texture Designs" + asterisk);
        //int currentSelectedTexture = selectedTexture;//keep tack of what we had selected to reset fields if changed

        int numberOfFiles = xmlfilelist.Count;
        string[] fileNames = new string[numberOfFiles];
        for (int t = 0; t < numberOfFiles; t++)
        {
            string filepath = xmlfilelist[t];
            string[] filepathsplit = filepath.Split(filenameDelimiters);
            string displayPath = filepathsplit[filepathsplit.Length-1];
            fileNames[t] = displayPath;
        }
        int newSelectedFile = EditorGUILayout.Popup(selectedFile, fileNames);
        if (newSelectedFile != selectedFile)
        {
            selectedFile = newSelectedFile;
            textureFilePath = xmlfilelist[selectedFile];
            CheckData();
            selectedTexture = 0;//reset the selected texture to 0 to avoid index out of range...
        }

        if (GUILayout.Button("Create New Texture Pack"))
        {
            if(EditorUtility.DisplayDialog("New Texture Pack","Are you sure you want to start a new texture pack.\nAll unsaved data will be shown the door.","ok, do it!","no no no no"))
                GenerateData();
        }

        int numberOfTextures = textures.Count;
        selectedTexture = Mathf.Clamp(selectedTexture, 0, numberOfTextures - 1);

        EditorGUILayout.BeginHorizontal();
        string[] texturePaths = GetTextureFilenames();
        string[] textureFilenames = new string[texturePaths.Length];
        for (int t = 0; t < texturePaths.Length; t++)
        {
            string[] splits = texturePaths[t].Split(filenameDelimiters);
            textureFilenames[t] = splits[splits.Length - 1];
        }
        addSelectedTexture = EditorGUILayout.Popup(addSelectedTexture, textureFilenames);
        if (GUILayout.Button("Add New Texture to Pack"))
        {
            string path = texturePaths[addSelectedTexture];
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (texture != null)
            {
                string[] splits = path.Split(filenameDelimiters);
                BuildrTexture newBTexture = new BuildrTexture(splits[splits.Length - 1]);
                newBTexture.texture = texture;
                textures.Add(newBTexture);
                selectedTexture = textures.Count - 1;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (numberOfTextures == 0)
        {
            EditorGUILayout.HelpBox("There are no textures to show", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Selected Texture");
        EditorGUILayout.BeginHorizontal();
        string[] textureNames = new string[numberOfTextures];
        for (int t = 0; t < numberOfTextures; t++)
            textureNames[t] = textures[t].name;
        selectedTexture = EditorGUILayout.Popup(selectedTexture, textureNames);

        EditorGUI.BeginDisabledGroup(selectedTexture <= 0);
        if (GUILayout.Button("<", GUILayout.Width(25)))
            selectedTexture--;
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(selectedTexture >= numberOfTextures - 1);
        if (GUILayout.Button(">", GUILayout.Width(25)))
            selectedTexture++;
        EditorGUI.EndDisabledGroup();

        BuildrTexture bTexture = textures[selectedTexture];

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Delete"))
            textures.Remove(bTexture);
        if (GUILayout.Button("Delete All"))
            textures.Clear();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Is Tiled");
        bTexture.tiled = EditorGUILayout.Toggle(bTexture.tiled, GUILayout.Width(18));
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(!bTexture.tiled);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Has Pattern");
        bTexture.patterned = EditorGUILayout.Toggle(bTexture.patterned, GUILayout.Width(18));
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        if (!bTexture.tiled)
            bTexture.patterned = false;

        Vector2 textureUnitSize = bTexture.textureUnitSize;
        if (bTexture.tiled)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Texture Width", GUILayout.Width(180));//, GUILayout.Width(42));
            textureUnitSize.x = EditorGUILayout.FloatField(bTexture.textureUnitSize.x, GUILayout.Width(35));
            EditorGUILayout.LabelField("metres", GUILayout.Width(45));//, GUILayout.Width(42));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Texture Height", GUILayout.Width(180));//, GUILayout.Width(42));
            textureUnitSize.y = EditorGUILayout.FloatField(bTexture.textureUnitSize.y, GUILayout.Width(35));
            EditorGUILayout.LabelField("metres", GUILayout.Width(45));
            EditorGUILayout.EndHorizontal();
            bTexture.textureUnitSize = textureUnitSize;
        }

        Vector2 tileUnitSize = bTexture.tileUnitUV;
        if (bTexture.patterned)
        {
            float minWidth = 2 / bTexture.texture.width;
            float minHeight = 2 / bTexture.texture.height;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Unit Width", GUILayout.Width(75));
            tileUnitSize.x = EditorGUILayout.Slider(tileUnitSize.x, minWidth, 1.0f);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Unit Height", GUILayout.Width(75));
            tileUnitSize.y = EditorGUILayout.Slider(tileUnitSize.y, minHeight, 1.0f);
            EditorGUILayout.EndHorizontal();
            bTexture.tileUnitUV = tileUnitSize;

            EditorGUILayout.Space();
        }

        const int previewTextureUnitSize = 120;
        const int previewTileUnitSize = 59;
        const int previewTileUnitPadding = 2;
        const int previewPadding = 25;

        EditorGUILayout.BeginHorizontal();
        if (bTexture.tiled)
        {
            EditorGUILayout.LabelField("1 Metre Squared", GUILayout.Width(110));
        }
        GUILayout.Space(previewPadding);
        if (bTexture.patterned)
        {
            EditorGUILayout.LabelField("Texture Pattern Units", GUILayout.Width(120));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        if (Event.current.type == EventType.Repaint)
            texturePreviewPostion = GUILayoutUtility.GetLastRect();

        if (bTexture.tiled)
        {
            Rect previewRect = new Rect(texturePreviewPostion.x, texturePreviewPostion.y, previewTextureUnitSize, previewTextureUnitSize);
            Rect sourceRect = new Rect(0, 0, (1.0f / textureUnitSize.x), (1.0f / textureUnitSize.y));

            Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);
        }

        if (bTexture.patterned)
        {
            Rect previewRect = new Rect(previewTextureUnitSize + previewPadding, 0, previewTileUnitSize, previewTileUnitSize);
            Rect sourceRect = new Rect(0, tileUnitSize.y, tileUnitSize.x, tileUnitSize.y);

            previewRect.x += texturePreviewPostion.x;
            previewRect.y += texturePreviewPostion.y;

            Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);

            sourceRect.x += tileUnitSize.x;
            previewRect.x += previewTileUnitSize + previewTileUnitPadding;

            Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);

            sourceRect.x += -tileUnitSize.x;
            sourceRect.y += -tileUnitSize.y;
            previewRect.x += -(previewTileUnitSize + previewTileUnitPadding);
            previewRect.y += previewTileUnitSize + previewTileUnitPadding;

            Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);

            sourceRect.x += tileUnitSize.x;
            previewRect.x += previewTileUnitSize + previewTileUnitPadding;

            Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);
        }

        if (!bTexture.tiled)
        {
            //EditorGUILayout.LabelField("Tile texture");

            EditorGUILayout.BeginHorizontal();
            int currentXTiles = bTexture.tiledX;
            GUILayout.Label("Texture Tile X");
            currentXTiles = EditorGUILayout.IntField(currentXTiles, GUILayout.Width(30));
            if (GUILayout.Button("+", GUILayout.Width(25)))
                currentXTiles++;
            EditorGUI.BeginDisabledGroup(currentXTiles < 2);
            if (GUILayout.Button("-", GUILayout.Width(25)))
                currentXTiles--;
            EditorGUI.EndDisabledGroup();
            bTexture.tiledX = currentXTiles;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            int currentYTiles = bTexture.tiledY;
            GUILayout.Label("Texture Tile Y");
            currentYTiles = EditorGUILayout.IntField(currentYTiles, GUILayout.Width(30));
            if (GUILayout.Button("+", GUILayout.Width(25)))
                currentYTiles++;
            EditorGUI.BeginDisabledGroup(currentYTiles < 2);
            if (GUILayout.Button("-", GUILayout.Width(25)))
                currentYTiles--;
            EditorGUI.EndDisabledGroup();
            bTexture.tiledY = currentYTiles;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.Space();
            if (Event.current.type == EventType.Repaint)
                texturePreviewPostion = GUILayoutUtility.GetLastRect();

            Rect previewRect = new Rect(texturePreviewPostion.x, texturePreviewPostion.y, previewTextureUnitSize, previewTextureUnitSize);
            Rect sourceRect = new Rect(0, 0, currentXTiles, currentYTiles);

            Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);
        }

        GUILayout.Space(previewTextureUnitSize);

        EditorGUILayout.LabelField("Texture Usage");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Walls");
        bTexture.wall = EditorGUILayout.Toggle(bTexture.wall);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Doors");
        bTexture.door = EditorGUILayout.Toggle(bTexture.door);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Windows");
        bTexture.window = EditorGUILayout.Toggle(bTexture.window);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Roofs");
        bTexture.roof = EditorGUILayout.Toggle(bTexture.roof);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Save Data"))
            SaveData();

        if (GUILayout.Button("Save Data As..."))
            SaveDataAs();
    }

    private void CheckData()
    {
        XmlNodeList xmlTextures = null;

        if (File.Exists(textureFilePath))
        {
            XmlDocument xml = new XmlDocument();
            StreamReader sr = new StreamReader(textureFilePath);
            xml.LoadXml(sr.ReadToEnd());
            sr.Close();
            xmlTextures = xml.SelectNodes("data/textures/texture");
        }

        if (xmlTextures != null)
        {
            textures.Clear();
            foreach (XmlNode node in xmlTextures)
            {
                string filepath = node["filepath"].FirstChild.Value;
                string[] splits = filepath.Split(filenameDelimiters);
                BuildrTexture bTexture = new BuildrTexture(splits[splits.Length - 1]);
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(filepath, typeof(Texture2D));
                bTexture.texture = texture;
                bTexture.tiled = node["tiled"].FirstChild.Value == "True";
                bTexture.patterned = node["patterned"].FirstChild.Value == "True";
                Vector2 tileUnitUV;
                tileUnitUV.x = float.Parse(node["tileUnitUVX"].FirstChild.Value);
                tileUnitUV.y = float.Parse(node["tileUnitUVY"].FirstChild.Value);
                bTexture.tileUnitUV = tileUnitUV;

                Vector2 textureUnitSize;
                textureUnitSize.x = float.Parse(node["textureUnitSizeX"].FirstChild.Value);
                textureUnitSize.y = float.Parse(node["textureUnitSizeY"].FirstChild.Value);
                bTexture.textureUnitSize = textureUnitSize;

                bTexture.tiledX = int.Parse(node["tiledX"].FirstChild.Value);
                bTexture.tiledY = int.Parse(node["tiledY"].FirstChild.Value);

                bTexture.door = node["door"].FirstChild.Value == "True";
                bTexture.window = node["window"].FirstChild.Value == "True";
                bTexture.wall = node["wall"].FirstChild.Value == "True";
                bTexture.roof = node["roof"].FirstChild.Value == "True";

                textures.Add(bTexture);
            }
        }
        /*else
        {
            GenerateData();
        }*/
    }

    private string[] GetTextureFilenames()
    {
        string[] paths = Directory.GetFiles("Assets/Buildr/Textures");
        List<string> returnTexturePaths = new List<string>();
        foreach (string path in paths)
        {
            if (path.Contains(".meta")) continue;
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (texture != null)
            {
                returnTexturePaths.Add(path);
            }
        }
        return returnTexturePaths.ToArray();
    }

    private void GenerateData()
    {
        string[] paths = Directory.GetFiles("Assets/Buildr/Textures");
        textures.Clear();
        foreach (string path in paths)
        {
            if (path.Contains(".meta")) continue;
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (texture != null)
            {
                string[] splits = path.Split(filenameDelimiters);
                BuildrTexture bTexture = new BuildrTexture(splits[splits.Length - 1]);
                bTexture.texture = texture;
                textures.Add(bTexture);
            }
        }
    }

    private void ScrapeXMLFilenames()
    {
        string[] paths = Directory.GetFiles("Assets/Buildr/XML");
        xmlfilelist.Clear();
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
                    xmlfilelist.Add(path);
                }
            }
        }
    }

    private void SaveDataAs()
    {
        textureFilePath = EditorUtility.SaveFilePanel(
                    "Save Data to XML",
                    "Assets/Buildr/XML",
                    "data.xml",
                    "xml");
        SaveData();
        xmlfilelist.Add(textureFilePath);
        selectedFile = xmlfilelist.Count - 1;
    }

    private void SaveData()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version='1.0' encoding='ISO-8859-15'?>");
        sb.AppendLine("<!-- Unity3D Asset Buildr Texture Database http://buildr.jasperstocker.com -->");
        sb.AppendLine("<data>");
        sb.AppendLine("<datatype>TexturePack</datatype>");
        sb.AppendLine("<textures>");

        foreach (BuildrTexture bTexture in textures)
        {
            sb.AppendLine("<texture>");

            sb.AppendLine("<filepath>" + AssetDatabase.GetAssetPath(bTexture.texture) + "</filepath>");
            sb.AppendLine("<tiled>" + bTexture.tiled + "</tiled>");
            sb.AppendLine("<patterned>" + bTexture.patterned + "</patterned>");
            sb.AppendLine("<tileUnitUVX>" + bTexture.tileUnitUV.x + "</tileUnitUVX>");
            sb.AppendLine("<tileUnitUVY>" + bTexture.tileUnitUV.y + "</tileUnitUVY>");
            sb.AppendLine("<textureUnitSizeX>" + bTexture.textureUnitSize.x + "</textureUnitSizeX>");
            sb.AppendLine("<textureUnitSizeY>" + bTexture.textureUnitSize.y + "</textureUnitSizeY>");
            sb.AppendLine("<tiledX>" + bTexture.tiledX + "</tiledX>");
            sb.AppendLine("<tiledY>" + bTexture.tiledY + "</tiledY>");
            sb.AppendLine("<door>" + bTexture.door + "</door>");
            sb.AppendLine("<window>" + bTexture.window + "</window>");
            sb.AppendLine("<wall>" + bTexture.wall + "</wall>");
            sb.AppendLine("<roof>" + bTexture.roof + "</roof>");

            sb.AppendLine("</texture>");
        }
        sb.AppendLine("</textures>");
        sb.AppendLine("</data>");

        StreamWriter sw = new StreamWriter(textureFilePath);
        sw.Write(sb.ToString());//write out contents of data to XML
        sw.Close();
    }
}