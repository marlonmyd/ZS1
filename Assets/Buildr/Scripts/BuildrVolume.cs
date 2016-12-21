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
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BuildrVolume : ScriptableObject
{
    public const float MINIMUM_VOLUME_HEIGHT = 2.5f;

    public List<int> points = new List<int>();
    public List<bool> renderFacade = new List<bool>();
    [SerializeField]
    private float _height = 10;
    [SerializeField]
    private int _numberOfFloors = 1;
    [SerializeField]
    private int _numberOfBasementFloors = 0;
    public BuildrVolumeStyles styles;
    public int roofDesignID = 0;
    public bool generateStairs = true;
    public float staircaseWidth = 1.0f;
    public float stepHeight = 0.2f;
    public List<Vector3> stairBaseVector = new List<Vector3>();//a list used internally to register where the stairs should be placed
        
    [SerializeField]
    private List<bool> renderFloor = new List<bool>();
    [SerializeField]
    private List<int> floorTextures = new List<int>();
    [SerializeField]
    private List<int> wallTextures = new List<int>();
    [SerializeField]
    private List<int> ceilingTextures = new List<int>();
    [SerializeField]
    private List<int> basementFloorTextures = new List<int>();
    [SerializeField]
    private List<int> basementWallTextures = new List<int>();
    [SerializeField]
    private List<int> basementCeilingTextures = new List<int>();

    public int stairwellFloorTexture = 0;
    public int stairwellStepTexture = 0;
    public int stairwellWallTexture = 0;
    public int stairwellCeilingTexture = 0;

    public void Init()
    {
        styles = ScriptableObject.CreateInstance<BuildrVolumeStyles>();
    }

    public int numberOfFloors
    {
        get { return _numberOfFloors; }
        set
        {
            _numberOfFloors = Mathf.Max(1, value);//make sure the minimun value is one - can't have a volume that's 0
            //or cant we? maybe in a patch we add park/garden/plaza stuff
            CheckTextureData();
        }
    }

    public int numberOfBasementFloors
    {
        get { return _numberOfBasementFloors; }
        set
        {
            _numberOfBasementFloors = Mathf.Max(0, value);//make sure the minimun value is one - can't have a volume that's 0
            //or cant we? maybe in a patch we add park/garden/plaza stuff
            CheckTextureData();
        }
    }

    public int numberOfFacades
    {
        get {return points.Count;}
    }

    private void CheckTextureData()
    {
        while (renderFloor.Count < _numberOfFloors)
            renderFloor.Add(true);

        while (floorTextures.Count < _numberOfFloors)
            floorTextures.Add(1);
        while (wallTextures.Count < _numberOfFloors)
            wallTextures.Add(0);
        while (ceilingTextures.Count < _numberOfFloors)
            ceilingTextures.Add(2);

        while (basementFloorTextures.Count < _numberOfBasementFloors)
            basementFloorTextures.Add(floorTextures[0]);
        while (basementWallTextures.Count < _numberOfBasementFloors)
            basementWallTextures.Add(wallTextures[0]);
        while (basementCeilingTextures.Count < _numberOfBasementFloors)
            basementCeilingTextures.Add(ceilingTextures[0]);
    }

    /// <summary>
    /// Add new volume point
    /// </summary>
    /// <param name="newPointIndex"></param>
    public void Add(int newPointIndex)
    {
        points.Add(newPointIndex);
        renderFacade.Add(true);
        styles.AddStyle(0, newPointIndex, 1);
    }

    public void Add(int newPointIndex, BuildrVolumeStylesUnit[] styleunits)
    {
        points.Add(newPointIndex);
        renderFacade.Add(true);

        foreach (BuildrVolumeStylesUnit style in styleunits)
            styles.AddStyle(style.styleID, style.facadeID, style.floors);
    }

    public void AddRange(int[] newInts)
    {
        foreach (int newInt in newInts)
        {
            points.Add(newInt);
            renderFacade.Add(true);
            styles.AddStyle(0, newInt, 1);
        }
    }

    public void Insert(int index, int newInt)
    {
        points.Insert(index, newInt);
        renderFacade.Insert(index, true);
        styles.NudgeFacadeValues(index);
        styles.AddStyle(0, newInt, 1);
    }

    public void RemoveAndUpdate(int pointID)
    {
        Remove(pointID);
        UpdateIndexUponRemoval(pointID);
    }

    public void Remove(int pointID)
    {
        int index = points.IndexOf(pointID);

        points.Remove(pointID);
        renderFacade.RemoveAt(index);
        //styles.UpdatePointIDRemoval(pointID);
        styles.RemoveStyleByFacadeID(pointID);
    }

    public void UpdateIndexUponRemoval(int pointID)
    {
        styles.UpdatePointIDRemoval(pointID);
        int numberOfPoints = points.Count;
        for (int p = 0; p < numberOfPoints; p++)
            if (points[p] > pointID)
                points[p]--;
    }

    public void RemoveAt(int pointIndex)
    {
        RemoveAndUpdate(points[pointIndex]);
    }

    public int Count
    {
        get { return points.Count; }
    }

    public float height
    {
        get { return _height; }
        set
        {
            _height = Mathf.Max(value, MINIMUM_VOLUME_HEIGHT);
        }
    }

    public bool Contains(int i)
    {
        return points.Contains(i);
    }

    public int IndexOf(int i)
    {
        return points.IndexOf(i);
    }

    public int[] ToArray()
    {
        return points.ToArray();
    }

    public int GetWallIndex(int a, int b)
    {
        int size = Count;
        for (int i = 0; i < size; i++)
        {
            int i2 = (i + 1) % size;

            int pointIndexA = points[i];
            int pointIndexB = points[i2];

            if (pointIndexA == a && pointIndexB == b)
                return i;

            if (pointIndexB == a && pointIndexA == b)
                return i;
        }

        return -1;
    }

    public int FloorTexture(int floorIndex)
    {
        if(floorIndex >= 0)
        {
            if(floorIndex >= floorTextures.Count)
                CheckTextureData();
            return floorTextures[floorIndex];
        }
        else
        {
            int basementTextureIndex = -(floorIndex + 1);
            if (basementTextureIndex >= basementFloorTextures.Count)
                CheckTextureData();
            return basementFloorTextures[basementTextureIndex];
        }
    }

    public int WallTexture(int floorIndex)
    {
        if (floorIndex >= 0)
        {
            if (floorIndex >= wallTextures.Count)
                CheckTextureData();
            return wallTextures[floorIndex];
        }
        else
        {
            int basementTextureIndex = -(floorIndex + 1);
            if (basementTextureIndex >= basementWallTextures.Count)
                CheckTextureData();
            return basementWallTextures[basementTextureIndex];
        }
    }

    public int CeilingTexture(int floorIndex)
    {
        if (floorIndex >= 0)
        {
            if (floorIndex >= ceilingTextures.Count)
                CheckTextureData();
            return ceilingTextures[floorIndex];
        }
        else
        {
            int basementTextureIndex = -(floorIndex + 1);
            if (basementTextureIndex >= basementCeilingTextures.Count)
                CheckTextureData();
            return basementCeilingTextures[basementTextureIndex];
        }
    }

    public void FloorTexture(int floorIndex, int textureIndex)
    {
        if(floorIndex >= 0)
        {
            floorTextures[floorIndex] = textureIndex;
        }
        else
        {
            int basementTextureIndex = -(floorIndex + 1);
            basementFloorTextures[basementTextureIndex] = textureIndex;
        }
    }

    public void WallTexture(int floorIndex, int textureIndex)
    {
        if(floorIndex >= 0)
        {
            wallTextures[floorIndex] = textureIndex;
        }
        else
        {
            int basementTextureIndex = -(floorIndex + 1);
            basementWallTextures[basementTextureIndex] = textureIndex;
        }
    }

    public void CeilingTexture(int floorIndex, int textureIndex)
    {
        if(floorIndex >= 0)
        {
            ceilingTextures[floorIndex] = textureIndex;
        }
        else
        {
            int basementTextureIndex = -(floorIndex + 1);
            basementCeilingTextures[basementTextureIndex] = textureIndex;
        }
    }

    public bool ContainsFacade(int facadeIndex)
    {
        return styles.ContainsFacade(facadeIndex);
    }
}
