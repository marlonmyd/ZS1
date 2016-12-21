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

public class BuildrStairs
{
    public enum StairModes
    {
        Flat,
        Stepped
    }

    private static BuildrData data;
    private static BuildrTexture[] textures;
    private static DynamicMeshGenericMultiMaterialMesh mesh;
    
    //TODO: functions to find out minimum footprint of stairwell for checking against cores?

    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data, int volumeIndex, StairModes stairMode, bool zeroMesh)
    {
        data = _data;
        mesh = _mesh;
        mesh.name = "Stairs Mesh Volume " + volumeIndex;
        textures = data.textures.ToArray();
        
//        BuildrFacadeDesign facadeDesign = data.facades[0];
        BuildrPlan plan = data.plan;
        BuildrVolume volume = plan.volumes[volumeIndex];
        float floorHeight = data.floorHeight;
//        Vector3 floorHeightVector = Vector3.up * floorHeight;

        if(!volume.generateStairs)
            return;
        
        //Calculate the internal floor plan points
        int numberOfVolumePoints = volume.points.Count;
        Vector2z[] volumePoints = new Vector2z[numberOfVolumePoints];
        for(int i = 0; i < numberOfVolumePoints; i++)
            volumePoints[i] = plan.points[volume.points[i]];
        List<Rect> volumeCores = new List<Rect>();
//        List<int> linkedPoints = new List<int>();
        foreach (Rect core in plan.cores)
        {
            Vector2z coreCenter = new Vector2z(core.center);
            if (BuildrUtils.PointInsidePoly(coreCenter, volumePoints))
                volumeCores.Add(core);
        }
        int numberOfVolumeCores = volumeCores.Count;
        int numberOfFloors = volume.numberOfFloors + volume.numberOfBasementFloors;
        float basementBaseHeight = (volume.numberOfBasementFloors) * floorHeight;//plus one for the initial floor
        float staircaseWidth = volume.staircaseWidth;
        float stairwellWidth = staircaseWidth * 2.5f;
        float stairwellDepth = staircaseWidth * 2 + Mathf.Sqrt(floorHeight+floorHeight);
        float staircaseThickness = Mathf.Sqrt(volume.stepHeight * volume.stepHeight + volume.stepHeight * volume.stepHeight);

        Vector3 flightVector = floorHeight * Vector3.up;
        Vector3 staircaseWidthVector = staircaseWidth * Vector3.right;
        Vector3 staircaseDepthVector = stairwellDepth * 0.5f * Vector3.forward;
        Vector3 stairHeightVector = staircaseThickness * Vector3.up;
        Vector3 landingDepthVector = staircaseWidth * Vector3.forward;

        //Texture submeshes
        int floorSubmesh = volume.stairwellFloorTexture;
        int stepSubmesh = volume.stairwellStepTexture;
        int wallSubmesh = volume.stairwellWallTexture;
        int ceilingSubmesh = volume.stairwellCeilingTexture;

        volume.stairBaseVector.Clear();
        for(int c = 0; c < numberOfVolumeCores; c++)
        {
            Rect coreBounds = volumeCores[c];
            Vector3 stairBaseVector = new Vector3(-stairwellWidth / 2, 0, -stairwellDepth/2);
            Vector3 stairPosition = new Vector3(coreBounds.xMin, -basementBaseHeight, coreBounds.yMin) - stairBaseVector;
            
            for(int f = 0; f < numberOfFloors; f++)
            {
                Vector3 flightBaseVector = stairBaseVector + (flightVector * f);
                if(!zeroMesh) flightBaseVector += stairPosition;

                Vector3 landingStart0 = flightBaseVector;
                Vector3 landingStart1 = landingStart0 + staircaseWidthVector*2.5f;
                Vector3 landingStart2 = landingStart0 + landingDepthVector;
                Vector3 landingStart3 = landingStart1 + landingDepthVector;
                Vector3 landingStart4 = landingStart0 - stairHeightVector;
                Vector3 landingStart5 = landingStart1 - stairHeightVector;
                Vector3 landingStart6 = landingStart2 - stairHeightVector;
                Vector3 landingStart7 = landingStart3 - stairHeightVector;
                if(f > 0)
                {
                    AddPlane(landingStart1, landingStart0, landingStart3, landingStart2, floorSubmesh, false, Vector2.zero, new Vector2(staircaseWidth * 2.5f, staircaseWidth));//top
                    AddPlane(landingStart4, landingStart5, landingStart6, landingStart7, ceilingSubmesh, false, Vector2.zero, new Vector2(staircaseWidth * 2.5f, staircaseWidth));//bottom
                    AddPlane(landingStart0, landingStart1, landingStart4, landingStart5, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth * 2.5f, staircaseThickness));//frontside
                    AddPlane(landingStart3, landingStart2, landingStart7, landingStart6, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth * 2.5f, staircaseThickness));//backside
                    AddPlane(landingStart0, landingStart4, landingStart2, landingStart6, wallSubmesh, false, Vector2.zero, new Vector2(staircaseThickness, staircaseWidth));//sideleft
                    AddPlane(landingStart5, landingStart1, landingStart7, landingStart3, wallSubmesh, false, Vector2.zero, new Vector2(staircaseThickness, staircaseWidth));//sideright
                }

                if(f < numberOfFloors - 1)
                {
                    Vector3 bottom0 = landingStart2;
                    Vector3 bottom1 = landingStart2 + staircaseWidthVector;
                    Vector3 bottom2 = bottom0 - stairHeightVector;
                    Vector3 bottom3 = bottom1 - stairHeightVector;

                    Vector3 top0 = bottom0 + (flightVector * 0.5f) + staircaseDepthVector;
                    Vector3 top1 = bottom1 + (flightVector * 0.5f) + staircaseDepthVector;
                    Vector3 top2 = top0 - stairHeightVector;
                    Vector3 top3 = top1 - stairHeightVector;

                    Vector3 bottomB0 = top1 + Vector3.right * staircaseWidth*0.5f;
                    Vector3 bottomB1 = bottomB0 + staircaseWidthVector;
                    Vector3 bottomB2 = bottomB0 - stairHeightVector;
                    Vector3 bottomB3 = bottomB1 - stairHeightVector;

                    Vector3 topB0 = bottomB0 + (flightVector * 0.5f) - staircaseDepthVector;
                    Vector3 topB1 = bottomB1 + (flightVector * 0.5f) - staircaseDepthVector;
                    Vector3 topB2 = topB0 - stairHeightVector;
                    Vector3 topB3 = topB1 - stairHeightVector;

                    float stairHypontenuse = Vector3.Distance(bottom0, top0);
                    int numberOfSteps = Mathf.CeilToInt((floorHeight / 2.0f) / volume.stepHeight);

                    switch(stairMode)
                    {
                        case StairModes.Flat:
                            //flight A
                            AddPlane(bottom1, bottom0, top1, top0, stepSubmesh, false, Vector2.zero, new Vector2(1, numberOfSteps));//step face
                            AddPlane(bottom3, bottom1, top3, top1, ceilingSubmesh, false, Vector2.zero, new Vector2(staircaseWidth, stairHypontenuse));//underside
                            AddPlane(bottom0, bottom2, top0, top2, wallSubmesh, false, new Vector2(bottom2.z, bottom2.y), new Vector2(top0.z, top0.y));//left side
                            AddPlane(bottom2, bottom3, top2, top3, wallSubmesh, false, new Vector2(bottom3.z, bottom3.y), new Vector2(top2.z, top2.y));//right side
                            //flight B
                            AddPlane(bottomB0, bottomB1, topB0, topB1, stepSubmesh, false, Vector2.zero, new Vector2(1, numberOfSteps));//step face
                            AddPlane(bottomB1, bottomB3, topB1, topB3, ceilingSubmesh, false, Vector2.zero, new Vector2(staircaseWidth, stairHypontenuse));//underside
                            AddPlane(bottomB2, bottomB0, topB2, topB0, wallSubmesh, false, Vector2.zero, Vector2.one);//left side
                            AddPlane(bottomB3, bottomB2, topB3, topB2, wallSubmesh, false, Vector2.zero, Vector2.one);//right side
                            break;

                        case StairModes.Stepped:

                            float stepHypontenuse = stairHypontenuse / numberOfSteps;
                            float stairAngle = Mathf.Atan2(floorHeight, stairwellDepth);
                            float stepDepth = Mathf.Cos(stairAngle) * stepHypontenuse;
                            float skipStep = (stepDepth / (numberOfSteps - 1));
                            stepDepth += skipStep;
                            float stepRiser = Mathf.Sin(stairAngle) * stepHypontenuse;

                            //flight one
                            float lerpIncrement = 1.0f / numberOfSteps;
                            float lerpIncrementB = 1.0f / (numberOfSteps-1);
                            for (int s = 0; s < numberOfSteps-1; s++)
                            {
                                float lerpValue = lerpIncrement * s;
                                Vector3 skipStepVector = Vector3.forward * (skipStep * s);
                                Vector3 s0 = Vector3.Lerp(bottom1, top1, lerpValue) + skipStepVector;
                                Vector3 s1 = Vector3.Lerp(bottom0, top0, lerpValue) + skipStepVector;
                                Vector3 s2 = s0 + Vector3.up * stepRiser;
                                Vector3 s3 = s1 + Vector3.up * stepRiser;
                                Vector3 s4 = s2 + Vector3.forward * stepDepth;
                                Vector3 s5 = s3 + Vector3.forward * stepDepth;
                                AddPlane(s0, s1, s2, s3, wallSubmesh, false, Vector2.zero, new Vector2(1,staircaseWidth));
                                AddPlane(s2, s3, s4, s5, stepSubmesh, false, Vector2.zero, new Vector2(1,staircaseWidth));
                                //sides
                                float lerpValueB = lerpIncrementB * s;
                                Vector3 s6 = Vector3.Lerp(bottom3, top3, lerpValueB);
                                Vector3 s7 = Vector3.Lerp(bottom3, top3, lerpValueB + lerpIncrementB);
                                AddPlane(s2, s4, s6, s7, wallSubmesh, false, Vector2.zero, new Vector2(stepDepth,staircaseThickness));

                                Vector3 s8 = Vector3.Lerp(bottom2, top2, lerpValueB);
                                Vector3 s9 = Vector3.Lerp(bottom2, top2, lerpValueB + lerpIncrementB);
                                AddPlane(s5, s3, s9, s8, wallSubmesh, false, Vector2.zero, new Vector2(stepDepth,staircaseThickness));
                            }
                            AddPlane(bottom2, bottom3, top2, top3, ceilingSubmesh, false, Vector2.zero, Vector2.one);

                            //flight two
                            for(int s = 0; s < numberOfSteps-1; s++)
                            {
                                float lerpValue = lerpIncrement * s;
                                Vector3 skipStepVector = -Vector3.forward * (skipStep * s);
                                Vector3 s0 = Vector3.Lerp(bottomB0, topB0, lerpValue) + skipStepVector;
                                Vector3 s1 = Vector3.Lerp(bottomB1, topB1, lerpValue) + skipStepVector;
                                Vector3 s2 = s0 + Vector3.up * stepRiser;
                                Vector3 s3 = s1 + Vector3.up * stepRiser;
                                Vector3 s4 = s2 - Vector3.forward * stepDepth;
                                Vector3 s5 = s3 - Vector3.forward * stepDepth;
                                AddPlane(s0, s1, s2, s3, wallSubmesh, false, Vector2.zero, new Vector2(1, staircaseWidth));
                                AddPlane(s2, s3, s4, s5, stepSubmesh, false, Vector2.zero, new Vector2(1, staircaseWidth));
                                float lerpValueB = lerpIncrementB * s;
                                //sides
                                Vector3 s6 = Vector3.Lerp(bottomB2, topB2, lerpValueB);
                                Vector3 s7 = Vector3.Lerp(bottomB2, topB2, lerpValueB + lerpIncrementB);
                                AddPlane(s2, s4, s6, s7, wallSubmesh, false, Vector2.zero, new Vector2(stepDepth, staircaseThickness));

                                Vector3 s8 = Vector3.Lerp(bottomB3, topB3, lerpValueB);
                                Vector3 s9 = Vector3.Lerp(bottomB3, topB3, lerpValueB + lerpIncrementB);
                                AddPlane(s5, s3, s9, s8, wallSubmesh, false, Vector2.zero, new Vector2(stepDepth, staircaseThickness));
                            }
                            AddPlane(bottomB3, bottomB2, topB3, topB2, ceilingSubmesh, false, Vector2.zero, Vector2.one);

                            break;
                    }

                    Vector3 landingEnd0 = top0 + landingDepthVector;
                    Vector3 landingEnd1 = bottomB1 + landingDepthVector;
                    Vector3 landingEnd2 = landingEnd0 - stairHeightVector;
                    Vector3 landingEnd3 = landingEnd1 - stairHeightVector;
                    Vector3 landingEnd4 = top0 - stairHeightVector;
                    Vector3 landingEnd5 = bottomB1 - stairHeightVector;

                    AddPlane(bottomB1, top0, landingEnd1, landingEnd0, floorSubmesh, false, Vector2.zero, new Vector2(staircaseWidth*2.5f, staircaseWidth));//top
                    AddPlane(landingEnd4, landingEnd5, landingEnd2, landingEnd3, ceilingSubmesh, false, Vector2.zero, new Vector2(staircaseWidth * 2.5f, staircaseWidth));//bottom
                    AddPlane(top0, bottomB1, landingEnd4, landingEnd5, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth * 2.5f, staircaseThickness));//frontside
                    AddPlane(landingEnd1, landingEnd0, landingEnd3, landingEnd2, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth * 2.5f, staircaseThickness));//backside
                    AddPlane(landingEnd0, top0, landingEnd2, landingEnd4, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth, staircaseThickness));//sideleft
                    AddPlane(bottomB1, landingEnd1, landingEnd5, landingEnd3, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth, staircaseThickness));//sideright
                }
            }
            //Center wall
            float coreHeight = (numberOfFloors * floorHeight);
            Vector3 coreHeightVector = Vector3.up * coreHeight;
            Vector3 corePosition = (zeroMesh) ? Vector3.zero : stairPosition;
            Vector3 w0 = new Vector3(-staircaseWidth / 4.0f, 0, -(stairwellDepth - (staircaseWidth * 2)) / 2.0f) + corePosition;
            Vector3 w1 = w0 + Vector3.right * staircaseWidth/2;
            Vector3 w2 = w0 + staircaseDepthVector;
            Vector3 w3 = w1 + staircaseDepthVector;
            Vector3 w4 = w0 + coreHeightVector;
            Vector3 w5 = w1 + coreHeightVector;
            Vector3 w6 = w2 + coreHeightVector;
            Vector3 w7 = w3 + coreHeightVector;

            AddPlane(w1, w0, w5, w4, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth / 2, coreHeight));//
            AddPlane(w3, w1, w7, w5, wallSubmesh, false, Vector2.zero, new Vector2(stairwellDepth / 2, coreHeight));//
            AddPlane(w2, w3, w6, w7, wallSubmesh, false, Vector2.zero, new Vector2(staircaseWidth / 2, coreHeight));//
            AddPlane(w0, w2, w4, w6, wallSubmesh, false, Vector2.zero, new Vector2(stairwellDepth / 2, coreHeight));//

            int it = 100;
            while(volume.stairBaseVector.Count < mesh.meshCount)
            {
                if (zeroMesh)
                    volume.stairBaseVector.Add(stairPosition);
                else
                    volume.stairBaseVector.Add(Vector3.zero);
                it--;
                if(it == 0)
                    break;
            }

            if(c<numberOfVolumeCores-1)
                mesh.ForceNewMesh();
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
        for (int i = 0; i < numberOfUVs; i++)
        {
            uvs[i].Scale(uvScale);
            if (flipped)
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
            uvs[i] = new Vector2(verts[i].x * uvScale.x, verts[i].z * uvScale.y);
            if (flipped)
            {
                Vector2 flippedUV = new Vector2(uvs[i].y, uvs[i].x);
                uvs[i] = flippedUV;
            }
        }

        mesh.AddData(verts, uvs, tris, textureSubmesh);
    }
}
