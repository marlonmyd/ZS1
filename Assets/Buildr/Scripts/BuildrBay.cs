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

/// <summary>
/// This class contains the design constriants for a specific bay that will feature in a facade
/// </summary>

[System.Serializable]
public class BuildrBay
{

    public string name = "Bay design";
    public bool isOpening = true;
    public bool renderBack = true;
    public float openingWidth = 1.25f;
    public float openingHeight = 0.85f;
    public float minimumBayWidth = 0.5f;
    public float openingWidthRatio = 0.5f;//the ratio of space between the left and right walls from the opening
    public float openingHeightRatio = 0.95f;//the ratio of space between above and below the opening
    public float openingDepth = 0.1f;
    public float columnDepth = 0.0f;
    public float rowDepth = 0.0f;
    public float crossDepth = 0.0f;
    [SerializeField]
    private float _faceDepth = 1.0f;

    public GameObject bayModel;

    public BuildrBay(string newName)
    {
        name = newName;
    }

    //textures
    public int numberOfTextures
    {
        get { return System.Enum.GetValues(typeof(TextureNames)).Length; }
    }

    public enum TextureNames
    {
        OpeningBackTexture,
        OpeningSideTexture,
        OpeningSillTexture,
        OpeningCeilingTexture,
        ColumnTexture,
        RowTexture,
        CrossTexture,
        WallTexture
    }

    [SerializeField]
    private int[] _textureValues;
    [SerializeField]
    private bool[] _flipValues;
    public int wallTexture = 0;//this is use then there are no windows

    public BuildrBay Duplicate()
    {
        BuildrBay newBay = new BuildrBay(name + " copy");
        newBay.isOpening = isOpening;
        newBay.openingWidth = openingWidth;
        newBay.openingHeight = openingHeight;
        newBay.minimumBayWidth = minimumBayWidth;
        newBay.openingWidthRatio = openingWidthRatio;//the ratio of space between the left and right walls from the opening
        newBay.openingHeightRatio = openingHeightRatio;//the ratio of space between above and below the opening
        newBay.openingDepth = openingDepth;
        newBay.columnDepth = columnDepth;
        newBay.rowDepth = rowDepth;
        newBay.crossDepth = crossDepth;
        newBay.textureValues = (int[])textureValues.Clone();
        newBay.flipValues = (bool[])flipValues.Clone();
        newBay.bayModel = bayModel;

        return newBay;
    }

    public int[] textureValues
    {
        get
        {
            if (_textureValues == null)
                _textureValues = new int[0];

            if (_textureValues.Length != numberOfTextures)
            {
                int[] tempArr = (int[])_textureValues.Clone();
                int oldSize = tempArr.Length;
                _textureValues = new int[numberOfTextures];
                if (oldSize > 0)
                {
                    for (int i = 0; i < oldSize; i++)
                    {
                        _textureValues[i] = tempArr[i];
                    }
                }
                else
                {
                    _textureValues = new[] { 1, 0, 0, 0, 0, 0, 0, 0 };
                }
            }

            return _textureValues;
        }
        set
        {
            _textureValues = value;
        }
    }

    public bool[] flipValues
    {
        get
        {
            if (_flipValues == null)
                _flipValues = new bool[0];

            if (_flipValues.Length != numberOfTextures)
            {
                bool[] tempArr = (bool[])_flipValues.Clone();
                int oldSize = tempArr.Length;
                _flipValues = new bool[numberOfTextures];
                if (oldSize > 0)
                {
                    for (int i = 0; i < oldSize; i++)
                    {
                        _flipValues[i] = tempArr[i];
                    }
                }
                else
                {
                    _flipValues = new bool[8];
                }
            }

            return _flipValues;
        }
        set
        {
            _flipValues = value;
        }
    }

    /// <summary>
    /// Depth of the back face as a ratio from front to back (0 being flush with facade, 1 being at the back
    /// Default is 1.
    /// Clamped 0-1
    /// </summary>
    public float faceDepth {get {return _faceDepth;} set {_faceDepth = Mathf.Clamp01(value);}}

    public int GetTexture(TextureNames textureName)
    {
        return textureValues[(int)textureName];
    }

    public void SetTexture(TextureNames textureName, int textureIndex)
    {
        textureValues[(int)textureName] = textureIndex;
    }

    public bool IsFlipped(TextureNames textureName)
    {
        return flipValues[(int)textureName];
    }

    public float deepestValue
    {
        get { return Mathf.Max(openingDepth,columnDepth,rowDepth,crossDepth); }
    }
}
