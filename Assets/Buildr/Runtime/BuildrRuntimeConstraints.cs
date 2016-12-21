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

public class BuildrRuntimeConstraints : ScriptableObject
{

    public const float MINIMUM_BUILDING_HEIGHT = 2.5f;
    public const float MAXIMUM_BUILDING_HEIGHT = 100.0f;

    public bool useSeed = false;
    private int _seed = 0;
    public RandomGen rgen;

    public float minimumFloorHeight = 2.0f;
    public float maximumFloorHeight = 3.5f;

    public bool constrainHeight = false;
    private float _minimumHeight = 12.5f;
    private float _maximumHeight = 40.0f;

    public bool constrainFloorNumber = false;
    public int floorNumber = 4;

    public bool constrainPlanByArea = false;
    public Rect area = new Rect(-10, -10, 20, 20);

    public bool constrainPlanByPlan = false;
    public BuildrPlan plan;

    public bool constrainDesign = false;

    public float minimumHeight { get { return _minimumHeight; } set { _minimumHeight = Mathf.Max(value, MINIMUM_BUILDING_HEIGHT); } }

    public float maximumHeight { get { return _maximumHeight; } set { _maximumHeight = Mathf.Max(value, MINIMUM_BUILDING_HEIGHT); } }

    public int seed { get { return _seed; } set { _seed = Mathf.Max(value, 0); } }


    public bool rowStyled = false;
    public bool columnStyled = false;
    public bool externalAirConUnits = false;
    public bool splitLevel = false;
    public bool taperedLevels = false;
    public bool singleLevel = false;
    public bool atticDesign = false;
    public bool shopGroundFloor = false;

    //TEXTURE CONSTRAINTS
    public List<BuildrTexture> walltextures = new List<BuildrTexture>();
    public List<BuildrTexture> windowtextures = new List<BuildrTexture>();
    public List<BuildrTexture> doortextures = new List<BuildrTexture>();
    public List<BuildrTexture> rooftextures = new List<BuildrTexture>();

    //FACADE CONSTRAINTS
    public float openingMinimumWidth = 0.75f;
    public float openingMaximumWidth = 3.25f;
    public float openingMinimumHeight = 1.25f;
    public float openingMaximumHeight = 2.75f;
    public float minimumBayMinimumWidth = 0.25f;
    public float minimumBayMaximumWidth = 1.95f;
    public float openingMinimumDepth = 0.3f;
    public float openingMaximumDepth = 0.1f;
    public float facadeMinimumDepth = -0.2f;
    public float facadeMaximumDepth = 0.2f;
    //probably won't use the following...
    public float columnMinimumDepth = 0.0f;
    public float columnMaximumDepth = 0.0f;
    public float rowMinimumDepth = 0.0f;
    public float rowMaximumDepth = 0.0f;
    public float crossMinimumDepth = 0.0f;
    public float crossMaximumDepth = 0.0f;

    //ROOF CONSTRAINTS

    public float minimumRoofHeight = 1.0f;
    public float maximumRoofHeight = 2.0f;
    public float minimumRoofDepth = 0.10f;//used for mansard roofs
    public float maximumRoofDepth = 1.0f;//used for mansard roofs
    public float minimumRoofFloorDepth = 0.0f;//used for mansard roofs
    public float maximumRoofFloorDepth = 1.0f;//used for mansard roofs

    public bool roofStyleFlat = true;
    public bool roofStyleMansard = true;
    public bool roofStyleBarrel = true;
    public bool roofStyleGabled = true;
    public bool roofStyleHipped = true;
    public bool roofStyleLeanto = true;
    public bool roofStyleSteepled = true;
    public bool roofStyleSawtooth = true;
    public bool allowParapet = true;
    public float parapetChance = 0.5f;
    public bool allowDormers = true;
    public float dormerChance = 0.5f;

    public float dormerMinimumWidth = 0.5f;
    public float dormerMaximumWidth = 1.0f;
    public float dormerMinimumHeight = 0.5f;
    public float dormerMaximumHeight = 1.0f;
    public float dormerMinimumRoofHeight = 0.5f;
    public float dormerMaximumRoofHeight = 1.0f;
    public float dormerMinimumSpacing = 0.25f;
    public float dormerMaximumSpacing = 3.0f;

    public float parapetMinimumDesignWidth = 0.0f;
    public float parapetMaximumDesignWidth = 1.0f;
    public float parapetMinimumHeight = 0.25f;
    public float parapetMaximumHeight = 0.75f;
    public float parapetMinimumFrontDepth = 0.1f;
    public float parapetMaximumFrontDepth = 0.4f;
    public float parapetMinimumBackDepth = 0.2f;
    public float parapetMaximumBackDepth = 0.4f;

    public void Init()
    {
        plan = CreateInstance<BuildrPlan>();

        rgen = new RandomGen((useSeed) ? (uint)_seed : (uint)Random.Range(0, int.MaxValue));
    }


}
