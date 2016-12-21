// BuildR
// Available on the Unity3D Asset Store
// Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

public class BuildrXMLImporter
{
    private static string valueTrue = "True";

    //data should be already created, attached to the gameobject as a component and init run.
    public static void Import(string XMLPath, BuildrData data)
    {
        Debug.Log("Import " + XMLPath);
        XmlDocument xml = new XmlDocument();
        using(StreamReader sr = new StreamReader(XMLPath))
        {
            xml.LoadXml(sr.ReadToEnd());
        }

        float xmlVersionNumber = float.Parse(xml["buildr"].Attributes["version"].Value);

        if(xmlVersionNumber != BuildrVersion.NUMBER)
            Debug.Log("Version upgrade " + xmlVersionNumber + " > " + BuildrVersion.NUMBER);


        data.name = xml["buildr"]["buildingname"].FirstChild.Value;
        data.floorHeight = float.Parse(xml["buildr"]["floorheight"].FirstChild.Value);
        data.foundationHeight = float.Parse(xml["buildr"]["foundationHeight"].FirstChild.Value);
        data.drawUnderside = bool.Parse(xml["buildr"]["drawUnderside"].FirstChild.Value);
        data.generateCollider = (BuildrData.ColliderGenerationModes)System.Enum.Parse(typeof(BuildrData.ColliderGenerationModes), xml["buildr"]["generateCollider"].FirstChild.Value);
        data.renderInteriors = bool.Parse(xml["buildr"]["renderInteriors"].FirstChild.Value);
        data.interiorCeilingHeight = float.Parse(xml["buildr"]["interiorCeilingHeight"].FirstChild.Value);
        data.cullBays = bool.Parse(xml["buildr"]["cullBays"].FirstChild.Value);

        //volume points
        foreach(XmlNode node in xml.SelectNodes("buildr/plan/points/point"))
        {
            data.plan.points.Add(new Vector2z(node["x"].FirstChild.Value, node["z"].FirstChild.Value));
        }

        //volume cores
        foreach (XmlNode node in xml.SelectNodes("buildr/plan/cores/core"))
        {
            data.plan.cores.Add(new Rect(float.Parse(node["xMin"].FirstChild.Value), float.Parse(node["yMin"].FirstChild.Value), float.Parse(node["width"].FirstChild.Value), float.Parse(node["height"].FirstChild.Value)));
        }

        ImportVolumes(xml,data);
        ImportFacades(xml,data);
        ImportRoofs(xml,data);
        ImportTextures(xml,data);
        ImportBays(xml,data);
        ImportDetails(xml,data);
    }

    public static void ImportTextures(string XMLPath, BuildrData data)
    {
        Debug.Log("Import Textures " + XMLPath);
        XmlDocument xml = new XmlDocument();
        using (StreamReader sr = new StreamReader(XMLPath))
        {
            xml.LoadXml(sr.ReadToEnd());
        }

        data.textures.Clear();
        ImportTextures(xml, data);
    }

    public static void ImportRoofs(string XMLPath, BuildrData data)
    {
        Debug.Log("Import Textures " + XMLPath);
        XmlDocument xml = new XmlDocument();
        using (StreamReader sr = new StreamReader(XMLPath))
        {
            xml.LoadXml(sr.ReadToEnd());
        }

        data.roofs.Clear();
        ImportRoofs(xml, data);
    }

    public static void ImportFacades(string XMLPath, BuildrData data)
    {
        Debug.Log("Import Facades " + XMLPath);
        XmlDocument xml = new XmlDocument();
        using (StreamReader sr = new StreamReader(XMLPath))
        {
            xml.LoadXml(sr.ReadToEnd());
        }

        data.facades.Clear();
        ImportFacades(xml, data);
    }

