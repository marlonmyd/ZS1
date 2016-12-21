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

//this class will deal with future updates to BuildR
//and will ensure appropriate updates to data passed through.
public class BuildrUpgrader
{
    public static bool UpgradeData(BuildrData data)
    {
        float currentVersion = BuildrVersion.NUMBER;
        float dataVersion = data.versionNumber;

        if (currentVersion == dataVersion)
        {
            //The data matches the current version of Buildr - do nothing.
            return true;
        }

        if (currentVersion < dataVersion)
        {
            Debug.LogError("BuildR v." + currentVersion + ": Great scot! This data is from the future! (version:" + dataVersion + ") - need to avoid contact to ensure the survival of the universe...");
            return false;//don't touch ANYTHING!
        }

        Debug.Log("BuildR v." + currentVersion + " Upgrading the data from version " + dataVersion + " to version " + currentVersion + "\nRemember to backup your data!");

        if (dataVersion < 0.7f)
        {
            //upgrade the facade data so we use BuildrBay design elements
            int facadeDesignCount = data.facades.Count;
            for (int i = 0; i < facadeDesignCount; i++)
            {
                BuildrFacadeDesign design = data.facades[i];
                if (!design.alternatingSpacing)
                {
                    design.type = BuildrFacadeDesign.types.simple;
                    design.simpleBay.isOpening = design.hasWindows;
                    design.simpleBay.openingWidth = design.windowWidth;
                    design.simpleBay.openingHeight = design.windowHeight;
                    design.simpleBay.minimumBayWidth = design.minimumWindowSpacing;
                    design.simpleBay.openingWidthRatio = 0.5f;
                    design.simpleBay.openingHeightRatio = design.windowHeightRatio;
                    design.simpleBay.openingDepth = design.windowDepth;
                    design.simpleBay.columnDepth = design.columnDepth;
                    design.simpleBay.rowDepth = design.rowDepth;
                    design.simpleBay.crossDepth = design.crossDepth;

                    design.simpleBay.textureValues[0] = design.textureValues[4];//openingBackTexture,
                    design.simpleBay.textureValues[1] = design.textureValues[5];//openingSideTexture,
                    design.simpleBay.textureValues[2] = design.textureValues[6];//openingSillTexture,
                    design.simpleBay.textureValues[3] = design.textureValues[7];//openingCeilingTexture,
                    design.simpleBay.textureValues[4] = design.textureValues[0];//columnTexture,
                    design.simpleBay.textureValues[5] = design.textureValues[1];//rowTexture,
                    design.simpleBay.textureValues[6] = design.textureValues[2];//crossTexture,
                    design.simpleBay.textureValues[7] = design.textureValues[0];//wallTexture

                    design.simpleBay.flipValues[0] = design.flipValues[4];//openingBackTexture,
                    design.simpleBay.flipValues[1] = design.flipValues[5];//openingSideTexture,
                    design.simpleBay.flipValues[2] = design.flipValues[6];//openingSillTexture,
                    design.simpleBay.flipValues[3] = design.flipValues[7];//openingCeilingTexture,
                    design.simpleBay.flipValues[4] = design.flipValues[0];//columnTexture,
                    design.simpleBay.flipValues[5] = design.flipValues[1];//rowTexture,
                    design.simpleBay.flipValues[6] = design.flipValues[2];//crossTexture,
                    design.simpleBay.flipValues[7] = design.flipValues[0];//wallTexture
                }
                else
                {
                    design.type = BuildrFacadeDesign.types.patterned;
                    float alternateSpacerRatio = (1 / (1 + (1 / design.minimumWindowSpacingAlt)));
                    BuildrBay bayA = new BuildrBay(design.name + "a");
                    bayA.isOpening = design.hasWindows;
                    bayA.openingWidth = design.windowWidth;
                    bayA.openingHeight = design.windowHeight;
                    bayA.minimumBayWidth = design.minimumWindowSpacing;
                    bayA.openingWidthRatio = 1 - alternateSpacerRatio;
                    bayA.openingHeightRatio = design.windowHeightRatio;
                    bayA.openingDepth = design.windowDepth;
                    bayA.columnDepth = design.columnDepth;
                    bayA.rowDepth = design.rowDepth;
                    bayA.crossDepth = design.crossDepth;

                    bayA.textureValues[0] = design.textureValues[4];//openingBackTexture,
                    bayA.textureValues[1] = design.textureValues[5];//openingSideTexture,
                    bayA.textureValues[2] = design.textureValues[6];//openingSillTexture,
                    bayA.textureValues[3] = design.textureValues[7];//openingCeilingTexture,
                    bayA.textureValues[4] = design.textureValues[0];//columnTexture,
                    bayA.textureValues[5] = design.textureValues[1];//rowTexture,
                    bayA.textureValues[6] = design.textureValues[2];//crossTexture,
                    bayA.textureValues[7] = design.textureValues[0];//wallTexture
                    bayA.flipValues[0] = design.flipValues[4];//openingBackTexture,
                    bayA.flipValues[1] = design.flipValues[5];//openingSideTexture,
                    bayA.flipValues[2] = design.flipValues[6];//openingSillTexture,
                    bayA.flipValues[3] = design.flipValues[7];//openingCeilingTexture,
                    bayA.flipValues[4] = design.flipValues[0];//columnTexture,
                    bayA.flipValues[5] = design.flipValues[1];//rowTexture,
                    bayA.flipValues[6] = design.flipValues[2];//crossTexture,
                    bayA.flipValues[7] = design.flipValues[0];//wallTexture

                    BuildrBay bayB = new BuildrBay(design.name + "b");
                    bayB.isOpening = design.hasWindows;
                    bayB.openingWidth = design.windowWidth;
                    bayB.openingHeight = design.windowHeight;
                    bayB.minimumBayWidth = design.minimumWindowSpacing;
                    bayB.openingWidthRatio = alternateSpacerRatio;
                    bayB.openingHeightRatio = design.windowHeightRatio;
                    bayB.openingDepth = design.windowDepth;
                    bayB.columnDepth = design.columnDepth;
                    bayB.rowDepth = design.rowDepth;
                    bayB.crossDepth = design.crossDepth;

                    bayB.textureValues[0] = design.textureValues[4];//openingBackTexture,
                    bayB.textureValues[1] = design.textureValues[5];//openingSideTexture,
                    bayB.textureValues[2] = design.textureValues[6];//openingSillTexture,
                    bayB.textureValues[3] = design.textureValues[7];//openingCeilingTexture,
                    bayB.textureValues[4] = design.textureValues[0];//columnTexture,
                    bayB.textureValues[5] = design.textureValues[1];//rowTexture,
                    bayB.textureValues[6] = design.textureValues[2];//crossTexture,
                    bayB.textureValues[7] = design.textureValues[0];//wallTexture
                    bayB.flipValues[0] = design.flipValues[4];//openingBackTexture,
                    bayB.flipValues[1] = design.flipValues[5];//openingSideTexture,
                    bayB.flipValues[2] = design.flipValues[6];//openingSillTexture,
                    bayB.flipValues[3] = design.flipValues[7];//openingCeilingTexture,
                    bayB.flipValues[4] = design.flipValues[0];//columnTexture,
                    bayB.flipValues[5] = design.flipValues[1];//rowTexture,
                    bayB.flipValues[6] = design.flipValues[2];//crossTexture,
                    bayB.flipValues[7] = design.flipValues[0];//wallTexture

                    int bayIndex = data.bays.Count;
                    data.bays.Add(bayA);
                    data.bays.Add(bayB);
                    design.bayPattern.Clear();
                    design.bayPattern.Add(bayIndex);
                    design.bayPattern.Add(bayIndex + 1);
                }
            }
        }

        if (dataVersion < 0.9f)
        {
            //new details addition
            data.details = new List<BuildrDetail>();

            //update Volume to account for the new array of Render bools
            BuildrPlan plan = data.plan;
            foreach(BuildrVolume volume in plan.volumes)
            {
                int volumeSize = volume.points.Count;
                for(int i = 0; i < volumeSize; i++)
                    volume.renderFacade.Add(true);
            }
        }


        if (dataVersion < 1.0f)
        {
            foreach(BuildrBay bay in data.bays)
            {
                bay.renderBack = true;
            }
        }

        data.versionNumber = BuildrVersion.NUMBER;//update the data version number once upgrade is complete
        return true;
    }
}
