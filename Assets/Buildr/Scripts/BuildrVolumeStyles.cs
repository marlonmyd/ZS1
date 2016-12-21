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
public class BuildrVolumeStylesUnit
{
    public int styleID = 0;
    public int facadeID = 0;
    public int floors = 0;
    public int entryID = 0;

    public BuildrVolumeStylesUnit(int style, int facade, int numberOfFloors, int entry)
    {
        styleID = style;
        facadeID = facade;
        floors = numberOfFloors;
        entryID = entry;
    }
}

[System.Serializable]
public class BuildrVolumeStyles : ScriptableObject
{
    [SerializeField]
    private List<int> styleID = new List<int>();
    [SerializeField]
    private List<int> facadeID = new List<int>();
    [SerializeField]
    private List<int> floors = new List<int>();

    public void MoveEntry(int fromIndex, int toIndex)
    {
        int stlID = styleID[fromIndex];
        int fcdID = facadeID[fromIndex];
        int flrs = floors[fromIndex];

        styleID.RemoveAt(fromIndex);
        facadeID.RemoveAt(fromIndex);
        floors.RemoveAt(fromIndex);

        toIndex = Mathf.Max(toIndex, 1);//clamp to 1
        styleID.Insert(toIndex - 1, stlID);
        facadeID.Insert(toIndex - 1, fcdID);
        floors.Insert(toIndex - 1, flrs);
    }

    public void Clear()
    {
        styleID.Clear();
        facadeID.Clear();
        floors.Clear();
    }

    public BuildrVolumeStyles()
    {

    }

    public int numberOfEntries
    {
        get { return styleID.Count; }
    }

    public void AddStyle(int style, int facade, int numberOfFloors)
    {
        styleID.Add(style);
        facadeID.Add(facade);
        floors.Add(numberOfFloors);
    }

    public void RemoveStyle(int index)
    {
        styleID.RemoveAt(index);
        facadeID.RemoveAt(index);
        floors.RemoveAt(index);
    }

    public void RemoveStyleByFacadeID(int facadeIndex)
    {
        int index = facadeID.IndexOf(facadeIndex);
        styleID.RemoveAt(index);
        facadeID.RemoveAt(index);
        floors.RemoveAt(index);
    }

    public void UpdatePointIDRemoval(int pointID)
    {
        int count = numberOfEntries;
        for (int m = 0; m < count; m++)
        {
            if (facadeID[m] > pointID)
            {
                facadeID[m]--;
            }
            else if (facadeID[m] == pointID)
            {
                facadeID.RemoveAt(m);
                styleID.RemoveAt(m);
                floors.RemoveAt(m);
                m--;
                count--;
            }
        }
    }

    public void ModifyStyle(int index, int style)
    {
        styleID[index] = style;
    }

    public void ModifyFacadeID(int index, int facade)
    {
        facadeID[index] = facade;
    }

    public void ModifyFloors(int index, int floor)
    {
        floors[index] = floor;
    }

    public void NudgeFacadeValues(int startIndex)
    {
        int count = numberOfEntries;
        for (int m = 0; m < count; m++)
        {
            if (styleID[m] >= startIndex)
                styleID[m] = styleID[m] + 1;
        }
    }

    public BuildrVolumeStylesUnit GetEntry(int index)
    {
        return new BuildrVolumeStylesUnit(styleID[index], facadeID[index], floors[index], index);
    }

    public BuildrVolumeStylesUnit[] GetContents()
    {
        int numberOfEntries = styleID.Count;
        BuildrVolumeStylesUnit[] output = new BuildrVolumeStylesUnit[numberOfEntries];
        for (int i = 0; i < numberOfEntries; i++)
        {
            output[i] = new BuildrVolumeStylesUnit(styleID[i], facadeID[i], floors[i], i);
        }
        return output;
    }

    public BuildrVolumeStylesUnit[] GetContentsByFacade(int facadeIndex)
    {
        int numberOfEntries = styleID.Count;
        List<BuildrVolumeStylesUnit> output = new List<BuildrVolumeStylesUnit>();
        for (int i = 0; i < numberOfEntries; i++)
        {
            if (facadeID[i] == facadeIndex)
                output.Add(new BuildrVolumeStylesUnit(styleID[i], facadeID[i], floors[i], i));
        }
        return output.ToArray();
    }

    public void CheckRemovedStyle(int facadeStyleID)
    {
        for (int se = 0; se < numberOfEntries; se++)
        {
            if (styleID[se] >= facadeStyleID && styleID[se] > 0)
                styleID[se]--;
        }
    }

    public bool ContainsFacade(int facadeIndex)
    {
        return styleID.Contains(facadeIndex);
    }
}
