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

public class BuildrEditModeFloorplan
{

    public static void SceneGUI(BuildrEditMode editMode, BuildrPlan plan, bool shouldSnap, float handleSize)
    {
        Vector3 position = editMode.transform.position;
        Plane buildingPlane = new Plane(Vector3.up, position);
        float distance;
        Vector3 mousePlanePoint = Vector3.zero;
        Camera sceneCamera = Camera.current;
        Ray ray = sceneCamera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
        if (buildingPlane.Raycast(ray, out distance))
        {
            mousePlanePoint = ray.GetPoint(distance);
        }
        Quaternion mouseLookDirection = Quaternion.LookRotation(-ray.direction);

        //Draw the floorplan outline
        int numberOfVolumes = plan.numberOfVolumes;
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volumeLinks = plan.volumes[s];
            int volumeSize = volumeLinks.Count;

            for (int l = 0; l < volumeSize; l++)
            {
                Handles.color = Color.white;
                Vector3[] wallPositions = plan.GetWallVectors(s, l);
                Handles.DrawLine(wallPositions[0] + position, wallPositions[1] + position);
            }
        }

        //draw outlines of building cores
        int numberOfCores = plan.cores.Count;
        for(int c = 0; c < numberOfCores; c++)
        {
            Rect coreOutline = plan.cores[c];
            Vector3 coreCenter = new Vector3(coreOutline.center.x,0,coreOutline.center.y);
            Handles.Label(coreCenter + position, "Core " + (c + 1));
            Vector3 coreBL = new Vector3(coreOutline.xMin,0, coreOutline.yMin) + position;
            Vector3 coreBR = new Vector3(coreOutline.xMax,0, coreOutline.yMin) + position;
            Vector3 coreTL = new Vector3(coreOutline.xMin,0, coreOutline.yMax) + position;
            Vector3 coreTR = new Vector3(coreOutline.xMax,0, coreOutline.yMax) + position;
            Handles.DrawLine(coreBL,coreBR);
            Handles.DrawLine(coreBR,coreTR);
            Handles.DrawLine(coreTR,coreTL);
            Handles.DrawLine(coreTL,coreBL);
        }

        //Draw red lines over illegal point/lines
        int numberOfIllegalPoints = plan.numberOfIllegalPoints;
        if (numberOfIllegalPoints > 0)
        {
            Handles.color = Color.red;
            Vector2z[] illegalPoints = plan.illegalPoints;
            for (int i = 0; i < numberOfIllegalPoints - 1; i += 2)
            {
                Vector3 a, b;
                a = illegalPoints[i].vector3 + position;
                b = illegalPoints[i + 1].vector3 + position;
                Handles.DrawLine(a, b);
            }
        }