    private static void ImportVolumes(XmlDocument xml, BuildrData data)
    {
        foreach(XmlNode node in xml.SelectNodes("buildr/plan/volumes/volume"))
        {
            BuildrVolume volume = ScriptableObject.CreateInstance<BuildrVolume>();
            BuildrVolumeStyles styles = ScriptableObject.CreateInstance<BuildrVolumeStyles>();
            data.plan.volumes.Add(volume);

            foreach(XmlNode pointnode in node.SelectNodes("points/point"))
            {
                volume.points.Add(int.Parse(pointnode.FirstChild.Value));
            }
            foreach(XmlNode pointnode in node.SelectNodes("facades/render"))
            {
                volume.renderFacade.Add(pointnode.FirstChild.Value == valueTrue);
            }
            volume.height = float.Parse(node["height"].FirstChild.Value);
            volume.numberOfFloors = int.Parse(node["numberoffloors"].FirstChild.Value);
            volume.roofDesignID = int.Parse(node["roofdesignid"].FirstChild.Value);

            foreach(XmlNode stylenode in node.SelectNodes("styles/style"))
            {
                int styleID = int.Parse(stylenode["styleid"].FirstChild.Value);
                int facadeID = int.Parse(stylenode["facadeid"].FirstChild.Value);
                int floors = int.Parse(stylenode["floors"].FirstChild.Value);
                styles.AddStyle(styleID, facadeID, floors);
            }
            volume.styles = styles;


            volume.generateStairs = bool.Parse(node["generateStairs"].FirstChild.Value);
            volume.staircaseWidth = float.Parse(node["staircaseWidth"].FirstChild.Value);
            volume.stepHeight = float.Parse(node["stepHeight"].FirstChild.Value);
            volume.stairwellCeilingTexture = int.Parse(node["stairwellCeilingTexture"].FirstChild.Value);
            volume.stairwellFloorTexture = int.Parse(node["stairwellFloorTexture"].FirstChild.Value);
            volume.stairwellStepTexture = int.Parse(node["stairwellStepTexture"].FirstChild.Value);
            volume.stairwellWallTexture = int.Parse(node["stairwellWallTexture"].FirstChild.Value);
            volume.numberOfBasementFloors = int.Parse(node["numberOfBasementFloors"].FirstChild.Value);

            int itemCount = 0;
            foreach (XmlNode basementNode in node.SelectNodes("basementtextures/basementfloortextures"))
            {
                volume.FloorTexture(itemCount, int.Parse(basementNode["FloorTexture"].FirstChild.Value));
                volume.WallTexture(itemCount, int.Parse(basementNode["WallTexture"].FirstChild.Value));
                volume.CeilingTexture(itemCount, int.Parse(basementNode["CeilingTexture"].FirstChild.Value));
                itemCount++;
            }
        }
    }

    private static void ImportFacades(XmlDocument xml, BuildrData data)
    {
        foreach(XmlNode node in xml.SelectNodes("buildr/facades/facade"))
        {
            BuildrFacadeDesign facadeDesign = new BuildrFacadeDesign();
            data.facades.Add(facadeDesign);

            facadeDesign.name = node["name"].FirstChild.Value;
            facadeDesign.hasWindows = node["hasWindows"].FirstChild.Value == valueTrue;
            facadeDesign.type = (BuildrFacadeDesign.types)System.Enum.Parse(typeof(BuildrFacadeDesign.types), (string)node["type"].FirstChild.Value);

            facadeDesign.simpleBay.name = node["simpleBayName"].FirstChild.Value;
            facadeDesign.simpleBay.openingWidth = float.Parse(node["simpleBayOpeningWidth"].FirstChild.Value);
            facadeDesign.simpleBay.openingHeight = float.Parse(node["simpleBayOpeningHeight"].FirstChild.Value);
            facadeDesign.simpleBay.minimumBayWidth = float.Parse(node["simpleBayMinimumBayWidth"].FirstChild.Value);
            facadeDesign.simpleBay.openingWidthRatio = float.Parse(node["simpleBayOpeningWidthRatio"].FirstChild.Value);
            facadeDesign.simpleBay.openingHeightRatio = float.Parse(node["simpleBayOpeningHeightRatio"].FirstChild.Value);
            facadeDesign.simpleBay.openingDepth = float.Parse(node["simpleBayOpeningDepth"].FirstChild.Value);
            facadeDesign.simpleBay.columnDepth = float.Parse(node["simpleBayColumnDepth"].FirstChild.Value);
            facadeDesign.simpleBay.rowDepth = float.Parse(node["simpleBayRowDepth"].FirstChild.Value);
            facadeDesign.simpleBay.crossDepth = float.Parse(node["simpleBayCrossDepth"].FirstChild.Value);

            for(int i = 0; i < 8; i++)
            {
                facadeDesign.simpleBay.textureValues[i] = int.Parse(node.SelectNodes("simpleBayTextures/texture")[i].FirstChild.Value);
                facadeDesign.simpleBay.flipValues[i] = node.SelectNodes("simpleBayflipvalues/flipvalue")[i].FirstChild.Value == valueTrue;
            }

            foreach(XmlNode baynode in node.SelectNodes("bays/bayid"))
            {
                facadeDesign.bayPattern.Add(int.Parse(baynode.FirstChild.Value));
            }
        }
    }

