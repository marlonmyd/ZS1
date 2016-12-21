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

public class BuildrEditModeTextures
{

    private static MenuCommand mc;
    private static BuildrData data;
    private static int selectedTexture = 0;
    private static Rect texturePreviewPostion = new Rect(0, 0, 0, 0);
    private static Color defaultCol = new Color(0.8f, 0.8f, 0.8f, 1);

    public static void SceneGUI(BuildrEditMode editMode, BuildrData data, bool shouldSnap, float handleSize)
    {
        if (GUI.changed)
        {
            EditorUtility.SetDirty(editMode);
            EditorUtility.SetDirty(data);
            editMode.UpdateRender();
        }
    }

    public static void InspectorGUI(BuildrEditMode _editMode, BuildrData _data)
    {

        data = _data;
        BuildrTexture[] textures = data.textures.ToArray();
        int numberOfTextures = textures.Length;
        selectedTexture = Mathf.Clamp(selectedTexture, 0, numberOfTextures - 1);
        int currentSelectedTexture = selectedTexture;//keep tack of what we had selected to reset fields if changed

        Undo.RecordObject(data, "Texture Modified");

        if(numberOfTextures == 0)
        {
            EditorGUILayout.HelpBox("There are no textures to show", MessageType.Info);
            if(GUILayout.Button("Add New"))
            {
                data.textures.Add(new BuildrTexture("new texture " + numberOfTextures));
                numberOfTextures++;
                selectedTexture = numberOfTextures - 1;
            }
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Texture", GUILayout.Width(75));
        string[] textureNames = new string[numberOfTextures];
        for(int t = 0; t < numberOfTextures; t++)
            textureNames[t] = textures[t].name;
        selectedTexture = EditorGUILayout.Popup(selectedTexture, textureNames);
        EditorGUILayout.EndHorizontal();

        BuildrTexture bTexture = textures[selectedTexture];
        
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.Space();

        if(GUILayout.Button("Add New", GUILayout.Width(81)))
        {
            data.textures.Add(new BuildrTexture("new texture " + numberOfTextures));
            numberOfTextures++;
            selectedTexture = numberOfTextures - 1;
        }


        if(GUILayout.Button("Duplicate", GUILayout.Width(90)))
        {
            data.textures.Add(bTexture.Duplicate());
            numberOfTextures++;
            selectedTexture = numberOfTextures - 1;

        }

        if(GUILayout.Button("Delete", GUILayout.Width(71)))
        {
            if(EditorUtility.DisplayDialog("Deleting Texture Entry", "Are you sure you want to delete this texture?", "Delete", "Cancel"))
            {
                data.RemoveTexture(bTexture);
                selectedTexture = 0;
                GUI.changed = true;

                return;
            }
        }

        if (GUILayout.Button("Import", GUILayout.Width(71)))
        {
            string xmlPath = EditorUtility.OpenFilePanel("Select the XML file...", "Assets/BuildR/Exported/", "xml");
            if (xmlPath == "")
                return;
            BuildrXMLImporter.ImportTextures(xmlPath, _data);
            textures = data.textures.ToArray();
            selectedTexture = 0;
            GUI.changed = true;
        }

        if (GUILayout.Button("Export", GUILayout.Width(71)))
        {
            string xmlPath = EditorUtility.SaveFilePanel("Export as...", "Assets/BuildR/Exported/", _data.name + "_textureLibrary", "xml");
            if (xmlPath == "")
                return;
            BuildrXMLExporter.ExportTextures(xmlPath, _data);
            GUI.changed = true;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        textures = data.textures.ToArray();
        textureNames = new string[numberOfTextures];
        for(int t = 0; t < numberOfTextures; t++)
            textureNames[t] = textures[t].name;
        bTexture = textures[selectedTexture];//reassign

        string textureName = bTexture.name;
        GUIStyle redText = new GUIStyle(GUI.skin.textField);
        if(textureName.Contains(" "))
        {
            redText.focused.textColor = Color.red;
            textureName = EditorGUILayout.TextField("Name", textureName, redText);
        }
        else
        {
            redText.focused.textColor = defaultCol;
            textureName = EditorGUILayout.TextField("Name", textureName, redText);
        }
        bTexture.name = textureName;

        bool conflictingName = false;
        for(int i = 0; i < textureNames.Length; i++)
        {
            if(selectedTexture != i)
            {
                if(textureNames[i] == bTexture.name)
                    conflictingName = true;
            }
        }

        if(conflictingName)
            EditorGUILayout.HelpBox("You have named this texture the same as another.", MessageType.Warning);


        if(currentSelectedTexture != selectedTexture)
        {
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
        }

        bTexture.type = (BuildrTexture.Types)EditorGUILayout.EnumPopup("Type", bTexture.type);

        switch(bTexture.type)
        {
            case BuildrTexture.Types.Basic:

            if(bTexture.texture != null)
            {
                string texturePath = AssetDatabase.GetAssetPath(bTexture.texture);
                TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);

                if(!textureImporter.isReadable)
                {
                    EditorGUILayout.HelpBox("The texture you have selected is not readable." + "\nPlease select the readable checkbox under advanced texture settings." + "\nOr move this texture to the BuildR texture folder and reimport.", MessageType.Error);
                }
            }

            //Shader Time
            Shader[] tempshaders = (Shader[])Resources.FindObjectsOfTypeAll(typeof(Shader));
            List<string> shaderNames = new List<string>(ShaderProperties.NAMES);
            foreach(Shader shader in tempshaders)
            {
                if(!string.IsNullOrEmpty(shader.name) && !shader.name.StartsWith("__") && !shader.name.Contains("hidden"))
                    shaderNames.Add(shader.name);
            }
            int selectedShaderIndex = shaderNames.IndexOf(bTexture.material.shader.name);
            int newSelectedShaderIndex = EditorGUILayout.Popup("Shader", selectedShaderIndex, shaderNames.ToArray());
            if(selectedShaderIndex != newSelectedShaderIndex)
            {
                bTexture.material.shader = Shader.Find(shaderNames[newSelectedShaderIndex]);
            }

            Shader selectedShader = bTexture.material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(selectedShader);

            for(int s = 0; s < propertyCount; s++)
            {
                ShaderUtil.ShaderPropertyType propertyTpe = ShaderUtil.GetPropertyType(selectedShader, s);
                string shaderPropertyName = ShaderUtil.GetPropertyName(selectedShader, s);
                switch(propertyTpe)
                {
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        Texture shaderTexture = bTexture.material.GetTexture(shaderPropertyName);
                        Texture newShaderTexture = (Texture)EditorGUILayout.ObjectField(shaderPropertyName, shaderTexture, typeof(Texture), false);
                        if(shaderTexture != newShaderTexture)
                        {
                            bTexture.material.SetTexture(shaderPropertyName, newShaderTexture);
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.Color:
                        Color shaderColor = bTexture.material.GetColor(shaderPropertyName);
                        Color newShaderColor = EditorGUILayout.ColorField(shaderPropertyName, shaderColor);
                        if(shaderColor != newShaderColor)
                        {
                            bTexture.material.SetColor(shaderPropertyName, newShaderColor);
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.Float:
                        float shaderFloat = bTexture.material.GetFloat(shaderPropertyName);
                        float newShaderFloat = EditorGUILayout.FloatField(shaderPropertyName, shaderFloat);
                        if(shaderFloat != newShaderFloat)
                        {
                            bTexture.material.SetFloat(shaderPropertyName, newShaderFloat);
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.Range:
                        float shaderRange = bTexture.material.GetFloat(shaderPropertyName);
                        float rangeMin = ShaderUtil.GetRangeLimits(selectedShader, s, 1);
                        float rangeMax = ShaderUtil.GetRangeLimits(selectedShader, s, 2);
                        float newShaderRange = EditorGUILayout.Slider(shaderPropertyName, shaderRange, rangeMin, rangeMax);
                        if(shaderRange != newShaderRange)
                        {
                            bTexture.material.SetFloat(shaderPropertyName, newShaderRange);
                        }
                        break;

                    case ShaderUtil.ShaderPropertyType.Vector:
                        Vector3 shaderVector = bTexture.material.GetVector(shaderPropertyName);
                        Vector3 newShaderVector = EditorGUILayout.Vector3Field(shaderPropertyName, shaderVector);
                        if(shaderVector != newShaderVector)
                        {
                            bTexture.material.SetVector(shaderPropertyName, newShaderVector);
                        }
                        break;
                }
            }

            bool tiled = EditorGUILayout.Toggle("Is Tiled", bTexture.tiled);
            if(tiled != bTexture.tiled)
            {
                bTexture.tiled = tiled;
            }
            if(bTexture.tiled)
            {
                bool patterned = EditorGUILayout.Toggle("Has Pattern", bTexture.patterned);
                if(patterned != bTexture.patterned)
                {
                    bTexture.patterned = patterned;
                }
            }
            else
                bTexture.patterned = false;

            if(bTexture.texture == null)
                return;

            Vector2 textureUnitSize = bTexture.textureUnitSize;
            if(bTexture.tiled)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("texture width", GUILayout.Width(75));//, GUILayout.Width(42));
                textureUnitSize.x = EditorGUILayout.FloatField(bTexture.textureUnitSize.x, GUILayout.Width(25));
                EditorGUILayout.LabelField("metres", GUILayout.Width(40));//, GUILayout.Width(42));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("texture height", GUILayout.Width(75));//, GUILayout.Width(42));
                textureUnitSize.y = EditorGUILayout.FloatField(bTexture.textureUnitSize.y, GUILayout.Width(25));
                EditorGUILayout.LabelField("metres", GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
                if(bTexture.textureUnitSize != textureUnitSize)
                {
                    bTexture.textureUnitSize = textureUnitSize;
                }
            }

            Vector2 tileUnitSize = bTexture.tileUnitUV;
            if(bTexture.patterned)
            {
                float minWidth = 2 / bTexture.texture.width;
                float minHeight = 2 / bTexture.texture.height;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("unit width", GUILayout.Width(75));
                float tileUnitSizex = EditorGUILayout.Slider(tileUnitSize.x, minWidth, 1.0f);
                if(tileUnitSizex != tileUnitSize.x)
                {
                    tileUnitSize.x = tileUnitSizex;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("unit height", GUILayout.Width(75));
                float tileUnitSizey = EditorGUILayout.Slider(tileUnitSize.y, minHeight, 1.0f);
                if(tileUnitSizey != tileUnitSize.y)
                {
                    tileUnitSize.y = tileUnitSizey;
                }
                EditorGUILayout.EndHorizontal();
                bTexture.tileUnitUV = tileUnitSize;

                EditorGUILayout.Space();
            }

            const int previewTextureUnitSize = 120;
            const int previewTileUnitSize = 59;
            const int previewTileUnitPadding = 2;
            const int previewPadding = 25;

            EditorGUILayout.BeginHorizontal();
            if(bTexture.tiled)
            {
                EditorGUILayout.LabelField("1 Metre Squared", GUILayout.Width(previewTextureUnitSize));
            }
            GUILayout.Space(previewPadding);
            if(bTexture.patterned)
            {
                EditorGUILayout.LabelField("Texture Pattern Units", GUILayout.Width(previewTileUnitSize * 2));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();


            if(Event.current.type == EventType.Repaint)
                texturePreviewPostion = GUILayoutUtility.GetLastRect();

            if(bTexture.tiled)
            {
                Rect previewRect = new Rect(texturePreviewPostion.x, texturePreviewPostion.y, previewTextureUnitSize, previewTextureUnitSize);
                Rect sourceRect = new Rect(0, 0, (1.0f / textureUnitSize.x), (1.0f / textureUnitSize.y));

                Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);
            }

            if(bTexture.patterned)
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

            if(!bTexture.tiled)
            {
                EditorGUILayout.LabelField("Tile texture");

                EditorGUILayout.BeginHorizontal();
                int currentXTiles = bTexture.tiledX;
                GUILayout.Label("tile x", GUILayout.Width(38));
                currentXTiles = EditorGUILayout.IntField(currentXTiles, GUILayout.Width(20));
                if(GUILayout.Button("+", GUILayout.Width(25)))
                {
                    currentXTiles++;
                }
                EditorGUI.BeginDisabledGroup(currentXTiles < 2);
                if(GUILayout.Button("-", GUILayout.Width(25)))
                {
                    currentXTiles--;
                }
                EditorGUI.EndDisabledGroup();
                bTexture.tiledX = currentXTiles;

                int currentYTiles = bTexture.tiledY;
                GUILayout.Label("tile y", GUILayout.Width(38));
                currentYTiles = EditorGUILayout.IntField(currentYTiles, GUILayout.Width(20));
                if(GUILayout.Button("+", GUILayout.Width(25)))
                {
                    currentYTiles++;
                }
                EditorGUI.BeginDisabledGroup(currentYTiles < 2);
                if(GUILayout.Button("-", GUILayout.Width(25)))
                {
                    currentYTiles--;
                }
                EditorGUI.EndDisabledGroup();
                bTexture.tiledY = currentYTiles;
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);
                EditorGUILayout.Space();
                if(Event.current.type == EventType.Repaint)
                    texturePreviewPostion = GUILayoutUtility.GetLastRect();

                Rect previewRect = new Rect(texturePreviewPostion.x, texturePreviewPostion.y, previewTextureUnitSize, previewTextureUnitSize);
                Rect sourceRect = new Rect(0, 0, currentXTiles, currentYTiles);

                Graphics.DrawTexture(previewRect, bTexture.texture, sourceRect, 0, 0, 0, 0);
            }

            GUILayout.Space(previewTextureUnitSize);

            break;
        case BuildrTexture.Types.Substance:

            bTexture.proceduralMaterial = (ProceduralMaterial)EditorGUILayout.ObjectField("Procedural Material", bTexture.proceduralMaterial, typeof(ProceduralMaterial), false);
        
            if(bTexture.proceduralMaterial != null)
            {
                ProceduralMaterial pMat = bTexture.proceduralMaterial;
                GUILayout.Label(pMat.GetGeneratedTexture(pMat.mainTexture.name), GUILayout.Width(400));
            }
            else
            {
                EditorGUILayout.HelpBox("There is no substance material set.", MessageType.Error);
            }
            break;

        case BuildrTexture.Types.User:
            bTexture.userMaterial = (Material)EditorGUILayout.ObjectField("User Material", bTexture.userMaterial, typeof(Material), false);

            if (bTexture.userMaterial != null)
            {
                Material mat = bTexture.userMaterial;
                GUILayout.Label(mat.mainTexture, GUILayout.Width(400));
            }
            else
                EditorGUILayout.HelpBox("There is no substance material set.", MessageType.Error);
            break;
        }
    }
}
