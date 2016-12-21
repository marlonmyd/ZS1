// BuildR
// Available on the Unity3D Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;

public class BuildrBuilding
{
    private static BuildrData data;
    private static BuildrTexture[] textures;
    private static DynamicMeshGenericMultiMaterialMesh mesh;


    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data)
    {
        int numberOfFacades = _data.plan.numberOfFacades;
        Rect[] dummyUVConstraints = new Rect[numberOfFacades];
        for (int i = 0; i < numberOfFacades; i++)
            dummyUVConstraints[i] = new Rect(0,0,1,1);
        Build(_mesh, _data, dummyUVConstraints);
    }

    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data, Rect[] uvConstraints)
    {
        data = _data;
        mesh = _mesh;
        textures = data.textures.ToArray();
        BuildrFacadeDesign facadeDesign = data.facades[0];
        BuildrPlan plan = data.plan;
        int numberOfVolumes = plan.numberOfVolumes;

        if(plan.numberOfFacades != uvConstraints.Length)
            Debug.LogError("Incompatible amount of uv constraints " + plan.numberOfFacades +" uvc: "+ uvConstraints.Length);

        float largestDepthValue = 0;//deepest value of a bay design in the building
        foreach (BuildrBay bay in data.bays)
            largestDepthValue = Mathf.Max(largestDepthValue, bay.deepestValue);//get the deepest value
        foreach (BuildrFacadeDesign facade in data.facades)
            largestDepthValue = Mathf.Max(largestDepthValue, facade.simpleBay.deepestValue);//get the deepest value

        int facadeCount = 0;
        for (int v = 0; v < numberOfVolumes; v++)
        {
            BuildrVolume volume = plan.volumes[v];
            int numberOfVolumePoints = volume.points.Count;
            for (int f = 0; f < numberOfVolumePoints; f++)
            {
                if (!volume.renderFacade[f])
                    continue;

                int indexAM = Mathf.Abs((f - 1) % numberOfVolumePoints);
                int indexA = f;
                int indexB = (f + 1) % numberOfVolumePoints;
                int indexBP = (f + 2) % numberOfVolumePoints;
                Vector3 p0m = plan.points[volume.points[indexAM]].vector3;
                Vector3 p0 = plan.points[volume.points[indexA]].vector3;
                Vector3 p1 = plan.points[volume.points[indexB]].vector3;
                Vector3 p1p = plan.points[volume.points[indexBP]].vector3;

                facadeCount++;

                float realFadeWidth = Vector3.Distance(p0, p1);
                float facadeWidth = realFadeWidth - largestDepthValue * 2.0f;
                Vector3 facadeDirection = (p1 - p0).normalized;
                Vector3 facadeCross = Vector3.Cross(facadeDirection, Vector3.up).normalized;
                Vector3 lastFacadeDirection = (p0 - p0m).normalized;
                Vector3 nextFacadeDirection = (p1p - p1).normalized;

                //only bother with facade directions when facade may intersect inverted geometry
                float facadeDirDotL = Vector3.Dot(-facadeDirection, lastFacadeDirection);
                float facadeCrossDotL = Vector3.Dot(-facadeCross, lastFacadeDirection);
                if (facadeDirDotL <= 0 || facadeCrossDotL <= 0) lastFacadeDirection = -facadeCross;

                float facadeDirDotN = Vector3.Dot(-facadeDirection, nextFacadeDirection);
                float facadeCrossDotN = Vector3.Dot(-facadeCross, nextFacadeDirection);
                if (facadeDirDotN <= 0 || facadeCrossDotN <= 0) nextFacadeDirection = facadeCross;


                int floorBase = plan.GetFacadeFloorHeight(v, volume.points[indexA], volume.points[indexB]);
                int numberOfFloors = volume.numberOfFloors - floorBase;
                if (numberOfFloors < 1)
                {
                    //no facade - adjacent facade is taller and covers this one
                    continue;
                }
                float floorHeight = data.floorHeight;
                Vector3 floorHeightStart = Vector3.up * (floorBase * floorHeight);
                p0 += floorHeightStart;
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

                float foundationHeight = data.foundationHeight;
                if (foundationHeight > 0 && floorBase == 0)
                {
                    int subMesh = _data.foundationTexture;
                    Vector3 foundationVector = Vector3.up * -foundationHeight;
                    Vector3 w0 = p0 + foundationVector;
                    Vector3 w1 = p1 + foundationVector;
                    Vector3 w2 = p0;
                    Vector3 w3 = p1;
                    AddPlane(w0, w1, w2, w3, subMesh, false, new Vector2(0, -foundationHeight), new Vector2(facadeWidth, 0));
                }

                Vector2 facadeUV = Vector2.zero;

                for (int r = 0; r < rows; r++)
                {
                    bool firstRow = r == 0;
                    bool lastRow = r == (rows - 1);
                    //Get the facade style id
                    //need to loop through the facade designs floor by floor until we get to the right one
                    float currentHeight = floorHeight * r;
                    Vector3 facadeFloorBaseVector = p0 + Vector3.up * currentHeight;
                    int modFloor = (r % floorPatternSize);
                    int modFloorPlus = ((r + 1) % floorPatternSize);
                    int modFloorMinus = (r > 0) ? ((r - 1) % floorPatternSize) : 0;
                    BuildrFacadeDesign lastFacadeDesign = null;
                    BuildrFacadeDesign nextFacadeDesign = null;

                    facadeDesign = data.facades[styleUnits[facadePatternReference[modFloor]].styleID];
                    nextFacadeDesign = data.facades[styleUnits[facadePatternReference[modFloorPlus]].styleID];
                    lastFacadeDesign = data.facades[styleUnits[facadePatternReference[modFloorMinus]].styleID];

                    bool isBlankWall = !facadeDesign.hasWindows;
                    if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                    {
                        if(data.bays.Count == 0 || facadeDesign.bayPattern.Count == 0)
                            isBlankWall = true;
                        else
                        {
                            BuildrBay firstBay = data.bays[facadeDesign.bayPattern[0]];
                            if (firstBay.openingWidth > facadeWidth) isBlankWall = true;
                            if (facadeDesign.bayPattern.Count == 0) isBlankWall = true;
                        }
                    }
                    else
                    {
                        if (facadeDesign.simpleBay.openingWidth + facadeDesign.simpleBay.minimumBayWidth > facadeWidth)
                            isBlankWall = true;
                    }

                    Vector3 windowBase = facadeFloorBaseVector;
                    facadeUV.x = 0;
                    facadeUV.y += currentHeight;
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

                            //                            float openingHeight = bayStyle.openingHeight;
                            BuildrTexture columnTexture = textures[bayStyle.GetTexture(BuildrBay.TextureNames.ColumnTexture)];
                            Vector2 columnuvunits = columnTexture.tileUnitUV;
                            float openingHeight = bayStyle.openingHeight;
                            if (columnTexture.patterned) openingHeight = Mathf.Round(bayStyle.openingHeight / columnuvunits.y) * columnuvunits.y;
                            //if (bayStyle.openingHeight == floorHeight) bayStyle.openingHeight = floorHeight;

                            float rowBottomHeight = ((floorHeight - openingHeight) * bayStyle.openingHeightRatio);
                            if (columnTexture.patterned) rowBottomHeight = Mathf.Ceil(rowBottomHeight / columnuvunits.y) * columnuvunits.y;

                            float rowTopHeight = (floorHeight - rowBottomHeight - openingHeight);

                            bool previousBayIdentical = bayStyle == lastBay;
                            bool nextBayIdentical = bayStyle == nextBay;
                            if (previousBayIdentical && !firstColumn)
                                leftWidth = actualWindowSpacing;//if next design is identical - add the two parts together the reduce polycount

                            Vector3 w0, w1, w2, w3;
                            float windowSideDepth, bottomDepth;

                            if (!bayStyle.isOpening)
                            {
                                int wallSubMesh = bayStyle.GetTexture(BuildrBay.TextureNames.WallTexture);
                                bool wallFlipped = bayStyle.IsFlipped(BuildrBay.TextureNames.WallTexture);
                                float bayWidthSize = openingWidth + actualWindowSpacing;
                                if (firstColumn || lastColumn) bayWidthSize += largestDepthValue;
                                Vector3 bayWidth = facadeDirection * bayWidthSize;
                                Vector3 bayHeight = Vector3.up * floorHeight;
                                Vector3 bayDepth = facadeCross * largestDepthValue;
                                w0 = windowBase;
                                w1 = windowBase + bayWidth;
                                w2 = windowBase + bayHeight;
                                w3 = windowBase + bayWidth + bayHeight;
                                Vector2 bayOpeningUVEnd = facadeUV + new Vector2(openingWidth + actualWindowSpacing, floorHeight);
                                AddPlane(w0, w1, w2, w3, wallSubMesh, wallFlipped, facadeUV, bayOpeningUVEnd);

                                Vector2 UVEnd = new Vector2(1, floorHeight);
                                if (!previousBayIdentical && !firstColumn)//left
                                {
                                    Vector3 wA = w0 + bayDepth;
                                    Vector3 wB = w2 + bayDepth;
                                    AddPlane(w2, wB, w0, wA, wallSubMesh, wallFlipped, Vector2.zero, UVEnd);
                                }

                                if (!nextBayIdentical && !lastColumn)//right
                                {
                                    Vector3 wA = w1 + bayDepth;
                                    Vector3 wB = w3 + bayDepth;
                                    AddPlane(w1, wA, w3, wB, wallSubMesh, wallFlipped, Vector2.zero, UVEnd);
                                }

                                if (lastFacadeDesign != facadeDesign && !firstRow)//bottom
                                {
                                    Vector3 wA = w0 + ((!firstColumn) ? facadeCross*largestDepthValue : -lastFacadeDirection*largestDepthValue);
                                    Vector3 wB = w1 + ((!lastColumn) ? facadeCross*largestDepthValue : nextFacadeDirection*largestDepthValue);
                                    AddPlane(w0, wA, w1, wB, wallSubMesh, wallFlipped, Vector2.zero, UVEnd);
                                }
                                if (nextFacadeDesign != facadeDesign && !lastRow)//top
                                {
                                    Vector3 wA = w2 + ((!firstColumn) ? facadeCross*largestDepthValue : -lastFacadeDirection*largestDepthValue);
                                    Vector3 wB = w3 + ((!lastColumn) ? facadeCross*largestDepthValue : nextFacadeDirection*largestDepthValue);
                                    AddPlane(w3, wB, w2, wA, wallSubMesh, wallFlipped, Vector2.zero, UVEnd);
                                }

                                windowBase = w1;//move base vertor to next bay
                                facadeUV.x += bayWidthSize;
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

                            Vector3 openingDepthVector = facadeCross * bayStyle.openingDepth;
                            Vector3 crossDepthVector = facadeCross * bayStyle.crossDepth;
                            Vector3 rowDepthVector = facadeCross * bayStyle.rowDepth;
                            Vector3 columnDepthVector = facadeCross * bayStyle.columnDepth;
                            Vector3 largestDepthVector = facadeCross * largestDepthValue;
                            Vector2 uvStart, uvEnd;

                            int windowSubMesh = bayStyle.GetTexture(BuildrBay.TextureNames.OpeningBackTexture);
                            int submeshBottom = bayStyle.GetTexture(BuildrBay.TextureNames.OpeningSillTexture);
                            int submeshTop = bayStyle.GetTexture(BuildrBay.TextureNames.OpeningCeilingTexture);
                            int columnSubMesh = bayStyle.GetTexture(BuildrBay.TextureNames.ColumnTexture);
                            int crossSubMesh = bayStyle.GetTexture(BuildrBay.TextureNames.CrossTexture);
                            int rowSubMesh = bayStyle.GetTexture(BuildrBay.TextureNames.RowTexture);
                            bool windowFlipped = bayStyle.IsFlipped(BuildrBay.TextureNames.OpeningBackTexture);
                            bool flippedBottom = bayStyle.IsFlipped(BuildrBay.TextureNames.OpeningSillTexture);
                            bool flippedTop = bayStyle.IsFlipped(BuildrBay.TextureNames.OpeningCeilingTexture);
                            bool columnFlipped = bayStyle.IsFlipped(BuildrBay.TextureNames.ColumnTexture);
                            bool crossFlipped = bayStyle.IsFlipped(BuildrBay.TextureNames.CrossTexture);
                            bool rowFlipped = bayStyle.IsFlipped(BuildrBay.TextureNames.RowTexture);
                            int windowBoxSubmesh = bayStyle.GetTexture(BuildrBay.TextureNames.OpeningSideTexture);
                            bool windowBoxFlipped = bayStyle.IsFlipped(BuildrBay.TextureNames.OpeningSideTexture);

                            ///WINDOWS
                            w0 = verts[5] + openingDepthVector;
                            w1 = verts[6] + openingDepthVector;
                            w2 = verts[9] + openingDepthVector;
                            w3 = verts[10] + openingDepthVector;
                            Vector2 windowUVStart = facadeUV + new Vector2(leftWidth, rowBottomHeight);
                            Vector2 windowUVEnd = windowUVStart + new Vector2(openingWidth, openingHeight);
                            if (bayStyle.renderBack && !data.cullBays)
                                AddPlane(w0, w1, w2, w3, windowSubMesh, windowFlipped, windowUVStart, windowUVEnd);

                            //Window Sides
                            w0 = verts[5] + largestDepthVector;
                            w1 = verts[6] + largestDepthVector;
                            w2 = verts[9] + largestDepthVector;
                            w3 = verts[10] + largestDepthVector;
                            windowSideDepth = Mathf.Min(bayStyle.columnDepth, bayStyle.openingDepth) - largestDepthValue;//Window Left
                            float columnDiff = bayStyle.columnDepth - bayStyle.openingDepth;
                            if (data.renderInteriors)//Inner Window Walls
                            {
                                //left
                                uvStart = facadeUV + new Vector2(leftWidth, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(windowSideDepth, openingHeight);
                                Vector3 leftDepthVector = facadeCross * windowSideDepth;
                                if (firstColumn) leftDepthVector = facadeCross * -largestDepthValue;
                                Vector3 wl0 = w0 + leftDepthVector;
                                Vector3 wl2 = w2 + leftDepthVector;
                                AddPlane(wl0, w0, wl2, w2, windowBoxSubmesh, windowBoxFlipped, uvStart, uvEnd);
                                //right
                                uvStart = facadeUV + new Vector2(leftWidth + openingWidth - windowSideDepth, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(windowSideDepth, openingHeight);
                                Vector3 rightDepthVector = facadeCross * windowSideDepth;
                                if (lastColumn) rightDepthVector = facadeCross * -largestDepthValue;
                                Vector3 wr1 = w1 + rightDepthVector;
                                Vector3 wr3 = w3 + rightDepthVector;
                                AddPlane(wr3, w3, wr1, w1, windowBoxSubmesh, windowBoxFlipped, uvEnd, uvStart);
                            }

                            if (columnDiff > 0 || !data.renderInteriors)//External Window Sides
                            {
                                //left
                                uvStart = facadeUV + new Vector2(leftWidth, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(columnDiff, openingHeight);
                                Vector3 sideDepthVectorA = facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                Vector3 sideDepthVectorB = facadeCross * (bayStyle.openingDepth - largestDepthValue);
                                if (firstColumn) sideDepthVectorA = facadeCross * -largestDepthValue;
                                Vector3 wd0l = w0 + sideDepthVectorA;
                                Vector3 wd1l = w2 + sideDepthVectorA;
                                Vector3 wd2l = w0 + sideDepthVectorB;
                                Vector3 wd3l = w2 + sideDepthVectorB;
                                AddPlane(wd0l, wd2l, wd1l, wd3l, windowBoxSubmesh, windowBoxFlipped, uvStart, uvEnd);
                                //      right                          
                                uvStart = facadeUV + new Vector2(leftWidth + openingWidth - windowSideDepth, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(windowSideDepth, openingHeight);
                                sideDepthVectorA = facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                sideDepthVectorB = facadeCross * (bayStyle.openingDepth - largestDepthValue);
                                if (lastColumn) sideDepthVectorA = facadeCross * -largestDepthValue;
                                Vector3 wd0r = w1 + sideDepthVectorA;
                                Vector3 wd1r = w3 + sideDepthVectorA;
                                Vector3 wd2r = w1 + sideDepthVectorB;
                                Vector3 wd3r = w3 + sideDepthVectorB;
                                AddPlane(wd1r, wd3r, wd0r, wd2r, windowBoxSubmesh, windowBoxFlipped, uvStart, uvEnd);
                            }

                            //Window Row Sides/Sills
                            bottomDepth = Mathf.Min(bayStyle.rowDepth, bayStyle.openingDepth) - largestDepthValue;
                            float rowDiff = bayStyle.rowDepth - bayStyle.openingDepth;
                            if (data.renderInteriors)//Window Sill Interiors
                            {
                                uvStart = new Vector2(leftWidth, 0);
                                uvEnd = uvStart + new Vector2(openingWidth, bottomDepth);

//                                if (rowBottomHeight > 0)//Bottom
//                                {
                                    Vector3 bottomDepthVector = facadeCross * bottomDepth;
                                    Vector3 wl0 = w0 + bottomDepthVector;
                                    Vector3 wl1 = w1 + bottomDepthVector;
                                    AddPlane(wl0, wl1, w0, w1, submeshBottom, flippedBottom, uvStart, uvEnd);
//                                }
//                                if (rowTopHeight > 0)//Top
//                                {
                                    Vector3 topDepthVector = facadeCross * bottomDepth;
                                    Vector3 wl2 = w2 + topDepthVector;
                                    Vector3 wl3 = w3 + topDepthVector;
                                    AddPlane(w2, w3, wl2, wl3, submeshTop, flippedTop, uvStart, uvEnd);
//                                }
                            }

                            if (rowDiff > 0 || !data.renderInteriors)//Window External Sills
                            {
                                uvStart = facadeUV + new Vector2(leftWidth, 0);
                                uvEnd = uvStart + new Vector2(openingWidth, rowDiff);

//                                if (rowBottomHeight > 0)//Bottom
//                                {
                                    Vector3 wd0l = w0 + facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    Vector3 wd1l = w1 + facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    Vector3 wd2l = w0 + facadeCross * (bayStyle.openingDepth - largestDepthValue);
                                    Vector3 wd3l = w1 + facadeCross * (bayStyle.openingDepth - largestDepthValue);
                                    AddPlane(wd0l, wd1l, wd2l, wd3l, submeshBottom, flippedBottom, uvStart, uvEnd);
//                                }

//                                if (rowTopHeight > 0)//Top
//                                {
                                    Vector3 wd0r = w2 + facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    Vector3 wd1r = w3 + facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    Vector3 wd2r = w2 + facadeCross * (bayStyle.openingDepth - largestDepthValue);
                                    Vector3 wd3r = w3 + facadeCross * (bayStyle.openingDepth - largestDepthValue);
                                    AddPlane(wd1r, wd0r, wd3r, wd2r, submeshTop, flippedTop, uvStart, uvEnd);
//                                }
                            }

                            ///COLUMNS
                            //Column Face
                            if (leftWidth > 0)//Column Face Left
                            {
                                Vector3 leftColumnDepthVector = (!firstColumn) ? columnDepthVector : Vector3.zero;
                                w0 = verts[4] + leftColumnDepthVector;
                                w1 = verts[5] + leftColumnDepthVector;
                                w2 = verts[8] + leftColumnDepthVector;
                                w3 = verts[9] + leftColumnDepthVector;
                                Vector2 leftColumnUVStart = facadeUV + new Vector2(0, rowBottomHeight);
                                Vector2 leftColumnUVEnd = leftColumnUVStart + new Vector2(leftWidth, openingHeight);
                                AddPlane(w0, w1, w2, w3, columnSubMesh, columnFlipped, leftColumnUVStart, leftColumnUVEnd);

                                if (!firstColumn)//Left Column Top Bottom
                                {
                                    w0 = verts[4] + largestDepthVector;
                                    w1 = verts[5] + largestDepthVector;
                                    w2 = verts[8] + largestDepthVector;
                                    w3 = verts[9] + largestDepthVector;

                                    bottomDepth = Mathf.Min(bayStyle.crossDepth, bayStyle.columnDepth) - largestDepthValue;
                                    float colDiff = bayStyle.crossDepth - bayStyle.columnDepth;
                                    if (bottomDepth != 0)
                                    {
                                        if (data.renderInteriors)
                                        {
                                            uvStart = new Vector2(0, 0);
                                            uvEnd = uvStart + new Vector2(leftWidth, bottomDepth);
                                            Vector3 bottomDepthVector = facadeCross * bottomDepth;
//                                            if(rowBottomHeight>0)
//                                            {
                                                Vector3 wl0 = w0 + bottomDepthVector;
                                                Vector3 wl1 = w1 + bottomDepthVector;
                                                AddPlane(wl0, wl1, w0, w1, submeshBottom, flippedBottom, uvStart, uvEnd);
//                                            }
//                                            if(rowTopHeight > 0)
//                                            {
                                                Vector3 wl2 = w2 + bottomDepthVector;
                                                Vector3 wl3 = w3 + bottomDepthVector;
                                                AddPlane(w2, w3, wl2, wl3, submeshTop, flippedTop, uvStart, uvEnd);
//                                            }
                                        }

                                        if (colDiff > 0 || !data.renderInteriors)
                                        {
                                            uvStart = new Vector2(0, rowBottomHeight);
                                            uvEnd = uvStart + new Vector2(rowDiff, openingHeight);
//                                            if(rowBottomHeight > 0)//Bottom
//                                            {
                                                Vector3 wd0l = w0 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd1l = w1 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd2l = w0 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                Vector3 wd3l = w1 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                AddPlane(wd2l, wd0l, wd3l, wd1l, submeshBottom, flippedBottom, uvStart, uvEnd);
//                                            }
//                                            if(rowTopHeight > 0)//Top
//                                            {
                                                Vector3 wd0r = w2 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd1r = w3 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd2r = w2 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                Vector3 wd3r = w3 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                AddPlane(wd3r, wd1r, wd2r, wd0r, submeshTop, flippedTop, uvStart, uvEnd);
//                                            }
                                        }
                                    }
                                }
                            }
                            if ((!nextBayIdentical || lastColumn) && rightWidth > 0)//Column Right
                            {
                                Vector3 rightColumeDepthVector = (!lastColumn) ? columnDepthVector : Vector3.zero;
                                w0 = verts[6] + rightColumeDepthVector;
                                w1 = verts[7] + rightColumeDepthVector;
                                w2 = verts[10] + rightColumeDepthVector;
                                w3 = verts[11] + rightColumeDepthVector;
                                Vector2 rightColumnUVStart = facadeUV + new Vector2(leftWidth + openingWidth, rowBottomHeight);
                                Vector2 rightColumnUVEnd = rightColumnUVStart + new Vector2(rightWidth, openingHeight);
                                AddPlane(w0, w1, w2, w3, columnSubMesh, columnFlipped, rightColumnUVStart, rightColumnUVEnd);
                                if (!lastColumn)//Right Column Top Bottom
                                {
                                    w0 = verts[6] + largestDepthVector;
                                    w1 = verts[7] + largestDepthVector;
                                    w2 = verts[10] + largestDepthVector;
                                    w3 = verts[11] + largestDepthVector;

                                    bottomDepth = Mathf.Min(bayStyle.crossDepth, bayStyle.columnDepth) - largestDepthValue;//Window Left
                                    float colDiff = bayStyle.crossDepth - bayStyle.columnDepth;
                                    if (bottomDepth != 0)
                                    {
                                        if (data.renderInteriors)
                                        {
                                            uvStart = new Vector2(leftWidth+openingWidth, 0);
                                            uvEnd = uvStart + new Vector2(rightWidth, bottomDepth);
                                            Vector3 bottomDepthVector = facadeCross * bottomDepth;
                                            if (rowBottomHeight > 0)
                                            {
                                                Vector3 wl0 = w0 + bottomDepthVector;
                                                Vector3 wl1 = w1 + bottomDepthVector;
                                                AddPlane(wl0, wl1, w0, w1, submeshBottom, flippedBottom, uvStart, uvEnd);
                                            }
                                            if (rowTopHeight > 0)
                                            {
                                                Vector3 wl2 = w2 + bottomDepthVector;
                                                Vector3 wl3 = w3 + bottomDepthVector;
                                                AddPlane(wl3, wl2, w3, w2, submeshTop, flippedTop, uvStart, uvEnd);
                                            }
                                        }

                                        if (colDiff > 0 || !data.renderInteriors)
                                        {
                                            uvStart = facadeUV + new Vector2(0, rowBottomHeight);
                                            uvEnd = uvStart + new Vector2(rowDiff, openingHeight);
                                            if (rowBottomHeight > 0)//Bottom
                                            {
                                                Vector3 wd0l = w0 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd1l = w1 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd2l = w0 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                Vector3 wd3l = w1 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                AddPlane(wd2l, wd0l, wd3l, wd1l, submeshBottom, flippedBottom, uvStart, uvEnd);
                                            }
                                            if (rowTopHeight > 0)//Top
                                            {
                                                Vector3 wd0r = w2 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd1r = w3 + facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                                Vector3 wd2r = w2 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                Vector3 wd3r = w3 + facadeCross * (bayStyle.columnDepth - largestDepthValue);
                                                AddPlane(wd3r, wd1r, wd2r, wd0r, submeshTop, flippedTop, uvStart, uvEnd);
                                            }
                                        }
                                    }
                                }
                            }

                            ///ROWS
                            //Row Bottom
                            w0 = verts[1] + rowDepthVector;
                            w1 = verts[2] + rowDepthVector;
                            w2 = verts[5] + rowDepthVector;
                            w3 = verts[6] + rowDepthVector;
                            if (rowBottomHeight > 0)
                            {
                                Vector2 bottomRowUVStart = facadeUV + new Vector2(leftWidth, 0);
                                Vector2 bottomRowUVEnd = bottomRowUVStart + new Vector2(openingWidth, rowBottomHeight);
                                AddPlane(w0, w1, w2, w3, rowSubMesh, rowFlipped, bottomRowUVStart, bottomRowUVEnd);

                                //Row Sides
                                w0 = verts[1] + largestDepthVector;
                                w1 = verts[2] + largestDepthVector;
                                w2 = verts[5] + largestDepthVector;
                                w3 = verts[6] + largestDepthVector;
                                uvStart = facadeUV + new Vector2(0, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(columnDiff, rowBottomHeight);
                                if (leftWidth > 0)//Left Side
                                {
                                    Vector3 sideDepthVectorA = facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                    Vector3 sideDepthVectorB = facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    if (firstColumn) sideDepthVectorA = facadeCross * -largestDepthValue;
                                    Vector3 wd0l = w0 + sideDepthVectorA;
                                    Vector3 wd1l = w2 + sideDepthVectorA;
                                    Vector3 wd2l = w0 + sideDepthVectorB;
                                    Vector3 wd3l = w2 + sideDepthVectorB;
                                    AddPlane(wd0l, wd2l, wd1l, wd3l, rowSubMesh, rowFlipped, uvStart, uvEnd);
                                }

//                                if (rightWidth > 0)//Right Side
//                                {
                                    Vector3 sideDepthVectorC = facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                    Vector3 sideDepthVectorD = facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    if (lastColumn) sideDepthVectorC = facadeCross * -largestDepthValue;
                                    Vector3 wd0r = w1 + sideDepthVectorC;
                                    Vector3 wd1r = w3 + sideDepthVectorC;
                                    Vector3 wd2r = w1 + sideDepthVectorD;
                                    Vector3 wd3r = w3 + sideDepthVectorD;
                                    AddPlane(wd1r, wd3r, wd0r, wd2r, rowSubMesh, rowFlipped, uvStart, uvEnd);
//                                }
                            }

                            //Row Top
                            w0 = verts[9] + rowDepthVector;
                            w1 = verts[10] + rowDepthVector;
                            w2 = verts[13] + rowDepthVector;
                            w3 = verts[14] + rowDepthVector;

                            if (rowTopHeight > 0)
                            {
                                Vector2 topRowUVStart = facadeUV + new Vector2(leftWidth, rowBottomHeight + openingHeight);
                                Vector2 topRowUVEnd = topRowUVStart + new Vector2(openingWidth, rowTopHeight);
                                AddPlane(w0, w1, w2, w3, rowSubMesh, rowFlipped, topRowUVStart, topRowUVEnd);

                                //Row Sides
                                w0 = verts[9] + largestDepthVector;
                                w1 = verts[10] + largestDepthVector;
                                w2 = verts[13] + largestDepthVector;
                                w3 = verts[14] + largestDepthVector;
                                uvStart = facadeUV + new Vector2(0, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(columnDiff, rowBottomHeight);
                                if (leftWidth > 0)//Left Side
                                {
                                    Vector3 sideDepthVectorA = facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                    Vector3 sideDepthVectorB = facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    if (firstColumn) sideDepthVectorA = facadeCross * -largestDepthValue;
                                    Vector3 wd0l = w0 + sideDepthVectorA;
                                    Vector3 wd1l = w2 + sideDepthVectorA;
                                    Vector3 wd2l = w0 + sideDepthVectorB;
                                    Vector3 wd3l = w2 + sideDepthVectorB;
                                    AddPlane(wd0l, wd2l, wd1l, wd3l, rowSubMesh, rowFlipped, uvStart, uvEnd);
                                }

//                                if (rightWidth > 0)//Right Side
//                                {
                                    Vector3 sideDepthVectorC = facadeCross * (bayStyle.crossDepth - largestDepthValue);
                                    Vector3 sideDepthVectorD = facadeCross * (bayStyle.rowDepth - largestDepthValue);
                                    if (lastColumn) sideDepthVectorC = facadeCross * -largestDepthValue;
                                    Vector3 wd0r = w1 + sideDepthVectorC;
                                    Vector3 wd1r = w3 + sideDepthVectorC;
                                    Vector3 wd2r = w1 + sideDepthVectorD;
                                    Vector3 wd3r = w3 + sideDepthVectorD;
                                    AddPlane(wd1r, wd3r, wd0r, wd2r, rowSubMesh, rowFlipped, uvStart, uvEnd);
//                                }
                            }

                            //Cross Left Bottom
                            Vector3 leftCrossDepthVector = (!firstColumn) ? crossDepthVector : Vector3.zero;
                            w0 = verts[0] + leftCrossDepthVector;
                            w1 = verts[1] + leftCrossDepthVector;
                            w2 = verts[4] + leftCrossDepthVector;
                            w3 = verts[5] + leftCrossDepthVector;
                            Vector2 crossLBUVStart = facadeUV + new Vector2(0, 0);
                            Vector2 crossLBUVEnd = crossLBUVStart + new Vector2(leftWidth, rowBottomHeight);
                            AddPlane(w0, w1, w2, w3, crossSubMesh, crossFlipped, crossLBUVStart, crossLBUVEnd);
                           
                            //Cross Left Top
                            if (rowTopHeight > 0)
                            {
                                w0 = verts[8] + leftCrossDepthVector;
                                w1 = verts[9] + leftCrossDepthVector;
                                w2 = verts[12] + leftCrossDepthVector;
                                w3 = verts[13] + leftCrossDepthVector;
                                Vector2 crossLTUVStart = facadeUV + new Vector2(0, rowBottomHeight + openingHeight);
                                Vector2 crossLTUVEnd = crossLTUVStart + new Vector2(leftWidth, rowTopHeight);
                                AddPlane(w0, w1, w2, w3, crossSubMesh, crossFlipped, crossLTUVStart, crossLTUVEnd);
                            }

                            if ((!nextBayIdentical || lastColumn) && rightWidth > 0)
                            {
                                if(lastColumn) crossDepthVector = Vector3.zero;//zero the ends of buildings
                                //Cross Right Bottom
                                w0 = verts[2] + crossDepthVector;
                                w1 = verts[3] + crossDepthVector;
                                w2 = verts[6] + crossDepthVector;
                                w3 = verts[7] + crossDepthVector;
                                Vector2 crossRBUVStart = facadeUV + new Vector2(leftWidth + openingWidth, 0);
                                Vector2 crossRBUVEnd = crossRBUVStart + new Vector2(rightWidth, rowBottomHeight);
                                AddPlane(w0, w1, w2, w3, crossSubMesh, crossFlipped, crossRBUVStart, crossRBUVEnd);

                                //Cross Right Top
                                if (rowTopHeight > 0)
                                {
                                    w0 = verts[10] + crossDepthVector;
                                    w1 = verts[11] + crossDepthVector;
                                    w2 = verts[14] + crossDepthVector;
                                    w3 = verts[15] + crossDepthVector;
                                    Vector2 crossRTUVStart = facadeUV + new Vector2(leftWidth + openingWidth, rowBottomHeight + openingHeight);
                                    Vector2 crossRTUVEnd = crossRTUVStart + new Vector2(rightWidth, rowTopHeight);
                                    AddPlane(w0, w1, w2, w3, crossSubMesh, crossFlipped, crossRTUVStart, crossRTUVEnd);
                                }
                            }

                            ///FACADE BOTTOMS
                            if (lastFacadeDesign != facadeDesign && rowBottomHeight > 0)
                            {
                                //Row Bottom
                                w0 = verts[1] + largestDepthVector;
                                w1 = verts[2] + largestDepthVector;

                                bottomDepth = bayStyle.rowDepth - largestDepthValue;
                                uvStart = facadeUV + new Vector2(leftWidth, 0);
                                uvEnd = uvStart + new Vector2(openingWidth, bottomDepth);

                                Vector3 bottomDepthVector = facadeCross * bottomDepth;
                                Vector3 wl0 = w0 + bottomDepthVector;
                                Vector3 wl1 = w1 + bottomDepthVector;
                                AddPlane(w0, w1, wl0, wl1, submeshBottom, flippedBottom, uvStart, uvEnd);

                                if(!firstColumn)
                                {
                                    //Left Cross Bottom
                                    w0 = verts[0] + largestDepthVector;
                                    w1 = verts[1] + largestDepthVector;

                                    bottomDepth = bayStyle.crossDepth - largestDepthValue;
                                    uvStart = facadeUV + new Vector2(0, 0);
                                    uvEnd = uvStart + new Vector2(leftWidth, bottomDepth);

                                    bottomDepthVector = facadeCross * bottomDepth;
                                    wl0 = w0 + bottomDepthVector;
                                    wl1 = w1 + bottomDepthVector;
                                    AddPlane(w0, w1, wl0, wl1, submeshBottom, flippedBottom, uvStart, uvEnd);
                                }

                                //Right Cross Bottom
                                if ((!nextBayIdentical && !lastColumn) && rightWidth > 0)
                                {
                                    w0 = verts[2] + largestDepthVector;
                                    w1 = verts[3] + largestDepthVector;

                                    bottomDepth = bayStyle.crossDepth - largestDepthValue;
                                    uvStart = facadeUV + new Vector2(leftWidth + openingWidth, 0);
                                    uvEnd = uvStart + new Vector2(rightWidth, bottomDepth);

                                    bottomDepthVector = facadeCross * bottomDepth;
                                    wl0 = w0 + bottomDepthVector;
                                    wl1 = w1 + bottomDepthVector;
                                    AddPlane(w0, w1, wl0, wl1, submeshBottom, flippedBottom, uvStart, uvEnd);
                                }
                            }


                            ///FACADE TOPS
                            if (nextFacadeDesign != facadeDesign && rowTopHeight > 0)
                            {
                                //Row Top
                                w0 = verts[13] + largestDepthVector;
                                w1 = verts[14] + largestDepthVector;

                                bottomDepth = bayStyle.rowDepth - largestDepthValue;
                                uvStart = facadeUV + new Vector2(leftWidth, 0);
                                uvEnd = uvStart + new Vector2(openingWidth, bottomDepth);

                                Vector3 bottomDepthVector = facadeCross * bottomDepth;
                                Vector3 wl0 = w0 + bottomDepthVector;
                                Vector3 wl1 = w1 + bottomDepthVector;
                                AddPlane(wl0, wl1, w0, w1, submeshBottom, flippedBottom, uvStart, uvEnd);

                                //Left Cross Top
                                if (!firstColumn)
                                {
                                    w0 = verts[12] + largestDepthVector;
                                    w1 = verts[13] + largestDepthVector;

                                    bottomDepth = bayStyle.crossDepth - largestDepthValue;
                                    uvStart = facadeUV + new Vector2(0, 0);
                                    uvEnd = uvStart + new Vector2(leftWidth,bottomDepth);

                                    bottomDepthVector = facadeCross * bottomDepth;
                                    wl0 = w0 + bottomDepthVector;
                                    wl1 = w1 + bottomDepthVector;
                                    AddPlane(wl0, wl1, w0, w1, submeshBottom, flippedBottom, uvStart, uvEnd);
                                }

                                //Right Cross Top
                                if ((!nextBayIdentical && !lastColumn) && rightWidth > 0)
                                {
                                    w0 = verts[14] + largestDepthVector;
                                    w1 = verts[15] + largestDepthVector;

                                    bottomDepth = bayStyle.crossDepth - largestDepthValue;
                                    uvStart = facadeUV + new Vector2(leftWidth+openingWidth, 0);
                                    uvEnd = uvStart + new Vector2(rightWidth, bottomDepth);

                                    bottomDepthVector = facadeCross * bottomDepth;
                                    wl0 = w0 + bottomDepthVector;
                                    wl1 = w1 + bottomDepthVector;
                                    AddPlane(wl0, wl1, w0, w1, submeshBottom, flippedBottom, uvStart, uvEnd);
                                }
                            }

                            ///BAY SIDES
                            // LEFT
                            if(!previousBayIdentical)
                            {
                                //Column
                                w1 = verts[4] + largestDepthVector;
                                w3 = verts[8] + largestDepthVector;
                                windowSideDepth = bayStyle.columnDepth - largestDepthValue;
                                uvStart = facadeUV + new Vector2(0, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(windowSideDepth, openingHeight);
                                Vector3 depthVector = facadeCross * windowSideDepth;
                                Vector3 wr1 = w1 + depthVector;
                                Vector3 wr3 = w3 + depthVector;
                                AddPlane(wr3, w3, wr1, w1, columnSubMesh, columnFlipped, uvStart, uvEnd);

                                //Cross Bottom
                                w1 = verts[0] + largestDepthVector;
                                w3 = verts[4] + largestDepthVector;
                                windowSideDepth = bayStyle.crossDepth - largestDepthValue;
                                uvStart = facadeUV + new Vector2(0, 0);
                                uvEnd = uvStart + new Vector2(windowSideDepth, rowBottomHeight);
                                depthVector = facadeCross * windowSideDepth;
                                wr1 = w1 + depthVector;
                                wr3 = w3 + depthVector;
                                AddPlane(wr3, w3, wr1, w1, crossSubMesh, crossFlipped, uvStart, uvEnd);
                                //Cross Top
                                if (rowTopHeight > 0)
                                {
                                    w1 = verts[8] + largestDepthVector;
                                    w3 = verts[12] + largestDepthVector;
                                    windowSideDepth = bayStyle.crossDepth - largestDepthValue;
                                    uvStart = facadeUV + new Vector2(0, 0);
                                    uvEnd = uvStart + new Vector2(windowSideDepth, rowTopHeight);
                                    depthVector = facadeCross * windowSideDepth;
                                    wr1 = w1 + depthVector;
                                    wr3 = w3 + depthVector;
                                    AddPlane(wr3, w3, wr1, w1, crossSubMesh, crossFlipped, uvStart, uvEnd);
                                }
                            }

                            //RIGHT
                            if (!nextBayIdentical && !lastColumn)
                            {
                                //Column Sides
                                w1 = verts[7] + largestDepthVector;
                                w3 = verts[11] + largestDepthVector;
                                windowSideDepth = bayStyle.columnDepth - largestDepthValue;
                                uvStart = facadeUV + new Vector2(0, rowBottomHeight);
                                uvEnd = uvStart + new Vector2(windowSideDepth, openingHeight);
                                Vector3 depthVector = facadeCross * windowSideDepth;
                                Vector3 wr1 = w1 + depthVector;
                                Vector3 wr3 = w3 + depthVector;
                                AddPlane(w3, wr3, w1, wr1, columnSubMesh, columnFlipped, uvStart, uvEnd);

                                //Cross Bottom
                                w1 = verts[3] + largestDepthVector;
                                w3 = verts[7] + largestDepthVector;
                                windowSideDepth = bayStyle.crossDepth - largestDepthValue;
                                uvStart = facadeUV + new Vector2(0, 0);
                                uvEnd = uvStart + new Vector2(windowSideDepth, rowBottomHeight);
                                depthVector = facadeCross * windowSideDepth;
                                wr1 = w1 + depthVector;
                                wr3 = w3 + depthVector;
                                AddPlane(w3, wr3, w1, wr1, crossSubMesh, crossFlipped, uvStart, uvEnd);
                                //Cross Top
                                if (rowTopHeight > 0)
                                {
                                    w1 = verts[11] + largestDepthVector;
                                    w3 = verts[15] + largestDepthVector;
                                    windowSideDepth = bayStyle.crossDepth - largestDepthValue;
                                    uvStart = facadeUV + new Vector2(0, 0);
                                    uvEnd = uvStart + new Vector2(windowSideDepth, rowTopHeight);
                                    depthVector = facadeCross * windowSideDepth;
                                    wr1 = w1 + depthVector;
                                    wr3 = w3 + depthVector;
                                    AddPlane(w3, wr3, w1, wr1, crossSubMesh, crossFlipped, uvStart, uvEnd);
                                }
                            }
                        }
                    }
                    else
                    {
                        // windowless wall
                        Vector3 wallVector = (facadeDirection * (facadeWidth+largestDepthValue*2.0f));
                        Vector3 wallHeightVector = Vector3.up * floorHeight;
                        Vector3 w0 = facadeFloorBaseVector;
                        Vector3 w1 = facadeFloorBaseVector + wallVector;
                        Vector3 w2 = facadeFloorBaseVector + wallHeightVector;
                        Vector3 w3 = facadeFloorBaseVector + wallVector + wallHeightVector;
                        int wallSubmesh = facadeDesign.simpleBay.GetTexture(BuildrBay.TextureNames.WallTexture);
                        bool flipped = facadeDesign.simpleBay.IsFlipped(BuildrBay.TextureNames.WallTexture);
                        Vector2 wallUVStart = facadeUV;
                        Vector2 wallUVEnd = facadeUV + new Vector2(realFadeWidth, floorHeight);
                        AddPlane(w0, w1, w2, w3, wallSubmesh, flipped, wallUVStart, wallUVEnd);//face

                        if (nextFacadeDesign.hasWindows && !lastRow)
                        {
                            Vector3 wl2 = w2 - lastFacadeDirection*largestDepthValue;
                            Vector3 wl3 = w3 + nextFacadeDirection*largestDepthValue;
                            Vector2 uvEnd = new Vector2(facadeWidth, 1);
                            AddPlane(w3, wl3, w2, wl2, wallSubmesh, flipped, wallUVStart, uvEnd);//top
                        }
                    }
                }
            }
            //Bottom of the mesh - it's mostly to ensure the model can render certain shadows correctly
            if (data.drawUnderside)
            {
                Vector3 foundationDrop = Vector3.down * data.foundationHeight;
                var newEndVerts = new Vector3[numberOfVolumePoints];
                var newEndUVs = new Vector2[numberOfVolumePoints];
                for (int i = 0; i < numberOfVolumePoints; i++)
                {
                    newEndVerts[i] = plan.points[volume.points[i]].vector3 + foundationDrop;
                    newEndUVs[i] = Vector2.zero;
                }
                var tris = new List<int>(data.plan.GetTrianglesBySectorBase(v));
                tris.Reverse();
                int bottomSubMesh = facadeDesign.GetTexture(BuildrFacadeDesign.textureNames.columnTexture);
                mesh.AddData(newEndVerts, newEndUVs, tris.ToArray(), bottomSubMesh);
            }
        }
        data = null;
        mesh = null;
        textures = null;
    }

    private static void AddPlane(Vector3 w0, Vector3 w1, Vector3 w2, Vector3 w3, int subMesh, bool flipped, Vector2 facadeUVStart, Vector2 facadeUVEnd)
    {
        int textureSubmesh = subMesh;
        BuildrTexture texture = textures[textureSubmesh];
        Vector2 uvStart = facadeUVStart;
        Vector2 uvEnd = facadeUVEnd;

        if (texture.tiled)
        {
            uvStart = new Vector2((uvStart.x / texture.textureUnitSize.x), (uvStart.y / texture.textureUnitSize.y));
            uvEnd = new Vector2((uvEnd.x / texture.textureUnitSize.x), (uvEnd.y / texture.textureUnitSize.y));
        }
        else
        {
            uvStart = Vector2.zero;
            uvEnd.x = texture.tiledX;
            uvEnd.y = texture.tiledY;
        }

        if (!flipped)
            mesh.AddPlane(w0, w1, w2, w3, uvStart, uvEnd, textureSubmesh);
        else
        {
            uvStart = new Vector2(uvStart.y, uvStart.x);
            uvEnd = new Vector2(uvEnd.y, uvEnd.x);
            mesh.AddPlane(w2, w0, w3, w1, uvStart, uvEnd, textureSubmesh);
        }
    }
}