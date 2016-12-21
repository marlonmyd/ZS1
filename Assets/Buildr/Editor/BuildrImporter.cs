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

public class BuildrImporter : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        if (assetPath.Contains("Buildr/Textures/") || assetPath.Contains("Buildr/Details/") || assetPath.Contains("Buildr/Resources/Textures/"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.isReadable = true;
            textureImporter.npotScale = TextureImporterNPOTScale.ToLarger;
        }

        if (assetPath.Contains("Buildr/Exported/"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.maxTextureSize = 4096;
            textureImporter.filterMode = FilterMode.Trilinear;
        }

        if (assetPath.Contains("Buildr/Resources/GUI"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.textureType = TextureImporterType.GUI;
            textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
        }
    }

    public void OnPreprocessModel()
    {
        //
    }
}
