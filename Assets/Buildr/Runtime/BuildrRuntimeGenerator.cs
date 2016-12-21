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


public class BuildrRuntimeGenerator
{

    private static BuildrTexture wallTexture;
    private static BuildrTexture windowTexture;
    private static BuildrTexture doorTexture;
    private static BuildrTexture roofTexture;

    private static BuildrRuntimeConstraints constraints;

    public static void Generate(BuildrData data, BuildrRuntimeConstraints _constraints)
    {
        constraints = _constraints;
        uint seed = (uint)(constraints.useSeed ? constraints.seed : Random.Range(0, int.MaxValue));
        constraints.seed = (int)seed;//reassign value incase it's changed
        constraints.rgen = new RandomGen(seed);
        RandomGen rgen = constraints.rgen;
        //Debug.Log("Generate Seed "+seed);

        data.ResetData(constraints.constrainPlanByPlan);

        if (!constraints.constrainPlanByPlan)
            GenerateFloorPlan(data);
        else
            data.plan = constraints.plan;

        data.floorHeight = rgen.OutputRange(constraints.minimumFloorHeight, constraints.maximumFloorHeight);
        float minBuildingSize = (constraints.constrainHeight) ? constraints.minimumHeight : BuildrGenerateConstraints.MINIMUM_BUILDING_HEIGHT;
        float maxBuildingSize = (constraints.constrainHeight) ? constraints.maximumHeight : BuildrGenerateConstraints.MAXIMUM_BUILDING_HEIGHT;
        foreach(BuildrVolume volume in data.plan.volumes)
        {
            volume.height = rgen.OutputRange(minBuildingSize, maxBuildingSize);
            volume.numberOfFloors = Mathf.FloorToInt(volume.height / data.floorHeight);
        }

        //texture generation
        GetTextures(data);

        //facade generation
        GenerateFacades(data);

        //roof generation
        GenerateRoof(data);

        //building generation

        //build/optimise
    }

    private static void GenerateRoof(BuildrData data)
    {
        RandomGen rgen = constraints.rgen;
        BuildrRoofDesign roofDesign = new BuildrRoofDesign("default");

        List<int> availableRoofStyles = new List<int>();
        if(constraints.roofStyleFlat)availableRoofStyles.Add(0);
        if(constraints.roofStyleMansard)availableRoofStyles.Add(1);
        if(constraints.roofStyleBarrel)availableRoofStyles.Add(2);
        if(constraints.roofStyleGabled)availableRoofStyles.Add(3);
        if(constraints.roofStyleHipped)availableRoofStyles.Add(4);
        if(constraints.roofStyleLeanto)availableRoofStyles.Add(5);
        if(constraints.roofStyleSteepled)availableRoofStyles.Add(6);
        if(constraints.roofStyleSawtooth)availableRoofStyles.Add(7);

        System.Array A = System.Enum.GetValues(typeof(BuildrRoofDesign.styles));
        roofDesign.style = (BuildrRoofDesign.styles)A.GetValue(availableRoofStyles[rgen.OutputRange(0, availableRoofStyles.Count - 1)]);
        roofDesign.height = rgen.OutputRange(constraints.minimumRoofHeight, constraints.maximumRoofHeight);
        roofDesign.floorDepth = rgen.OutputRange(constraints.minimumRoofFloorDepth, constraints.maximumRoofFloorDepth);
        roofDesign.depth = rgen.OutputRange(Mathf.Min(constraints.minimumRoofDepth, roofDesign.floorDepth), constraints.maximumRoofDepth);

        roofDesign.hasDormers = (constraints.allowDormers) && (rgen.output <= constraints.dormerChance);
        roofDesign.dormerWidth = rgen.OutputRange(constraints.dormerMinimumWidth, constraints.dormerMaximumWidth);
        roofDesign.dormerHeight = rgen.OutputRange(constraints.dormerMinimumHeight, Mathf.Min(roofDesign.height,constraints.dormerMaximumHeight));
        roofDesign.dormerRoofHeight = rgen.OutputRange(constraints.dormerMinimumRoofHeight, constraints.dormerMaximumRoofHeight);
        roofDesign.minimumDormerSpacing = rgen.OutputRange(constraints.dormerMinimumSpacing, constraints.dormerMaximumSpacing);
        roofDesign.dormerHeightRatio = rgen.OutputRange(0.0f, 1.0f);

        roofDesign.parapet = (constraints.allowParapet) && (rgen.output <= constraints.parapetChance);
        roofDesign.parapetDesignWidth = rgen.OutputRange(constraints.parapetMinimumDesignWidth, constraints.parapetMaximumDesignWidth);
        roofDesign.parapetHeight = rgen.OutputRange(constraints.parapetMinimumHeight, constraints.parapetMaximumHeight);
        roofDesign.parapetFrontDepth = rgen.OutputRange(constraints.parapetMinimumFrontDepth, constraints.parapetMaximumFrontDepth);
        roofDesign.parapetBackDepth = rgen.OutputRange(constraints.parapetMinimumBackDepth, constraints.parapetMaximumBackDepth);

        if (roofDesign.style == BuildrRoofDesign.styles.sawtooth)
        {
            //make a new window texture for the sawtooth
        }

        data.roofs.Add(roofDesign);
    }