    private static void ImportRoofs(XmlDocument xml, BuildrData data)
    {
        foreach(XmlNode node in xml.SelectNodes("buildr/roofs/roof"))
        {
            BuildrRoofDesign roofDesign = new BuildrRoofDesign("");
            data.roofs.Add(roofDesign);

            roofDesign.name = node["name"].FirstChild.Value;
            roofDesign.style = (BuildrRoofDesign.styles)System.Enum.Parse(typeof(BuildrRoofDesign.styles), node["style"].FirstChild.Value);
            roofDesign.height = float.Parse(node["height"].FirstChild.Value);
            roofDesign.depth = float.Parse(node["depth"].FirstChild.Value);
            roofDesign.floorDepth = float.Parse(node["floorDepth"].FirstChild.Value);
            roofDesign.direction = int.Parse(node["direction"].FirstChild.Value);
            roofDesign.sawtoothTeeth = int.Parse(node["sawtoothTeeth"].FirstChild.Value);
            roofDesign.barrelSegments = int.Parse(node["barrelSegments"].FirstChild.Value);
            roofDesign.parapet = node["parapet"].FirstChild.Value == valueTrue;
            roofDesign.parapetStyle = (BuildrRoofDesign.parapetStyles)System.Enum.Parse(typeof(BuildrRoofDesign.parapetStyles), node["parapetStyle"].FirstChild.Value);
            roofDesign.parapetDesignWidth = float.Parse(node["parapetDesignWidth"].FirstChild.Value);
            roofDesign.parapetHeight = float.Parse(node["parapetHeight"].FirstChild.Value);
            roofDesign.parapetFrontDepth = float.Parse(node["parapetFrontDepth"].FirstChild.Value);
            roofDesign.parapetBackDepth = float.Parse(node["parapetBackDepth"].FirstChild.Value);
            roofDesign.hasDormers = node["hasDormers"].FirstChild.Value == valueTrue;
            roofDesign.dormerWidth = float.Parse(node["dormerWidth"].FirstChild.Value);
            roofDesign.dormerHeight = float.Parse(node["dormerHeight"].FirstChild.Value);
            roofDesign.dormerRoofHeight = float.Parse(node["dormerRoofHeight"].FirstChild.Value);
            roofDesign.minimumDormerSpacing = float.Parse(node["minimumDormerSpacing"].FirstChild.Value);
            roofDesign.dormerHeightRatio = float.Parse(node["dormerHeightRatio"].FirstChild.Value);


            for(int i = 0; i < 8; i++)
            {
                roofDesign.textureValues[i] = int.Parse(node.SelectNodes("textures/texture")[i].FirstChild.Value);
                roofDesign.flipValues[i] = node.SelectNodes("flipvalues/flipvalue")[i].FirstChild.Value == valueTrue;
            }
        }
    }

