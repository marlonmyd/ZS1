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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class BuildrXMLExporter
{

    private static string targetFolder = "Assets/Buildr/Exported/";
    private static string targetName = "XMLExport";
    private static BuildrData data = null;

    public static bool Export(string folder, string filename, BuildrData _data)
    {
        targetFolder = folder;
        targetName = filename;
        data = _data;

        if (folder.Contains(" "))
        {
            EditorUtility.DisplayDialog("Filename Error", "The filename can't contain spaces", "I'm sorry");
            return false;
        }

        if (filename.Contains(" "))
        {
            EditorUtility.DisplayDialog("Filename Error", "The filename can't contain spaces", "I'm sorry");
            return false;
        }

        //Export code
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version='1.0' encoding='ISO-8859-15'?>");
        sb.AppendLine("<!-- Unity3D Asset Buildr XML Exporter http://buildr.jasperstocker.com -->");

        sb.AppendLine("<buildr version='" + data.versionNumber + "'>");

        sb.Append(ExportData());

        //end
        sb.AppendLine("</buildr>");

        CreateTargetFolder();
        using (StreamWriter sw = new StreamWriter(targetFolder + "/" + targetName + ".xml"))
        {
            sw.Write(sb.ToString());//write out contents of data to XML
        }

        data = null;
        return true;
    }

    public static void ExportTextures(string filepath, BuildrData _data)
    {
        data = _data;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version='1.0' encoding='ISO-8859-15'?>");
        sb.AppendLine("<!-- Unity3D Asset Buildr XML Exporter http://buildr.jasperstocker.com -->");

        sb.AppendLine("<buildr version='" + data.versionNumber + "'>");
        sb.Append(ExportTextures());
        sb.AppendLine("</buildr>");

        using (StreamWriter sw = new StreamWriter(filepath))
        {
            sw.Write(sb.ToString());//write out contents of data to XML
        }

        data = null;
    }

    public static void ExportRoofs(string filepath, BuildrData _data)
    {
        data = _data;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version='1.0' encoding='ISO-8859-15'?>");
        sb.AppendLine("<!-- Unity3D Asset Buildr XML Exporter http://buildr.jasperstocker.com -->");

        sb.AppendLine("<buildr version='" + data.versionNumber + "'>");
        sb.Append(ExportRoofs());
        sb.AppendLine("</buildr>");

        using (StreamWriter sw = new StreamWriter(filepath))
        {
            sw.Write(sb.ToString());//write out contents of data to XML
        }

        data = null;
    }

    public static void ExportFacades(string filepath, BuildrData _data)
    {
        data = _data;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version='1.0' encoding='ISO-8859-15'?>");
        sb.AppendLine("<!-- Unity3D Asset Buildr XML Exporter http://buildr.jasperstocker.com -->");

        sb.AppendLine("<buildr version='" + data.versionNumber + "'>");
        sb.Append(ExportFacades());
        sb.AppendLine("</buildr>");

        using (StreamWriter sw = new StreamWriter(filepath))
        {
            sw.Write(sb.ToString());//write out contents of data to XML
        }

        data = null;
    }

    private static string ExportData()
    {
        StringBuilder sb = new StringBuilder();


        sb.AppendLine("<buildingname>" + data.name + "</buildingname>");
        sb.AppendLine("<floorheight>" + data.floorHeight + "</floorheight>");
        sb.AppendLine("<drawUnderside>" + data.drawUnderside + "</drawUnderside>");
        sb.AppendLine("<foundationHeight>" + data.foundationHeight + "</foundationHeight>");
        sb.AppendLine("<generateCollider>" + data.generateCollider + "</generateCollider>");
        sb.AppendLine("<renderInteriors>" + data.renderInteriors + "</renderInteriors>");
        sb.AppendLine("<interiorCeilingHeight>" + data.interiorCeilingHeight + "</interiorCeilingHeight>");
        sb.AppendLine("<cullBays>" + data.cullBays + "</cullBays>");

        sb.Append(ExportPlan());
        sb.Append(ExportVolumes());
        sb.Append(ExportFacades());
        sb.Append(ExportRoofs());
        sb.Append(ExportTextures());
        sb.Append(ExportBays());
        sb.Append(ExportDetails());

        return sb.ToString();
    }

    private static string ExportPlan()
    {
        StringBuilder sb = new StringBuilder();
        //Plan
        sb.AppendLine("<plan>");

        //points
        sb.AppendLine("<points>");
        foreach(Vector2z point in data.plan.points)
        {
            sb.AppendLine("<point>");
            sb.AppendLine("<x>" + point.x + "</x>");
            sb.AppendLine("<z>" + point.z + "</z>");
            sb.AppendLine("</point>");
        }
        sb.AppendLine("</points>");

        //core
        sb.AppendLine("<cores>");
        foreach (Rect core in data.plan.cores)
        {
            sb.AppendLine("<core>");
            sb.AppendLine("<xMin>" + core.xMin + "</xMin>");
            sb.AppendLine("<yMin>" + core.yMin + "</yMin>");
            sb.AppendLine("<width>" + core.width + "</width>");
            sb.AppendLine("<height>" + core.height + "</height>");
            sb.AppendLine("</core>");
        }
        sb.AppendLine("</cores>");


        sb.Append(ExportVolumes());

        sb.AppendLine("</plan>");

        return sb.ToString();
    }
    
    private static string ExportVolumes()
    {
        StringBuilder sb = new StringBuilder();
        //volumes
        sb.AppendLine("<volumes>");
        foreach(BuildrVolume volume in data.plan.volumes)
        {
            sb.AppendLine("<volume>");
            //points
            sb.AppendLine("<points>");
            foreach(int point in volume.points)
            {
                sb.AppendLine("<point>" + point + "</point>");
            }
            sb.AppendLine("</points>");

            //render facades bool
            sb.AppendLine("<facades>");
            foreach(bool render in volume.renderFacade)
            {
                sb.AppendLine("<render>" + render + "</render>");
            }
            sb.AppendLine("</facades>");

            sb.AppendLine("<height>" + volume.height + "</height>");
            sb.AppendLine("<numberoffloors>" + volume.numberOfFloors + "</numberoffloors>");
            sb.AppendLine("<roofdesignid>" + volume.roofDesignID + "</roofdesignid>");

            //style
            sb.AppendLine("<styles>");
            BuildrVolumeStylesUnit[] styles = volume.styles.GetContents();
            foreach(BuildrVolumeStylesUnit style in styles)
            {
                sb.AppendLine("<style>");
                sb.AppendLine("<styleid>" + style.styleID + "</styleid>");
                sb.AppendLine("<facadeid>" + style.facadeID + "</facadeid>");
                sb.AppendLine("<floors>" + style.floors + "</floors>");
                sb.AppendLine("</style>");
            }
            sb.AppendLine("</styles>");

            //interior stuff
            sb.AppendLine("<generateStairs>" + volume.generateStairs + "</generateStairs>");
            sb.AppendLine("<staircaseWidth>" + volume.staircaseWidth + "</staircaseWidth>");
            sb.AppendLine("<stepHeight>" + volume.stepHeight + "</stepHeight>");
            sb.AppendLine("<stairwellCeilingTexture>" + volume.stairwellCeilingTexture + "</stairwellCeilingTexture>");
            sb.AppendLine("<stairwellFloorTexture>" + volume.stairwellFloorTexture + "</stairwellFloorTexture>");
            sb.AppendLine("<stairwellStepTexture>" + volume.stairwellStepTexture + "</stairwellStepTexture>");
            sb.AppendLine("<stairwellWallTexture>" + volume.stairwellWallTexture + "</stairwellWallTexture>");
            sb.AppendLine("<numberOfBasementFloors>" + volume.numberOfBasementFloors + "</numberOfBasementFloors>");

            sb.AppendLine("<basementtextures>");
            for(int i = 0; i < volume.numberOfBasementFloors; i++)
            {
                sb.AppendLine("<basementfloortextures>");
                sb.AppendLine("<FloorTexture>" + volume.FloorTexture(i) + "</FloorTexture>");
                sb.AppendLine("<WallTexture>" + volume.WallTexture(i) + "</WallTexture>");
                sb.AppendLine("<CeilingTexture>" + volume.CeilingTexture(i) + "</CeilingTexture>");
                sb.AppendLine("</basementfloortextures>");
            }
            sb.AppendLine("</basementtextures>");

            sb.AppendLine("</volume>");
        }
        sb.AppendLine("</volumes>");

        return sb.ToString();
    }

    private static string ExportFacades()
    {
        StringBuilder sb = new StringBuilder();
        //Facades
        sb.AppendLine("<facades>");
        foreach(BuildrFacadeDesign facade in data.facades)
        {
            sb.AppendLine("<facade>");
            sb.AppendLine("<name>" + facade.name + "</name>");
            sb.AppendLine("<type>" + facade.type + "</type>");
            sb.AppendLine("<hasWindows>" + facade.hasWindows + "</hasWindows>");

            sb.AppendLine("<simpleBayName>" + facade.simpleBay.name + "</simpleBayName>");
            sb.AppendLine("<simpleBayOpeningWidth>" + facade.simpleBay.openingWidth + "</simpleBayOpeningWidth>");
            sb.AppendLine("<simpleBayOpeningHeight>" + facade.simpleBay.openingHeight + "</simpleBayOpeningHeight>");
            sb.AppendLine("<simpleBayMinimumBayWidth>" + facade.simpleBay.minimumBayWidth + "</simpleBayMinimumBayWidth>");
            sb.AppendLine("<simpleBayOpeningWidthRatio>" + facade.simpleBay.openingWidthRatio + "</simpleBayOpeningWidthRatio>");
            sb.AppendLine("<simpleBayOpeningHeightRatio>" + facade.simpleBay.openingHeightRatio + "</simpleBayOpeningHeightRatio>");
            sb.AppendLine("<simpleBayOpeningDepth>" + facade.simpleBay.openingDepth + "</simpleBayOpeningDepth>");
            sb.AppendLine("<simpleBayColumnDepth>" + facade.simpleBay.columnDepth + "</simpleBayColumnDepth>");
            sb.AppendLine("<simpleBayRowDepth>" + facade.simpleBay.rowDepth + "</simpleBayRowDepth>");
            sb.AppendLine("<simpleBayCrossDepth>" + facade.simpleBay.crossDepth + "</simpleBayCrossDepth>");

            sb.AppendLine("<simpleBayTextures>");
            foreach(int textureValue in facade.simpleBay.textureValues)
            {
                sb.AppendLine("<texture>" + textureValue + "</texture>");
            }
            sb.AppendLine("</simpleBayTextures>");

            sb.AppendLine("<simpleBayflipvalues>");
            foreach(bool flipvalue in facade.simpleBay.flipValues)
            {
                sb.AppendLine("<flipvalue>" + flipvalue + "</flipvalue>");
            }
            sb.AppendLine("</simpleBayflipvalues>");

            sb.AppendLine("<bays>");
            foreach(int bayId in facade.bayPattern)
            {
                sb.AppendLine("<bayid>" + bayId + "</bayid>");
            }
            sb.AppendLine("</bays>");

            sb.AppendLine("</facade>");
        }
        sb.AppendLine("</facades>");

        return sb.ToString();
    }
    
    private static string ExportRoofs()
    {
        StringBuilder sb = new StringBuilder();
        //Roof
        sb.AppendLine("<roofs>");
        foreach(BuildrRoofDesign roof in data.roofs)
        {
            sb.AppendLine("<roof>");
            sb.AppendLine("<name>" + roof.name + "</name>");
            sb.AppendLine("<style>" + roof.style + "</style>");
            sb.AppendLine("<height>" + roof.height + "</height>");
            sb.AppendLine("<depth>" + roof.depth + "</depth>");
            sb.AppendLine("<floorDepth>" + roof.floorDepth + "</floorDepth>");
            sb.AppendLine("<direction>" + roof.direction + "</direction>");
            sb.AppendLine("<sawtoothTeeth>" + roof.sawtoothTeeth + "</sawtoothTeeth>");
            sb.AppendLine("<barrelSegments>" + roof.barrelSegments + "</barrelSegments>");
            sb.AppendLine("<parapet>" + roof.parapet + "</parapet>");
            sb.AppendLine("<parapetStyle>" + roof.parapetStyle + "</parapetStyle>");
            sb.AppendLine("<parapetDesignWidth>" + roof.parapetDesignWidth + "</parapetDesignWidth>");
            sb.AppendLine("<parapetHeight>" + roof.parapetHeight + "</parapetHeight>");
            sb.AppendLine("<parapetFrontDepth>" + roof.parapetFrontDepth + "</parapetFrontDepth>");
            sb.AppendLine("<parapetBackDepth>" + roof.parapetBackDepth + "</parapetBackDepth>");
            sb.AppendLine("<hasDormers>" + roof.hasDormers + "</hasDormers>");
            sb.AppendLine("<dormerWidth>" + roof.dormerWidth + "</dormerWidth>");
            sb.AppendLine("<dormerHeight>" + roof.dormerHeight + "</dormerHeight>");
            sb.AppendLine("<dormerRoofHeight>" + roof.dormerRoofHeight + "</dormerRoofHeight>");
            sb.AppendLine("<minimumDormerSpacing>" + roof.minimumDormerSpacing + "</minimumDormerSpacing>");
            sb.AppendLine("<dormerHeightRatio>" + roof.dormerHeightRatio + "</dormerHeightRatio>");

            sb.AppendLine("<textures>");
            foreach(int textureValue in roof.textureValues)
            {
                sb.AppendLine("<texture>" + textureValue + "</texture>");
            }
            sb.AppendLine("</textures>");

            sb.AppendLine("<flipvalues>");
            foreach(bool flipvalue in roof.flipValues)
            {
                sb.AppendLine("<flipvalue>" + flipvalue + "</flipvalue>");
            }
            sb.AppendLine("</flipvalues>");
            sb.AppendLine("</roof>");
        }
        sb.AppendLine("</roofs>");

        return sb.ToString();
    }
    
    private static string ExportTextures()
    {
        StringBuilder sb = new StringBuilder();
        //Textures
        sb.AppendLine("<textures>");
        foreach(BuildrTexture texture in data.textures)
        {
            sb.AppendLine("<texture>");
            sb.AppendLine("<name>" + texture.name + "</name>");
            sb.AppendLine("<tiled>" + texture.tiled + "</tiled>");
            sb.AppendLine("<patterned>" + texture.patterned + "</patterned>");
            sb.AppendLine("<texture>" + AssetDatabase.GetAssetPath(texture.texture) + "</texture>");
            sb.AppendLine("<tileUnitUV>");
            sb.AppendLine("<x>" + texture.tileUnitUV.x + "</x>");
            sb.AppendLine("<y>" + texture.tileUnitUV.y + "</y>");
            sb.AppendLine("</tileUnitUV>");
            sb.AppendLine("<textureUnitSize>");
            sb.AppendLine("<x>" + texture.textureUnitSize.x + "</x>");
            sb.AppendLine("<y>" + texture.textureUnitSize.y + "</y>");
            sb.AppendLine("</textureUnitSize>");
            sb.AppendLine("<tiledX>" + texture.tiledX + "</tiledX>");
            sb.AppendLine("<tiledY>" + texture.tiledY + "</tiledY>");
            sb.AppendLine("</texture>");
        }
        sb.AppendLine("</textures>");

        return sb.ToString();
    }

    private static string ExportBays()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<bays>");
        foreach(BuildrBay bay in data.bays)
        {
            sb.AppendLine("<bay>");

            sb.AppendLine("<bayName>" + bay.name + "</bayName>");
            sb.AppendLine("<isOpening>" + bay.isOpening + "</isOpening>");
            sb.AppendLine("<bayOpeningWidth>" + bay.openingWidth + "</bayOpeningWidth>");
            sb.AppendLine("<bayOpeningHeight>" + bay.openingHeight + "</bayOpeningHeight>");
            sb.AppendLine("<bayMinimumBayWidth>" + bay.minimumBayWidth + "</bayMinimumBayWidth>");
            sb.AppendLine("<bayOpeningWidthRatio>" + bay.openingWidthRatio + "</bayOpeningWidthRatio>");
            sb.AppendLine("<bayOpeningHeightRatio>" + bay.openingHeightRatio + "</bayOpeningHeightRatio>");
            sb.AppendLine("<bayOpeningDepth>" + bay.openingDepth + "</bayOpeningDepth>");
            sb.AppendLine("<bayColumnDepth>" + bay.columnDepth + "</bayColumnDepth>");
            sb.AppendLine("<bayRowDepth>" + bay.rowDepth + "</bayRowDepth>");
            sb.AppendLine("<bayCrossDepth>" + bay.crossDepth + "</bayCrossDepth>");

            sb.AppendLine("<bayTextures>");
            foreach(int textureValue in bay.textureValues)
            {
                sb.AppendLine("<texture>" + textureValue + "</texture>");
            }
            sb.AppendLine("</bayTextures>");

            sb.AppendLine("<bayflipvalues>");
            foreach(bool flipvalue in bay.flipValues)
            {
                sb.AppendLine("<flipvalue>" + flipvalue + "</flipvalue>");
            }
            sb.AppendLine("</bayflipvalues>");

            sb.AppendLine("</bay>");

        }
        sb.AppendLine("</bays>");

        return sb.ToString();
    }

    private static string ExportDetails()
    {
        StringBuilder sb = new StringBuilder();
        //Details
        sb.AppendLine("<details>");
        foreach(BuildrDetail detail in data.details)
        {
            sb.AppendLine("<detail>");

            sb.AppendLine("<name>" + detail.name + "</name>");
            sb.AppendLine("<mesh>" + AssetDatabase.GetAssetPath(detail.mesh) + "</mesh>");
            sb.AppendLine("<texture>" + AssetDatabase.GetAssetPath(detail.material.mainTexture) + "</texture>");
            sb.AppendLine("<faceuvx>" + detail.faceUv.x + "</faceuvx>");
            sb.AppendLine("<faceuvy>" + detail.faceUv.y + "</faceuvy>");
            sb.AppendLine("<faceheight>" + detail.faceHeight + "</faceheight>");
            sb.AppendLine("<scalex>" + detail.scale.x + "</scalex>");
            sb.AppendLine("<scaley>" + detail.scale.y + "</scaley>");
            sb.AppendLine("<scalez>" + detail.scale.z + "</scalez>");
            sb.AppendLine("<orientation>" + detail.orientation + "</orientation>");
            sb.AppendLine("<userRotationx>" + detail.userRotation.x + "</userRotationx>");
            sb.AppendLine("<userRotationy>" + detail.userRotation.y + "</userRotationy>");
            sb.AppendLine("<userRotationz>" + detail.userRotation.z + "</userRotationz>");
            sb.AppendLine("<face>" + detail.face + "</face>");
            sb.AppendLine("<type>" + detail.type + "</type>");

            sb.AppendLine("</detail>");
        }
        sb.AppendLine("</details>");

        return sb.ToString();
    }

    private static void CreateTargetFolder()
    {
        string newDirectory = targetFolder + targetName + "/";
        Debug.Log("Create " + newDirectory);
        if (System.IO.Directory.Exists(newDirectory))
        {
            if (EditorUtility.DisplayDialog("File directory exists", "Are you sure you want to overwrite the contents of this file?", "Cancel", "Overwrite"))
            {
                return;
            }
        }

        try
        {
            System.IO.Directory.CreateDirectory(newDirectory);
        }
        catch
        {
            EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
            return;
        }
    }
}
