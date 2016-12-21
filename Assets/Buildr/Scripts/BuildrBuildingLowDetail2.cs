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

public class BuildrBuildingLowDetail2
{
    private const int PIXELS_PER_METER = 100;
    private const int ATLAS_PADDING = 16;

    private static int MAXIMUM_TEXTURESIZE = 1024;
    private static BuildrData data;
    private static BuildrTexture[] textures;
    private static DynamicMeshGenericMultiMaterialMesh mesh;
    private static List<int> roofTextureIndex = new List<int>();
//    private static float timestart;
    private static int numberOfFacades;
    private static Color32[] colourArray;
    private static int textureWidth;
    private static int textureSize;
    private static List<Rect> packedTexturePositions = new List<Rect>();
    private static float packedScale;
    private static List<BuildrTexture> roofTextures = new List<BuildrTexture>();

    private static void LogTimer(string label)
    {
//        Debug.Log("BuildrBuildingLowDetail2 " + label + " : " + (Time.realtimeSinceStartup - timestart));
    }


    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data)
    {

//        timestart = Time.realtimeSinceStartup;
        data = _data;
        mesh = _mesh;
        textures = data.textures.ToArray();
        BuildrPlan plan = data.plan;

        int facadeIndex = 0;
        numberOfFacades = 0;
        int numberOfVolumes = data.plan.numberOfVolumes;

        LogTimer("Start");

        //define rectangles that represent the facades
        packedTexturePositions.Clear();
        for (int v = 0; v < numberOfVolumes; v++)
        {
            BuildrVolume volume = plan.volumes[v];
            int numberOfVolumePoints = volume.points.Count;

            for (int f = 0; f < numberOfVolumePoints; f++)
            {
                if (!volume.renderFacade[f])
                    continue;
                int indexA = f;
                int indexB = (f < numberOfVolumePoints - 1) ? f + 1 : 0;
                Vector2z p0 = plan.points[volume.points[indexA]];
                Vector2z p1 = plan.points[volume.points[indexB]];

                float facadeWidth = Vector2z.Distance(p0, p1) * PIXELS_PER_METER;
                int floorBase = plan.GetFacadeFloorHeight(v, volume.points[indexA], volume.points[indexB]);

                int numberOfFloors = volume.numberOfFloors - floorBase;
                if (numberOfFloors < 1)//no facade - adjacent facade is taller and covers this one
                    continue;

                float floorHeight = data.floorHeight;
                float facadeHeight = (volume.numberOfFloors - floorBase) * floorHeight * PIXELS_PER_METER;
                if (facadeHeight < 0)
                {
                    facadeWidth = 0;
                    facadeHeight = 0;
                }

                Rect newFacadeRect = new Rect(0, 0, facadeWidth, facadeHeight);
                packedTexturePositions.Add(newFacadeRect);

                numberOfFacades++;
            }
        }

        //Build ROOF
        DynamicMeshGenericMultiMaterialMesh dynMeshRoof = new DynamicMeshGenericMultiMaterialMesh();
        dynMeshRoof.name = "Roof Mesh";
        dynMeshRoof.subMeshCount = textures.Length;
        BuildrRoof.Build(dynMeshRoof, data, true);
        dynMeshRoof.CheckMaxTextureUVs(data);
        LogTimer("Roof A");

        roofTextures.Clear();
        roofTextureIndex.Clear();
        foreach (BuildrRoofDesign roofDesign in data.roofs)
        {
            foreach (int textureIndex in roofDesign.textureValues)
            {
                if (!roofTextureIndex.Contains(textureIndex))
                {
                    BuildrTexture bTexture = data.textures[textureIndex];
                    Vector2 largestSubmeshPlaneSize = new Vector2(1,1);
                    Vector2 minWorldUvSize = dynMeshRoof.MinWorldUvSize(textureIndex);
                    Vector2 maxWorldUvSize = dynMeshRoof.MaxWorldUvSize(textureIndex);
                    largestSubmeshPlaneSize.x = maxWorldUvSize.x - minWorldUvSize.x;
                    largestSubmeshPlaneSize.y = maxWorldUvSize.y - minWorldUvSize.y;
                    int roofTextureWidth = Mathf.RoundToInt(largestSubmeshPlaneSize.x * PIXELS_PER_METER);
                    int roofTextureHeight = Mathf.RoundToInt(largestSubmeshPlaneSize.y * PIXELS_PER_METER);
                    Rect newRoofTexutureRect = new Rect(0, 0, roofTextureWidth, roofTextureHeight);
                    packedTexturePositions.Add(newRoofTexutureRect);
                    roofTextures.Add(bTexture);
                    roofTextureIndex.Add(textureIndex);
                }
            }
        }

        //run a custom packer to define their postions
        textureWidth = RectanglePack.Pack(packedTexturePositions,ATLAS_PADDING);

        //determine the resize scale and apply that to the rects
        packedScale = 1;
        int numberOfRects = packedTexturePositions.Count;
        if (textureWidth > MAXIMUM_TEXTURESIZE)
        {
            packedScale = MAXIMUM_TEXTURESIZE / (float)textureWidth;
            for (int i = 0; i < numberOfRects; i++)
            {
                Rect thisRect = packedTexturePositions[i];
                thisRect.x *= packedScale;
                thisRect.y *= packedScale;
                thisRect.width *= packedScale;
                thisRect.height *= packedScale;
                packedTexturePositions[i] = thisRect;
                //Debug.Log("Rects "+roofTextures[i-+packedTexturePositions[i]);
            }
            textureWidth = Mathf.RoundToInt(packedScale * textureWidth);
        }
        else
        {
            textureWidth = (int)Mathf.Pow(2, (Mathf.FloorToInt(Mathf.Log(textureWidth - 1, 2)) + 1));//find the next power of two
        }
        //Debug.Log("Texture Width "+textureWidth);
        //TODO: maybe restrict the resize to a power of two?
        LogTimer("Packed Rect Generated");

        textureSize = textureWidth * textureWidth;
        colourArray = new Color32[textureSize];
        //TestRectColours();//this test paints all the facades with rainbow colours - real pretty
        BuildTextures();

        LogTimer("texture created");
        Texture2D packedTexture = new Texture2D(textureWidth, textureWidth, TextureFormat.ARGB32, true);
        packedTexture.filterMode = FilterMode.Bilinear;
        packedTexture.SetPixels32(colourArray);
        packedTexture.Apply(true, false);
        LogTimer("apply");

        if (data.LODTextureAtlas != null)
            Object.DestroyImmediate(data.LODTextureAtlas);
        data.LODTextureAtlas = packedTexture;
        data.LODTextureAtlas.name = "Low Detail Texture";

        //build the model with new uvs

        if (data.drawUnderside)
        {
            for (int s = 0; s < numberOfVolumes; s++)
            {
                BuildrVolume volume = plan.volumes[s];
                int numberOfVolumePoints = volume.points.Count;
                Vector3[] newEndVerts = new Vector3[numberOfVolumePoints];
                Vector2[] newEndUVs = new Vector2[numberOfVolumePoints];
                for (int i = 0; i < numberOfVolumePoints; i++)
                {
                    newEndVerts[i] = plan.points[volume.points[i]].vector3;
                    newEndUVs[i] = Vector2.zero;
                }

                List<int> tris = new List<int>(data.plan.GetTrianglesBySectorBase(s));
                tris.Reverse();
                mesh.AddData(newEndVerts, newEndUVs, tris.ToArray(), 0);
            }
        }
        LogTimer("Floor");

        //Build facades
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volume = plan.volumes[s];
            int numberOfVolumePoints = volume.points.Count;

            for (int f = 0; f < numberOfVolumePoints; f++)
            {
                if (!volume.renderFacade[f])
                    continue;
                int indexA = f;
                int indexB = (f < numberOfVolumePoints - 1) ? f + 1 : 0;
                Vector3 p0 = plan.points[volume.points[indexA]].vector3;
                Vector3 p1 = plan.points[volume.points[indexB]].vector3;

                int floorBase = plan.GetFacadeFloorHeight(s, volume.points[indexA], volume.points[indexB]);
                int numberOfFloors = volume.numberOfFloors - floorBase;
                if (numberOfFloors < 1)
                {
                    //no facade - adjacent facade is taller and covers this one
                    continue;
                }
                float floorHeight = data.floorHeight;

                Vector3 floorHeightStart = Vector3.up * (floorBase * floorHeight);
                Vector3 wallHeight = Vector3.up * (volume.numberOfFloors * floorHeight) - floorHeightStart;

                p0 += floorHeightStart;
                p1 += floorHeightStart;

                Vector3 w0 = p0;
                Vector3 w1 = p1;
                Vector3 w2 = w0 + wallHeight;
                Vector3 w3 = w1 + wallHeight;

                Rect facadeRect = packedTexturePositions[facadeIndex];

                float imageSize = textureWidth;
                Vector2 uvMin = new Vector2(facadeRect.xMin / imageSize, facadeRect.yMin / imageSize);
                Vector2 uvMax = new Vector2(facadeRect.xMax / imageSize, facadeRect.yMax / imageSize);

                mesh.AddPlane(w0, w1, w2, w3, uvMin, uvMax, 0);
                facadeIndex++;
            }
        }
        LogTimer("Facades");

        //ROOF Textures
        int roofRectBase = numberOfFacades;
        List<Rect> newAtlasRects = new List<Rect>();
        for (int i = roofRectBase; i < packedTexturePositions.Count; i++)
        {
            Rect uvRect = new Rect();//generate a UV based rectangle off the packed one
            uvRect.x = packedTexturePositions[i].x / textureWidth;
            uvRect.y = packedTexturePositions[i].y / textureWidth;
            uvRect.width = packedTexturePositions[i].width / textureWidth;
            uvRect.height = packedTexturePositions[i].height / textureWidth;
            newAtlasRects.Add(uvRect);
        }
        dynMeshRoof.Atlas(roofTextureIndex.ToArray(), newAtlasRects.ToArray(), data.textures.ToArray());
        //Add the atlased mesh data to the main model data at submesh 0
        mesh.AddData(dynMeshRoof.vertices, dynMeshRoof.uv, dynMeshRoof.triangles, 0);
        
        LogTimer("Roof B");

        data = null;
        mesh = null;
        textures = null;
        //atlasRects = null;

        LogTimer("Done");

        System.GC.Collect();
    }

    private class TexturePaintObject
    {
        public Color32[] pixels;
        public int width = 1;
        public int height = 1;
        public bool tiled = true;
        public Vector2 tiles = Vector2.one;//the user set amount ot tiling this untiled texture exhibits
    }

    private static void BuildTextures()
    {
        List<TexturePaintObject> buildSourceTextures = new List<TexturePaintObject>();
        foreach (BuildrTexture btexture in data.textures)//Gather the source textures, resized into Color32 arrays
        {
            TexturePaintObject texturePaintObject = new TexturePaintObject();
            texturePaintObject.pixels = ((Texture2D)btexture.texture).GetPixels32();
            texturePaintObject.width = btexture.texture.width;
            texturePaintObject.height = btexture.texture.height;
            texturePaintObject.tiles = new Vector2(btexture.tiledX,btexture.tiledY);
            if (btexture.tiled)
            {
                int resizedTextureWidth = Mathf.RoundToInt(btexture.textureUnitSize.x * PIXELS_PER_METER * packedScale);
                int resizedTextureHeight = Mathf.RoundToInt(btexture.textureUnitSize.y * PIXELS_PER_METER * packedScale);
                texturePaintObject.pixels = TextureScale.NearestNeighbourSample(texturePaintObject.pixels, texturePaintObject.width, texturePaintObject.height, resizedTextureWidth, resizedTextureHeight);
                texturePaintObject.width = resizedTextureWidth;
                texturePaintObject.height = resizedTextureHeight;
            }
            else
            {
                texturePaintObject.tiled = false;
            }
            buildSourceTextures.Add(texturePaintObject);
        }
        LogTimer("Gather Source into Arrays");
        TexturePaintObject[] sourceTextures = buildSourceTextures.ToArray();
        textures = data.textures.ToArray();
        BuildrFacadeDesign facadeDesign = data.facades[0];
        BuildrPlan plan = data.plan;

        int numberOfVolumes = data.plan.numberOfVolumes;
        int facadeNumber = 0;
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volume = plan.volumes[s];
            int numberOfVolumePoints = volume.points.Count;

            for (int f = 0; f < numberOfVolumePoints; f++)
            {
                if (!volume.renderFacade[f])
                    continue;
                int indexA, indexB;
                Vector3 p0, p1;
                indexA = f;
                indexB = (f < numberOfVolumePoints - 1) ? f + 1 : 0;
                p0 = plan.points[volume.points[indexA]].vector3;
                p1 = plan.points[volume.points[indexB]].vector3;
                Rect packedPosition = packedTexturePositions[facadeNumber];

                float facadeWidth = Vector3.Distance(p0, p1);
                int floorBase = plan.GetFacadeFloorHeight(s, volume.points[indexA], volume.points[indexB]);
                int numberOfFloors = volume.numberOfFloors - floorBase;
                if (numberOfFloors < 1)
                {
                    //no facade - adjacent facade is taller and covers this one
                    continue;
                }
                float floorHeight = data.floorHeight;

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

                Vector2 bayBase = Vector2.zero;
                float currentFloorBase = 0;
                for (int r = 0; r < rows; r++)
                {
                    currentFloorBase = floorHeight * r;
                    int modFloor = (r % floorPatternSize);

                    facadeDesign = data.facades[styleUnits[facadePatternReference[modFloor]].styleID];

                    bool isBlankWall = !facadeDesign.hasWindows;
                    if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                    {
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
                        BuildrBay[] bays;
                        int numberOfBayDesigns = 0;
                        if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                        {
                            numberOfBayDesigns = facadeDesign.bayPattern.Count;
                            bays = new BuildrBay[numberOfBayDesigns];
                            for (int i = 0; i < numberOfBayDesigns; i++)
                            {
                                bays[i] = data.bays[facadeDesign.bayPattern[i]];
                            }
                        }
                        else
                        {
                            bays = new[] { facadeDesign.simpleBay };
                            numberOfBayDesigns = 1;
                        }
                        //start with first window width - we'll be adding to this until we have filled the facade width
                        int it = 100;
                        while (true)
                        {
                            int patternModIndex = numberOfBays % numberOfBayDesigns;
                            float patternAddition = bays[patternModIndex].openingWidth + bays[patternModIndex].minimumBayWidth;
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

                        float windowXBase = 0;
                        for (int c = 0; c < numberOfBays; c++)
                        {
                            BuildrBay bayStyle;
                            if (facadeDesign.type == BuildrFacadeDesign.types.patterned)
                            {
                                int numberOfBayStyles = facadeDesign.bayPattern.Count;
                                bayStyle = bays[c % numberOfBayStyles];
                            }
                            else
                            {
                                bayStyle = facadeDesign.simpleBay;
                            }
                            float actualWindowSpacing = bayStyle.minimumBayWidth + perBayAdditionalSpacing;
                            float leftSpace = actualWindowSpacing * bayStyle.openingWidthRatio;
                            float rightSpace = actualWindowSpacing - leftSpace;
                            float openingSpace = bayStyle.openingWidth;

                            Vector3 bayDimensions;
                            int subMesh;
                            bool flipped;

                            if (!bayStyle.isOpening)
                            {
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.WallTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.WallTexture);
                                bayBase.x = windowXBase;
                                bayBase.y = currentFloorBase;
                                float bayWidth = (openingSpace + actualWindowSpacing);
                                float bayHeight = floorHeight;
                                bayDimensions = new Vector2(bayWidth, bayHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);

                                windowXBase += bayWidth;//move base vertor to next bay
                                continue;//bay filled - move onto next bay
                            }

                            float rowBottomHeight = ((floorHeight - bayStyle.openingHeight) * bayStyle.openingHeightRatio);
                            float rowTopHeight = (floorHeight - rowBottomHeight - bayStyle.openingHeight);

                            //Window
                            subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.OpeningBackTexture);
                            flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.OpeningBackTexture);
                            bayBase.x = windowXBase + leftSpace;
                            bayBase.y = currentFloorBase + rowBottomHeight;
                            bayDimensions = new Vector2(bayStyle.openingWidth, bayStyle.openingHeight);
                            DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);

                            //Column Left
                            if (leftSpace > 0)
                            {
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.ColumnTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.ColumnTexture);
                                bayBase.x = windowXBase;
                                bayBase.y = currentFloorBase + rowBottomHeight;
                                bayDimensions = new Vector2(leftSpace, bayStyle.openingHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);
                            }

                            //Column Right
                            if (rightSpace > 0)
                            {
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.ColumnTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.ColumnTexture);
                                bayBase.x = windowXBase + leftSpace + openingSpace;
                                bayBase.y = currentFloorBase + rowBottomHeight;
                                bayDimensions = new Vector2(rightSpace, bayStyle.openingHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);
                            }

                            //Row Bottom
                            if (rowBottomHeight > 0)
                            {
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.RowTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.RowTexture);
                                bayBase.x = windowXBase + leftSpace;
                                bayBase.y = currentFloorBase;
                                bayDimensions = new Vector2(openingSpace, rowBottomHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);
                            }

                            //Row Top
                            if (rowTopHeight > 0)
                            {
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.RowTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.RowTexture);
                                bayBase.x = windowXBase + leftSpace;
                                bayBase.y = currentFloorBase + rowBottomHeight + bayStyle.openingHeight;
                                bayDimensions = new Vector2(openingSpace, rowTopHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);
                            }

                            //Cross Left
                            if (leftSpace > 0)
                            {
                                //Cross Left Bottom
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.CrossTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.CrossTexture);
                                bayBase.x = windowXBase;
                                bayBase.y = currentFloorBase;
                                bayDimensions = new Vector2(leftSpace, rowBottomHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);

                                //Cross Left Top
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.CrossTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.CrossTexture);
                                bayBase.x = windowXBase;
                                bayBase.y = currentFloorBase + rowBottomHeight + bayStyle.openingHeight;
                                bayDimensions = new Vector2(leftSpace, rowTopHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);
                            }

                            //Cross Right
                            if (rightSpace > 0)
                            {
                                //Cross Left Bottom
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.CrossTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.CrossTexture);
                                bayBase.x = windowXBase + leftSpace + openingSpace;
                                bayBase.y = currentFloorBase;
                                bayDimensions = new Vector2(rightSpace, rowBottomHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);

                                //Cross Left Top
                                subMesh = bayStyle.GetTexture(BuildrBay.TextureNames.CrossTexture);
                                flipped = bayStyle.IsFlipped(BuildrBay.TextureNames.CrossTexture);
                                bayBase.x = windowXBase + leftSpace + openingSpace;
                                bayBase.y = currentFloorBase + rowBottomHeight + bayStyle.openingHeight;
                                bayDimensions = new Vector2(rightSpace, rowTopHeight);
                                DrawFacadeTexture(sourceTextures, bayBase, bayDimensions, subMesh, flipped, packedPosition);
                            }

                            windowXBase += leftSpace + openingSpace + rightSpace;//move base vertor to next bay
                        }
                    }
                    else
                    {
                        // windowless wall
                        int subMesh = facadeDesign.simpleBay.GetTexture(BuildrBay.TextureNames.WallTexture);
                        bool flipped = facadeDesign.simpleBay.IsFlipped(BuildrBay.TextureNames.WallTexture);
                        bayBase.x = 0;
                        bayBase.y = currentFloorBase;
                        Vector2 dimensions = new Vector2(facadeWidth, floorHeight);
                        DrawFacadeTexture(sourceTextures, bayBase, dimensions, subMesh, flipped, packedPosition);
                    }
                }
                facadeNumber++;
            }
        }
        LogTimer("generate facade textures");

        //add roof textures
        int numberOfroofTextures = roofTextures.Count;
        int scaledPadding = Mathf.FloorToInt(ATLAS_PADDING * packedScale);
        for (int i = 0; i < numberOfroofTextures; i++)
        {
            Rect roofTexturePosition = packedTexturePositions[i+facadeNumber];
            BuildrTexture bTexture = roofTextures[i];
            int roofTextureWidth = bTexture.texture.width;
            int roofTextureHeight = bTexture.texture.height;
            int targetTextureWidth = Mathf.RoundToInt(roofTexturePosition.width);
            int targetTextureHeight = Mathf.RoundToInt(roofTexturePosition.height);
            if(bTexture.maxUVTile == Vector2.zero)
            {
                LogTimer("BuildTextures: Skip texture " + bTexture.name + " as it appears it's not used");
                continue;
            }
            int sourceTextureWidth = Mathf.RoundToInt(targetTextureWidth / (bTexture.tiled ? bTexture.maxUVTile.x : bTexture.tiledX));
            int sourceTextureHeight = Mathf.RoundToInt(targetTextureHeight / (bTexture.tiled ? bTexture.maxUVTile.y : bTexture.tiledY));
            int sourceTextureSize = sourceTextureWidth * sourceTextureHeight;
            if(sourceTextureSize==0)
            {
                //Debug.Log(sourceTextureWidth+" "+sourceTextureHeight+" "+bTexture.tiledX+" "+bTexture.maxUVTile+" "+bTexture.tiledX+","+bTexture.tiledY);
                continue;
            }
            Color32[] roofColourArray = TextureScale.NearestNeighbourSample(((Texture2D)bTexture.texture).GetPixels32(),roofTextureWidth,roofTextureHeight,sourceTextureWidth,sourceTextureHeight);
            //Color32[] roofColourArray = bTexture.texture.GetPixels32();

            for (int x = 0; x < targetTextureWidth; x++)
            {
                for (int y = 0; y < targetTextureHeight; y++)
                {
                    int drawX = Mathf.FloorToInt(x + roofTexturePosition.x);
                    int drawY = Mathf.FloorToInt(y + roofTexturePosition.y);
                    int colourIndex = drawX + drawY * textureWidth;

                    int sx = x % sourceTextureWidth;
                    int sy = y % sourceTextureHeight;
                    int sourceIndex = sx + sy * sourceTextureWidth;
                    if (sourceIndex >= sourceTextureSize)
                        Debug.Log("Source Index too big " + sx + " " + sy + " " + sourceTextureWidth + " " + sourceTextureSize + " " + bTexture.maxUVTile+" "+bTexture.name);
                    Color32 sourceColour = roofColourArray[sourceIndex];
                    if(colourIndex>=textureSize)
                        Debug.Log("Output Index Too big "+drawX+" "+drawY+" "+colourIndex+" "+textureSize+" "+roofTexturePosition);
                    colourArray[colourIndex] = sourceColour;

                    //Padding
                    if (x == 0)
                    {
                        for (int p = 0; p < scaledPadding; p++)
                        {
                            colourArray[colourIndex - p] = sourceColour;
                        }
                    }
                    if (x == targetTextureWidth - 1)
                    {
                        for (int p = 0; p < scaledPadding; p++)
                        {
                            colourArray[colourIndex + p] = sourceColour;
                        }
                    }

                    if (y == 0)
                    {
                        for (int p = 0; p < scaledPadding; p++)
                        {
                            colourArray[colourIndex - (p*textureWidth)] = sourceColour;
                        }
                    }

                    if (y == targetTextureHeight - 1)
                    {
                        for (int p = 0; p < scaledPadding; p++)
                        {
                            colourArray[colourIndex + (p * textureWidth)] = sourceColour;
                        }
                    }
                }
            }
        }
        LogTimer("generate roof textures");
    }

    private static void DrawFacadeTexture(TexturePaintObject[] sourceTextures, Vector2 bayBase, Vector2 bayDimensions, int subMesh, bool flipped, Rect packedPosition)
    {
        int scaledPadding = Mathf.FloorToInt(ATLAS_PADDING * packedScale);
        int paintWidth = Mathf.RoundToInt(bayDimensions.x * PIXELS_PER_METER * packedScale);
        int paintHeight = Mathf.RoundToInt(bayDimensions.y * PIXELS_PER_METER*packedScale);

        TexturePaintObject paintObject = sourceTextures[subMesh];
        Color32[] sourceColours = paintObject.pixels;
        int sourceWidth = paintObject.width;
        int sourceHeight = paintObject.height;
        int sourceSize = sourceColours.Length;
        Vector2 textureStretch = Vector2.one*packedScale;
        if (!paintObject.tiled)
        {
            textureStretch.x = (float)sourceWidth / (float)paintWidth;
            textureStretch.y = (float)sourceHeight / (float)paintHeight;
        }
        int baseX = Mathf.RoundToInt((bayBase.x * PIXELS_PER_METER) * packedScale + packedPosition.x);
        int baseY = Mathf.RoundToInt((bayBase.y * PIXELS_PER_METER) * packedScale + packedPosition.y);
        int baseCood = baseX + baseY * textureWidth;

        //fill in a little bit more to cover rounding errors
        paintWidth++;
        paintHeight++;

        int useWidth = !flipped ? paintWidth : paintHeight;
        int useHeight = !flipped ? paintHeight : paintWidth;
        for (int px = 0; px < useWidth; px++)
        {
            for (int py = 0; py < useHeight; py++)
            {
                int paintPixelIndex = (!flipped) ? px + py * textureWidth : py + px * textureWidth;
                int six, siy;
                if (paintObject.tiled)
                {
                    six = px % sourceWidth;
                    siy = py % sourceHeight;
                }
                else
                {
                    six = Mathf.RoundToInt(px * textureStretch.x * paintObject.tiles.x) % sourceWidth;
                    siy = Mathf.RoundToInt(py * textureStretch.y * paintObject.tiles.y) % sourceHeight;
                }
                int sourceIndex = Mathf.Clamp(six + siy * sourceWidth, 0, sourceSize - 1);
                int pixelCoord = Mathf.Clamp(baseCood + paintPixelIndex, 0, textureSize - 1);
                Color32 sourceColour = sourceColours[sourceIndex];
                colourArray[pixelCoord] = sourceColour;


                //Padding
                if (bayBase.x == 0 && px == 0)
                {
                    for (int p = 1; p < scaledPadding; p++)
                    {
                        int paintCoord = pixelCoord - p;
                        if (paintCoord < 0)
                            break;
                        colourArray[paintCoord] = sourceColour;
                    }
                }
                if ((baseX + paintWidth) > packedPosition.xMax && px == useWidth - 1)
                {
                    for (int p = 1; p < scaledPadding; p++)
                    {
                        int paintCoord = pixelCoord + p;
                        if (paintCoord >= textureSize)
                            break;
                        colourArray[paintCoord] = sourceColour;
                    }
                }

                if (bayBase.y == 0 && py == 0)
                {
                    for (int p = 1; p < scaledPadding; p++)
                    {
                        int paintCoord = pixelCoord - (p * textureWidth);
                        if (paintCoord < 0)
                            break;
                        colourArray[paintCoord] = sourceColour;
                    }
                }

                if ((baseY + paintHeight) > packedPosition.yMax && py == useHeight - 1)
                {
                    for (int p = 1; p < scaledPadding; p++)
                    {
                        int paintCoord = pixelCoord + (p * textureWidth);
                        if (paintCoord >= textureSize)
                            break;
                        colourArray[paintCoord] = sourceColour;
                    }
                }
            }
        }
    }

    private static void TestRectColours()
    {
        Color[] colours = new []{new Color(1,0,0), new Color(1,0.6f,0.1f), new Color(1,1,0), new Color(0,1,0), new Color(0,1,0.5f), new Color(0,1,1), new Color(0,0.5f,1), new Color(0,0,1), new Color(0.5f,0,1), new Color(1,0,1)};
        int noColours = colours.Length;
        int numberOfRects = packedTexturePositions.Count;

        float[] rectSizes = new float[numberOfRects];
        for (int i = 0; i < numberOfRects; i++)
            rectSizes[i] = packedTexturePositions[i].width;// only judge based on width   // *rects[i].height;
        List<int> rectSizeOrder = new List<int>();
        for (int i = 0; i < numberOfRects; i++)
        {
            float largestSize = 0;
            int largestSizeIndex = 0;
            for (int j = 0; j < numberOfRects; j++)
            {
                if (rectSizeOrder.Contains(j))
                    continue;
                float thisSize = rectSizes[j];
                if (thisSize > largestSize)
                {
                    largestSize = thisSize;
                    largestSizeIndex = j;
                }
            }
            rectSizeOrder.Add(largestSizeIndex);
        }

        for(int r = 0; r < numberOfRects; r++)
        {
            Rect rect = packedTexturePositions[rectSizeOrder[r]];
            int x = (int)rect.x;
            int y = (int)rect.y;
            int widthX = (int)rect.width+x;
            int heightY = (int)rect.height+y;
            Color useCol = colours[r%noColours];
            for(int px = x; px < widthX; px++)
            {
                for(int py = y; py < heightY; py++)
                {
                    int colCoord = px + py * textureWidth;
                    colourArray[colCoord] = useCol;
                }
            }
        }
    }
}
