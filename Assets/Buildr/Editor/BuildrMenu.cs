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

public class BuildrMenu : EditorWindow {

	[MenuItem("GameObject/Create New BuildR Building",false,0)]
    public static void CreateNewBuilding()
    {
        GameObject newBuilding = new GameObject("New Building");
        Undo.RegisterCreatedObjectUndo(newBuilding, "Created New BuildR Building");
		newBuilding.AddComponent<BuildrEditMode>();
	}
}
