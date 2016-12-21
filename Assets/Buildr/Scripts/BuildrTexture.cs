// BuildR
// Available on the Unity3D Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

[System.Serializable]
public class BuildrTexture
{
    public enum Types
    {
        Basic,
        Substance,
        User
    }

    public string name = "new texture";
    public bool tiled = true;
    public bool patterned = false;
    public Types type = Types.Basic;
    //[SerializeField]
    //private Texture2D _texture;
    [SerializeField]
    private Vector2 _tileUnitUV = Vector2.one;//the UV coords of the end of a pattern in the texture - used to match up textures to geometry
    [SerializeField]
    private Vector2 _textureUnitSize = Vector2.one;//the world size of the texture - default 1m x 1m
    public int tiledX = 1;//the amount of times the texture should be repeated along the x axis
    public int tiledY = 1;//the amount of times the texture should be repeated along the y axis
    public Vector2 maxUVTile = Vector2.zero;//used for texture atlasing
    public Vector2 minWorldUnits = Vector2.zero;//also used for atlasing
    public Vector2 maxWorldUnits = Vector2.zero;//also used for atlasing
    public Material material;
    [SerializeField]
    private ProceduralMaterial _proceduralMaterial;
    [SerializeField]
    private Material _userMaterial;
    public Texture2D tiledTexture = null;//this is used for texture packing

    public bool door = false;
    public bool window = false;
    public bool wall = false;
    public bool roof = false;

    public BuildrTexture(string newName)
    {
        name = newName;
        material = new Material(Shader.Find("Diffuse"));
    }


    public BuildrTexture Duplicate()
    {
        return Duplicate(name + " copy");
    }

    public BuildrTexture Duplicate(string newName)
    {
        BuildrTexture newTexture = new BuildrTexture(newName);

        newTexture.tiled = true;
        newTexture.patterned = false;
        newTexture.tileUnitUV = _tileUnitUV;
        newTexture.textureUnitSize = _textureUnitSize;
        newTexture.tiledX = tiledX;
        newTexture.tiledY = tiledY;
        newTexture.maxUVTile = maxUVTile;
        newTexture.material = new Material(material);
        newTexture.tiledTexture = tiledTexture;
        newTexture.door = door;
        newTexture.window = window;
        newTexture.wall = wall;
        newTexture.roof = roof;
        newTexture.proceduralMaterial = _proceduralMaterial;
        newTexture.userMaterial = _userMaterial;

        return newTexture;
    }

    public Texture2D texture
    {
        get
        {
            if(material.mainTexture is ProceduralTexture)
            {
                type = Types.Substance;
                material.mainTexture = null;
            }
            switch (type)
            {
                default:
                    if (material.mainTexture == null)
                        return null;
                    return (Texture2D)material.mainTexture;

                case Types.Substance:
                    if (_proceduralMaterial == null)
                        return null;
                    if (_proceduralMaterial.mainTexture == null)
                        return null;
                    return (Texture2D)_proceduralMaterial.mainTexture;

                case Types.User:
                    if (_userMaterial == null)
                        return null;
                    if (_userMaterial.mainTexture == null)
                        return null;
                    return (Texture2D)_userMaterial.mainTexture;
            }
        }

        set
        {
            if (value == null)
                return;
            if (value != texture)
            {
                switch (type)
                {
                    case Types.Basic:
                        material.mainTexture = value;
                        break;

                    case Types.Substance:
                        _proceduralMaterial.mainTexture = value;
                        break;

                    case Types.User:
                        _userMaterial.mainTexture = value;
                        break;
                }
            }
        }
    }

    public bool isSubstance
    {
        get
        {
            return type == Types.Substance && _proceduralMaterial != null;
        }
    }

    public bool isUSer
    {
        get
        {
            return type == Types.User && _userMaterial != null;
        }
    }

    public Material usedMaterial
    {
        get
        {
            if (isSubstance)
                return _proceduralMaterial;
            if(isUSer)
                return _userMaterial;
            //else
            return material;
        }
    }

    public Vector2 tileUnitUV
    {
        get { return _tileUnitUV; }
        set { _tileUnitUV = value; }
    }

    public Vector2 textureUnitSize
    {
        get { return _textureUnitSize; }
        set { _textureUnitSize = value; }
    }

    public ProceduralMaterial proceduralMaterial
    {
        get {return _proceduralMaterial;} 
        set
        {
            if(value == null)
                return;
            _proceduralMaterial = value;
            _proceduralMaterial.isReadable = true;
        }
    }

    public Material userMaterial
    {
        get
        {
            return _userMaterial;
        }
        set
        {
            if (value != userMaterial)
            {
                _userMaterial = value;
            }
        }
    }

    public void CheckMaxUV(Vector2 checkUV)
    {
        if (checkUV.x > maxUVTile.x)
        {
            maxUVTile.x = checkUV.x;
        }
        if (checkUV.y > maxUVTile.y)
        {
            maxUVTile.y = checkUV.y;
        }
    }

    public void MaxWorldUnitsFromUVs(Vector2 uv)
    {
        float xsize = uv.x * _textureUnitSize.x;
        float ysize = uv.y * _textureUnitSize.y;
        if (xsize > maxWorldUnits.x)
        {
            maxWorldUnits.x = xsize;
        }
        if (ysize > maxWorldUnits.y)
        {
            maxWorldUnits.y = ysize;
        }
    }
    
#if UNITY_EDITOR
    public string filePath
    {
        get
        {
            switch (type)
            {
                default:
                    return AssetDatabase.GetAssetPath(texture);

                case Types.Substance:
                    if (isSubstance)
                        return AssetDatabase.GetAssetPath(proceduralMaterial);
                    else
                        return AssetDatabase.GetAssetPath(texture);

                case Types.User:
                    if (isUSer)
                        return AssetDatabase.GetAssetPath(userMaterial);
                    else
                        return AssetDatabase.GetAssetPath(texture);
            }
        }
    }
#endif

    public Material GetMaterial()
    {
        switch (type)
        {
            default:
                return material;

            case Types.Substance:
                if (isSubstance)
                    return proceduralMaterial;
                else
                    return material;

            case Types.User:
                if (isUSer)
                    return userMaterial;
                else
                    return material;
        }
    }
}
