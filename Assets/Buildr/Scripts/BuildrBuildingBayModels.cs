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

public class BuildrBuildingBayModels : MonoBehaviour {


    public static GameObject[] Place(BuildrData data)
    {
        BuildrPlan plan = data.plan;
        int numberOfVolumes = plan.numberOfVolumes;
        BuildrTexture[] textures = data.textures.ToArray();
        List<GameObject> output = new List<GameObject>();

        float largestDepthValue = 0;//deepest value of a bay design in the building
        foreach (BuildrBay bay in data.bays)
            largestDepthValue = Mathf.Max(largestDepthValue, bay.deepestValue);//get the deepest value
        foreach (BuildrFacadeDesign facade in data.facades)
            largestDepthValue = Mathf.Max(largestDepthValue, facade.simpleBay.deepestValue);//get the deepest value

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

                float facadeWidth = Vector3.Distance(p0, p1) - largestDepthValue*2.0f;
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

                for(int r = 0; r < rows; r++)
                {
                    //Get the facade style id
                    //need to loop through the facade designs floor by floor until we get to the right one
                    float currentHeight = floorHeight * r;
                    Vector3 facadeFloorBaseVector = p0 + Vector3.up * currentHeight;
                    int modFloor = (r % floorPatternSize);

                    BuildrFacadeDesign facadeDesign = data.facades[styleUnits[facadePatternReference[modFloor]].styleID];

                    bool isBlankWall = !facadeDesign.hasWindows;
                    if(facadeDesign.type == BuildrFacadeDesign.types.patterned)
                    {
                        if(data.bays.Count == 0 || facadeDesign.bayPattern.Count == 0)
                        {
                            data.illegal = true;
                            return output.ToArray();
                        }

                        BuildrBay firstBay = data.bays[facadeDesign.bayPattern[0]];
                        if(firstBay.openingWidth > facadeWidth) isBlankWall = true;
                        if(facadeDesign.bayPattern.Count == 0) isBlankWall = true;
                    }
                    else
                    {
                        if(facadeDesign.simpleBay.openingWidth + facadeDesign.simpleBay.minimumBayWidth > facadeWidth)
                            isBlankWall = true;
                    }

                    if(!isBlankWall)
                    {
                        float patternSize = 0;//the space the pattern fills, there will be a gap that will be distributed to all bay styles
                        int numberOfBays = 0;
                        //float actualWindowSpacing;
                        BuildrBay[] bayDesignPattern;
                        int numberOfBayDesigns;
                        if(facadeDesign.type == BuildrFacadeDesign.types.patterned)
                        {
                            numberOfBayDesigns = facadeDesign.bayPattern.Count;
                            bayDesignPattern = new BuildrBay[numberOfBayDesigns];
                            for(int i = 0; i < numberOfBayDesigns; i++)
                            {
                                bayDesignPattern[i] = data.bays[facadeDesign.bayPattern[i]];
                            }
                        }
                        else
                        {
                            bayDesignPattern = new[]{facadeDesign.simpleBay};
                            numberOfBayDesigns = 1;
                        }
                        //start with first window width - we'll be adding to this until we have filled the facade width
                        int it = 100;
                        while(true)
                        {
                            int patternModIndex = numberOfBays % numberOfBayDesigns;
                            float patternAddition = bayDesignPattern[patternModIndex].openingWidth + bayDesignPattern[patternModIndex].minimumBayWidth;
                            if(patternSize + patternAddition < facadeWidth)
                            {
                                patternSize += patternAddition;
                                numberOfBays++;
                            }
                            else
                                break;
                            it--;
                            if(it < 0)
                                break;
                        }

                        Vector3 windowBase = facadeFloorBaseVector;
                        float perBayAdditionalSpacing = (facadeWidth - patternSize) / numberOfBays;
                        for(int c = 0; c < numberOfBays; c++)
                        {
                            BuildrBay bayStyle;

                            if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                            {
                                int numberOfBayStyles = facadeDesign.bayPattern.Count;
                                bayStyle = bayDesignPattern[c % numberOfBayStyles];
                            }
                            else
                                bayStyle = facadeDesign.simpleBay;

                            GameObject bayModel = bayStyle.bayModel;

                            BuildrTexture columnTexture = textures[bayStyle.GetTexture(BuildrBay.TextureNames.ColumnTexture)];
                            Vector2 columnuvunits = columnTexture.tileUnitUV;
                            float openingWidth = bayStyle.openingWidth;
                            float openingHeight = bayStyle.openingHeight;
                            if (columnTexture.patterned) openingHeight = Mathf.Ceil(bayStyle.openingHeight / columnuvunits.y) * columnuvunits.y;
                            if (bayStyle.openingHeight == floorHeight) bayStyle.openingHeight = floorHeight;
                            float actualWindowSpacing = bayStyle.minimumBayWidth + perBayAdditionalSpacing;
                            float leftWidth = actualWindowSpacing * bayStyle.openingWidthRatio;
                            bool firstColumn = c == 0;
                            if (firstColumn) leftWidth += largestDepthValue;

                            float rowBottomHeight = ((floorHeight - openingHeight) * bayStyle.openingHeightRatio);
                            if (columnTexture.patterned) rowBottomHeight = Mathf.Ceil(rowBottomHeight / columnuvunits.y) * columnuvunits.y;

                            float bayWidthSize = openingWidth + actualWindowSpacing;
                            Vector3 bayWidth = facadeDirection * bayWidthSize;
                            if (!bayStyle.isOpening || bayModel == null)
                            {
                                windowBase += bayWidth;//move base vertor to next bay
                                if (firstColumn) windowBase += facadeDirection * (largestDepthValue);
                                continue;//bay filled - move onto next bay
                            }

                            GameObject newInstance = (GameObject)Instantiate(bayModel);
                            MeshRenderer[] rends = newInstance.GetComponentsInChildren<MeshRenderer>();
                            Bounds modelBounds = rends[0].bounds;
                            foreach(MeshRenderer meshRenderer in rends)
                                modelBounds.Encapsulate(meshRenderer.bounds);

                            if(rends.Length == 0)
                                continue;

                            Vector3 modelSize = modelBounds.size;
                            Vector3 baySize = new Vector3(openingWidth, openingHeight, modelSize.z);
                            Vector3 modelScale;
                            modelScale.x = baySize.x / modelSize.x;
                            modelScale.y = baySize.y / modelSize.y;
                            modelScale.z = baySize.z / modelSize.z;
                            newInstance.transform.localScale = modelScale;

                            Vector3 upVector = Vector3.up * rowBottomHeight;
                            Vector3 leftVector = leftWidth * facadeDirection;
                            Vector3 modelPosition = windowBase + leftVector + upVector;

                            modelBounds = rends[0].bounds;
                            foreach (MeshRenderer meshRenderer in rends)
                                modelBounds.Encapsulate(meshRenderer.bounds);
                            modelPosition += Quaternion.LookRotation(facadeCross) * (-modelBounds.min);
                            newInstance.transform.position = modelPosition;
                            newInstance.transform.rotation = Quaternion.LookRotation(-facadeCross);
                            output.Add(newInstance);

                            windowBase += bayWidth;//move base vertor to next bay
                            if (firstColumn) windowBase += facadeDirection * (largestDepthValue);
                        }
                    }
                }
            }
        }
        return output.ToArray();
    }
}