    private static void GenerateFacades(BuildrData data)
    {
        RandomGen rgen = constraints.rgen;

        //generate bays
        //blank
        BuildrBay blankBay = new BuildrBay("Blank");
        blankBay.isOpening = false;
        data.bays.Add(blankBay);
        //door
        BuildrBay doorBay = new BuildrBay("Door");
        doorBay.openingHeight = data.floorHeight * 0.9f;
        doorBay.openingHeightRatio = 0.0f;
        float doorWidth = (doorTexture.texture != null) ? (doorTexture.texture.width / (float)doorTexture.texture.height) * doorBay.openingHeight : doorBay.openingHeight*0.5f;
        doorBay.openingWidth = doorWidth;
        doorBay.openingDepth = rgen.OutputRange(0.0f, 0.3f);
        doorBay.SetTexture(BuildrBay.TextureNames.OpeningBackTexture, data.textures.IndexOf(doorTexture));
        data.bays.Add(doorBay);
        //ground window
        BuildrBay groundWindow = new BuildrBay("Ground Window");
        groundWindow.openingWidth = rgen.OutputRange(constraints.openingMinimumWidth, constraints.openingMaximumWidth);
        groundWindow.openingHeight = rgen.OutputRange(constraints.openingMinimumHeight, Mathf.Min(data.floorHeight,constraints.openingMinimumHeight));
        groundWindow.openingDepth = rgen.OutputRange(constraints.openingMinimumDepth, constraints.openingMaximumDepth);
        groundWindow.openingHeightRatio = 0.8f;
        data.bays.Add(groundWindow);

        BuildrTexture groundFloorWindowTexture = windowTexture.Duplicate("groundWindowTexture");
        groundFloorWindowTexture.tiled = false;
        groundFloorWindowTexture.tiledX = Mathf.RoundToInt(groundWindow.openingWidth / groundWindow.openingHeight);
        int groundtextureIndex = data.textures.Count;
        data.textures.Add(groundFloorWindowTexture);
        groundWindow.SetTexture(BuildrBay.TextureNames.OpeningBackTexture, groundtextureIndex);
        //other windows
        BuildrBay windowBay = new BuildrBay("Window");
        data.bays.Add(windowBay);
        //util window
        BuildrBay utilBay = new BuildrBay("Utility Window");
        data.bays.Add(utilBay);

        //generate facades
        BuildrFacadeDesign basicFacadeDesign = new BuildrFacadeDesign("default");
        basicFacadeDesign.simpleBay.openingWidth = rgen.OutputRange(constraints.openingMinimumWidth, constraints.openingMaximumWidth);
        basicFacadeDesign.simpleBay.openingHeight = rgen.OutputRange(constraints.openingMinimumHeight, Mathf.Min(data.floorHeight, constraints.openingMinimumHeight));
        basicFacadeDesign.simpleBay.openingDepth = rgen.OutputRange(constraints.openingMinimumDepth, constraints.openingMaximumDepth);
        basicFacadeDesign.simpleBay.minimumBayWidth = rgen.OutputRange(constraints.minimumBayMaximumWidth,constraints.minimumBayMaximumWidth);
        data.facades.Add(basicFacadeDesign);
        //ground floor with and without door
        BuildrFacadeDesign groundFloorDoor = new BuildrFacadeDesign("Ground Floor With Door");
        groundFloorDoor.type = BuildrFacadeDesign.types.patterned;
        int patternSize = rgen.OutputRange(1, 8);
        for(int i = 0; i < patternSize; i++)
            groundFloorDoor.bayPattern.Add(rgen.output>0.2f?2:0);
        groundFloorDoor.bayPattern.Insert(rgen.OutputRange(0, patternSize), 1);//insert door into pattern
        data.facades.Add(groundFloorDoor);

        BuildrPlan plan = data.plan;
        for(int v = 0; v < plan.numberOfVolumes; v++)
        {
            BuildrVolume volume = plan.volumes[v];
            int numberOfFloors = volume.numberOfFloors;
            volume.styles.Clear();
            for(int f = 0; f < volume.points.Count; f++)
            {
                int facadeIndex = volume.points[f];
                volume.styles.AddStyle(0, facadeIndex, numberOfFloors - 1);
                volume.styles.AddStyle(1, facadeIndex, 1);
            }
        }
    }

