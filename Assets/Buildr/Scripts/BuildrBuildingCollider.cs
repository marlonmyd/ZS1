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

public class BuildrBuildingCollider
{

    public static void Build(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data)
    {
        switch(_data.generateCollider)
        {
                case BuildrData.ColliderGenerationModes.None:
                return;
//                break;

                case BuildrData.ColliderGenerationModes.Simple:
                BuildSimple(_mesh,_data);
                break;

                case BuildrData.ColliderGenerationModes.Complex:
                BuildrBuilding.Build(_mesh,_data);
                BuildrRoof.Build(_mesh, _data);
                int numberOfVolumes = _data.plan.numberOfVolumes;
                for(int v = 0; v < numberOfVolumes; v++)
                {
                    BuildrInteriors.Build(_mesh,_data,v);
                    BuildrStairs.Build(_mesh,_data,v,BuildrStairs.StairModes.Flat,false);
                }
                _mesh.CollapseSubmeshes();
                break;
        }
    }

    private static void BuildSimple(DynamicMeshGenericMultiMaterialMesh _mesh, BuildrData _data) 
    {
        BuildrData data = _data;
        DynamicMeshGenericMultiMaterialMesh mesh = _mesh;
        BuildrPlan plan = data.plan;

        int facadeIndex = 0;
        int numberOfVolumes = data.plan.numberOfVolumes;

        //Build Floor
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

        //Build ROOF
        DynamicMeshGenericMultiMaterialMesh dynMeshRoof = new DynamicMeshGenericMultiMaterialMesh();
        dynMeshRoof.subMeshCount = data.textures.Count;
        BuildrRoof.Build(dynMeshRoof, data, true);
        mesh.AddData(dynMeshRoof.vertices, dynMeshRoof.uv, dynMeshRoof.triangles, 0);

        Vector3 foundationVector = Vector3.down * data.foundationHeight;
        //Build facades
        for (int s = 0; s < numberOfVolumes; s++)
        {
            BuildrVolume volume = plan.volumes[s];
            int numberOfVolumePoints = volume.points.Count;

            for (int l = 0; l < numberOfVolumePoints; l++)
            {
                int indexA = l;
                int indexB = (l < numberOfVolumePoints - 1) ? l + 1 : 0;
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

                if(floorBase == 0)
                {
                    w0 += foundationVector;
                    w1 += foundationVector;
                }

                mesh.AddPlane(w0, w1, w2, w3, Vector2.zero, Vector2.zero, 0);
                facadeIndex++;
            }
        }

        data = null;
        mesh = null;
    }
}
