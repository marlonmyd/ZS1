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
using System.Collections.Generic;

public class BuildrInteriors
{

    private static BuildrData data;
    private static BuildrTexture[] textures;
    private static DynamicMeshGenericMultiMaterialMesh mesh;

    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data, int volumeIndex)
    {
        data = _data;
        mesh = _mesh;
        mesh.name = "Interior Mesh Volume " + volumeIndex;
        textures = data.textures.ToArray();

        if(!data.renderInteriors)
            return;

        float largestDepthValue = 0;//deepest value of a bay design in the building
        float tallestBay = 0;
        foreach (BuildrBay bay in data.bays)
        {
            largestDepthValue = Mathf.Max(largestDepthValue, bay.deepestValue);//get the deepest value
            tallestBay = Mathf.Max(tallestBay, bay.openingHeight + (data.floorHeight - bay.openingHeight) * bay.openingHeightRatio);
        }
        foreach (BuildrFacadeDesign facade in data.facades)
        {
            if(facade.type != BuildrFacadeDesign.types.simple)
                continue;
            largestDepthValue = Mathf.Max(largestDepthValue, facade.simpleBay.deepestValue);//get the deepest value
            if(facade.simpleBay.isOpening)
                tallestBay = Mathf.Max(tallestBay, facade.simpleBay.openingHeight + (data.floorHeight - facade.simpleBay.openingHeight) * facade.simpleBay.openingHeightRatio);
        }


        BuildrFacadeDesign facadeDesign = data.facades[0];
        BuildrPlan plan = data.plan;
        BuildrVolume volume = plan.volumes[volumeIndex];
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 floorHeightVector = Vector3.up * floorHeight;
        float ceilingHeight = tallestBay + (floorHeight - tallestBay) * data.interiorCeilingHeight;

        //Calculate the internal floor plan points
        int numberOfVolumePoints = volume.points.Count;
        Vector2z[] interiorVolumePoints = new Vector2z[numberOfVolumePoints];
        for (int i = 0; i < numberOfVolumePoints; i++)
        {
            Vector3 lastPoint = plan.points[volume.points[(i > 0) ? i - 1 : numberOfVolumePoints - 1]].vector3;
            Vector3 thisPoint = plan.points[volume.points[i]].vector3;
            Vector3 nextPoint = plan.points[volume.points[(i + 1) % numberOfVolumePoints]].vector3;
            Vector3 normalA = Vector3.Cross(thisPoint - lastPoint, Vector3.up).normalized;
            Vector3 normalB = Vector3.Cross(nextPoint - thisPoint, Vector3.up).normalized;

            Vector2z facadeALine = new Vector2z(thisPoint - lastPoint);
            Vector2z facadeBLine = new Vector2z(thisPoint - nextPoint);
            //Calculate facade inner origins for floors
            Vector3 facadeOriginV3A = lastPoint + normalA * largestDepthValue;
            Vector3 facadeOriginV3B = nextPoint + normalB * largestDepthValue;
            Vector2z facadeOriginA = new Vector2z(facadeOriginV3A);
            Vector2z facadeOriginB = new Vector2z(facadeOriginV3B);
            Vector2z facadeLineIntersection = BuildrUtils.FindIntersection(facadeALine, facadeOriginA, facadeBLine, facadeOriginB);

            interiorVolumePoints[i] = facadeLineIntersection;
        }
        List<Vector2z> interiorVolumePointList = new List<Vector2z>(interiorVolumePoints);
        List<Rect> volumeCores = new List<Rect>();
        List<int> linkedPoints = new List<int>();
        foreach (Rect core in plan.cores)
        {
            Vector2z coreCenter = new Vector2z(core.center);
            if (BuildrUtils.PointInsidePoly(coreCenter, interiorVolumePoints))
            {
                volumeCores.Add(core);
            }
        }
        int numberOfVolumeCores = volumeCores.Count;
        bool print = plan.volumes.IndexOf(volume) == 3;
        for (int c = 0; c < numberOfVolumeCores; c++)
        {
            int numberOfInteriorPoints = interiorVolumePointList.Count;
            Rect coreBounds = volumeCores[c];
            Vector2z coreCenter = new Vector2z(coreBounds.center);
            Vector2z coreBL = new Vector2z(coreBounds.xMin, coreBounds.yMin);
            Vector2z coreBR = new Vector2z(coreBounds.xMax, coreBounds.yMin);
            Vector2z coreTL = new Vector2z(coreBounds.xMin, coreBounds.yMax);
            Vector2z coreTR = new Vector2z(coreBounds.xMax, coreBounds.yMax);
            Vector2z[] corePointArray;
                corePointArray = new[] { coreBL, coreBR, coreTR, coreTL };
            //Find the nearest legal cut we can make to join the core and interior point poly
            int connectingPoint = -1;
            float connectingPointDistance = Mathf.Infinity;
            for (int p = 0; p < numberOfInteriorPoints; p++)
            {
                if(linkedPoints.Contains(p))
                    continue;
                Vector2z thisPoint = interiorVolumePointList[p];
                float thisPointDistance = Vector2z.SqrMag(thisPoint, coreCenter);
                if (thisPointDistance < connectingPointDistance)
                {
                    bool legalCut = true;
                    for (int pc = 0; pc < numberOfInteriorPoints; pc++)
                    {
                        Vector2z p0 = interiorVolumePointList[pc];
                        Vector2z p1 = interiorVolumePointList[(pc + 1) % numberOfInteriorPoints];
                        if (BuildrUtils.FastLineIntersection(coreCenter, thisPoint, p0, p1))//check against all lines that this new cut doesn't intersect
                        {
                            if (print)
                                Debug.Log("FLI "+pc+" "+coreCenter+" "+thisPoint+" "+p0+" "+p1);
                            legalCut = false;
                            break;
                        }
                    }
                    if (legalCut)
                    {
                        connectingPoint = p;
                        connectingPointDistance = thisPointDistance;
                    }
                }
            }
            if(connectingPoint==-1)
            {
                Debug.Log("Buildr Could not place core");
                continue;
            }
            Vector2z chosenPoint = interiorVolumePointList[connectingPoint];
            int connectingCorePoint = 0;
            float connectingCorePointDistance = Mathf.Infinity;// Vector2z.SqrMag(corePointArray[0], chosenPoint);
            for (int cp = 0; cp < 4; cp++)//find the core point to make the cut
            {
                float thisCorePointDistance = Vector2z.SqrMag(corePointArray[cp], chosenPoint);
                if (thisCorePointDistance < connectingCorePointDistance)
                {
                    connectingCorePoint = cp;
                    connectingCorePointDistance = thisCorePointDistance;
                }
            }
            interiorVolumePointList.Insert(connectingPoint, chosenPoint);//loop back on the floorplan to close it
            for (int acp = 0; acp < 5; acp++)//loop back on itself to close the core
            {
                interiorVolumePointList.Insert(connectingPoint + 1, corePointArray[(connectingCorePoint + acp) % 4]);
            }
            for(int i = 0; i < linkedPoints.Count; i++)
            {
                if (linkedPoints[i] > connectingPoint)
                    linkedPoints[i] += 7;
            }
            linkedPoints.AddRange(new[]{connectingPoint, connectingPoint + 1, connectingPoint + 2, connectingPoint + 3, connectingPoint + 4, connectingPoint + 5, connectingPoint + 6});
//            linkedPoints.AddRange(new []{connectingPoint,connectingPoint+6});
        }
//        if(linkedPoints.Count > 0)
//        Debug.Log(linkedPoints.Count+" "+linkedPoints[0]);
        Vector2z[] interiorPointListCore = interiorVolumePointList.ToArray();

        for (int f = 0; f < numberOfVolumePoints; f++)
        {
            ///WALLS

            int indexAM = Mathf.Abs((f - 1) % numberOfVolumePoints);
            int indexA = f;
            int indexB = (f + 1) % numberOfVolumePoints;
            int indexBP = (f + 2) % numberOfVolumePoints;

            Vector3 p0m = plan.points[volume.points[indexAM]].vector3;
            Vector3 p0 = plan.points[volume.points[indexA]].vector3;
            Vector3 p1 = plan.points[volume.points[indexB]].vector3;
            Vector3 p1p = plan.points[volume.points[indexBP]].vector3;
            Vector3 p0interior = interiorVolumePoints[indexA].vector3;
            Vector3 p1interior = interiorVolumePoints[indexB].vector3;

            float facadeWidth = Vector3.Distance(p0, p1) - largestDepthValue * 2.0f;
            Vector3 facadeDirection = (p1 - p0).normalized;
            Vector3 facadeCross = Vector3.Cross(facadeDirection, Vector3.up);
            Vector3 lastFacadeDirection = (p0 - p0m).normalized;
            Vector3 nextFacadeDirection = (p1p - p1).normalized;

            //only bother with facade directions when facade may intersect inverted geometry
            float facadeDirDotL = Vector3.Dot(-facadeDirection, lastFacadeDirection);
            float facadeCrossDotL = Vector3.Dot(-facadeCross, lastFacadeDirection);
            if (facadeDirDotL <= 0 || facadeCrossDotL <= 0) lastFacadeDirection = -facadeCross;

            float facadeDirDotN = Vector3.Dot(-facadeDirection, nextFacadeDirection);
            float facadeCrossDotN = Vector3.Dot(-facadeCross, nextFacadeDirection);
            if (facadeDirDotN <= 0 || facadeCrossDotN <= 0) nextFacadeDirection = facadeCross;


            int floorBase = plan.GetFacadeFloorHeight(volumeIndex, volume.points[indexA], volume.points[indexB]);
            BuildrVolumeStylesUnit[] styleUnits = volume.styles.GetContentsByFacade(volume.points[indexA]);
            int floorPatternSize = 0;
            List<int> facadePatternReference = new List<int>();//this contains a list of all the facade style indices to refence when looking for the appropriate style per floor
            int patternCount = 0;
            foreach (BuildrVolumeStylesUnit styleUnit in styleUnits)//need to knw how big all the styles are together so we can loop through them
            {
                floorPatternSize += styleUnit.floors;
                for (int i = 0; i < styleUnit.floors; i++)
                    facadePatternReference.Add(patternCount);
                patternCount++;
            }
            facadePatternReference.Reverse();

            int rows = numberOfFloors;


            Vector2 facadeUV = Vector2.zero;

            for (int r = 0; r < rows; r++)
            {
                float currentFloorHeight = floorHeight * r;
                Vector3 currentFloorHeightVector = Vector3.up * (data.floorHeight * r);
                Vector3 facadeFloorBaseVector = p0 + Vector3.up * currentFloorHeight;
                Vector3 ceilingVector = Vector3.up * ceilingHeight;
                if (r < floorBase)
                {
                    //no facade rendered
                    //facade gap filler

                    //interior gap points
                    Vector3 i0 = p1 - facadeDirection.normalized * largestDepthValue;
                    Vector3 i1 = p0 + facadeDirection.normalized * largestDepthValue;

                    Vector3 w0 = i0 + currentFloorHeightVector;
                    Vector3 w1 = i1 + currentFloorHeightVector;
                    Vector3 w2 = w0 + facadeCross * largestDepthValue;
                    Vector3 w3 = w1 + facadeCross * largestDepthValue;
                    Vector3 w4 = w0 + ceilingVector;
                    Vector3 w5 = w1 + ceilingVector;
                    Vector3 w6 = w2 + ceilingVector;
                    Vector3 w7 = w3 + ceilingVector;
                    Vector3 w8 = p1interior + currentFloorHeightVector;
                    Vector3 w9 = p0interior + currentFloorHeightVector;
                    Vector3 w10 = w8 + ceilingVector;
                    Vector3 w11 = w9 + ceilingVector;

                    //floor
                    AddData(new[] { w0, w1, w2, w3 }, new[] { 0, 1, 2, 1, 3, 2 }, volume.FloorTexture(r), false);

                    //ceiling
                    AddData(new[] { w5, w4, w7, w6 }, new[] { 0, 1, 2, 1, 3, 2 }, volume.CeilingTexture(r), false);

                    //sides
                    int wallSubmesh = volume.WallTexture(r);
                    AddPlane(w0, w2, w4, w6, wallSubmesh, false, Vector3.zero, new Vector2(largestDepthValue, floorHeight));
                    AddPlane(w3, w1, w7, w5, wallSubmesh, false, Vector3.zero, new Vector2(largestDepthValue, floorHeight));

                    //other gaps
                    float uvWidth1 = Vector3.Distance(w2, w8);
                    AddPlane(w2, w8, w6, w10, wallSubmesh, false, Vector3.zero, new Vector2(uvWidth1, floorHeight));
                    float uvWidth2 = Vector3.Distance(w3, w9);
                    AddPlane(w9, w3, w11, w7, wallSubmesh, false, Vector3.zero, new Vector2(uvWidth2, floorHeight));

                    continue;
                }

                //Get the facade style id
                //need to loop through the facade designs floor by floor until we get to the right one
                int modFloor = ((r - floorBase) % floorPatternSize);

                facadeDesign = data.facades[styleUnits[facadePatternReference[modFloor]].styleID];

                bool isBlankWall = !facadeDesign.hasWindows;
                if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                {
                    if (data.bays.Count == 0 || facadeDesign.bayPattern.Count == 0)
                    {
                        data.illegal = true;
                        return;
                    }

                    BuildrBay firstBay = data.bays[facadeDesign.bayPattern[0]];
                    if (firstBay.openingWidth > facadeWidth) isBlankWall = true;
                    if (facadeDesign.bayPattern.Count == 0) isBlankWall = true;
                }
                else
                {
                    if (facadeDesign.simpleBay.openingWidth + facadeDesign.simpleBay.minimumBayWidth > facadeWidth)
                        isBlankWall = true;
                }

                if (!isBlankWall)
                {
                    float patternSize = 0;//the space the pattern fills, there will be a gap that will be distributed to all bay styles
                    int numberOfBays = 0;
                    BuildrBay[] bayDesignPattern;
                    int numberOfBayDesigns;
                    if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                    {
                        numberOfBayDesigns = facadeDesign.bayPattern.Count;
                        bayDesignPattern = new BuildrBay[numberOfBayDesigns];
                        for (int i = 0; i < numberOfBayDesigns; i++)
                        {
                            bayDesignPattern[i] = data.bays[facadeDesign.bayPattern[i]];
                        }
                    }
                    else
                    {
                        bayDesignPattern = new[] { facadeDesign.simpleBay };
                        numberOfBayDesigns = 1;
                    }
                    //start with first window width - we'll be adding to this until we have filled the facade width
                    int it = 100;
                    while (true)
                    {
                        int patternModIndex = numberOfBays % numberOfBayDesigns;
                        float patternAddition = bayDesignPattern[patternModIndex].openingWidth + bayDesignPattern[patternModIndex].minimumBayWidth;
                        if (patternSize + patternAddition < facadeWidth)
                        {
                            patternSize += patternAddition;
                            numberOfBays++;
                        }
                        else
                            break;
                        it--;
                        if (it < 0)
                            break;
                    }

                    Vector3 windowBase = facadeFloorBaseVector;
                    facadeUV.x = 0;
                    facadeUV.y += floorHeight;
                    float perBayAdditionalSpacing = (facadeWidth - patternSize) / numberOfBays;
                    for (int c = 0; c < numberOfBays; c++)
                    {
                        BuildrBay bayStyle;
                        BuildrBay lastBay;
                        BuildrBay nextBay;
                        bool firstColumn = c == 0;
                        bool lastColumn = c == numberOfBays - 1;
                        if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                        {
                            int numberOfBayStyles = facadeDesign.bayPattern.Count;
                            bayStyle = bayDesignPattern[c % numberOfBayStyles];
                            int lastBayIndex = (c > 0) ? (c - 1) % numberOfBayStyles : 0;
                            lastBay = bayDesignPattern[lastBayIndex];
                            nextBay = bayDesignPattern[(c + 1) % numberOfBayStyles];
                        }
                        else
                        {
                            bayStyle = facadeDesign.simpleBay;
                            lastBay = facadeDesign.simpleBay;
                            nextBay = facadeDesign.simpleBay;
                        }
                        float actualWindowSpacing = bayStyle.minimumBayWidth + perBayAdditionalSpacing;
                        float leftWidth = actualWindowSpacing * bayStyle.openingWidthRatio;
                        float rightWidth = actualWindowSpacing - leftWidth;
                        float openingWidth = bayStyle.openingWidth;

                        if (firstColumn) leftWidth += largestDepthValue;
                        if (lastColumn) rightWidth += largestDepthValue;

                        BuildrTexture columnTexture = textures[bayStyle.GetTexture(BuildrBay.TextureNames.ColumnTexture)];
                        Vector2 columnuvunits = columnTexture.tileUnitUV;
                        float openingHeight = bayStyle.openingHeight;
                        if (columnTexture.patterned) openingHeight = Mathf.Ceil(bayStyle.openingHeight / columnuvunits.y) * columnuvunits.y;
                        if (bayStyle.openingHeight == floorHeight) bayStyle.openingHeight = floorHeight;

                        float rowBottomHeight = ((floorHeight - openingHeight) * bayStyle.openingHeightRatio);
                        if (columnTexture.patterned) rowBottomHeight = Mathf.Ceil(rowBottomHeight / columnuvunits.y) * columnuvunits.y;

                        float rowTopHeight = (floorHeight - rowBottomHeight - openingHeight);

                        bool previousBayIdentical = bayStyle == lastBay;
                        bool nextBayIdentical = bayStyle == nextBay;
                        if (previousBayIdentical && !firstColumn)
                            leftWidth = actualWindowSpacing;//if next design is identical - add the two parts together the reduce polycount

                        Vector3 w0, w1, w2, w3;

                        int wallSubmesh = volume.WallTexture(r);
                        bool wallFlipped = false;
                        if (!bayStyle.isOpening)
                        {

                            float bayWidthSize = openingWidth + actualWindowSpacing;
                            if (firstColumn || lastColumn) bayWidthSize += largestDepthValue;
                            Vector3 bayWidth = facadeDirection * bayWidthSize;
                            Vector3 bayHeight = Vector3.up * floorHeight;
                            Vector3 bayDepth = facadeCross * largestDepthValue;
                            w0 = windowBase + bayDepth;
                            w1 = windowBase + bayWidth + bayDepth;
                            w2 = windowBase + bayHeight + bayDepth;
                            w3 = windowBase + bayWidth + bayHeight + bayDepth;
                            Vector2 bayOpeningUVEnd = facadeUV + new Vector2(openingWidth + actualWindowSpacing, floorHeight);
                            AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, facadeUV, bayOpeningUVEnd);

                            windowBase = windowBase + bayWidth;//move base vertor to next bay
                            facadeUV.x += openingWidth + actualWindowSpacing;
                            continue;//bay filled - move onto next bay
                        }

                        var verts = new Vector3[16];
                        verts[0] = windowBase;
                        verts[1] = verts[0] + leftWidth * facadeDirection;
                        verts[2] = verts[1] + openingWidth * facadeDirection;
                        verts[3] = verts[2] + rightWidth * facadeDirection;
                        windowBase = (nextBayIdentical) ? verts[2] : verts[3];//move to next window - if next design is identical - well add the two parts together the reduce polycount
                        facadeUV.x += (nextBayIdentical) ? openingWidth : openingWidth + rightWidth;

                        Vector3 rowBottomVector = Vector3.up * rowBottomHeight;
                        verts[4] = verts[0] + rowBottomVector;
                        verts[5] = verts[1] + rowBottomVector;
                        verts[6] = verts[2] + rowBottomVector;
                        verts[7] = verts[3] + rowBottomVector;

                        Vector3 openingVector = Vector3.up * openingHeight;
                        verts[8] = verts[4] + openingVector;
                        verts[9] = verts[5] + openingVector;
                        verts[10] = verts[6] + openingVector;
                        verts[11] = verts[7] + openingVector;

                        Vector3 rowTopVector = Vector3.up * rowTopHeight;
                        verts[12] = verts[8] + rowTopVector;
                        verts[13] = verts[9] + rowTopVector;
                        verts[14] = verts[10] + rowTopVector;
                        verts[15] = verts[11] + rowTopVector;

                        //Realign facade end points
                        if (firstColumn)
                        {
                            verts[0] = p0interior - facadeCross * largestDepthValue + currentFloorHeightVector;
                            verts[4] = verts[0] + rowBottomVector;
                            verts[8] = verts[4] + openingVector;
                            verts[12] = verts[8] + rowTopVector;
                        }

                        if (lastColumn)
                        {
                            verts[3] = p1interior - facadeCross * largestDepthValue + currentFloorHeightVector;
                            verts[7] = verts[3] + rowBottomVector;
                            verts[11] = verts[7] + openingVector;
                            verts[15] = verts[11] + rowTopVector;
                        }

                        Vector3 openingDepthVector = facadeCross * bayStyle.openingDepth;
                        Vector3 wallDepthVecotr = facadeCross * largestDepthValue;

                        ///WINDOWS
                        int windowSubmesh = bayStyle.GetTexture(BuildrBay.TextureNames.OpeningBackTexture); 
                        bool windowFlipped = bayStyle.IsFlipped(BuildrBay.TextureNames.OpeningBackTexture);
                        w0 = verts[10] + openingDepthVector;
                        w1 = verts[9] + openingDepthVector;
                        w2 = verts[6] + openingDepthVector;
                        w3 = verts[5] + openingDepthVector;
                        Vector2 windowUVStart = new Vector2(0, 0);
                        Vector2 windowUVEnd = new Vector2(openingWidth, openingHeight);
                        if (bayStyle.renderBack && !data.cullBays)
                            AddPlane(w0, w1, w2, w3, windowSubmesh, windowFlipped, windowUVStart, windowUVEnd);

                        ///COLUMNS
                        //Column Face
                        if (leftWidth > 0)//Column Face Left
                        {
                            w0 = verts[4] + wallDepthVecotr;
                            w1 = verts[5] + wallDepthVecotr;
                            w2 = verts[8] + wallDepthVecotr;
                            w3 = verts[9] + wallDepthVecotr;
                            Vector2 leftColumnUVStart = facadeUV + new Vector2(0, rowBottomHeight);
                            Vector2 leftColumnUVEnd = leftColumnUVStart + new Vector2(leftWidth, openingHeight);
                            AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, leftColumnUVStart, leftColumnUVEnd);
                        }
                        if ((!nextBayIdentical || lastColumn) && rightWidth > 0)//Column Right
                        {
                            w0 = verts[6] + wallDepthVecotr;
                            w1 = verts[7] + wallDepthVecotr;
                            w2 = verts[10] + wallDepthVecotr;
                            w3 = verts[11] + wallDepthVecotr;
                            Vector2 rightColumnUVStart = facadeUV + new Vector2(leftWidth + openingWidth, rowBottomHeight);
                            Vector2 rightColumnUVEnd = rightColumnUVStart + new Vector2(rightWidth, openingHeight);
                            AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, rightColumnUVStart, rightColumnUVEnd);
                        }
                        ///ROWS
                        //Row Bottom
                        if (rowBottomHeight > 0)
                        {
                            w0 = verts[1] + wallDepthVecotr;
                            w1 = verts[2] + wallDepthVecotr;
                            w2 = verts[5] + wallDepthVecotr;
                            w3 = verts[6] + wallDepthVecotr;
                            Vector2 bottomRowUVStart = facadeUV + new Vector2(leftWidth, 0);
                            Vector2 bottomRowUVEnd = bottomRowUVStart + new Vector2(openingWidth, rowBottomHeight);
                            AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, bottomRowUVStart, bottomRowUVEnd);
                        }
                        //Row Top
                        if (rowTopHeight > 0)
                        {
                            w0 = verts[9] + wallDepthVecotr;
                            w1 = verts[10] + wallDepthVecotr;
                            w2 = verts[13] + wallDepthVecotr;
                            w3 = verts[14] + wallDepthVecotr;
                            Vector2 topRowUVStart = facadeUV + new Vector2(leftWidth, rowBottomHeight + openingHeight);
                            Vector2 topRowUVEnd = topRowUVStart + new Vector2(openingWidth, rowTopHeight);
                            AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, topRowUVStart, topRowUVEnd);
                        }

                        //Cross Left Bottom
                        w0 = verts[0] + wallDepthVecotr;
                        w1 = verts[1] + wallDepthVecotr;
                        w2 = verts[4] + wallDepthVecotr;
                        w3 = verts[5] + wallDepthVecotr;
                        Vector2 crossLBUVStart = facadeUV + new Vector2(0, 0);
                        Vector2 crossLBUVEnd = crossLBUVStart + new Vector2(leftWidth, rowBottomHeight);
                        AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, crossLBUVStart, crossLBUVEnd);

                        //Cross Left Top
                        w0 = verts[8] + wallDepthVecotr;
                        w1 = verts[9] + wallDepthVecotr;
                        w2 = verts[12] + wallDepthVecotr;
                        w3 = verts[13] + wallDepthVecotr;
                        Vector2 crossLTUVStart = facadeUV + new Vector2(0, rowBottomHeight + openingHeight);
                        Vector2 crossLTUVEnd = crossLTUVStart + new Vector2(leftWidth, rowTopHeight);
                        AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, crossLTUVStart, crossLTUVEnd);

                        if ((!nextBayIdentical || lastColumn) && rightWidth > 0)
                        {
                            //Cross Right Bottom
                            w0 = verts[2] + wallDepthVecotr;
                            w1 = verts[3] + wallDepthVecotr;
                            w2 = verts[6] + wallDepthVecotr;
                            w3 = verts[7] + wallDepthVecotr;
                            Vector2 crossRBUVStart = facadeUV + new Vector2(leftWidth + openingWidth, 0);
                            Vector2 crossRBUVEnd = crossRBUVStart + new Vector2(rightWidth, rowBottomHeight);
                            AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, crossRBUVStart, crossRBUVEnd);

                            //Cross Right Top
                            w0 = verts[10] + wallDepthVecotr;
                            w1 = verts[11] + wallDepthVecotr;
                            w2 = verts[14] + wallDepthVecotr;
                            w3 = verts[15] + wallDepthVecotr;
                            Vector2 crossRTUVStart = facadeUV + new Vector2(leftWidth + openingWidth, rowBottomHeight + openingHeight);
                            Vector2 crossRTUVEnd = crossRTUVStart + new Vector2(rightWidth, rowTopHeight);
                            AddPlane(w0, w1, w2, w3, wallSubmesh, wallFlipped, crossRTUVStart, crossRTUVEnd);
                        }
                    }
                }
                else
                {
                    // windowless wall
                    Vector3 interiorStart = p0interior + currentFloorHeightVector;
                    Vector3 interiorEnd = p1interior + currentFloorHeightVector;
                    //                        Vector3 wallVector = (facadeDirection * facadeWidth);
                    Vector3 wallHeightVector = Vector3.up * floorHeight;
                    Vector3 w0 = interiorStart;
                    Vector3 w1 = interiorEnd;
                    Vector3 w2 = interiorStart + wallHeightVector;
                    Vector3 w3 = interiorEnd + wallHeightVector;
                    BuildrTexture texture = textures[facadeDesign.simpleBay.GetTexture(BuildrBay.TextureNames.WallTexture)];
                    var uvSize = new Vector2(facadeWidth * (1.0f / texture.textureUnitSize.x), floorHeight * (1.0f / texture.textureUnitSize.y));
                    Vector2 uvunits = texture.tileUnitUV;
                    uvSize.x = Mathf.Ceil(uvSize.x / uvunits.x) * uvunits.x;
                    uvSize.y = Mathf.Ceil(uvSize.y / uvunits.y) * uvunits.y;
                    int wallSubmesh = 0;
                    bool flipped = false;
                    Vector2 wallUVStart = facadeUV;
                    Vector2 wallUVEnd = facadeUV + new Vector2(facadeWidth, floorHeight);
                    AddPlane(w0, w1, w2, w3, wallSubmesh, flipped, wallUVStart, wallUVEnd);

                }
            }
        }

        ///FLOORS AND CEILING
        int numberOfBasements = volume.numberOfBasementFloors;
        int numberOfFloorPoints = interiorVolumePoints.Length;
        int[] baseFloorPlanTriangles = EarClipper.Triangulate(interiorVolumePoints);
        int baseFloorVectors = interiorVolumePoints.Length;
        var newEndVerts = new Vector3[baseFloorVectors];
        Vector3 basementBaseDrop = -floorHeightVector * numberOfBasements;
        for (int i = 0; i < baseFloorVectors; i++)
            newEndVerts[i] = interiorVolumePoints[i].vector3 + basementBaseDrop;
        var tris = new List<int>(baseFloorPlanTriangles);

        //Bottom Floor
        int floorSubmesh = volume.FloorTexture(-numberOfBasements);
        AddData(newEndVerts, baseFloorPlanTriangles, floorSubmesh, false);

        //Top Ceiling
        if (true)//Todo: add conditional for roof opening
        {
            Vector3 ceilingHeightVector = floorHeightVector * (numberOfFloors - 1 + numberOfBasements) + Vector3.up * ceilingHeight;
            for (int i = 0; i < baseFloorVectors; i++)
                newEndVerts[i] += ceilingHeightVector;
            tris.Reverse(); 
            AddData(newEndVerts, tris.ToArray(), volume.CeilingTexture(numberOfFloors-1), false);
        }

        //inner floors
        int[] floorPlanTriangles = EarClipper.Triangulate(interiorPointListCore);
        int numberOfFloorVectors = interiorPointListCore.Length;
        for (int floorIndex = -numberOfBasements; floorIndex < numberOfFloors; floorIndex++)
        {
            Vector3 floorVectorHeight = floorHeightVector * floorIndex;
            newEndVerts = new Vector3[numberOfFloorVectors];
            for (int i = 0; i < numberOfFloorVectors; i++)
            {
                newEndVerts[i] = interiorPointListCore[i].vector3 + floorVectorHeight;
            }
            tris = new List<int>(floorPlanTriangles);

            //Floor
            if (floorIndex > -numberOfBasements)
                AddData(newEndVerts, tris.ToArray(), volume.FloorTexture(floorIndex), false);

            //Ceiling
            if (floorIndex < numberOfFloors - 1)
            {
                Vector3 ceilingHeightVector = Vector3.up * ceilingHeight;
                for (int i = 0; i < numberOfFloorVectors; i++)
                {
                    newEndVerts[i] += ceilingHeightVector;
                }
                tris.Reverse();
                AddData(newEndVerts, tris.ToArray(), volume.CeilingTexture(floorIndex), false);
            }

            //basement walls
            if(floorIndex < 0)
            {
                for (int f = 0; f < numberOfFloorPoints; f++)
                {
                    Vector3 basementVector = floorHeightVector * floorIndex;
                    int indexA = f;
                    int indexB = (f + 1) % numberOfFloorPoints;

                    Vector3 p0 = interiorVolumePoints[indexA].vector3 + basementVector;
                    Vector3 p1 = interiorVolumePoints[indexB].vector3 + basementVector;
                    Vector3 p2 = p0 + floorHeightVector;
                    Vector3 p3 = p1 + floorHeightVector;
                    Vector2 uv1 = new Vector2(Vector3.Distance(p0,p1), floorHeight);
                    AddPlane(p0, p1, p2, p3, volume.WallTexture(floorIndex), false, Vector2.zero, uv1);
                }
            }
        }

        //Core walls
        for (int c = 0; c < numberOfVolumeCores; c++)
        {
            Rect coreBounds = volumeCores[c];
            Vector3 coreBL = new Vector3(coreBounds.xMin, 0, coreBounds.yMin);
            Vector3 coreBR = new Vector3(coreBounds.xMax, 0, coreBounds.yMin);
            Vector3 coreTL = new Vector3(coreBounds.xMin, 0, coreBounds.yMax);
            Vector3 coreTR = new Vector3(coreBounds.xMax, 0, coreBounds.yMax);

            for (int floorIndex = -numberOfBasements; floorIndex < numberOfFloors - 1; floorIndex++)
            {
                Vector3 c0 = floorHeightVector * floorIndex + Vector3.up * ceilingHeight;
                Vector3 f0 = floorHeightVector * floorIndex + Vector3.up * floorHeight;
                float gapHeight = floorHeight - ceilingHeight;
                AddPlane(coreBL + c0, coreBR + c0, coreBL + f0, coreBR + f0, 0, false, Vector2.zero, new Vector2(coreBounds.width, gapHeight));
                AddPlane(coreBR + c0, coreTR + c0, coreBR + f0, coreTR + f0, 0, false, Vector2.zero, new Vector2(coreBounds.width, gapHeight));
                AddPlane(coreTR + c0, coreTL + c0, coreTR + f0, coreTL + f0, 0, false, Vector2.zero, new Vector2(coreBounds.width, gapHeight));
                AddPlane(coreTL + c0, coreBL + c0, coreTL + f0, coreBL + f0, 0, false, Vector2.zero, new Vector2(coreBounds.width, gapHeight));
            }
        }
    }

    private static void AddPlane(Vector3 w0, Vector3 w1, Vector3 w2, Vector3 w3, int subMesh, bool flipped, Vector2 facadeUVStart, Vector2 facadeUVEnd)
    {
        int textureSubmesh = subMesh;
        BuildrTexture texture = textures[textureSubmesh];
        Vector2 uvStart = facadeUVStart;
        Vector2 uvEnd = facadeUVEnd;

        if (texture.tiled)
        {
            uvStart = new Vector2(facadeUVStart.x * (1.0f / texture.textureUnitSize.x), facadeUVStart.y * (1.0f / texture.textureUnitSize.y));
            uvEnd = new Vector2(facadeUVEnd.x * (1.0f / texture.textureUnitSize.x), facadeUVEnd.y * (1.0f / texture.textureUnitSize.y));
            if (texture.patterned)
            {
                Vector2 uvunits = texture.tileUnitUV;
                uvStart.x = Mathf.Max(Mathf.Floor(uvStart.x / uvunits.x), 0) * uvunits.x;
                uvStart.y = Mathf.Max(Mathf.Floor(uvStart.y / uvunits.y), 0) * uvunits.y;
                uvEnd.x = Mathf.Max(Mathf.Ceil(uvEnd.x / uvunits.x), 1) * uvunits.x;
                uvEnd.y = Mathf.Max(Mathf.Ceil(uvEnd.y / uvunits.y), 1) * uvunits.y;
            }
        }
        else
        {
            uvStart = Vector2.zero;
            uvEnd.x = texture.tiledX;
            uvEnd.y = texture.tiledY;
        }

        if (!flipped)
            mesh.AddPlane(w2, w3, w0, w1, uvStart, uvEnd, textureSubmesh);
        else
        {
            uvStart = new Vector2(uvStart.y, uvStart.x);
            uvEnd = new Vector2(uvEnd.y, uvEnd.x);
            mesh.AddPlane(w3, w1, w2, w0, uvStart, uvEnd, textureSubmesh);
        }
    }

    private static void AddData(Vector3[] verts, Vector2[] uvs, int[] tris, int subMesh, bool flipped)
    {
        int textureSubmesh = subMesh;
        BuildrTexture texture = textures[textureSubmesh];
        Vector2 uvScale = Vector2.one;
        
        if (texture.tiled)
        {
            uvScale.x = (1.0f / texture.textureUnitSize.x);
            uvScale.y = (1.0f / texture.textureUnitSize.y);
            if (texture.patterned)
            {
                Vector2 uvunits = texture.tileUnitUV;
                uvScale.x = Mathf.Max(Mathf.Floor(uvScale.x / uvunits.x), 0) * uvunits.x;
                uvScale.y = Mathf.Max(Mathf.Floor(uvScale.y / uvunits.y), 0) * uvunits.y;
            }
        }

        int numberOfUVs = uvs.Length;
        for(int i = 0; i < numberOfUVs; i++)
        {
            uvs[i].Scale(uvScale);
            if(flipped)
            {
                Vector2 flippedUV = new Vector2(uvs[i].y, uvs[i].x);
                uvs[i] = flippedUV;
            }
        }

        mesh.AddData(verts, uvs, tris, textureSubmesh);
    }

    private static void AddData(Vector3[] verts, int[] tris, int subMesh, bool flipped)
    {
        int textureSubmesh = subMesh;
        BuildrTexture texture = textures[textureSubmesh];
        Vector2 uvScale = Vector2.one;

        if (texture.tiled)
        {
            uvScale.x = (1.0f / texture.textureUnitSize.x);
            uvScale.y = (1.0f / texture.textureUnitSize.y);
            if (texture.patterned)
            {
                Vector2 uvunits = texture.tileUnitUV;
                uvScale.x = Mathf.Max(Mathf.Floor(uvScale.x / uvunits.x), 0) * uvunits.x;
                uvScale.y = Mathf.Max(Mathf.Floor(uvScale.y / uvunits.y), 0) * uvunits.y;
            }
        }

        int numberOfVerts = verts.Length;
        Vector2[] uvs = new Vector2[numberOfVerts];
        for (int i = 0; i < numberOfVerts; i++)
        {
            uvs[i] = new Vector2(verts[i].x*uvScale.x, verts[i].z*uvScale.y);
            if (flipped)
            {
                Vector2 flippedUV = new Vector2(uvs[i].y, uvs[i].x);
                uvs[i] = flippedUV;
            }
        }

        mesh.AddData(verts, uvs, tris, textureSubmesh);
    }
}
