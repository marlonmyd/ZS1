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

public class BuildrEditModeHUD 
{
	public static void SceneGUI(BuildrEditMode editMode, BuildrData data, bool shouldSnap, float handleSize)
	{
		if(editMode.fullMesh==null)
			return;
		
		Rect HUDRect = new Rect(0,0,300,300);
		Handles.BeginGUI();
		GUILayout.BeginArea(HUDRect);
		
			EditorGUILayout.LabelField("Buildr");
			EditorGUILayout.LabelField("Vertices: "+editMode.fullMesh.vertexCount);
			EditorGUILayout.LabelField("Triangles "+editMode.fullMesh.triangleCount/3);
		
		GUILayout.EndArea();
		Handles.EndGUI();

        bool isLegal = !(data.plan.illegalPoints.Length > 0);
        if (isLegal)
            isLegal = editMode.transform.localScale == Vector3.one;

        if(isLegal)
            return;

        int numberOfFacades = data.facades.Count;
        int numberOfRoofs = data.roofs.Count;
        int numberOfTextures = data.textures.Count;

        if (numberOfFacades == 0 || numberOfRoofs == 0 || numberOfTextures == 0)
            return;

        Vector3 position = editMode.transform.position;

        BuildrPlan area = data.plan;
        int numberOfVolumes = area.numberOfVolumes;

        int facadeCounter = 0;
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volume = data.plan.volumes[s];
            int volumeSize = volume.Count;
            Vector3 floorCentre = Vector3.zero;
            Handles.color = Color.red;

            for (int p = 0; p < volumeSize; p++)
            {
                int point = volume.points[p];
                Vector3 pointPos = area.points[point].vector3;
                floorCentre += pointPos;

                List<Vector3> verts = new List<Vector3>();
                int indexB = (p < volumeSize - 1) ? p + 1 : 0;
                Vector3 volumeHeight = Vector3.up * (volume.numberOfFloors * data.floorHeight);
                verts.Add(pointPos + position);
                verts.Add(area.points[volume.points[indexB]].vector3 + position);
                verts.Add(verts[1] + volumeHeight);
                verts.Add(verts[0] + volumeHeight);
                Handles.DrawSolidRectangleWithOutline(verts.ToArray(), new Color(1,0,0,0.2f), Color.red);
                Handles.DrawLine(verts[2], verts[3]);
                facadeCounter++;
            }
        }
    }
}