        SceneView.focusedWindow.wantsMouseMove = false;
        Vector3 vertA;
        Vector3 vertB;
        int selectedPoint;
        switch (editMode.mode)
        {

            case BuildrEditMode.modes.floorplan:
                Vector3 sliderPos = Vector3.zero;
                int numberOfPoints = plan.points.Count;
                int numberOfSelectedPoints = editMode.selectedPoints.Count;

                //Per point scene gui
                for (int i = 0; i < numberOfPoints; i++)
                {
                    Vector2z point = plan.points[i];
                    Vector3 pointPos = point.vector3 + position;
                    float pointHandleSize = HandleUtility.GetHandleSize(pointPos);
                    bool selected = editMode.selectedPoints.Contains(i);
                    if (selected)
                    {
                        Handles.color = Color.green;
                        //Handles.Label(pointPos, "point "+i);
                        sliderPos += point.vector3;
                        if (Handles.Button(pointPos, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f, Handles.DotCap))
                        {
                            editMode.selectedPoints.Remove(i);
                        }

                    }
                    else
                    {
                        Handles.color = Color.white;
                        if (Handles.Button(pointPos, Quaternion.identity, pointHandleSize * 0.05f, pointHandleSize * 0.05f, Handles.DotCap))
                        {
                            if (!shouldSnap)
                                editMode.selectedPoints.Clear();
                            editMode.selectedPoints.Add(i);
                        }
                    }

                    float pointDot = Vector3.Dot(sceneCamera.transform.forward, pointPos - sceneCamera.transform.position);
                    if(pointDot > 0.0f)
                    {
                        Handles.color = Color.white;
                        GUIStyle pointLabelStyle = new GUIStyle();
                        pointLabelStyle.normal.textColor = Color.white;
                        pointLabelStyle.fontStyle = FontStyle.Bold;
                        pointLabelStyle.alignment = TextAnchor.MiddleCenter;
                        pointLabelStyle.fixedWidth = 50.0f;
                        Handles.Label(pointPos + Vector3.up * 2, "point " + i, pointLabelStyle);
                    }
                }

                //draw plan dimensions
                if (editMode.showDimensionLines)
                {
                    Handles.color = Color.white;
                    for (int v = 0; v < numberOfVolumes; v++)
                    {
                        BuildrVolume volume = plan.volumes[v];
                        int volumeSize = volume.Count;
                        for (int l = 0; l < volumeSize; l++)
                        {
                            if (plan.GetConnectingVolumeIndex(v, volume.points[l], volume.points[(l + 1) % volumeSize]) != -1)
                                continue;
                            vertA = plan.points[volume.points[l]].vector3 + position;
                            vertB = plan.points[volume.points[(l + 1) % volumeSize]].vector3 + position;
                            float wallWidth = Vector3.Distance(vertA, vertB);
                            Vector3 facadeDirection = Vector3.Cross((vertA - vertB), Vector3.up).normalized;
                            Vector3 labelPos = (vertA + vertB) * 0.5f + facadeDirection;
                            GUIStyle widthStyle = new GUIStyle();
                            widthStyle.normal.textColor = Color.white;
                            widthStyle.alignment = TextAnchor.MiddleCenter;
                            widthStyle.fixedWidth = 50.0f;
                            Handles.Label(labelPos, wallWidth.ToString("F2") + "m", widthStyle);
                            if (wallWidth > 3)//draw guidelines
                            {
                                float gapSpace = (HandleUtility.GetHandleSize(labelPos) * 0.5f) / wallWidth;
                                Vector3 lineStopA = Vector3.Lerp(vertA, vertB, (0.5f - gapSpace)) + facadeDirection;
                                Vector3 lineStopB = Vector3.Lerp(vertA, vertB, (0.5f + gapSpace)) + facadeDirection;
                                Handles.DrawLine(vertA + facadeDirection, lineStopA);
                                Handles.DrawLine(vertA + facadeDirection, vertA);
                                Handles.DrawLine(vertB + facadeDirection, lineStopB);
                                Handles.DrawLine(vertB + facadeDirection, vertB);
                            }
                        }
                    }
                }

                //selected point scene gui
                if (numberOfSelectedPoints > 0)
                {
//                    Undo.SetSnapshotTarget(plan, "Floorplan Node Moved");
                    sliderPos /= numberOfSelectedPoints;
                    Vector3 dirX = (sliderPos.x < 0) ? Vector3.right : Vector3.left;
                    Vector3 dirZ = (sliderPos.z < 0) ? Vector3.forward : Vector3.back;
                    sliderPos += position;
                    Vector3 newSliderPos;
                    Handles.color = Color.red;
                    newSliderPos = Handles.Slider(sliderPos, dirX, HandleUtility.GetHandleSize(sliderPos) * 0.666f, Handles.ArrowCap, 0.0f);
                    Handles.color = Color.blue;
                    newSliderPos = Handles.Slider(newSliderPos, dirZ, HandleUtility.GetHandleSize(newSliderPos) * 0.666f, Handles.ArrowCap, 0.0f);

                    Vector3 sliderDiff = newSliderPos - sliderPos;

                    for (int i = 0; i < numberOfPoints; i++)
                    {
                        if (editMode.selectedPoints.Contains(i))
                        {
                            if(sliderDiff != Vector3.zero)
                            {
                                Vector2z point = plan.points[i];
                                point.vector3 += sliderDiff;

                                if(editMode.snapFloorplanToGrid)
                                {
                                    Vector3 snappedPoint = point.vector3;
                                    snappedPoint.x -= snappedPoint.x % editMode.floorplanGridSize;
                                    snappedPoint.z -= snappedPoint.z % editMode.floorplanGridSize;
                                    point.vector3 = snappedPoint;
                                }
                            }
                        }
                    }
                }

                //core gui
                for(int c = 0; c < numberOfCores; c++)
                {
//                    Undo.SetSnapshotTarget(plan, "Core Node Moved");
                    Rect coreOutline = plan.cores[c];

                    Vector3 coreLeft = new Vector3(coreOutline.xMin, 0, (coreOutline.yMin + coreOutline.yMax)/2);
                    Vector3 coreRight = new Vector3(coreOutline.xMax, 0, (coreOutline.yMin + coreOutline.yMax)/2);
                    Vector3 coreBottom = new Vector3((coreOutline.xMin + coreOutline.xMax) / 2, 0, coreOutline.yMin);
                    Vector3 coreTop = new Vector3((coreOutline.xMin + coreOutline.xMax) / 2, 0, coreOutline.yMax);

                    Vector3 newCoreLeft = Handles.Slider(coreLeft + position, Vector3.left, HandleUtility.GetHandleSize(coreLeft) * 0.666f, Handles.ArrowCap, 0.0f);
                    Vector3 newCoreRight = Handles.Slider(coreRight + position, Vector3.right, HandleUtility.GetHandleSize(coreLeft) * 0.666f, Handles.ArrowCap, 0.0f);
                    Vector3 newCoreBottom = Handles.Slider(coreBottom + position, Vector3.back, HandleUtility.GetHandleSize(coreLeft) * 0.666f, Handles.ArrowCap, 0.0f);
                    Vector3 newCoreTop = Handles.Slider(coreTop + position, Vector3.forward, HandleUtility.GetHandleSize(coreLeft) * 0.666f, Handles.ArrowCap, 0.0f);

                    newCoreLeft -= position;
                    newCoreRight -= position;
                    newCoreBottom -= position;
                    newCoreTop -= position;

                    if (coreLeft != newCoreLeft)
                        coreOutline.xMin = Mathf.Min(newCoreLeft.x, coreOutline.xMax - 1.0f);
                    if (coreRight != newCoreRight)
                        coreOutline.xMax = Mathf.Max(newCoreRight.x, coreOutline.xMin + 1.0f);
                    if (coreBottom != newCoreBottom)
                        coreOutline.yMin = Mathf.Min(newCoreBottom.z, coreOutline.yMax - 1.0f);
                    if (coreTop != newCoreTop)
                        coreOutline.yMax = Mathf.Max(newCoreTop.z, coreOutline.yMin + 1.0f);


                    plan.cores[c] = coreOutline;
                }

                break;

            case BuildrEditMode.modes.addNewVolume:
                Vector3 basePoint = mousePlanePoint;
                Vector3 width = Vector3.right * 10;
                Vector3 height = Vector3.forward * 10;
                Vector3[] verts = new Vector3[4] { basePoint - width - height, basePoint + width - height, basePoint + width + height, basePoint + height - width };

                Handles.DrawSolidRectangleWithOutline(verts, new Color(0.2f, 0.4f, 0.9f, 0.5f), new Color(0.1f, 0.2f, 1.0f, 0.85f));
                Handles.Label(mousePlanePoint, "Click to place a new volume");
                if (Handles.Button(basePoint, Quaternion.identity, 0, 10, Handles.CircleCap))
                {
//                    Undo.RegisterUndo(plan.GetUndoObjects(), "Add new volume");
                    plan.AddVolume(verts, -position);
                    editMode.SetMode(BuildrEditMode.modes.floorplan);
                    EditorUtility.SetDirty(plan);
                }

                break;

            case BuildrEditMode.modes.addNewVolumeByDraw:

                if (editMode.startVolumeDraw == Vector3.zero)
                {
                    Handles.Label(mousePlanePoint, "Click to select the start point of this volume");
                    if (Handles.Button(mousePlanePoint, Quaternion.identity, handleSize * 0.1f, handleSize * 0.1f, Handles.CircleCap))
                    {
                        editMode.startVolumeDraw = mousePlanePoint;
                    }
                }
                else
                {
                    Vector3 baseDrawPoint = editMode.startVolumeDraw;
                    Vector3 finishDrawPoint = mousePlanePoint;
                    Vector3[] drawVerts = new Vector3[4];
                    drawVerts[0] = new Vector3(baseDrawPoint.x, 0, baseDrawPoint.z);
                    drawVerts[1] = new Vector3(finishDrawPoint.x, 0, baseDrawPoint.z);
                    drawVerts[2] = new Vector3(finishDrawPoint.x, 0, finishDrawPoint.z);
                    drawVerts[3] = new Vector3(baseDrawPoint.x, 0, finishDrawPoint.z);

                    Handles.DrawSolidRectangleWithOutline(drawVerts, new Color(0.2f, 0.4f, 0.9f, 0.5f), new Color(0.1f, 0.2f, 1.0f, 0.85f));
                    Handles.Label(mousePlanePoint, "Click to finish and add a new volume");
                    if (Handles.Button(mousePlanePoint, Quaternion.identity, 0, 10, Handles.CircleCap))
                    {
//                        Undo.RegisterUndo(plan.GetUndoObjects(), "Add new volume");
                        plan.AddVolume(drawVerts, -position);
                        editMode.SetMode(BuildrEditMode.modes.floorplan);
                        EditorUtility.SetDirty(plan);
                    }
                }
                break;

            case BuildrEditMode.modes.addNewVolumeByPoints:

                int numberOfDrawnPoints = editMode.volumeDrawPoints.Count;
                bool allowNewPoint = true;
                for (int p = 0; p < numberOfDrawnPoints; p++)
                {
                    Vector3 point = editMode.volumeDrawPoints[p];
                    if (p == 0 && Vector3.Distance(point, mousePlanePoint) < 3 && numberOfDrawnPoints >= 3)
                    {
                        allowNewPoint = false;//hovering over the first point - don't add a new point - ready to complete the volume plan
                        Handles.color = Color.green;
                    }
                    Vector3 lookDirection = -(point - Camera.current.transform.position);

                    int p2 = (p + 1) % numberOfDrawnPoints;
                    if (p < numberOfDrawnPoints - 1 || !allowNewPoint)//don't draw last line
                        Handles.DrawLine(point, editMode.volumeDrawPoints[p2]);
                    float pointhandleSize = HandleUtility.GetHandleSize(point);
                    if (Handles.Button(point, Quaternion.LookRotation(lookDirection), pointhandleSize * 0.1f, pointhandleSize * 0.1f, Handles.DotCap))
                    {
                        if (p == 0 && numberOfDrawnPoints >= 3)
                        {
                            plan.AddVolume(editMode.volumeDrawPoints.ToArray(), -position);
                            editMode.SetMode(BuildrEditMode.modes.floorplan);
                            EditorUtility.SetDirty(plan);
                            GUI.changed = true;
                            return;
                        }
                    }
                }

                if (allowNewPoint)
                {

                    bool isLegal = true;
                    if (numberOfDrawnPoints >= 1)
                    {
                        Vector2z newPoint = new Vector2z(mousePlanePoint);
                        Vector2z lastPoint = new Vector2z(editMode.volumeDrawPoints[numberOfDrawnPoints - 1]);
                        for (int op = 0; op < numberOfDrawnPoints - 1; op++)//don't do the final line
                        {
                            int op2 = (op + 1) % numberOfDrawnPoints;

                            if (BuildrUtils.FastLineIntersection(newPoint, lastPoint, new Vector2z(editMode.volumeDrawPoints[op]), new Vector2z(editMode.volumeDrawPoints[op2])))
                                isLegal = false;
                        }
                        for (int v = 0; v < numberOfVolumes; v++)
                        {
                            BuildrVolume volume = plan.volumes[v];
                            int volumeSize = volume.Count;
                            for (int l = 0; l < volumeSize; l++)
                            {
                                int vp1 = l;
                                int vp2 = (l + 1) % volumeSize;

                                Vector2z v2zPos = new Vector2z(position);
                                Vector2z p1 = newPoint - v2zPos;
                                Vector2z p2 = lastPoint - v2zPos;
                                Vector2z p3 = plan.points[volume.points[vp1]];
                                Vector2z p4 = plan.points[volume.points[vp2]];

                                if (BuildrUtils.FastLineIntersection(p1, p2, p3, p4))
                                    isLegal = false;
                            }
                        }

                        Handles.Label(mousePlanePoint, "Click to add another point to the volume wall");

                        if (!isLegal)
                            Handles.color = Color.red;

                        Handles.DrawLine(lastPoint.vector3, newPoint.vector3);
                    }
                    else
                    {
                        Handles.Label(mousePlanePoint, "Click to add the first point in this volume wall");
                    }

                    Handles.color = Color.white;
                    if (Handles.Button(mousePlanePoint, mouseLookDirection, 0, 10, Handles.CircleCap))
                    {
                        if (isLegal)
                        {
//                            Undo.RegisterUndo(plan.GetUndoObjects(), "Add new wall line for new volume");
                            editMode.volumeDrawPoints.Add(mousePlanePoint);
                            EditorUtility.SetDirty(plan);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", "Wall lines cannot intersect other wall lines", "ok, sorry");
                        }
                    }
                }
                else
                {
                    Handles.Label(mousePlanePoint, "Click to complete the volume wall plan");
                }

                break;

            case BuildrEditMode.modes.removeVolume:

                for (int v = 0; v < numberOfVolumes; v++)
                {
                    BuildrVolume volume = plan.volumes[v];
                    int volumeSize = volume.Count;
                    Vector3 centerPoint = Vector3.zero;
                    for (int l = 0; l < volumeSize; l++)
                    {
                        centerPoint += plan.points[volume.points[l]].vector3;
                    }
                    centerPoint /= volumeSize;
                    centerPoint += position;

                    Handles.color = Color.red;
                    float centerPointHandleSize = HandleUtility.GetHandleSize(centerPoint);
                    if (Handles.Button(centerPoint, Quaternion.identity, centerPointHandleSize * 0.1f, centerPointHandleSize * 0.1f, Handles.DotCap))
                    {
//                        Undo.RegisterUndo(plan.GetUndoObjects(), "Remove volume");
//                        Undo.RegisterSceneUndo("Remove Volume");
                        plan.RemoveVolume(volume);
                        numberOfVolumes--;
                        v--;
                        editMode.SetMode(BuildrEditMode.modes.floorplan);
                        EditorUtility.SetDirty(plan);
                    }
                }

                break;

            case BuildrEditMode.modes.mergeVolumes:

                List<int> usedPointsA = new List<int>();
                List<int> usedPointsB = new List<int>();
                for (int v = 0; v < numberOfVolumes; v++)
                {
                    BuildrVolume volume = plan.volumes[v];
                    int volumeSize = volume.Count;
                    for (int p = 0; p < volumeSize; p++)
                    {
                        int a = volume.points[p];
                        int b = volume.points[(p + 1) % volumeSize];

                        bool alreadyDrawn = false;
                        foreach (int pa in usedPointsA)
                        {
                            if (pa == a || pa == b)
                            {
                                int pb = usedPointsB[usedPointsA.IndexOf(pa)];
                                if (pb == a || pb == b)
                                {
                                    alreadyDrawn = true;
                                    break;
                                }
                            }
                        }

                        if (!alreadyDrawn)
                        {

                            usedPointsA.Add(a);
                            usedPointsA.Add(b);
                            usedPointsB.Add(b);
                            usedPointsB.Add(a);

                            int otherV = plan.GetConnectingVolumeIndex(v, a, b);
                            if (otherV == -1)
                                continue;//it's not connected to another volume

                            vertA = plan.points[a].vector3 + position;
                            vertB = plan.points[b].vector3 + position;
                            Vector3 diff = vertA - vertB;
                            Vector3 facadeDirection = Vector3.Cross(diff, Vector3.up).normalized;
                            Vector3 midPoint = Vector3.Lerp(vertA, vertB, 0.5f);

                            float mergeHandleSize = HandleUtility.GetHandleSize(midPoint) * 0.1f;
                            Vector3 outPointA = midPoint + (facadeDirection * mergeHandleSize * 6);
                            Vector3 outPointB = midPoint - (facadeDirection * mergeHandleSize * 6);
                            Handles.ArrowCap(0, outPointA, Quaternion.LookRotation(-facadeDirection), mergeHandleSize * 4);
                            Handles.ArrowCap(0, outPointB, Quaternion.LookRotation(facadeDirection), mergeHandleSize * 4);

                            GUIStyle pointLabelStyle = new GUIStyle();
                            pointLabelStyle.normal.textColor = Color.white;
                            pointLabelStyle.fontStyle = FontStyle.Bold;
                            pointLabelStyle.alignment = TextAnchor.MiddleCenter;
                            pointLabelStyle.fixedWidth = 50.0f;
                            Handles.Label(midPoint + Vector3.up * mergeHandleSize * 3, "Merge", pointLabelStyle);

                            if (Handles.Button(midPoint, Quaternion.identity, mergeHandleSize, mergeHandleSize, Handles.DotCap))
                            {
//                                Undo.RegisterSceneUndo("Merge Volume");
                                int otherVolume = plan.GetConnectingVolumeIndex(v, a, b);
                                plan.MergeVolumes(v, otherVolume);
                                numberOfVolumes--;
                                editMode.SetMode(BuildrEditMode.modes.floorplan);
                                EditorUtility.SetDirty(plan);
                            }
                        }
                    }
                }

                break;

            //SUB VOLUME FUNCTIONS

            case BuildrEditMode.modes.splitwall:

                SceneView.focusedWindow.wantsMouseMove = true;
                float pointDistance = 999;
                int wallIndex = -1;
                int volumeIndex = -1;
                Vector3 closestPoint = Vector3.zero;
                Vector3 usePoint = Vector3.zero;
                vertA = Vector3.zero;
                vertB = Vector3.zero;

                for (int s = 0; s < numberOfVolumes; s++)
                {
                    BuildrVolume volumeLinks = plan.volumes[s];
                    int volumeSize = volumeLinks.Count;

                    for (int l = 0; l < volumeSize; l++)
                    {
                        Vector3[] wallVectors = plan.GetWallVectors(s, l);
                        closestPoint = BuildrUtils.ClosestPointOnLine(wallVectors[0] + position, wallVectors[1] + position, mousePlanePoint);
                        float thisDist = Vector3.Distance(closestPoint, mousePlanePoint);
                        if (thisDist < pointDistance)
                        {
                            wallIndex = l;
                            volumeIndex = s;
                            vertA = wallVectors[0];
                            vertB = wallVectors[1];
                            usePoint = closestPoint;
                            pointDistance = thisDist;
                            editMode.selectedPoints.Clear();
                        }

                        Handles.color = Color.white;
                        float wallHandleSize = HandleUtility.GetHandleSize(wallVectors[0] + position);
                        Handles.DotCap(0, wallVectors[0] + position, Quaternion.identity, wallHandleSize * 0.05f);
                    }
                }

                if (wallIndex != -1 && pointDistance < 5 && volumeIndex != -1)
                {
                    float pointHandleSize = HandleUtility.GetHandleSize(usePoint);
                    if (Handles.Button(usePoint, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f, Handles.DotCap))
                    {
//                        Undo.RegisterUndo(plan.GetUndoObjects(), "Split Wall");
                        int newPointID = plan.AddWallPoint(usePoint - position, wallIndex, volumeIndex);
                        editMode.selectedPoints.Clear();
                        editMode.selectedPoints.Add(newPointID);
                        editMode.SetMode(BuildrEditMode.modes.floorplan);
                        EditorUtility.SetDirty(plan);
                    }

                    Handles.color = Color.white;
                    GUIStyle widthStyle = new GUIStyle();
                    widthStyle.normal.textColor = Color.white;
                    widthStyle.alignment = TextAnchor.MiddleCenter;
                    widthStyle.fixedWidth = 50.0f;
                    Vector3 facadeDirection = Vector3.Cross((vertA - vertB), Vector3.up).normalized;

                    float wallWidthA = Vector3.Distance(vertA, usePoint);
                    Vector3 labelPosA = (vertA + usePoint) * 0.5f + facadeDirection;
                    Handles.Label(labelPosA, wallWidthA.ToString("F2") + "m", widthStyle);
                    if (wallWidthA > 3)//draw guidelines
                    {
                        float gapSpace = (pointHandleSize * 0.5f) / wallWidthA;
                        Vector3 lineStopA = Vector3.Lerp(vertA, usePoint, (0.5f - gapSpace)) + facadeDirection;
                        Vector3 lineStopB = Vector3.Lerp(vertA, usePoint, (0.5f + gapSpace)) + facadeDirection;
                        Handles.DrawLine(vertA + facadeDirection, lineStopA);
                        Handles.DrawLine(vertA + facadeDirection, vertA);
                        Handles.DrawLine(usePoint + facadeDirection, lineStopB);
                        Handles.DrawLine(usePoint + facadeDirection, usePoint);
                    }

                    float wallWidthB = Vector3.Distance(usePoint, vertB);
                    Vector3 labelPosB = (usePoint + vertB) * 0.5f + facadeDirection;
                    Handles.Label(labelPosB, wallWidthB.ToString("F2") + "m", widthStyle);
                    if (wallWidthB > 3)//draw guidelines
                    {
                        float gapSpace = (pointHandleSize * 0.5f) / wallWidthB;
                        Vector3 lineStopA = Vector3.Lerp(vertB, usePoint, (0.5f - gapSpace)) + facadeDirection;
                        Vector3 lineStopB = Vector3.Lerp(vertB, usePoint, (0.5f + gapSpace)) + facadeDirection;
                        Handles.DrawLine(vertB + facadeDirection, lineStopA);
                        Handles.DrawLine(vertB + facadeDirection, vertB);
                        Handles.DrawLine(usePoint + facadeDirection, lineStopB);
                        Handles.DrawLine(usePoint + facadeDirection, usePoint);
                    }
                }
                Handles.color = Color.white;
                break;

            case BuildrEditMode.modes.removewall:

                int index = 0;
                foreach (Vector2z point in plan.points)
                {
                    Handles.color = Color.white;
                    Vector3 pointPos = point.vector3 + position;
                    float pointHandleSize = HandleUtility.GetHandleSize(pointPos);
                    if (Handles.Button(pointPos, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f, Handles.DotCap))
                    {
//                        Undo.RegisterSceneUndo("Delete Wall Point");
                        plan.RemovePoint(index);
                        editMode.SetMode(BuildrEditMode.modes.floorplan);
                        break;
                    }

                    index++;
                }

                break;

            case BuildrEditMode.modes.extrudewallselect:

                Handles.color = Color.blue;
                for (int s = 0; s < numberOfVolumes; s++)
                {
                    BuildrVolume volume = plan.volumes[s];
                    int volumeSize = volume.Count;

                    for (int l = 0; l < volumeSize; l++)
                    {
                        int a = volume.points[l];
                        int b = (l < volume.points.Count - 1) ? volume.points[l + 1] : volume.points[0];
                        if (plan.GetConnectingVolumeIndex(s, a, b) == -1)//if the volume wall has not been connected to two volumes yet...
                        {
                            Vector3[] pIndexes = plan.GetWallVectors(s, l);
                            Vector3 pA = pIndexes[0];
                            Vector3 pB = pIndexes[1];
                            Vector3 pC = (pA + pB) / 2 + position;
                            float pointHandleSize = HandleUtility.GetHandleSize(pC);

                            if (Handles.Button(pC, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f, Handles.DotCap))
                            {
//                                Undo.RegisterSceneUndo("Extrude wall");
                                int wallIndexA = l;
                                int newPointAIndex = plan.AddWallPoint(pA, wallIndexA, s);
                                int wallIndexB = l + 1;

                                int newPointBIndex = plan.AddWallPoint(pB, wallIndexB, s);

                                editMode.SetMode(BuildrEditMode.modes.floorplan);

                                editMode.selectedPoints.Clear();
                                editMode.selectedPoints.Add(newPointAIndex);
                                editMode.selectedPoints.Add(newPointBIndex);
                                break;
                            }
                        }
                    }
                }

                Handles.color = Color.white;

                break;

            case BuildrEditMode.modes.addVolumeByPoint:

                Handles.color = Color.blue;
                for (int s = 0; s < numberOfVolumes; s++)
                {
                    BuildrVolume volume = plan.volumes[s];
                    int volumeSize = volume.Count;

                    for (int l = 0; l < volumeSize; l++)
                    {
                        int a = volume.points[l];
                        int b = (l < volume.points.Count - 1) ? volume.points[l + 1] : volume.points[0];
                        if (plan.GetConnectingVolumeIndex(s, a, b) == -1)//if the volume wall has not been connected to two volumes yet...
                        {
                            Vector3[] pointVectors = plan.GetWallVectors(s, l);
                            Vector3 pA = pointVectors[0];
                            Vector3 pB = pointVectors[1];
                            Vector3 pC = (pA + pB) / 2 + position;
                            float pointHandleSize = HandleUtility.GetHandleSize(pC);
                            if (Handles.Button(pC, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f, Handles.DotCap))
                            {
                                Vector2z[] newPoints = new Vector2z[1];
                                float pointDist = Vector3.Distance(pA, pB);
                                Vector3 newPointPos = Vector3.Cross(pA - pB, Vector3.up).normalized * pointDist;
                                newPoints[0] = new Vector2z(pC + newPointPos);
                                int indexa, indexb;
                                indexa = volume.points[l];
                                if (l < volumeSize - 1)
                                    indexb = volume.points[l + 1];
                                else
                                    indexb = volume.points[0];
                                plan.AddVolume(indexa, indexb, newPoints);

                                editMode.SetMode(BuildrEditMode.modes.floorplan);
                                break;
                            }
                        }
                    }
                }
                Handles.color = Color.white;
                break;

            case BuildrEditMode.modes.addVolumeByWall:

                Handles.color = Color.blue;
                for (int s = 0; s < numberOfVolumes; s++)
                {
                    BuildrVolume volume = plan.volumes[s];
                    int volumeSize = volume.Count;

                    for (int l = 0; l < volumeSize; l++)
                    {
                        int a = volume.points[l];
                        int b = (l < volume.points.Count - 1) ? volume.points[l + 1] : volume.points[0];
                        if (plan.GetConnectingVolumeIndex(s, a, b) == -1)
                        {
                            Vector3[] pIndexes = plan.GetWallVectors(s, l);
                            Vector3 pA = pIndexes[0];
                            Vector3 pB = pIndexes[1];
                            Vector3 pC = (pA + pB) / 2 + position;
                            float pointHandleSize = HandleUtility.GetHandleSize(pC);
                            if (Handles.Button(pC, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f, Handles.DotCap))
                            {
//                                Undo.RegisterSceneUndo("Add volume by wall");
                                Vector2z[] newPoints = new Vector2z[2];
                                float pointDist = Vector3.Distance(pA, pB);
                                Vector3 newPointPos = Vector3.Cross(pA - pB, Vector3.up).normalized * pointDist;
                                newPoints[0] = new Vector2z(pA + newPointPos);
                                newPoints[1] = new Vector2z(pB + newPointPos);
                                int indexa, indexb;
                                indexa = volume.points[l];
                                if (l < volumeSize - 1)
                                    indexb = volume.points[l + 1];
                                else
                                    indexb = volume.points[0];
                                plan.AddVolume(indexa, indexb, newPoints);

                                editMode.SetMode(BuildrEditMode.modes.floorplan);
                                break;
                            }
                        }
                    }
                }
                Handles.color = Color.white;
                break;

            case BuildrEditMode.modes.addPointToVolume:

                numberOfPoints = plan.numberOfPoints;
                if (editMode.selectedPoint == -1)
                {
                    for (int p = 0; p < numberOfPoints; p++)
                    {
                        Vector2z point = plan.points[p];
                        Handles.color = Color.white;
                        Vector3 pointPos = point.vector3 + position;
                        float pointhandleSize = HandleUtility.GetHandleSize(pointPos);
                        if (Handles.Button(pointPos, Quaternion.identity, pointhandleSize * 0.1f, pointhandleSize * 0.1f, Handles.DotCap))
                        {
//                            Undo.RegisterSceneUndo("Select Wall Point");
                            editMode.selectedPoint = p;
                            break;
                        }
                    }
                }
                else
                {

                    selectedPoint = editMode.selectedPoint;
                    Vector2z startPoint = plan.points[selectedPoint];
                    Vector2z endPoint = new Vector2z(mousePlanePoint - position);

                    bool isLegal = true;
                    for (int s = 0; s < numberOfVolumes; s++)
                    {
                        BuildrVolume volume = plan.volumes[s];
                        if (!volume.Contains(selectedPoint))
                            continue;
                        int volumeSize = volume.Count;

                        for (int l = 0; l < volumeSize; l++)
                        {
                            int a = volume.points[l];
                            int b = volume.points[(l + 1) % volumeSize];

                            if (a == selectedPoint || b == selectedPoint)
                                continue;

                            if (BuildrUtils.FastLineIntersection(startPoint, endPoint, plan.points[a], plan.points[b]))
                                isLegal = false;
                        }
                    }

                    Handles.color = isLegal ? Color.white : Color.red;
                    Handles.DrawLine(startPoint.vector3 + position, endPoint.vector3 + position);

                }

                break;

            case BuildrEditMode.modes.splitVolume:

                numberOfPoints = plan.numberOfPoints;
                selectedPoint = editMode.selectedPoint;
                if (selectedPoint == -1)
                {
                    for (int p = 0; p < numberOfPoints; p++)
                    {
                        Vector2z point = plan.points[p];
                        Handles.color = Color.white;
                        Vector3 pointPos = point.vector3 + position;
                        float pointhandleSize = HandleUtility.GetHandleSize(pointPos);

                        if (Handles.Button(pointPos, Quaternion.identity, pointhandleSize * 0.1f, pointhandleSize * 0.1f, Handles.DotCap))
                        {
//                            Undo.RegisterSceneUndo("Select Wall Point");
                            editMode.selectedPoint = p;
                            break;
                        }
                    }
                }
                else
                {

                    for (int s = 0; s < numberOfVolumes; s++)
                    {
                        BuildrVolume volume = plan.volumes[s];
                        if (!volume.Contains(selectedPoint))
                            continue;
                        int volumeSize = volume.Count;

                        for (int l = 0; l < volumeSize; l++)
                        {

                            int o = volume.points[l];
                            int a = selectedPoint;
                            int selectedVolumePoint = volume.IndexOf(selectedPoint);
                            int b = volume.points[(selectedVolumePoint + 1) % volumeSize];
                            int volb = (selectedVolumePoint - 1) % volumeSize;
                            if (volb == -1) volb = volumeSize - 1;
                            int c = volume.points[volb];

                            if (o == a || o == b || o == c)
                                continue;

                            Vector3 pointPos = plan.points[o].vector3 + position;

                            bool isLegal = true;
                            for (int j = 0; j < volumeSize; j++)
                            {
                                int ob = volume.points[j];
                                int oc = volume.points[(j + 1) % volumeSize];
                                if (ob == selectedPoint || oc == selectedPoint || ob == o || oc == o)
                                    continue;
                                if (BuildrUtils.FastLineIntersection(plan.points[selectedPoint], plan.points[o], plan.points[ob], plan.points[oc]))
                                    isLegal = false;
                            }

                            Vector2z pA = plan.points[a];
                            Vector2z pB = plan.points[b];
                            Vector2z pC = plan.points[c];
                            Vector2z pO = plan.points[o];

                            float startAng, endAng, mouseAng, ang;
                            Vector3 cross;
                            Vector2z diff;

                            diff = pC - pA;
                            ang = Vector2.Angle(Vector2.up, diff.vector2);
                            cross = Vector3.Cross(Vector3.forward, diff.vector3);
                            startAng = (cross.y > 0) ? ang : 360 - ang;

                            diff = pB - pA;
                            ang = Vector2.Angle(Vector2.up, diff.vector2);
                            cross = Vector3.Cross(Vector3.forward, diff.vector3);
                            endAng = (cross.y > 0) ? ang : 360 - ang;

                            diff = pO - pA;
                            ang = Vector2.Angle(Vector2.up, diff.vector2);
                            cross = Vector3.Cross(Vector3.forward, diff.vector3);
                            mouseAng = (cross.y > 0) ? ang : 360 - ang;

                            mouseAng = (360 + (mouseAng % 360)) % 360;
                            startAng = (3600000 + startAng) % 360;
                            endAng = (3600000 + endAng) % 360;

                            bool isBetween = false;
                            if (startAng < endAng)
                                isBetween = startAng <= mouseAng && mouseAng <= endAng;
                            else
                                isBetween = startAng <= mouseAng || mouseAng <= endAng;

                            if (isLegal && !isBetween)
                                isLegal = false;

                            if (isLegal)
                            {
                                Handles.color = Color.white;
                                float pointhandleSize = HandleUtility.GetHandleSize(pointPos);
                                if (Handles.Button(pointPos, Quaternion.identity, pointhandleSize * 0.1f, pointhandleSize * 0.1f, Handles.DotCap))
                                {
//                                    Undo.RegisterSceneUndo("Split Volume");
                                    plan.SplitVolume(s, a, o);
                                    editMode.selectedPoint = -1;
                                    editMode.SetMode(BuildrEditMode.modes.floorplan);
                                    return;
                                }
                                Handles.color = new Color(1, 0, 0, 0.25f);
                                Handles.DrawLine(plan.points[selectedPoint].vector3 + position, plan.points[o].vector3 + position);
                            }
                        }
                    }
                }

                break;

                case BuildrEditMode.modes.addNewCore:

                SceneView.focusedWindow.wantsMouseMove = true;
                Vector3 coreBasePoint = mousePlanePoint;
                Vector3 coreWidth = Vector3.right * 2.5f;
                Vector3 coreHeight = Vector3.forward * 2.5f;
                Vector3[] coreVerts = new Vector3[4] { coreBasePoint - coreWidth - coreHeight, coreBasePoint + coreWidth - coreHeight, coreBasePoint + coreWidth + coreHeight, coreBasePoint + coreHeight - coreWidth };

                Color newCoreColour = BuildrColours.RED;
                newCoreColour.a = 0.5f;
                Handles.DrawSolidRectangleWithOutline(coreVerts, newCoreColour, BuildrColours.YELLOW);
                Handles.Label(mousePlanePoint, "Click to place a new core");
                if (Handles.Button(coreBasePoint, Quaternion.identity, 0, 10, Handles.CircleCap))
                {
//                    Undo.RegisterSceneUndo("Add new core");
                    Vector3 coreBase = coreBasePoint - position;
                    Rect newCoreRect = new Rect(coreBase.x - 2.5f, coreBase.z - 2.5f, 5.0f, 5.0f);
                    plan.cores.Add(newCoreRect);
                    editMode.SetMode(BuildrEditMode.modes.floorplan);
                    EditorUtility.SetDirty(plan);
                }

                break;

                case BuildrEditMode.modes.removeCore:


                for (int c = 0; c < numberOfCores; c++)
                {
                    Rect core = plan.cores[c];
                    Vector3 centerPoint = new Vector3(core.center.x, 0, core.center.y) + position;

                    Handles.color = Color.red;
                    float centerPointHandleSize = HandleUtility.GetHandleSize(centerPoint);
                    if (Handles.Button(centerPoint, Quaternion.identity, centerPointHandleSize * 0.1f, centerPointHandleSize * 0.1f, Handles.DotCap))
                    {
//                        Undo.RegisterSceneUndo("Remove Core");
                        plan.cores.RemoveAt(c);
                        numberOfCores--;
                        c--;
                        editMode.SetMode(BuildrEditMode.modes.floorplan);
                        EditorUtility.SetDirty(plan);
                    }
                }

                break;
        }

