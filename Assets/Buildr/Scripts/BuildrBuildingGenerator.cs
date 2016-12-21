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
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class BuildrBuildingGenerator
{
    private static char[] filenameDelimiters = new[] { '\\', '/' };
//    private static List<string> xmlfilelist = new List<string>();
//    private static int selectedFile = 0;

    private static BuildrTexture wallTexture;
    private static BuildrTexture windowTexture;
    private static BuildrTexture doorTexture;
    private static BuildrTexture roofTexture;
    private static BuildrTexture groundFloorWindowTexture;

    public static void Generate(BuildrData data)
    {

        BuildrGenerateConstraints constraints = data.generatorConstraints;

        uint seed = (uint)(constraints.useSeed ? constraints.seed : Random.Range(0, int.MaxValue));
        constraints.seed = (int)seed;//reassign value incase it's changed
        constraints.rgen = new RandomGen(seed);
        RandomGen rgen = constraints.rgen;

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
    }

    public static void RefreshTextures(BuildrData data)
    {
        data.textures.Clear();
        BuildrGenerateConstraints constraints = data.generatorConstraints;
        uint seed = (uint)(constraints.useSeed ? constraints.seed : Random.Range(0, int.MaxValue));
        constraints.rgen = new RandomGen(seed);
        GetTextures(data);
    }

    private static void GenerateRoof(BuildrData data)
    {
        BuildrGenerateConstraints constraints = data.generatorConstraints;
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
        BuildrGenerateConstraints constraints = data.generatorConstraints;
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
        float doorWidth = (doorTexture.texture.width / (float)doorTexture.texture.height) * doorBay.openingHeight;
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

//        BuildrTexture groundFloorWindowTexture = windowTexture.Duplicate("groundWindowTexture");
        groundFloorWindowTexture.tiled = false;
        groundFloorWindowTexture.tiledX = Mathf.RoundToInt(groundWindow.openingWidth / groundWindow.openingHeight);
        int groundtextureIndex = data.textures.IndexOf(groundFloorWindowTexture);
//        data.textures.Add(groundFloorWindowTexture);
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
//        float roughBaySize = basicFacadeDesign.simpleBay.openingHeight + basicFacadeDesign.simpleBay.openingWidth;
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
        //couple of main facades
        //utility/back wall facade
        //maybe attic version

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
        BuildrGenerateConstraints constraints = data.generatorConstraints;
        RandomGen rgen = constraints.rgen;
        
        BuildrPlan plan = ScriptableObject.CreateInstance<BuildrPlan>();
        List<Vector2z> bounds = new List<Vector2z>();
        Rect floorplanBounds = new Rect(-15,-15,30,30);
        if(constraints.constrainPlanByArea)
            floorplanBounds = constraints.area;
        bounds.Add(new Vector2z(rgen.OutputRange(-5, floorplanBounds.xMin), rgen.OutputRange(-5, floorplanBounds.yMin)));
        bounds.Add(new Vector2z(rgen.OutputRange(5, floorplanBounds.xMax), rgen.OutputRange(-5, floorplanBounds.yMin)));
        bounds.Add(new Vector2z(rgen.OutputRange(5, floorplanBounds.xMax), rgen.OutputRange(5, floorplanBounds.yMax)));
        bounds.Add(new Vector2z(rgen.OutputRange(-5, floorplanBounds.xMin), rgen.OutputRange(5, floorplanBounds.yMax)));

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
        List<BuildrTexture> walltextures = new List<BuildrTexture>();
        List<BuildrTexture> windowtextures = new List<BuildrTexture>();
        List<BuildrTexture> doortextures = new List<BuildrTexture>();
        List<BuildrTexture> rooftextures = new List<BuildrTexture>();
        XmlNodeList xmlTextures = null;
        string textureFilePath = data.generatorConstraints.texturePackXML;

        if (File.Exists(textureFilePath))
        {
            XmlDocument xml = new XmlDocument();
            StreamReader sr = new StreamReader(textureFilePath);
            xml.LoadXml(sr.ReadToEnd());
            sr.Close();
            xmlTextures = xml.SelectNodes("data/textures/texture");

            if (xmlTextures == null)
                return;

            foreach (XmlNode node in xmlTextures)
            {
                string filepath = node["filepath"].FirstChild.Value;
                string[] splits = filepath.Split(filenameDelimiters);
                BuildrTexture bTexture = new BuildrTexture(splits[splits.Length - 1]);
#if UNITY_EDITOR
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(filepath, typeof(Texture2D));
#else
                Texture2D texture = new WWW (filepath).texture;
#endif
                bTexture.texture = texture;
                bTexture.tiled = node["tiled"].FirstChild.Value == "True";
                bTexture.patterned = node["patterned"].FirstChild.Value == "True";
                Vector2 tileUnitUV;
                tileUnitUV.x = float.Parse(node["tileUnitUVX"].FirstChild.Value);
                tileUnitUV.y = float.Parse(node["tileUnitUVY"].FirstChild.Value);
                bTexture.tileUnitUV = tileUnitUV;

                Vector2 textureUnitSize;
                textureUnitSize.x = float.Parse(node["textureUnitSizeX"].FirstChild.Value);
                textureUnitSize.y = float.Parse(node["textureUnitSizeY"].FirstChild.Value);
                bTexture.textureUnitSize = textureUnitSize;

                bTexture.tiledX = int.Parse(node["tiledX"].FirstChild.Value);
                bTexture.tiledY = int.Parse(node["tiledY"].FirstChild.Value);

                bTexture.door = node["door"].FirstChild.Value == "True";
                bTexture.window = node["window"].FirstChild.Value == "True";
                bTexture.wall = node["wall"].FirstChild.Value == "True";
                bTexture.roof = node["roof"].FirstChild.Value == "True";

                if (bTexture.wall) walltextures.Add(bTexture);
                if (bTexture.window) windowtextures.Add(bTexture);
                if (bTexture.door) doortextures.Add(bTexture);
                if (bTexture.roof) rooftextures.Add(bTexture);
            }
        }

        RandomGen rgen = data.generatorConstraints.rgen;
        wallTexture = walltextures[rgen.OutputRange(0, walltextures.Count - 1)];//wall
        data.textures.Add(wallTexture);

        windowTexture = windowtextures[rgen.OutputRange(0, windowtextures.Count - 1)];//window
        data.textures.Add(windowTexture);

        roofTexture = rooftextures[rgen.OutputRange(0, rooftextures.Count - 1)];//roof
        data.textures.Add(roofTexture);

        doorTexture = doortextures[rgen.OutputRange(0, doortextures.Count - 1)];//door
        data.textures.Add(doorTexture);

        groundFloorWindowTexture = windowtextures[rgen.OutputRange(0, windowtextures.Count - 1)];//window
        data.textures.Add(groundFloorWindowTexture);
    }
}
