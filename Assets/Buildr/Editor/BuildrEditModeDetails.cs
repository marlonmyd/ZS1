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

public class BuildrEditModeDetails
{

    private static int selectedDetail = 0;
    private static bool clickPlace = false;

    public static void SceneGUI(BuildrEditMode editMode, BuildrData data, bool shouldSnap, float handleSize)
    {
        if (data.details.Count == 0)
            return;
        Undo.RecordObject(data, "Detail Modified");

        int numberOfFacades = data.facades.Count;
        int numberOfRoofs = data.roofs.Count;
        int numberOfTextures = data.textures.Count;

        if (numberOfFacades == 0 || numberOfRoofs == 0 || numberOfTextures == 0)
            return;

        Vector3 position = editMode.transform.position;

        BuildrPlan plan = data.plan;
        int numberOfVolumes = plan.numberOfVolumes;
        BuildrDetail bDetail = data.details[selectedDetail];
        float volumeHeight = 0;
        Vector3 baseLeft = Vector3.zero;
        Vector3 baseRight = Vector3.zero;
        int faceIndex = bDetail.face;

        int facadeCounter = 0;
        for (int v = 0; v < numberOfVolumes; v++)
        {
            BuildrVolume volume = data.plan.volumes[v];
            int volumeSize = volume.Count;
            Vector3 floorCentre = Vector3.zero;
            Handles.color = Color.white;

            for (int p = 0; p < volumeSize; p++)
            {
                int point = volume.points[p];
                int indexB = volume.points[(p + 1) % volumeSize];
                Vector3 fb0 = plan.points[point].vector3;
                Vector3 fb1 = plan.points[indexB].vector3;
                if (bDetail.face == facadeCounter && bDetail.type == BuildrDetail.Types.Facade)
                {
                    baseLeft = fb0;
                    baseRight = fb1;
                }
                floorCentre += baseLeft;

                List<Vector3> verts = new List<Vector3>();
                volumeHeight = (volume.numberOfFloors * data.floorHeight);
                Vector3 volumeHeightVector = Vector3.up * volumeHeight;
                verts.Add(fb0 + position);
                verts.Add(fb1 + position);
                verts.Add(verts[1] + volumeHeightVector);
                verts.Add(verts[0] + volumeHeightVector);
                if (bDetail.face == facadeCounter && bDetail.type == BuildrDetail.Types.Facade)
                    //display something to highlight this facade
                    Handles.DrawSolidRectangleWithOutline(verts.ToArray(), Color.clear, BuildrColours.MAGENTA);
                Handles.color = BuildrColours.CYAN;
                if (v == bDetail.face && bDetail.type == BuildrDetail.Types.Roof)
                    Handles.DrawLine(verts[2], verts[3]);
                if (editMode.showFacadeMarkers)
                {
                    Handles.color = Color.white;
                    Vector3 camDirection = Camera.current.transform.forward;
                    Vector3 facadeDirection = Vector3.Cross((verts[0] - verts[1]), Vector3.up);
                    GUIStyle facadeLabelStyle = new GUIStyle();
                    facadeLabelStyle.normal.textColor = Color.white;
                    facadeLabelStyle.alignment = TextAnchor.MiddleCenter;
                    facadeLabelStyle.fixedWidth = 75.0f;
                    if (Vector3.Dot(camDirection, facadeDirection) < 0)//only display label when facade is facing camera
                    {
                        Vector3 centerPos = (verts[0] + verts[1]) * 0.5f;
                        Vector3 labelPos = centerPos + facadeDirection.normalized;
                        Handles.Label(labelPos, "facade " + facadeCounter, facadeLabelStyle);
                        Handles.DrawLine(centerPos, labelPos);
                    }
                }
                facadeCounter++;
            }
        }

        Vector3 handlePosition = bDetail.worldPosition + position;// new Vector3(basePos.x, volumeHeight * bDetail.faceUv.y, basePos.z);
        Vector3 baseDir = (baseRight - baseLeft).normalized;
                Vector3 baseCross = Vector3.Cross(Vector3.up, baseDir);
        Quaternion currentRot = Quaternion.Euler(bDetail.userRotation);
        Quaternion faceRotation = (bDetail.type == BuildrDetail.Types.Facade) ? Quaternion.LookRotation(baseCross) : Quaternion.identity;

        switch (Tools.current)
        {
            case Tool.Move:
                Vector3 dirX, dirY, dirZ;
                if (bDetail.type == BuildrDetail.Types.Facade)
                {
                    dirX = baseDir;
                    dirY = baseCross;
                    dirZ = Vector3.up;
                }
                else
                {
                    dirX = Vector3.right;
                    dirY = Vector3.up;
                    dirZ = Vector3.forward;
                }
                Vector3 newSliderPos;
                Handles.color = BuildrColours.RED;
                newSliderPos = Handles.Slider(handlePosition, dirX, handleSize * 0.666f, Handles.ArrowCap, 0.0f);
                Handles.color = BuildrColours.BLUE;
                newSliderPos = Handles.Slider(newSliderPos, dirZ, handleSize * 0.666f, Handles.ArrowCap, 0.0f);
                Handles.color = BuildrColours.GREEN;
                newSliderPos = Handles.Slider(newSliderPos, dirY, handleSize * 0.666f, Handles.ArrowCap, 0.0f);
                Vector3 sliderDiff = newSliderPos - handlePosition;

                if (sliderDiff != Vector3.zero)
                {
                    float newXUV = 0, newYUV = 0, newHeight = bDetail.faceHeight;
                    if (bDetail.type == BuildrDetail.Types.Facade)
                    {
                        float facadeWidth = Vector3.Distance(baseLeft, baseRight);
                        float sliderDiffX = Mathf.Sqrt(sliderDiff.x * sliderDiff.x + sliderDiff.z * sliderDiff.z) * Mathf.Sign(Vector3.Dot(baseDir, sliderDiff));
                        newXUV = sliderDiffX / facadeWidth + bDetail.faceUv.x;
                        newYUV = sliderDiff.y / volumeHeight + bDetail.faceUv.y;
                    }
                    else
                    {
                        BuildrVolume volume = plan.volumes[faceIndex];
                        int numberOfVolumePoints = volume.points.Count;
                        Vector3 minPoint = plan.points[volume.points[0]].vector3;
                        Vector3 maxPoint = plan.points[volume.points[0]].vector3;
                        for (int p = 1; p < numberOfVolumePoints; p++)
                        {
                            Vector3 fp0 = plan.points[volume.points[p]].vector3;
                            if (fp0.x < minPoint.x) minPoint.x = fp0.x;
                            if (fp0.z < minPoint.z) minPoint.z = fp0.z;
                            if (fp0.x > maxPoint.x) maxPoint.x = fp0.x;
                            if (fp0.z > maxPoint.z) maxPoint.z = fp0.z;
                        }
                        float roofWidth = maxPoint.x - minPoint.x;
                        float roofDepth = maxPoint.z - minPoint.z;
                        newXUV = sliderDiff.x / roofWidth + bDetail.faceUv.x;
                        newYUV = sliderDiff.z / roofDepth + bDetail.faceUv.y;
                        newHeight += sliderDiff.y;
                    }
                    bDetail.faceUv = new Vector2(newXUV, newYUV);
                    bDetail.faceHeight = newHeight;
                }
                break;

                case Tool.Rotate:
                currentRot = Handles.RotationHandle(currentRot, handlePosition);
                bDetail.userRotation = currentRot.eulerAngles;
                break;

                case Tool.Scale:
                bDetail.scale = Handles.ScaleHandle(bDetail.scale, handlePosition, currentRot * faceRotation, handleSize * 0.666f);

                break;
        }

        //draw mesh bounds
        if (bDetail.mesh != null)
        {
            Bounds meshBounds = bDetail.mesh.bounds;
            Quaternion rotation = bDetail.worldRotation;
            Vector3 p0 = rotation * (new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.min.z)) + bDetail.worldPosition;
            Vector3 p1 = rotation * (new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.min.z)) + bDetail.worldPosition;
            Vector3 p2 = rotation * (new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.max.z)) + bDetail.worldPosition;
            Vector3 p3 = rotation * (new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.max.z)) + bDetail.worldPosition;
            Vector3 p4 = rotation * (new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.min.z)) + bDetail.worldPosition;
            Vector3 p5 = rotation * (new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.min.z)) + bDetail.worldPosition;
            Vector3 p6 = rotation * (new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z)) + bDetail.worldPosition;
            Vector3 p7 = rotation * (new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z)) + bDetail.worldPosition;

            Handles.color = BuildrColours.BLUE;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p0, p2);
            Handles.DrawLine(p1, p3);
            Handles.DrawLine(p2, p3);
            Handles.DrawLine(p0, p4);
            Handles.DrawLine(p1, p5);
            Handles.DrawLine(p2, p6);
            Handles.DrawLine(p3, p7);
            Handles.DrawLine(p4, p5);
            Handles.DrawLine(p4, p6);
            Handles.DrawLine(p5, p7);
            Handles.DrawLine(p6, p7);

            if (clickPlace)
            {
                Vector3 planeBase = Vector3.zero;
                Vector3 planeNormal = Vector3.up;
                Vector3 planeSize = Vector3.zero;
                if (bDetail.type == BuildrDetail.Types.Facade)
                {
                    //find facade
                    int facadeCount = 0;
                    bool facadeFound = false;
                    for (int s = 0; s < numberOfVolumes; s++)
                    {
                        BuildrVolume volume = plan.volumes[s];
                        int numberOfVolumePoints = volume.points.Count;
                        for (int p = 0; p < numberOfVolumePoints; p++)
                        {
                            if (facadeCount == faceIndex)
                            {
                                int indexA = p;
                                int indexB = (p + 1) % numberOfVolumePoints;
                                Vector3 fp0 = plan.points[volume.points[indexA]].vector3;
                                Vector3 fp1 = plan.points[volume.points[indexB]].vector3;
                                planeBase = fp0;
                                planeNormal = Vector3.Cross(Vector3.up, fp1 - fp0).normalized;
                                planeSize.x = Vector3.Distance(fp0, fp1);
                                planeSize.y = volume.numberOfFloors * data.floorHeight;
                                facadeFound = true;
                                break;
                            }
                            facadeCount++;
                        }
                        if (facadeFound)
                            break;
                    }
                }
                else
                {
                    BuildrVolume volume = plan.volumes[faceIndex];
                    int numberOfVolumePoints = volume.points.Count;
                    Vector3 minPoint = plan.points[volume.points[0]].vector3;
                    Vector3 maxPoint = plan.points[volume.points[0]].vector3;
                    for (int p = 1; p < numberOfVolumePoints; p++)
                    {
                        Vector3 fp0 = plan.points[volume.points[p]].vector3;
                        if (fp0.x < minPoint.x) minPoint.x = fp0.x;
                        if (fp0.z < minPoint.z) minPoint.z = fp0.z;
                        if (fp0.x > maxPoint.x) maxPoint.x = fp0.x;
                        if (fp0.z > maxPoint.z) maxPoint.z = fp0.z;
                    }
                    planeSize.x = maxPoint.x - minPoint.x;
                    planeSize.z = maxPoint.z - minPoint.z;
                    planeBase = minPoint;
                    planeBase.y = (data.floorHeight * volume.numberOfFloors);
                }
                float distance;
                Plane buildingPlane = new Plane(planeNormal, planeBase);
                Ray ray = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
                if (buildingPlane.Raycast(ray, out distance))
                {
                    Vector3 mousePlanePoint = ray.GetPoint(distance);
                    Quaternion mouseLookDirection = Quaternion.LookRotation(buildingPlane.normal);

                    if (Handles.Button(mousePlanePoint, mouseLookDirection, handleSize * 0.1f, handleSize * 0.1f, Handles.CircleCap))
                    {
                        float xUv, yUv;
                        if (bDetail.type == BuildrDetail.Types.Facade)
                        {
                            Vector3 facadeBaseMousePoint = new Vector3(mousePlanePoint.x, 0, mousePlanePoint.z);
                            xUv = Vector3.Distance(planeBase, facadeBaseMousePoint) / planeSize.x;
                            yUv = (mousePlanePoint.y - planeBase.y) / planeSize.y;
                        }
                        else
                        {
                            xUv = (mousePlanePoint.x - planeBase.x) / planeSize.x;
                            yUv = (mousePlanePoint.z - planeBase.z) / planeSize.z;
                        }
                        bDetail.faceUv = new Vector2(xUv, yUv);
                        clickPlace = false;
                        GUI.changed = true;
                    }
                }
            }
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(editMode);
            EditorUtility.SetDirty(data);
            editMode.UpdateRender();
        }
    }

    public static void InspectorGUI(BuildrEditMode editMode, BuildrData data)
    {

        BuildrDetail[] details = data.details.ToArray();
        int numberOfDetails = details.Length;
        selectedDetail = Mathf.Clamp(selectedDetail, 0, numberOfDetails - 1);

        if (numberOfDetails == 0)
        {
            EditorGUILayout.HelpBox("There are no details to show", MessageType.Info);
            if (GUILayout.Button("Add New"))
            {
                data.details.Add(new BuildrDetail("new detail " + numberOfDetails));
                numberOfDetails++;
                selectedDetail = numberOfDetails - 1;
            }
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Detail", GUILayout.Width(75));
        string[] detailNames = new string[numberOfDetails];
        for (int t = 0; t < numberOfDetails; t++)
            detailNames[t] = details[t].name;
        selectedDetail = EditorGUILayout.Popup(selectedDetail, detailNames);
        EditorGUILayout.EndHorizontal();

        BuildrDetail bDetail = details[selectedDetail];

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add New", GUILayout.Width(81)))
        {
            data.details.Add(new BuildrDetail("new detail " + numberOfDetails));
            numberOfDetails++;
            selectedDetail = numberOfDetails - 1;
        }


        if (GUILayout.Button("Duplicate", GUILayout.Width(90)))
        {
            data.details.Add(bDetail.Duplicate());
            numberOfDetails++;
            selectedDetail = numberOfDetails - 1;

        }

        if (GUILayout.Button("Delete", GUILayout.Width(71)))
        {
            if (EditorUtility.DisplayDialog("Deleting Building Detail Entry", "Are you sure you want to delete this detail?", "Delete", "Cancel"))
            {
                data.details.Remove(bDetail);
                selectedDetail = 0;
                GUI.changed = true;

                return;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        details = data.details.ToArray();
        detailNames = new string[numberOfDetails];
        for (int t = 0; t < numberOfDetails; t++)
            detailNames[t] = details[t].name;
        bDetail = details[selectedDetail];//reassign

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        bDetail.name = EditorGUILayout.TextField("Name", bDetail.name);

        bDetail.mesh = (Mesh)EditorGUILayout.ObjectField("Mesh", bDetail.mesh, typeof(Mesh), false);
        EditorGUIUtility.LookLikeControls();
        bDetail.material.mainTexture = (Texture)EditorGUILayout.ObjectField("Texture", bDetail.material.mainTexture, typeof(Texture), false, GUILayout.Height(140));
        
        if (bDetail.material.mainTexture != null)
        {
            string texturePath = AssetDatabase.GetAssetPath(bDetail.material.mainTexture);
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);

            if (!textureImporter.isReadable)
            {
                EditorGUILayout.HelpBox("The texture you have selected is not readable." +
                    "\nPlease select the readable checkbox under advanced texture settings." +
                    "\nOr move this texture to the BuildR texture folder and reimport.",
                    MessageType.Error);
            }
        }

        BuildrPlan plan = data.plan;
        int numberOfVolumes = plan.numberOfVolumes;
        int numberOfFaces = 0;
        List<int> faceSeletionsList = new List<int>();
        List<string> faceSeletionsStringList = new List<string>();
        if (bDetail.type == BuildrDetail.Types.Facade)
        {
            for (int s = 0; s < numberOfVolumes; s++)
            {
                int numberOfPoints = plan.volumes[s].Count;
                numberOfFaces += numberOfPoints;
                for (int p = 0; p < numberOfPoints; p++)
                {
                    int index = faceSeletionsList.Count;
                    faceSeletionsStringList.Add("facade " + index);
                    faceSeletionsList.Add(index);
                }
            }
        }
        else
        {
            bDetail.face = Mathf.Clamp(0, numberOfVolumes - 1, bDetail.face);
            for (int s = 0; s < numberOfVolumes; s++)
            {
                int index = faceSeletionsList.Count;
                faceSeletionsStringList.Add("roof " + index);
                faceSeletionsList.Add(index);
            }
        }

        if (!clickPlace)
        {
            if (GUILayout.Button("Place Detail with Mouse"))
            {
                clickPlace = true;
            }
        }
        else
        {
            if (GUILayout.Button("Cancel Place Detail"))
            {
                clickPlace = false;
            }
        }

        BuildrDetail.Types bDetailtype = (BuildrDetail.Types)EditorGUILayout.EnumPopup("Face Type", bDetail.type);
        if(bDetailtype != bDetail.type)
        {
            bDetail.type = bDetailtype;
        }
        int[] faceSelections = faceSeletionsList.ToArray();
        string[] faceSelectionString = faceSeletionsStringList.ToArray();
        int bDetailface = EditorGUILayout.IntPopup("Selected Face", bDetail.face, faceSelectionString, faceSelections);
        if(bDetailface != bDetail.face)
        {
            bDetail.face = bDetailface;
        }

        Vector2 bDetailfaceUv = EditorGUILayout.Vector2Field("Face UV", bDetail.faceUv);
        if(bDetailfaceUv != bDetail.faceUv)
        {
            bDetail.faceUv = bDetailfaceUv;
        }
        float bDetailfaceHeight = EditorGUILayout.FloatField("Face Height", bDetail.faceHeight);
        if(bDetailfaceHeight != bDetail.faceHeight)
        {
            bDetail.faceHeight = bDetailfaceHeight;
        }
        Vector3 bDetailuserRotation = EditorGUILayout.Vector3Field("Rotation", bDetail.userRotation);
        if(bDetailuserRotation != bDetail.userRotation)
        {
            bDetail.userRotation = bDetailuserRotation;
        }
        Vector3 bDetailscale = EditorGUILayout.Vector3Field("Object Scale", bDetail.scale);
        if(bDetailscale != bDetail.scale)
        {
            bDetail.scale = bDetailscale;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(120));
        if (bDetail.mesh != null)
        {
            Texture2D previewMeshImage = AssetPreview.GetAssetPreview(bDetail.mesh);
            GUILayout.Label(previewMeshImage);
        }
        else
        {
            Texture2D previewMeshImage = new Texture2D(118, 118);
            GUILayout.Label(previewMeshImage);
            GUILayout.Label("No Mesh Selected");
        }

        if (bDetail.material.mainTexture != null)
        {
            Texture previewMeshImage = bDetail.material.mainTexture;
            GUILayout.Label(previewMeshImage, GUILayout.Width(128), GUILayout.Height(128));
        }
        else
        {
            Texture2D previewMeshImage = new Texture2D(118, 118);
            GUILayout.Label(previewMeshImage);
            GUILayout.Label("No Texture Selected");
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
}