        bool clickedOutside = false;
        if (Event.current.isMouse)
        {
            RaycastHit hitInfo;
            clickedOutside = true;
            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.collider.gameObject == editMode.gameObject)
                    clickedOutside = false;
            }
        }

        if (clickedOutside)
            editMode.selectedPoints.Clear();

        if (GUI.changed)
        {
            plan.CheckPlan();
            EditorUtility.SetDirty(editMode);
            EditorUtility.SetDirty(plan);
            editMode.UpdateRender();
        }
    }

    public static void InspectorGUI(BuildrEditMode editMode, BuildrPlan plan)
    {
        EditorGUILayout.Space();

        Undo.RecordObject(plan, "Floorplan Modified");

        editMode.showDimensionLines = EditorGUILayout.Toggle("Show Wall Dimensions", editMode.showDimensionLines);
        if (editMode.mode != BuildrEditMode.modes.floorplan)
        {
            EditorGUILayout.LabelField("Current Mode: " + editMode.mode.ToString());
            if (GUILayout.Button("Cancel"))
            {
                editMode.SetMode(BuildrEditMode.modes.floorplan);
                UpdateGUI();
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Snap to Grid", GUILayout.Width(100));
        bool editModesnapFloorplanToGrid = EditorGUILayout.Toggle(editMode.snapFloorplanToGrid);
        if(editModesnapFloorplanToGrid != editMode.snapFloorplanToGrid)
        {
            //Snapping modified
            editMode.snapFloorplanToGrid = editModesnapFloorplanToGrid;
            if(editModesnapFloorplanToGrid)
            {
                int numberOfPoints = plan.points.Count;
                for (int i = 0; i < numberOfPoints; i++)
                {
                    Vector2z point = plan.points[i];
                    Vector3 snappedPoint = point.vector3;
                    snappedPoint.x -= snappedPoint.x % editMode.floorplanGridSize;
                    snappedPoint.z -= snappedPoint.z % editMode.floorplanGridSize;
                    point.vector3 = snappedPoint;
                }
            }
        }
        EditorGUI.BeginDisabledGroup(!editMode.snapFloorplanToGrid);
        EditorGUILayout.LabelField("Grid Size", GUILayout.Width(100));
        editMode.floorplanGridSize = EditorGUILayout.FloatField(editMode.floorplanGridSize);
        EditorGUILayout.LabelField("metres", GUILayout.Width(60));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        if(GUILayout.Button("Recenter Floorplan to Origin"))
        {
            Vector3 currentCenter = Vector3.zero;
            int numberOfPoints = plan.points.Count;
            for (int i = 0; i < numberOfPoints; i++)
                currentCenter += plan.points[i].vector3;
            currentCenter *= (1.0f / numberOfPoints);
            for (int i = 0; i < numberOfPoints; i++)
                plan.points[i].vector3 += -currentCenter;
            int numberOfCores = plan.cores.Count;
            for(int i = 0; i < numberOfCores; i++)
            {
                Rect core = plan.cores[i];
                plan.cores[i] = new Rect(core.xMin - currentCenter.x, core.yMin - currentCenter.z, core.width, core.height);
            }
        }


        if (GUILayout.Button("Recenter Origin to Floorplan"))
        {
            Vector3 currentCenter = Vector3.zero;
            int numberOfPoints = plan.points.Count;
            for (int i = 0; i < numberOfPoints; i++)
                currentCenter += plan.points[i].vector3;
            currentCenter *= (1.0f / numberOfPoints);
            for (int i = 0; i < numberOfPoints; i++)
                plan.points[i].vector3 += -currentCenter;
            int numberOfCores = plan.cores.Count;
            for (int i = 0; i < numberOfCores; i++)
            {
                Rect core = plan.cores[i];
                plan.cores[i] = new Rect(core.xMin - currentCenter.x, core.yMin - currentCenter.z, core.width, core.height);
            }
            editMode.transform.position += currentCenter;
        }

        EditorGUILayout.LabelField("New Volume Plans");
        if (GUILayout.Button("Add New Volume Square"))
        {
            editMode.SetMode(BuildrEditMode.modes.addNewVolume);
            UpdateGUI();
        }

        if (GUILayout.Button("Add New Volume By Drawing Rectangle"))
        {
            editMode.SetMode(BuildrEditMode.modes.addNewVolumeByDraw);
            UpdateGUI();
        }

        if (GUILayout.Button("Add New Volume By Drawing Points"))
        {
            editMode.SetMode(BuildrEditMode.modes.addNewVolumeByPoints);
            UpdateGUI();
        }

        if (GUILayout.Button("Add New Volume By Extending Wall"))
        {
            editMode.SetMode(BuildrEditMode.modes.addVolumeByWall);
            UpdateGUI();
        }

        if (GUILayout.Button("Merge Volumes"))
        {
            editMode.SetMode(BuildrEditMode.modes.mergeVolumes);
            UpdateGUI();
        }

        if (GUILayout.Button("Split Volumes"))
        {
            editMode.SetMode(BuildrEditMode.modes.splitVolume);
            UpdateGUI();
        }

        if (GUILayout.Button("Remove Volume"))
        {
            editMode.SetMode(BuildrEditMode.modes.removeVolume);
            UpdateGUI();
        }

        EditorGUILayout.LabelField("Volume Plan Modification");
        if (GUILayout.Button("Split Wall"))
        {
            editMode.SetMode(BuildrEditMode.modes.splitwall);
            UpdateGUI();
        }

        if (GUILayout.Button("Add Point To Volume"))
        {
            editMode.SetMode(BuildrEditMode.modes.addPointToVolume);
            UpdateGUI();
        }

        if (GUILayout.Button("Remove Wall Point"))
        {
            editMode.SetMode(BuildrEditMode.modes.removewall);
            UpdateGUI();
        }

        if (GUILayout.Button("Extrude Wall"))
        {
            editMode.SetMode(BuildrEditMode.modes.extrudewallselect);
            UpdateGUI();
        }

        EditorGUILayout.LabelField("Core Modification");
        if (GUILayout.Button("Add Building Core"))
        {
            editMode.SetMode(BuildrEditMode.modes.addNewCore);
            UpdateGUI();
        }

        if (GUILayout.Button("Remove Building Core"))
        {
            editMode.SetMode(BuildrEditMode.modes.removeCore);
            UpdateGUI();
        }

    }

    private static void UpdateGUI()
    {
        HandleUtility.Repaint();
        SceneView.RepaintAll();
    }
}