    private static void ImportTextures(XmlDocument xml, BuildrData data)
    {
        foreach (XmlNode node in xml.SelectNodes("buildr/textures/texture"))
        {
            BuildrTexture texture = new BuildrTexture("");
            data.textures.Add(texture);

            texture.name = node["name"].FirstChild.Value;
            texture.tiled = node["tiled"].FirstChild.Value == valueTrue;
            texture.patterned = node["patterned"].FirstChild.Value == valueTrue;
            texture.texture = (Texture2D)AssetDatabase.LoadAssetAtPath(node["texture"].FirstChild.Value, typeof(Texture2D));
            texture.tileUnitUV = new Vector2(float.Parse(node["tileUnitUV"]["x"].FirstChild.Value), float.Parse(node["tileUnitUV"]["y"].FirstChild.Value));
            texture.textureUnitSize = new Vector2(float.Parse(node["textureUnitSize"]["x"].FirstChild.Value), float.Parse(node["textureUnitSize"]["y"].FirstChild.Value));
        }
    }

    private static void ImportBays(XmlDocument xml, BuildrData data)
    {
        foreach (XmlNode node in xml.SelectNodes("buildr/bays/bay"))
        {
            BuildrBay bay = new BuildrBay("");
            data.bays.Add(bay);

            bay.name = node["bayName"].FirstChild.Value;
            bay.isOpening = bool.Parse(node["isOpening"].FirstChild.Value);
            bay.openingWidth = float.Parse(node["bayOpeningWidth"].FirstChild.Value);
            bay.openingHeight = float.Parse(node["bayOpeningHeight"].FirstChild.Value);
            bay.minimumBayWidth = float.Parse(node["bayMinimumBayWidth"].FirstChild.Value);
            bay.openingWidthRatio = float.Parse(node["bayOpeningWidthRatio"].FirstChild.Value);
            bay.openingHeightRatio = float.Parse(node["bayOpeningHeightRatio"].FirstChild.Value);
            bay.openingDepth = float.Parse(node["bayOpeningDepth"].FirstChild.Value);
            bay.columnDepth = float.Parse(node["bayColumnDepth"].FirstChild.Value);
            bay.rowDepth = float.Parse(node["bayRowDepth"].FirstChild.Value);
            bay.crossDepth = float.Parse(node["bayCrossDepth"].FirstChild.Value);

            for (int i = 0; i < 8; i++)
            {
                bay.textureValues[i] = int.Parse(node.SelectNodes("bayTextures/texture")[i].FirstChild.Value);
                bay.flipValues[i] = node.SelectNodes("bayflipvalues/flipvalue")[i].FirstChild.Value == valueTrue;
            }
        }
    }

    private static void ImportDetails(XmlDocument xml, BuildrData data)
    {
        foreach (XmlNode node in xml.SelectNodes("buildr/details/detail"))
        {
            BuildrDetail detail = new BuildrDetail("");
            data.details.Add(detail);

            detail.name = node["name"].FirstChild.Value;
            detail.mesh = (Mesh)AssetDatabase.LoadAssetAtPath(node["mesh"].FirstChild.Value, typeof(Mesh));
            detail.material.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(node["texture"].FirstChild.Value, typeof(Texture2D));
            Vector2 faceUV = new Vector2();
            faceUV.x = float.Parse(node["faceuvx"].FirstChild.Value);
            faceUV.y = float.Parse(node["faceuvy"].FirstChild.Value);
            detail.faceUv = faceUV;
            detail.faceHeight = float.Parse(node["faceheight"].FirstChild.Value);
            Vector3 scale = new Vector3();
            scale.x = float.Parse(node["scalex"].FirstChild.Value);
            scale.y = float.Parse(node["scaley"].FirstChild.Value);
            scale.z = float.Parse(node["scalez"].FirstChild.Value);
            detail.scale = scale;
            detail.orientation = (BuildrDetail.Orientations)System.Enum.Parse(typeof(BuildrDetail.Orientations), node["orientation"].FirstChild.Value);
            Vector3 userRotation = new Vector3();
            userRotation.x = float.Parse(node["userRotationx"].FirstChild.Value);
            userRotation.y = float.Parse(node["userRotationy"].FirstChild.Value);
            userRotation.z = float.Parse(node["userRotationz"].FirstChild.Value);
            detail.userRotation = userRotation;
            detail.face = int.Parse(node["face"].FirstChild.Value);
            detail.type = (BuildrDetail.Types)System.Enum.Parse(typeof(BuildrDetail.Types), node["type"].FirstChild.Value);
        }
    }

}
