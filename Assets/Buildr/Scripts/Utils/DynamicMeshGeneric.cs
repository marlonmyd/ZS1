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
using System.Collections;
using System.Collections.Generic;

public class DynamicMeshGeneric {

	public string name = "";
	public Mesh mesh = new Mesh();
	public List<Vector3> vertices;
	public List<Vector2> uv;
	public List<int> triangles;
	public int subMeshes = 1;
	public Dictionary<int,List<int>> subTriangles;
	public Vector3[] tan1;
	public Vector3[] tan2;
	public Vector4[] tangents;
	private bool _built = false;
	
	public DynamicMeshGeneric()
	{
		vertices = new List<Vector3>();
		uv = new List<Vector2>();
		triangles = new List<int>();
		subTriangles = new Dictionary<int, List<int>>();
	}
	
	public void Build()
	{
		Build(false);
	}
	
	public void Build(bool calcTangents)
	{
		mesh.Clear();
		mesh.name=name;
		mesh.vertices=vertices.ToArray();
		mesh.uv=uv.ToArray();
		
		if(triangles.Count > 0)
		{
			mesh.triangles=triangles.ToArray();
		}else{
			mesh.subMeshCount = subMeshes;
			List<int> setTris = new List<int>();
			foreach(KeyValuePair<int,List<int>> triData in subTriangles)
			{
				mesh.SetTriangles(triData.Value.ToArray(), triData.Key);
				setTris.AddRange(triData.Value);
			}
		}
		
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		
		if(calcTangents)
		{
			SolveTangents();
			mesh.tangents = tangents;
		}
		
		_built=true;
	}
	
	public void Clear()
	{
		mesh.Clear();
		vertices.Clear();
		uv.Clear();
		triangles.Clear();
		subTriangles.Clear();
		_built = false;
		subMeshes = 0;
	}
	
	public int vertexCount
	{
		get{
			return vertices.Count;
		}
	}
	
	public bool built
	{
		get{return _built;}
	}
	
	public int size
	{
		get{return vertices.Count;}
	}
	
	public void SolveTangents()
    {
		tan1=new Vector3[size];
		tan2=new Vector3[size];
		tangents=new Vector4[size];
        int triangleCount = triangles.Count / 3;
		
        for(int a = 0; a < triangleCount; a+=3)
        {
            int i1 = triangles[a+0];
            int i2 = triangles[a+1];
            int i3 = triangles[a+2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);

            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }


        for (int a = 0; a < size; ++a)
        {
            Vector3 n = mesh.normals[a];
            Vector3 t = tan1[a];

            Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
            tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);

            tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
        }
    }
	
	public void AddData(Vector3[] verts, Vector2[] uvs, int[] tris, bool useTriangles, int subMesh)
	{
		vertices.AddRange(verts);
		uv.AddRange(uvs);
		
		if(useTriangles)
		{
			triangles.AddRange(tris);
		}else{
			if(!subTriangles.ContainsKey(subMesh))
				subTriangles.Add(subMesh, new List<int>());
			
			subTriangles[subMesh].AddRange(tris);
			
			if(subMesh+1 > subMeshes)
				subMeshes = subMesh+1;
		}
	}
	
	public void AddTri(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		AddTri(p0, p1, p2, 0);
	}
	
	public void AddTri(Vector3 p0, Vector3 p1, Vector3 p2, int subMesh)
	{
		int indiceBase = vertices.Count;
		vertices.Add(p0);
		vertices.Add(p1);
		vertices.Add(p2);
		
		uv.Add(new Vector2(0,0));
		uv.Add(new Vector2(1,0));
		uv.Add(new Vector2(0,1));
		
		if(!subTriangles.ContainsKey(subMesh))
			subTriangles.Add(subMesh, new List<int>());
			
		subTriangles[subMesh].Add(indiceBase);
		subTriangles[subMesh].Add(indiceBase+2);
		subTriangles[subMesh].Add(indiceBase+1);
		
		if(subMesh+1 > subMeshes)
			subMeshes = subMesh+1;
	}
	
	public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		AddPlane(p0, p1, p2, p3, 0);
	}
	
	public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subMesh)
	{
		int indiceBase = vertices.Count;
		vertices.Add(p0);
		vertices.Add(p1);
		vertices.Add(p2);
		vertices.Add(p3);
		
		uv.Add(new Vector2(0,0));
		uv.Add(new Vector2(1,0));
		uv.Add(new Vector2(0,1));
		uv.Add(new Vector2(1,1));
		
		if(!subTriangles.ContainsKey(subMesh))
			subTriangles.Add(subMesh, new List<int>());
			
		subTriangles[subMesh].Add(indiceBase);
		subTriangles[subMesh].Add(indiceBase+2);
		subTriangles[subMesh].Add(indiceBase+1);
	
		subTriangles[subMesh].Add(indiceBase+1);
		subTriangles[subMesh].Add(indiceBase+2);
		subTriangles[subMesh].Add(indiceBase+3);
		
		if(subMesh+1 > subMeshes)
			subMeshes = subMesh+1;
	}
}
