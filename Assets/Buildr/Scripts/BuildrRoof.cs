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

public class BuildrRoof
{
    private static BuildrData data;
    private static BuildrTexture[] textures;
    private static DynamicMeshGenericMultiMaterialMesh mesh;

    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data)
    {
        Build(_mesh, _data, false);
    }

    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data, bool ignoreParapets)
    {
        data = _data;
        mesh = _mesh;
        textures = data.textures.ToArray();
        BuildrPlan plan = data.plan;

        int numberOfVolumes = data.plan.numberOfVolumes;
        for (int s = 0; s < numberOfVolumes; s++)
        {

            BuildrVolume volume = plan.volumes[s];
            BuildrRoofDesign design = data.roofs[volume.roofDesignID];

            BuildrRoofDesign.styles style = design.style;
            if (volume.points.Count != 4)
                if (design.style == BuildrRoofDesign.styles.leanto || design.style == BuildrRoofDesign.styles.sawtooth || design.style == BuildrRoofDesign.styles.barrel)
                    style = BuildrRoofDesign.styles.flat;//ignore style and just do a flat roof

            if (volume.points.Count != 4 && design.style == BuildrRoofDesign.styles.gabled)
                style = BuildrRoofDesign.styles.hipped;//ignore style and just do a hipped roof

            switch (style)
            {
                case BuildrRoofDesign.styles.flat:
                    FlatRoof(volume, design);
                    break;
                case BuildrRoofDesign.styles.mansard:
                    Mansard(volume, design);
                    if (design.hasDormers)
                        Dormers(volume, design);
                    break;
                case BuildrRoofDesign.styles.gabled:
                    Gabled(volume, design);
                    break;
                case BuildrRoofDesign.styles.hipped:
                    Hipped(volume, design);
                    break;
                case BuildrRoofDesign.styles.leanto:
                    LeanTo(volume, design);
                    break;
                case BuildrRoofDesign.styles.sawtooth:
                    Sawtooth(volume, design);
                    break;
                case BuildrRoofDesign.styles.barrel:
                    Barrel(volume, design);
                    break;
                case BuildrRoofDesign.styles.steepled:
                    Steeple(volume, design);
                    break;
            }

            if (design.parapet && !ignoreParapets)
                Parapet(volume, design);

        }

        data = null;
        mesh = null;
        textures = null;
    }

    private static void FlatRoof(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int volumeIndex = area.volumes.IndexOf(volume);
        int numberOfVolumePoints = volume.points.Count;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);

        //add top base of the flat roof
        Vector3[] newEndVerts = new Vector3[numberOfVolumePoints];
        Vector2[] newEndUVs = new Vector2[numberOfVolumePoints];
        int[] tris = new List<int>(area.GetTrianglesBySectorBase(volumeIndex)).ToArray();
        int roofTextureID = design.GetTexture(BuildrRoofDesign.textureNames.floor);
        BuildrTexture texture = data.textures[roofTextureID];
        for (int i = 0; i < numberOfVolumePoints; i++)
        {
            Vector2z point = area.points[volume.points[i]];
            newEndVerts[i] = point.vector3 + volumeFloorHeight;
            newEndUVs[i] = new Vector2(point.vector2.x / texture.textureUnitSize.x, point.vector2.y / texture.textureUnitSize.y);
        }

        AddData(newEndVerts, newEndUVs, tris, design.GetTexture(BuildrRoofDesign.textureNames.floor));
    }

    private static void Mansard(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfVolumePoints = volume.points.Count;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);

        //add top base of the flat roof
        Vector3[] topVerts = new Vector3[numberOfVolumePoints];
        Vector2[] topUVs = new Vector2[numberOfVolumePoints];
        int topTextureID = design.GetTexture(BuildrRoofDesign.textureNames.floorB);
        BuildrTexture texture = textures[topTextureID];

        for (int l = 0; l < numberOfVolumePoints; l++)
        {
            int indexA, indexB, indexA0, indexB0;
            Vector3 p0, p1, p00, p10;
            indexA = l;
            indexB = (l < numberOfVolumePoints - 1) ? l + 1 : 0;
            indexA0 = (l > 0) ? l - 1 : numberOfVolumePoints - 1;
            indexB0 = (l < numberOfVolumePoints - 2) ? l + 2 : l + 2 - numberOfVolumePoints;

            p0 = area.points[volume.points[indexA]].vector3;
            p1 = area.points[volume.points[indexB]].vector3;
            p00 = area.points[volume.points[indexA0]].vector3;
            p10 = area.points[volume.points[indexB0]].vector3;

            float facadeWidth = Vector3.Distance(p0, p1);
            Vector3 facadeDirection = (p1 - p0).normalized;
            Vector3 facadeDirectionLeft = (p0 - p00).normalized;
            Vector3 facadeDirectionRight = (p10 - p1).normalized;
            Vector3 facadeNormal = Vector3.Cross(facadeDirection, Vector3.up);
            Vector3 facadeNormalLeft = Vector3.Cross(facadeDirectionLeft, Vector3.up);
            Vector3 facadeNormalRight = Vector3.Cross(facadeDirectionRight, Vector3.up);

            float roofHeight = design.height;
            float baseDepth = design.floorDepth;
            float cornerLeftRad = Vector3.Angle(facadeDirection, -facadeDirectionLeft) * Mathf.Deg2Rad / 2;
            float cornerRightRad = Vector3.Angle(-facadeDirection, facadeDirectionRight) * Mathf.Deg2Rad / 2;
            float cornerDepthLeft = baseDepth / Mathf.Sin(cornerLeftRad);
            float cornerDepthRight = baseDepth / Mathf.Sin(cornerRightRad);
            float topDepth = design.depth;
            float cornerTopDepthLeft = topDepth / Mathf.Sin(cornerLeftRad);
            float cornerTopDepthRight = topDepth / Mathf.Sin(cornerRightRad);

            Vector3 pr = facadeDirection * facadeWidth;

            Vector3 leftDir = (facadeNormal + facadeNormalLeft).normalized;
            Vector3 rightDir = (facadeNormal + facadeNormalRight).normalized;

            p0 += volumeFloorHeight;
            p1 += volumeFloorHeight;

            Vector3 w0, w1, w2, w3, w4, w5;
            w0 = p0;
            w1 = p0 + pr;
            w2 = w0 + leftDir * cornerDepthLeft;
            w3 = w1 + rightDir * cornerDepthRight;
            w4 = w2 + leftDir * cornerTopDepthLeft + Vector3.up * roofHeight;
            w5 = w3 + rightDir * cornerTopDepthRight + Vector3.up * roofHeight;

            Vector3[] verts = new Vector3[6] { w0, w1, w2, w3, w4, w5 };
//            List<Vector2> uvs = new List<Vector2>();

            Vector2[] uvsFloor = BuildrProjectUVs.Project(new Vector3[4] { w0, w1, w2, w3 }, Vector2.zero, facadeNormal);
            if(baseDepth == 0)
                uvsFloor[3].x = facadeWidth;
            Vector2[] uvsMansard = BuildrProjectUVs.Project(new Vector3[3] { w2, w4, w5 }, uvsFloor[2], facadeNormal);

            Vector3[] vertsA = new Vector3[4] { verts[0], verts[1], verts[2], verts[3] };
            Vector2[] uvsA = new Vector2[4] { uvsFloor[0], uvsFloor[1], uvsFloor[2], uvsFloor[3] };
            int[] trisA = new int[6] { 1, 0, 2, 1, 2, 3 };
            int subMeshA = design.GetTexture(BuildrRoofDesign.textureNames.floor);
            mesh.AddData(vertsA, uvsA, trisA, subMeshA);

            Vector3[] vertsB = new Vector3[4] { verts[2], verts[3], verts[4], verts[5] };
            Vector2[] uvsB = new Vector2[4] { uvsFloor[2], uvsFloor[3], uvsMansard[1], uvsMansard[2] };
            int[] trisB = new int[6] { 0, 2, 1, 1, 2, 3 };
            int subMeshB = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
            mesh.AddData(vertsB, uvsB, trisB, subMeshB);

            //modify point for the top geometry
            Vector2z point = area.points[volume.points[l]];
            topVerts[l] = point.vector3 + volumeFloorHeight + Vector3.up * roofHeight + leftDir * (cornerDepthLeft + cornerTopDepthLeft);
            topUVs[l] = new Vector2(topVerts[l].x / texture.textureUnitSize.x, topVerts[l].z / texture.textureUnitSize.y);
        }

        Vector2z[] topVertV2z = new Vector2z[topVerts.Length];
        for (int i = 0; i < topVerts.Length; i++)
            topVertV2z[i] = new Vector2z(topVerts[i]);
        int[] topTris = EarClipper.Triangulate(topVertV2z);
        AddData(topVerts, topUVs, topTris, topTextureID);//top
    }

    private static void Barrel(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);

        Vector3[] points = new Vector3[4];
        //Vector3 ridgeVector;
        if (design.direction == 0)
        {
            points[0] = area.points[volume.points[0]].vector3 + volumeFloorHeight;
            points[1] = area.points[volume.points[3]].vector3 + volumeFloorHeight;
            points[2] = area.points[volume.points[1]].vector3 + volumeFloorHeight;
            points[3] = area.points[volume.points[2]].vector3 + volumeFloorHeight;
        }
        else
        {
            points[0] = area.points[volume.points[1]].vector3 + volumeFloorHeight;
            points[1] = area.points[volume.points[0]].vector3 + volumeFloorHeight;
            points[2] = area.points[volume.points[2]].vector3 + volumeFloorHeight;
            points[3] = area.points[volume.points[3]].vector3 + volumeFloorHeight;
        }

        int barrelSegments = design.barrelSegments + 1;
        Vector3[] bPoints = new Vector3[barrelSegments * 2];
        for (int i = 0; i < barrelSegments; i++)
        {
            float lerp = (float)i / (float)(barrelSegments - 1);
            Vector3 height = Mathf.Sin(lerp * Mathf.PI) * design.height * Vector3.up;
            float cosLerp = 1 - (Mathf.Cos((lerp) * Mathf.PI) + 1) / 2;
            bPoints[i] = Vector3.Lerp(points[0], points[1], cosLerp) + height;
            bPoints[i + barrelSegments] = Vector3.Lerp(points[2], points[3], cosLerp) + height;
        }

        int topIterations = barrelSegments - 1;
        int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
        bool flipped = design.IsFlipped(BuildrRoofDesign.textureNames.tiles);
        for (int t = 0; t < topIterations; t++)
            AddPlane(design, bPoints[t + 1], bPoints[t], bPoints[t + barrelSegments + 1], bPoints[t + barrelSegments], subMesh, flipped);//top

        Vector3 centerA = Vector3.Lerp(points[0], points[1], 0.5f);
        Vector3 centerB = Vector3.Lerp(points[2], points[3], 0.5f);
        for (int e = 0; e < topIterations; e++)
        {
            float lerpA = ((float)(e) / (float)(topIterations)) * Mathf.PI;
            float lerpB = ((float)(e + 1) / (float)(topIterations)) * Mathf.PI;
            Vector2[] uvs = new Vector2[3]{
				new Vector2(0.5f,0),
				new Vector2(1-(Mathf.Cos(lerpA)+1)/2,Mathf.Sin(lerpA)),
				new Vector2(1-(Mathf.Cos(lerpB)+1)/2,Mathf.Sin(lerpB))
			};

            Vector3[] verts = new Vector3[3] { centerA, bPoints[e], bPoints[e + 1] };
            int[] tri = new int[3] { 0, 2, 1 };
            AddData(verts, uvs, tri, design.GetTexture(BuildrRoofDesign.textureNames.window));

            verts = new Vector3[3] { centerB, bPoints[e + barrelSegments], bPoints[e + 1 + barrelSegments] };
            tri = new int[3] { 0, 1, 2 };
            AddData(verts, uvs, tri, design.GetTexture(BuildrRoofDesign.textureNames.window));
        }
    }

    private static void LeanTo(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);
        Vector3 ridgeVector = Vector3.up * design.height;

        int[] pointIndexes = new int[4];
        switch (design.direction)
        {
            case 0:
                pointIndexes = new int[4] { 0, 1, 2, 3 };
                break;
            case 1:
                pointIndexes = new int[4] { 1, 2, 3, 0 };
                break;
            case 2:
                pointIndexes = new int[4] { 2, 3, 0, 1 };
                break;
            case 3:
                pointIndexes = new int[4] { 3, 0, 1, 2 };
                break;
        }
        Vector3[] points = new Vector3[6];
        points[0] = area.points[volume.points[pointIndexes[0]]].vector3 + volumeFloorHeight;
        points[1] = area.points[volume.points[pointIndexes[1]]].vector3 + volumeFloorHeight;
        points[2] = area.points[volume.points[pointIndexes[2]]].vector3 + volumeFloorHeight;
        points[3] = area.points[volume.points[pointIndexes[3]]].vector3 + volumeFloorHeight;
        points[4] = area.points[volume.points[pointIndexes[2]]].vector3 + volumeFloorHeight + ridgeVector;
        points[5] = area.points[volume.points[pointIndexes[3]]].vector3 + volumeFloorHeight + ridgeVector;

        //top
        int subMeshTop = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
        bool flippedTop = design.IsFlipped(BuildrRoofDesign.textureNames.tiles);
        AddPlane(design, points[0], points[1], points[5], points[4], subMeshTop, flippedTop);

        //window
        int subMeshWindow = design.GetTexture(BuildrRoofDesign.textureNames.window);
        bool flippedWindow = design.IsFlipped(BuildrRoofDesign.textureNames.window);
        AddPlane(design, points[2], points[3], points[4], points[5], subMeshWindow, flippedWindow);

        //sides
        Vector3[] vertsA = new Vector3[3] { points[1], points[2], points[4] };
        Vector3[] vertsB = new Vector3[3] { points[0], points[3], points[5] };
        float uvWdith = Vector3.Distance(points[0], points[3]);
        float uvHeight = design.height;
        int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.wall);
        BuildrTexture texture = textures[subMesh];

        if (texture.tiled)
        {
            uvWdith *= (1.0f / texture.textureUnitSize.x);
            uvHeight *= (1.0f / texture.textureUnitSize.y);
            if (texture.patterned)
            {
                Vector2 uvunits = texture.tileUnitUV;
                uvWdith = Mathf.Ceil(uvWdith / uvunits.x) * uvunits.x;
                uvHeight = Mathf.Ceil(uvHeight / uvunits.y) * uvunits.y;
            }
        }
        else
        {
            uvWdith = texture.tiledX;
            uvHeight = texture.tiledY;
        }

        Vector2[] uvs = new Vector2[3] { new Vector2(0, 0), new Vector2(uvWdith, 0), new Vector2(uvWdith, uvHeight) };
        if (!design.IsFlipped(BuildrRoofDesign.textureNames.wall))
            uvs = new Vector2[3] { new Vector2(uvWdith, 0), new Vector2(0, 0), new Vector2(uvHeight, uvWdith / 2) };

        int[] triA = new int[3] { 1, 0, 2 };
        int[] triB = new int[3] { 0, 1, 2 };

        AddData(vertsA, uvs, triA, subMesh);
        AddData(vertsB, uvs, triB, subMesh);
    }

    private static void Sawtooth(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);
        Vector3 ridgeVector = Vector3.up * design.height;

        int[] pointIndexes = new int[4];
        switch (design.direction)
        {
            case 0:
                pointIndexes = new int[4] { 0, 1, 2, 3 };
                break;
            case 1:
                pointIndexes = new int[4] { 1, 2, 3, 0 };
                break;
            case 2:
                pointIndexes = new int[4] { 2, 3, 0, 1 };
                break;
            case 3:
                pointIndexes = new int[4] { 3, 0, 1, 2 };
                break;
        }
        Vector3[] basepoints = new Vector3[4];
        Vector3[] points = new Vector3[6];

        for (int i = 0; i < design.sawtoothTeeth; i++)
        {

            Vector3 toothBaseMovementA = (area.points[volume.points[pointIndexes[3]]].vector3 - area.points[volume.points[pointIndexes[0]]].vector3).normalized;
            float roofDepthA = Vector3.Distance(area.points[volume.points[pointIndexes[3]]].vector3, area.points[volume.points[pointIndexes[0]]].vector3);
            float toothDepthA = roofDepthA / design.sawtoothTeeth;
            Vector3 toothVectorA = toothBaseMovementA * toothDepthA;

            Vector3 toothBaseMovementB = (area.points[volume.points[pointIndexes[2]]].vector3 - area.points[volume.points[pointIndexes[1]]].vector3).normalized;
            float roofDepthB = Vector3.Distance(area.points[volume.points[pointIndexes[2]]].vector3, area.points[volume.points[pointIndexes[1]]].vector3);
            float toothDepthB = roofDepthB / design.sawtoothTeeth;
            Vector3 toothVectorB = toothBaseMovementB * toothDepthB;

            basepoints[0] = area.points[volume.points[pointIndexes[0]]].vector3 + toothVectorA * i;
            basepoints[1] = area.points[volume.points[pointIndexes[1]]].vector3 + toothVectorB * i;
            basepoints[2] = basepoints[1] + toothVectorB;
            basepoints[3] = basepoints[0] + toothVectorA;

            points[0] = basepoints[0] + volumeFloorHeight;
            points[1] = basepoints[1] + volumeFloorHeight;
            points[2] = basepoints[2] + volumeFloorHeight;
            points[3] = basepoints[3] + volumeFloorHeight;
            points[4] = basepoints[2] + volumeFloorHeight + ridgeVector;
            points[5] = basepoints[3] + volumeFloorHeight + ridgeVector;

            //top
            int subMeshTop = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
            bool flippedTop = design.IsFlipped(BuildrRoofDesign.textureNames.tiles);
            AddPlane(design, points[0], points[1], points[5], points[4], subMeshTop, flippedTop);

            //window
            int subMeshWindow = design.GetTexture(BuildrRoofDesign.textureNames.window);
            bool flippedWindow = design.IsFlipped(BuildrRoofDesign.textureNames.window);
            AddPlane(design, points[2], points[3], points[4], points[5], subMeshWindow, flippedWindow);

            //sides
            Vector3[] vertsA = new Vector3[3] { points[1], points[2], points[4] };
            Vector3[] vertsB = new Vector3[3] { points[0], points[3], points[5] };
            float uvWdith = Vector3.Distance(points[0], points[3]);
            float uvHeight = design.height;
            int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.wall);
            BuildrTexture texture = textures[subMesh];

            if (texture.tiled)
            {
                uvWdith *= (1.0f / texture.textureUnitSize.x);
                uvHeight *= (1.0f / texture.textureUnitSize.y);
                if (texture.patterned)
                {
                    Vector2 uvunits = texture.tileUnitUV;
                    uvWdith = Mathf.Ceil(uvWdith / uvunits.x) * uvunits.x;
                    uvHeight = Mathf.Ceil(uvHeight / uvunits.y) * uvunits.y;
                }
            }
            else
            {
                uvWdith = texture.tiledX;
                uvHeight = texture.tiledY;
            }

            Vector2[] uvs = new Vector2[3] { new Vector2(0, 0), new Vector2(uvWdith, 0), new Vector2(uvWdith, uvHeight) };
            int[] triA = new int[3] { 1, 0, 2 };
            int[] triB = new int[3] { 0, 1, 2 };
            AddData(vertsA, uvs, triA, subMesh);
            AddData(vertsB, uvs, triB, subMesh);

        }
    }

    private static void Steeple(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);
        Vector3 ridgeVector = Vector3.up * design.height;

        int numberOfVolumePoints = volume.points.Count;
        Vector3[] basePoints = new Vector3[numberOfVolumePoints];
        Vector3 centrePoint = Vector3.zero;
        for (int l = 0; l < numberOfVolumePoints; l++)
        {
            basePoints[l] = area.points[volume.points[l]].vector3 + volumeFloorHeight;
            centrePoint += area.points[volume.points[l]].vector3;
        }
        centrePoint = (centrePoint / numberOfVolumePoints) + volumeFloorHeight + ridgeVector;
        for (int l = 0; l < numberOfVolumePoints; l++)
        {
            int pointIndexA = l;
            int pointIndexB = (l < numberOfVolumePoints - 1) ? l + 1 : 0;
            Vector3[] verts = new Vector3[3] { basePoints[pointIndexA], basePoints[pointIndexB], centrePoint };
            float uvWdith = Vector3.Distance(basePoints[pointIndexA], basePoints[pointIndexB]);
            float uvHeight = design.height;
            int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
            BuildrTexture texture = textures[subMesh];

            if (texture.tiled)
            {
                uvWdith *= (1.0f / texture.textureUnitSize.x);
                uvHeight *= (1.0f / texture.textureUnitSize.y);
                if (texture.patterned)
                {
                    Vector2 uvunits = texture.tileUnitUV;
                    uvWdith = Mathf.Ceil(uvWdith / uvunits.x) * uvunits.x;
                    uvHeight = Mathf.Ceil(uvHeight / uvunits.y) * uvunits.y;
                }
            }
            else
            {
                uvWdith = texture.tiledX;
                uvHeight = texture.tiledY;
            }
            Vector2[] uvs = new Vector2[3] { new Vector2(-uvWdith / 2, 0), new Vector2(uvWdith / 2, 0), new Vector2(0, uvHeight) };
            int[] tri = new int[3] { 1, 0, 2 };
            AddData(verts, uvs, tri, subMesh);
        }
    }

    private static void Hipped(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfFloors = volume.numberOfFloors;
        float baseHeight = data.floorHeight * numberOfFloors;
        float roofHeight = design.height;
        int numberOfVolumePoints = volume.points.Count;
        int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.tiles);

        Vector2[] volumePoints = new Vector2[numberOfVolumePoints];
        for(int i = 0; i < numberOfVolumePoints; i++)
        {
            volumePoints[i] = area.points[volume.points[i]].vector2;
        }

        Vector2[][] meshData = StraightSkeleton.Calculate(volumePoints);
        Vector2[] triData = meshData[0];
        List<Vector2> interiorPoints = new List<Vector2>(meshData[1]);
        int numberOfVerts = triData.Length;
        Vector3[] verts = new Vector3[numberOfVerts];
        Vector2[] uvs = new Vector2[numberOfVerts];
        int[] tris = new int[numberOfVerts];
        for(int i = 0; i < triData.Length; i+=3)
        {
            Vector2 pa = triData[i];
            Vector2 pb = triData[i+1];
            Vector2 pc = triData[i+2];

            float ah = baseHeight + (interiorPoints.Contains(pa) ? roofHeight : 0);
            float bh = baseHeight + (interiorPoints.Contains(pb) ? roofHeight : 0);
            float ch = baseHeight + (interiorPoints.Contains(pc) ? roofHeight : 0);

            Vector3 v0 = new Vector3(pa.x, ah, pa.y);
            Vector3 v1 = new Vector3(pb.x, bh, pb.y);
            Vector3 v2 = new Vector3(pc.x, ch, pc.y);

            verts[i] = v0;
            verts[i+1] = v1;
            verts[i+2] = v2;

            Vector3 roofBaseDir = (interiorPoints.Contains(pc)) ? v1 - v0 : v2 - v1;
            Vector3 roofBaseNormal = Vector3.Cross(roofBaseDir, Vector3.up);
            Vector2[] uvsMansard = BuildrProjectUVs.Project(new Vector3[3] { v0, v1, v2 }, Vector2.zero, roofBaseNormal);

            uvs[i] = uvsMansard[0];
            uvs[i + 1] = uvsMansard[1];
            uvs[i + 2] = uvsMansard[2];

            tris[i] = i;
            tris[i + 1] = i+2;
            tris[i + 2] = i+1;
        }

        AddData(verts,uvs,tris,subMesh);

        /*Vector3 ridgeVector = Vector3.up * design.height;

        Vector3[] basePoints = new Vector3[4];
        if (design.direction == 0)
        {
            basePoints[0] = area.points[volume.points[0]].vector3 + volumeFloorHeight;
            basePoints[1] = area.points[volume.points[1]].vector3 + volumeFloorHeight;
            basePoints[2] = area.points[volume.points[2]].vector3 + volumeFloorHeight;
            basePoints[3] = area.points[volume.points[3]].vector3 + volumeFloorHeight;
        }
        else
        {
            basePoints[0] = area.points[volume.points[1]].vector3 + volumeFloorHeight;
            basePoints[1] = area.points[volume.points[2]].vector3 + volumeFloorHeight;
            basePoints[2] = area.points[volume.points[3]].vector3 + volumeFloorHeight;
            basePoints[3] = area.points[volume.points[0]].vector3 + volumeFloorHeight;
        }
        Vector3 centrePoint = Vector3.zero;
        for (int l = 0; l < 4; l++)
            centrePoint += area.points[volume.points[l]].vector3;
        centrePoint = (centrePoint / 4) + volumeFloorHeight + ridgeVector;

        Vector3 r0 = Vector3.Lerp(basePoints[0], basePoints[1], 0.5f) + ridgeVector;
        Vector3 r1 = Vector3.Lerp(basePoints[2], basePoints[3], 0.5f) + ridgeVector;
        Vector3 ridgeDirection = (r1 - r0);
        float roofLength = Vector3.Distance(r0, r1);
        float ridgeLengthA = Vector3.Distance(basePoints[0], basePoints[1]);
        if (ridgeLengthA > roofLength)
            ridgeLengthA = roofLength;
        float ridgeLengthB = Vector3.Distance(basePoints[2], basePoints[3]);
        if (ridgeLengthB > roofLength)
            ridgeLengthB = roofLength;
        r0 += ridgeDirection.normalized * ridgeLengthA / 2;
        r1 += -ridgeDirection.normalized * ridgeLengthB / 2;

        int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
        bool flipped = design.IsFlipped(BuildrRoofDesign.textureNames.tiles);
        AddPlane(design, basePoints[0], r0, basePoints[3], r1, subMesh, flipped);//top
        AddPlane(design, basePoints[2], r1, basePoints[1], r0, subMesh, flipped);//top

        Vector3[] vertsA = new Vector3[3] { basePoints[0], basePoints[1], r0 };
        Vector3[] vertsB = new Vector3[3] { basePoints[2], basePoints[3], r1 };
        float uvWdithA = Vector3.Distance(basePoints[0], basePoints[1]);
        float uvWdithB = Vector3.Distance(basePoints[2], basePoints[3]);
        float uvHeight = design.height;
        BuildrTexture texture = textures[subMesh];

        if (texture.tiled)
        {
            uvWdithA *= (1.0f / texture.textureUnitSize.x);
            uvWdithB *= (1.0f / texture.textureUnitSize.x);
            uvHeight *= (1.0f / texture.textureUnitSize.y);
            if (texture.patterned)
            {
                Vector2 uvunits = texture.tileUnitUV;
                uvWdithA = Mathf.Ceil(uvWdithA / uvunits.x) * uvunits.x;
                uvWdithB = Mathf.Ceil(uvWdithB / uvunits.x) * uvunits.x;
                uvHeight = Mathf.Ceil(uvHeight / uvunits.y) * uvunits.y;
            }
        }
        else
        {
            uvWdithA = texture.tiledX;
            uvWdithB = texture.tiledX;
            uvHeight = texture.tiledY;
        }
        Vector2[] uvsA = new Vector2[3] { new Vector2(-uvWdithA / 2, 0), new Vector2(uvWdithA / 2, 0), new Vector2(0, uvHeight) };
        Vector2[] uvsB = new Vector2[3] { new Vector2(-uvWdithB / 2, 0), new Vector2(uvWdithB / 2, 0), new Vector2(0, uvHeight) };
        int[] tri = new int[3] { 1, 0, 2 };
        AddData(vertsA, uvsA, tri, subMesh);
        AddData(vertsB, uvsB, tri, subMesh);*/
    }

    private static void Gabled(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);
        Vector3 ridgeVector = Vector3.up * design.height;

        Vector3[] basePoints = new Vector3[4];
        if (design.direction == 0)
        {
            basePoints[0] = area.points[volume.points[0]].vector3 + volumeFloorHeight;
            basePoints[1] = area.points[volume.points[1]].vector3 + volumeFloorHeight;
            basePoints[2] = area.points[volume.points[2]].vector3 + volumeFloorHeight;
            basePoints[3] = area.points[volume.points[3]].vector3 + volumeFloorHeight;
        }
        else
        {
            basePoints[0] = area.points[volume.points[1]].vector3 + volumeFloorHeight;
            basePoints[1] = area.points[volume.points[2]].vector3 + volumeFloorHeight;
            basePoints[2] = area.points[volume.points[3]].vector3 + volumeFloorHeight;
            basePoints[3] = area.points[volume.points[0]].vector3 + volumeFloorHeight;
        }
        Vector3 centrePoint = Vector3.zero;
        for (int l = 0; l < 4; l++)
            centrePoint += area.points[volume.points[l]].vector3;
        centrePoint = (centrePoint / 4) + volumeFloorHeight + ridgeVector;

        Vector3 r0 = Vector3.Lerp(basePoints[0], basePoints[1], 0.5f) + ridgeVector;
        Vector3 r1 = Vector3.Lerp(basePoints[2], basePoints[3], 0.5f) + ridgeVector;

        int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
        bool flipped = design.IsFlipped(BuildrRoofDesign.textureNames.tiles);
        AddPlane(design, basePoints[0], r0, basePoints[3], r1, subMesh, flipped);//top
        AddPlane(design, basePoints[2], r1, basePoints[1], r0, subMesh, flipped);//top

        Vector3[] vertsA = new Vector3[3] { basePoints[0], basePoints[1], r0 };
        Vector3[] vertsB = new Vector3[3] { basePoints[2], basePoints[3], r1 };
        float uvWdithA = Vector3.Distance(basePoints[0], basePoints[1]);
        float uvWdithB = Vector3.Distance(basePoints[2], basePoints[3]);
        float uvHeight = design.height;
        subMesh = design.GetTexture(BuildrRoofDesign.textureNames.wall);
        BuildrTexture texture = textures[subMesh];

        if (texture.tiled)
        {
            uvWdithA *= (1.0f / texture.textureUnitSize.x);
            uvWdithB *= (1.0f / texture.textureUnitSize.x);
            uvHeight *= (1.0f / texture.textureUnitSize.y);
            if (texture.patterned)
            {
                Vector2 uvunits = texture.tileUnitUV;
                uvWdithA = Mathf.Ceil(uvWdithA / uvunits.x) * uvunits.x;
                uvWdithB = Mathf.Ceil(uvWdithB / uvunits.x) * uvunits.x;
                uvHeight = Mathf.Ceil(uvHeight / uvunits.y) * uvunits.y;
            }
        }
        else
        {
            uvWdithA = texture.tiledX;
            uvWdithB = texture.tiledX;
            uvHeight = texture.tiledY;
        }
        Vector2[] uvsA = new Vector2[3] { new Vector2(-uvWdithA / 2, 0), new Vector2(uvWdithA / 2, 0), new Vector2(0, uvHeight) };
        Vector2[] uvsB = new Vector2[3] { new Vector2(-uvWdithB / 2, 0), new Vector2(uvWdithB / 2, 0), new Vector2(0, uvHeight) };
        int[] tri = new int[3] { 1, 0, 2 };
        AddData(vertsA, uvsA, tri, subMesh);
        AddData(vertsB, uvsB, tri, subMesh);

    }

    private static void Parapet(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int volumeIndex = area.volumes.IndexOf(volume);
        int numberOfVolumePoints = volume.points.Count;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);

        for (int l = 0; l < numberOfVolumePoints; l++)
        {

            int indexA, indexB, indexA0, indexB0;
            Vector3 p0, p1, p00, p10;
            indexA = l;
            indexB = (l < numberOfVolumePoints - 1) ? l + 1 : 0;
            indexA0 = (l > 0) ? l - 1 : numberOfVolumePoints - 1;
            indexB0 = (l < numberOfVolumePoints - 2) ? l + 2 : l + 2 - numberOfVolumePoints;

            int adjacentFloorHeight = area.GetFacadeFloorHeight(volumeIndex, volume.points[indexA], volume.points[indexB]);
            bool leftParapet = area.GetFacadeFloorHeight(volumeIndex, volume.points[indexA0], volume.points[indexA]) < numberOfFloors;
            bool rightParapet = area.GetFacadeFloorHeight(volumeIndex, volume.points[indexB], volume.points[indexB0]) < numberOfFloors;

            if (adjacentFloorHeight >= numberOfFloors)
                continue;//do not draw a roof edge

            p0 = area.points[volume.points[indexA]].vector3;
            p1 = area.points[volume.points[indexB]].vector3;
            p00 = area.points[volume.points[indexA0]].vector3;
            p10 = area.points[volume.points[indexB0]].vector3;

            float facadeWidth = Vector3.Distance(p0, p1);
            Vector3 facadeDirection = (p1 - p0).normalized;
            Vector3 facadeDirectionLeft = (p0 - p00).normalized;
            Vector3 facadeDirectionRight = (p10 - p1).normalized;
            Vector3 facadeNormal = Vector3.Cross(facadeDirection, Vector3.up);
            Vector3 facadeNormalLeft = Vector3.Cross(facadeDirectionLeft, Vector3.up);
            Vector3 facadeNormalRight = Vector3.Cross(facadeDirectionRight, Vector3.up);

            float parapetHeight = design.parapetHeight;
            float parapetFrontDepth = design.parapetFrontDepth;
            float parapetBackDepth = design.parapetBackDepth;

            Vector3 w0, w1, w2, w3, w4, w5, w6, w7;
            Vector3 pr = facadeDirection * facadeWidth;
            Vector3 pu = Vector3.up * parapetHeight;

            Vector3 pbdl, pbdr, pfdl, pfdr;
            if (leftParapet)
            {
                pbdl = -(facadeNormal + facadeNormalLeft).normalized * parapetFrontDepth;
                pfdl = (facadeNormal + facadeNormalLeft).normalized * parapetBackDepth;
            }
            else
            {
                pbdl = facadeDirectionLeft * parapetFrontDepth;
                pfdl = -facadeDirectionLeft * parapetBackDepth;
            }
            if (rightParapet)
            {
                pbdr = -(facadeNormal + facadeNormalRight).normalized * parapetFrontDepth;
                pfdr = (facadeNormal + facadeNormalRight).normalized * parapetBackDepth;
            }
            else
            {
                pbdr = -facadeDirectionRight * parapetFrontDepth;
                pfdr = facadeDirectionRight * parapetBackDepth;
            }

            p0 += volumeFloorHeight;
            p1 += volumeFloorHeight;

            w2 = p0 + pbdl;//front left
            w3 = p0 + pr + pbdr;//front right
            w0 = p0 + pfdl;//back left
            w1 = p0 + pr + pfdr;//back right
            w6 = p0 + pbdl + pu;//front left top
            w7 = p0 + pr + pbdr + pu;//front right top
            w4 = p0 + pfdl + pu;//back left top
            w5 = p0 + pr + pfdr + pu;//back right top

            int subMesh = design.GetTexture(BuildrRoofDesign.textureNames.parapet);
            bool flipped = design.IsFlipped(BuildrRoofDesign.textureNames.parapet);
            AddPlane(design, w1, w0, w5, w4, subMesh, flipped);//front
            AddPlaneComplex(design, w6, w7, w4, w5, subMesh, flipped, facadeNormal);//top
            AddPlane(design, w2, w3, w6, w7, subMesh, flipped);//back

            if (parapetFrontDepth > 0)
                AddPlaneComplex(design, w3, w2, w1, w0, subMesh, flipped, facadeNormal);//bottom

            if (!leftParapet)
                AddPlane(design, w0, w2, w4, w6, subMesh, flipped);//left cap

            if (!rightParapet)
                AddPlane(design, w3, w1, w7, w5, subMesh, flipped);//left cap
        }
    }

    private static void Dormers(BuildrVolume volume, BuildrRoofDesign design)
    {
        BuildrPlan area = data.plan;
        int numberOfVolumePoints = volume.points.Count;
        int numberOfFloors = volume.numberOfFloors;
        float floorHeight = data.floorHeight;
        Vector3 volumeFloorHeight = Vector3.up * (numberOfFloors * floorHeight);

        for (int l = 0; l < numberOfVolumePoints; l++)
        {
            int indexA, indexB, indexA0, indexB0;
            Vector3 p0, p1, p00, p10;
            indexA = l;
            indexB = (l < numberOfVolumePoints - 1) ? l + 1 : 0;
            indexA0 = (l > 0) ? l - 1 : numberOfVolumePoints - 1;
            indexB0 = (l < numberOfVolumePoints - 2) ? l + 2 : l + 2 - numberOfVolumePoints;

            p0 = area.points[volume.points[indexA]].vector3;
            p1 = area.points[volume.points[indexB]].vector3;
            p00 = area.points[volume.points[indexA0]].vector3;
            p10 = area.points[volume.points[indexB0]].vector3;

            Vector3 facadeDirection = (p1 - p0).normalized;
            Vector3 facadeDirectionLeft = (p0 - p00).normalized;
            Vector3 facadeDirectionRight = (p10 - p1).normalized;
            Vector3 facadeNormal = Vector3.Cross(p1 - p0, Vector3.up).normalized;
            Vector3 facadeNormalLeft = Vector3.Cross(facadeDirectionLeft, Vector3.up);
            Vector3 facadeNormalRight = Vector3.Cross(facadeDirectionRight, Vector3.up);
            Vector3 leftDir = (facadeNormal + facadeNormalLeft).normalized;
            Vector3 rightDir = (facadeNormal + facadeNormalRight).normalized;

            float windowBottom = (design.height - design.dormerHeight) * design.dormerHeightRatio;

            float baseDepth = design.floorDepth;
            float cornerLeftRad = Vector3.Angle(facadeDirection, -facadeDirectionLeft) * Mathf.Deg2Rad / 2;
            float cornerRightRad = Vector3.Angle(-facadeDirection, facadeDirectionRight) * Mathf.Deg2Rad / 2;
            float cornerDepthLeft = baseDepth / Mathf.Sin(cornerLeftRad);
            float cornerDepthRight = baseDepth / Mathf.Sin(cornerRightRad);
            float topDepth = design.depth;
            float cornerTopDepthLeft = topDepth / Mathf.Sin(cornerLeftRad);
            float cornerTopDepthRight = topDepth / Mathf.Sin(cornerRightRad);

            float dormerDepth = design.depth * (design.dormerHeight / design.height);
            float windowBottomRat = Mathf.Lerp(0, 1 - design.dormerHeight / design.height, design.dormerHeightRatio);

            p0 += volumeFloorHeight + leftDir * cornerDepthLeft;
            p1 += volumeFloorHeight + rightDir * cornerDepthRight;

            float leftStartTopRad = Vector3.Angle(facadeDirectionLeft, facadeDirection) * Mathf.Deg2Rad * 0.5f;
            float leftStartMargin = cornerTopDepthLeft * Mathf.Sin(leftStartTopRad);

            float rightStartTopRad = Vector3.Angle(facadeDirection, facadeDirectionRight) * Mathf.Deg2Rad * 0.5f;
            float rightStartMargin = cornerTopDepthRight * Mathf.Sin(rightStartTopRad);

            Vector3 dormerStartPosition = leftDir * (windowBottomRat * cornerTopDepthLeft) + facadeDirection * (leftStartMargin);
            Vector3 dormerEndPosition = rightDir * (windowBottomRat * cornerTopDepthRight) - facadeDirection * (rightStartMargin + design.dormerWidth);
            float dormerPositionWidth = Vector3.Distance((p0 + dormerStartPosition), (p1 + dormerEndPosition));
            int numberOfWindows = Mathf.FloorToInt((dormerPositionWidth) / (design.dormerWidth + design.minimumDormerSpacing));
            float actualWindowSpacing = (dormerPositionWidth - (numberOfWindows * design.dormerWidth)) / (numberOfWindows + 1);
            numberOfWindows++;//add the final window

            Vector3 dormerWidthVector = facadeDirection * design.dormerWidth;
            Vector3 dormerHeightVectorA = Vector3.up * (design.dormerHeight - design.dormerRoofHeight);
            Vector3 dormerHeightVectorB = Vector3.up * design.dormerHeight;
            Vector3 dormerDepthVector = facadeNormal * dormerDepth;
            Vector3 dormerSpace = facadeDirection * (actualWindowSpacing + design.dormerWidth);
            Vector3 dormerSpacer = facadeDirection * (actualWindowSpacing);
            Vector3 dormerYPosition = Vector3.up * windowBottom;

            Vector3 w0, w1, w2, w3, w4, w5, w6, w7, w8, w9;
            for (int i = 0; i < numberOfWindows; i++)
            {
                w0 = p0 + dormerSpace * (i) + dormerStartPosition + dormerYPosition + dormerSpacer * 0.5f;
                w1 = w0 + dormerWidthVector;
                w2 = w0 + dormerHeightVectorA;
                w3 = w1 + dormerHeightVectorA;
                w4 = w0 + dormerWidthVector / 2 + dormerHeightVectorB;

                w5 = w0 + dormerDepthVector;
                w6 = w1 + dormerDepthVector;
                w7 = w2 + dormerDepthVector;
                w8 = w3 + dormerDepthVector;
                w9 = w4 + dormerDepthVector;

                int subMeshwindow = design.GetTexture(BuildrRoofDesign.textureNames.window);
                int subMeshwall = design.GetTexture(BuildrRoofDesign.textureNames.wall);
                int subMeshtiles = design.GetTexture(BuildrRoofDesign.textureNames.tiles);
                bool flippedwall = design.IsFlipped(BuildrRoofDesign.textureNames.wall);
                bool flippedtiles = design.IsFlipped(BuildrRoofDesign.textureNames.tiles);

                AddPlane(design, w1, w6, w3, w8, subMeshwall, flippedwall);//side
                AddPlane(design, w5, w0, w7, w2, subMeshwall, flippedwall);//side
                AddPlane(design, w3, w8, w4, w9, subMeshtiles, flippedtiles);//roof
                AddPlane(design, w7, w2, w9, w4, subMeshtiles, flippedtiles);//roof

                Vector3[] verts = new Vector3[5] { w0, w1, w2, w3, w4 };
                float roofBottom = (design.dormerHeight - design.dormerRoofHeight) / design.dormerHeight;
                Vector2[] uvs = new Vector2[5]{
					new Vector2(0,0),
					new Vector2(1,0),
					new Vector2(0,roofBottom),
					new Vector2(1,roofBottom),
					new Vector2(0.5f,1),
				};
                int[] tris = new int[9] { 1, 0, 2, 1, 2, 3, 2, 4, 3 };
                mesh.AddData(verts, uvs, tris, subMeshwindow);
            }
        }
    }

    private static void AddData(Vector3[] verts, Vector2[] uvs, int[] tris, int subMesh)
    {
        mesh.AddData(verts, uvs, tris, subMesh);
    }

    private static void AddPlane(BuildrRoofDesign design, Vector3 w0, Vector3 w1, Vector3 w2, Vector3 w3, int subMesh, bool flipped)
    {
        int textureSubmesh = subMesh;
        BuildrTexture texture = textures[textureSubmesh];
        Vector2 uvSize = Vector2.one;
        if (texture.tiled)
        {
            float planeWidth = Vector3.Distance(w0, w1);
            float planeHeight = Vector3.Distance(w0, w2);
            uvSize = new Vector2(planeWidth * (1.0f / texture.textureUnitSize.x), planeHeight * (1.0f / texture.textureUnitSize.y));
            if (texture.patterned)
            {
                Vector2 uvunits = texture.tileUnitUV;
                uvSize.x = Mathf.Ceil(uvSize.x / uvunits.x) * uvunits.x;
                uvSize.y = Mathf.Ceil(uvSize.y / uvunits.y) * uvunits.y;
            }
        }
        else
        {
            uvSize.x = texture.tiledX;
            uvSize.y = texture.tiledY;
        }
        if (!flipped)
        {
            mesh.AddPlane(w0, w1, w2, w3, Vector2.zero, uvSize, textureSubmesh);
        }
        else
        {
            uvSize = new Vector2(uvSize.y, uvSize.x);
            mesh.AddPlane(w0, w1, w2, w3, Vector2.zero, uvSize, textureSubmesh);
        }
    }

    private static void AddPlaneComplex(BuildrRoofDesign design, Vector3 w0, Vector3 w1, Vector3 w2, Vector3 w3, int subMesh, bool flipped, Vector3 facadeNormal)
    {
        Vector3[] verts = new Vector3[4] { w0, w1, w2, w3 };
        Vector2[] uvs = BuildrProjectUVs.Project(verts, Vector2.zero, facadeNormal);
        int[] tris = new int[6] { 1, 0, 2, 1, 2, 3 };

        float xDiff = uvs[1].x - uvs[0].x;
        float yDiff = uvs[1].y - uvs[0].y;
        for (int p = 1; p < 4; p++)
        {
            Vector2 uv = uvs[p];
            float ratio = (uv.x - uvs[0].x) / xDiff;
            uv.y += -yDiff * ratio;
            uvs[p] = uv;
        }

        mesh.AddData(verts, uvs, tris, subMesh);
    }
}
