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

[CanEditMultipleObjects]
[CustomEditor(typeof(BuildrData))]
public class BuildrEditorData : Editor 
{
	
	public static bool defaultInspector = false;
	
	public override void OnInspectorGUI()
	{
		if(GUILayout.Button("Export as XML"))
		{
			BuildrXMLExporter.Export("Assets/BuildR/Exported/","buildrdataexport",(BuildrData)target);
			AssetDatabase.Refresh();
		}
		
		DrawDefaultInspector();
	}
	
}
