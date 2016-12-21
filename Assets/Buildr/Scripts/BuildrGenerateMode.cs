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

[ExecuteInEditMode]
public class BuildrGenerateMode : MonoBehaviour
{

    private BuildrData _data = null;

    public GameObject model = null;
    public DynamicMeshGeneric mesh = null;
    public DynamicMeshGenericMultiMaterial colliderMesh = null;
    public DynamicMeshGenericMultiMaterial fullMesh = null;
    public MeshFilter meshFilt = null;
    public MeshRenderer meshRend = null;
    public List<Material> materials;// = new List<Material>();

    public bool showWireframe = true;
    public bool includeCollider = false;

    void Awake()
    {
        if (model == null)
        {
            model = new GameObject("model");
            model.transform.parent = transform;
            model.transform.localPosition = Vector3.zero;
        }

        if (!(meshFilt = model.GetComponent<MeshFilter>()))
            meshFilt = model.AddComponent<MeshFilter>();

        if (!(meshRend = model.GetComponent<MeshRenderer>()))
            meshRend = model.AddComponent<MeshRenderer>();

        materials = new List<Material>();
    }

    public BuildrData data
    {
        get
        {
            if (_data == null)
                _data = gameObject.GetComponent<BuildrData>();
            return _data;
        }
    }
}