    private static void GenerateFloorPlan(BuildrData data)
    {
        RandomGen rgen = constraints.rgen;
        
        BuildrPlan plan = ScriptableObject.CreateInstance<BuildrPlan>();
        List<Vector2z> bounds = new List<Vector2z>();
        bounds.Add(new Vector2z(rgen.OutputRange(-5, -15), rgen.OutputRange(-5, -15)));
        bounds.Add(new Vector2z(rgen.OutputRange(5, 15), rgen.OutputRange(-5, -15)));
        bounds.Add(new Vector2z(rgen.OutputRange(5, 15), rgen.OutputRange(5, 15)));
        bounds.Add(new Vector2z(rgen.OutputRange(-5, -15), rgen.OutputRange(5, 15)));

        if(rgen.output < 0.25f)//should we split the volume?
        {
            float ratio = rgen.OutputRange(0.25f, 0.75f);
            bounds.Insert(1, Vector2z.Lerp(bounds[0], bounds[1], ratio));
            bounds.Insert(4, Vector2z.Lerp(bounds[3], bounds[4], ratio));

            plan.AddVolume(new []{bounds[0], bounds[1], bounds[4], bounds[5]});
            plan.AddVolume(1,2,new []{bounds[2], bounds[3]});
        }else
        {
            plan.AddVolume(bounds.ToArray());
        }

        data.plan = plan;
    }

    private static void GetTextures(BuildrData data)
    {
        //Load in textures from the RESOURCES folder
        wallTexture = new BuildrTexture("wall");//wall
        wallTexture.texture = (Texture2D)Resources.Load("Textures/BrickSmallBrown0078_2_S");
        data.textures.Add(wallTexture);

        windowTexture = new BuildrTexture("window");//window
        windowTexture.texture = (Texture2D)Resources.Load("Textures/WindowsHouseOld0260_S");
        data.textures.Add(windowTexture);

        roofTexture = new BuildrTexture("roof");//roof
        roofTexture.texture = (Texture2D)Resources.Load("Textures/RooftilesMetal0012_2_S");
        data.textures.Add(roofTexture);

        doorTexture = new BuildrTexture("door");//door
        doorTexture.texture = (Texture2D)Resources.Load("Textures/DoorsWoodPanelled0124_S");
        data.textures.Add(doorTexture);
    }
}
