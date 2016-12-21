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

[System.Serializable]
public class BuildrPlan : ScriptableObject
{
	
	public List<Vector2z> points = new List<Vector2z>();
    public List<BuildrVolume> volumes = new List<BuildrVolume>();
    public List<Rect> cores = new List<Rect>();
	
    [SerializeField]
	private List<Vector2z> _illegalPoints = new List<Vector2z>();
	
	public bool CheckPlan()
	{
		List<Vector2z> lines = new List<Vector2z>();//this will be a list of lines in sets of two vector points
		int volumeCount = numberOfVolumes;
		for(int s=0; s<volumeCount; s++)
		{
			int volumeLinkCount = volumes[s].Count;
			for(int l=0; l<volumeLinkCount; l++)
			{
				int indexB = (l<volumeLinkCount-1) ? l+1 : 0;
				lines.Add(points[volumes[s].points[l]]);
				lines.Add(points[volumes[s].points[indexB]]);
			}
		    int numberOfCores = cores.Count;
		    for(int coreIndex = 0; coreIndex < numberOfCores; coreIndex++)
            {
                Rect coreOutline = cores[coreIndex];
                Vector2z coreBL = new Vector2z(coreOutline.xMin, coreOutline.yMin);
                Vector2z coreBR = new Vector2z(coreOutline.xMax, coreOutline.yMin);
                Vector2z coreTL = new Vector2z(coreOutline.xMin, coreOutline.yMax);
                Vector2z coreTR = new Vector2z(coreOutline.xMax, coreOutline.yMax);
                lines.Add(coreBL);
                lines.Add(coreBR);
                lines.Add(coreBR);
                lines.Add(coreTR);
                lines.Add(coreTR);
                lines.Add(coreTL);
                lines.Add(coreTL);
                lines.Add(coreBL);
		    }
		}
		_illegalPoints.Clear();
		int numberOfLines = lines.Count;
		Vector2z a,b,c,d;
		while(numberOfLines > 2)
		{
			//get the first line
			a = lines[0];
			b = lines[1];
			for(int i=2; i<numberOfLines; i+=2)
			{
				c=lines[i];
				d=lines[i+1];
				if(a==c||a==d||b==c||b==d)//don't test lines that connect
					continue;
				if(BuildrUtils.FastLineIntersection(a,b,c,d))
				{
					_illegalPoints.Add(a);
					_illegalPoints.Add(b);
					_illegalPoints.Add(c);
					_illegalPoints.Add(d);
				}
			}
			
			lines.RemoveRange(0,2);//remove the first linked line
			numberOfLines = lines.Count;
		}
		return _illegalPoints.Count > 0;
	}

    public void Clear()
    {
        BuildrVolume[] removevolumes = volumes.ToArray();
        for(int i = 0; i < numberOfVolumes; i++)
        {
            RemoveVolume(removevolumes[i]);
        }
        for(int i = 0; i < numberOfPoints; i++)
        {
            RemovePoint(0);
        }
        points.Clear();
        volumes.Clear();
    }
	
	public Vector2z[] illegalPoints
	{
		get{return _illegalPoints.ToArray();}
	}
	
	public int numberOfIllegalPoints
	{
		get{return _illegalPoints.Count;}
	}
	
	public int numberOfPoints
	{
		get{return points.Count;}
	}
	
	public int numberOfVolumes
	{
		get{return volumes.Count;}
	}

    public int numberOfFacades
    {
        get 
        {
            int output = 0;
            foreach(BuildrVolume vol in volumes)
                output += vol.numberOfFacades;
            return output;
        }
    }
	
	/*public BuildrVolume GetVolume(int index)
	{
		return volumes[index];
	}*/
	
	public int GetVolumeSize(int volumeIndex)
	{
		return volumes[volumeIndex].Count;
	}
	
	public Vector2z GetVolumePoint(int volumeIndex, int pointIndex)
	{
		return points[GetVolumePointIndex(volumeIndex,pointIndex)];
	}
	
	public int GetVolumePointIndex(int volumeIndex, int pointIndex)
	{
		return volumes[volumeIndex].points[pointIndex];
	}
	
	public int GetNumberOfVolumeFloors(int volumeIndex)
	{
		return volumes[volumeIndex].numberOfFloors;
	}
	
	public void SetNumberOfVolumeFloors(int volumeIndex, int floors)
	{
		volumes[volumeIndex].numberOfFloors = floors;
	}
	
	public BuildrVolumeStylesUnit[] GetVolumeStylesByFacade(int volumeIndex, int facadeIndex)
	{
		return volumes[volumeIndex].styles.GetContentsByFacade(facadeIndex);
	}
	
	public int GetVolumeRoofDesignIndex(int volumeIndex)
	{
		return volumes[volumeIndex].roofDesignID;
	}
	
	public void RemoveFacadeStyle(int facadeFacadeIndex)
	{
		foreach(BuildrVolume volume in volumes)
			volume.styles.CheckRemovedStyle(facadeFacadeIndex);
	}
	
	public void RemoveRoofStyle(int roofStyleIndex)
	{
		foreach(BuildrVolume volume in volumes)
		{
			if(volume.roofDesignID >= roofStyleIndex && volume.roofDesignID>0)
				volume.roofDesignID--;
		}
	}
	
	public Vector3[] GetWallVectors(int volumeIndex, int aIndex)
	{
		int numberOfPoints = volumes[volumeIndex].Count;
		int bIndex = (aIndex>=numberOfPoints-1)?0:aIndex+1;
		Vector3[] wallPositions = new Vector3[2];
		wallPositions[0] = points[volumes[volumeIndex].points[aIndex]].vector3;
		wallPositions[1] = points[volumes[volumeIndex].points[bIndex]].vector3;
		return wallPositions;
	}
	
	public List<Vector3> GetPointsAsVector3()
	{
		List<Vector3> vectorPoints = new List<Vector3>();
		int pointCount = points.Count;
		for(int i=0; i<pointCount; i++)
			vectorPoints.Add(points[i].vector3);
		
		return vectorPoints;
	}
	
	/// <summary>
	/// Gets the ordered points from a volume.
	/// </summary>
	/// <returns>
	/// A list of the ordered points.
	/// </returns>
	/// <param name='volumeIndex'>
	/// Volume index.
	/// </param>
	public List<Vector2z> GetOrderedPoints(int volumeIndex)
	{
		List<Vector2z> orderedPoints = new List<Vector2z>();
		int volumeCount = volumes[volumeIndex].Count;
		for(int i=0; i<volumeCount; i++)
			orderedPoints.Add(points[volumes[volumeIndex].points[i]]);
		
		return orderedPoints;
	}
	
	public int GetFacadeFloorHeight(int volumeIndex, int pointA, int pointB)
	{
		int returnFloorHeight = 0;
		List<int> volumeIDs = GetVolumeIDs(pointA, pointB);
		
		if(!volumeIDs.Contains(volumeIndex))
			Debug.LogError("Error, this wall isn't within this volume");
		
		switch(volumeIDs.Count)
		{
		case 0:
			Debug.LogError("Error, this wall isn't within this volume");
			break;
			
		case 1:
			return returnFloorHeight;//no adjacent volume - floor height = 0
			
		case 2:
			int otherVolume = (volumeIDs.IndexOf(volumeIndex) == 0)? volumeIDs[1] : volumeIDs[0];
			return volumes[otherVolume].numberOfFloors;
			
		default:
			Debug.LogError("Error, a wall can't have more than one volume");
			break;
		}
		
		return returnFloorHeight;
	}
	
	/// <summary>
	/// Adds the wall point by vector3.
	/// </summary>
	/// <returns>
	/// The new wall point index.
	/// </returns>
	/// <param name='point'>
	/// Point.
	/// </param>
	/// <param name='wallIndex'>
	/// Wall index.
	/// </param>
	/// <param name='volumeIndex'>
	/// volume.
	/// </param>
	public int AddWallPoint(Vector3 point, int wallIndex, int volumeIndex)
	{
		Vector2z V2Zpoint = new Vector2z(point);
		return AddWallPoint(volumeIndex, wallIndex, V2Zpoint);
	}
	
	/// <summary>
	/// Adds the wall point by vector2z.
	/// </summary>
	/// <returns>
	/// The wall point index.
	/// </returns>
	/// <param name='volumeIndex'>
	/// volume index.
	/// </param>
	/// <param name='volumeWallIndex'>
	/// At point.
	/// </param>
	/// <param name='newPoint'>
	/// New point.
	/// </param>
	public int AddWallPoint(int volumeIndex, int volumeWallIndex, Vector2z newPoint)
	{
		int newPointIndex = points.Count;
		points.Add(newPoint);
		
		return AddWallPoint(volumeIndex, volumeWallIndex, newPointIndex);
	}
	
	public int AddWallPoint(int volumeIndex, int volumeWallIndex, int pointIndex)
	{
		int volumeSize = volumes[volumeIndex].Count;
		int pointAIndex = volumes[volumeIndex].points[volumeWallIndex];
		int pointBIndex = volumes[volumeIndex].points[(volumeWallIndex+1)%volumeSize];
		int otherVolume = GetConnectingVolumeIndex(volumeIndex,pointAIndex,pointBIndex);
		
		volumes[volumeIndex].Insert(volumeWallIndex+1, pointIndex);
		if(otherVolume!=-1)
		{
			int otherVolumeWallIndex = volumes[otherVolume].GetWallIndex(pointAIndex,pointBIndex);
			volumes[otherVolume].Insert(otherVolumeWallIndex+1, pointIndex);
		}
		
		return pointIndex;
	}
	
	public void RemovePoint(int removePointIndex)
	{
		points.RemoveAt(removePointIndex);
		int volumeCount = numberOfVolumes;
		for(int v=0; v<volumeCount; v++)
		{
			volumes[v].RemoveAndUpdate(removePointIndex);
		}
		for(int v=0; v<volumeCount; v++)
		{
			BuildrVolume volume = volumes[v];
			if(volume.Count < 3)
			{
				RemoveVolume(volume);
				volumeCount--;
				v--;
			}
		}
		CleanUpVolumes();
	}
	
	public void RemoveVolume(BuildrVolume volume)
	{
		volumes.Remove(volume);
		DestroyImmediate(volume);
		CleanUpVolumes();
	}
	
	public void MergeVolumes(int volumeIndexA, int volumeIndexB)
	{
		BuildrVolume volumeA = volumes[volumeIndexA];
		BuildrVolume volumeB = volumes[volumeIndexB];
		int volumeASize = volumeA.Count;
		int volumeBSize = volumeB.Count;
		List<int> keepPoints = new List<int>();
		
		string outpt = "";
		foreach(int p in volumeA.points)
			outpt+=","+p;
		//Debug.Log(outpt);
		outpt = "";
		foreach(int p in volumeB.points)
			outpt+=","+p;
		//Debug.Log(outpt);
		
		//find the start of volume A - where the connection to volume B ends.
		bool insideConnections = false;
		int volumeAStart = 0;
		for(int pa=0; pa<volumeASize; pa++)
		{
			int paa = volumeA.points[pa];
			int pab = volumeA.points[(pa+1)%volumeASize];
			int linkedVA = GetConnectingVolumeIndex(volumeIndexA,paa,pab);
			if(insideConnections)
			{
				if(linkedVA != volumeIndexB)//if we leave the connection - mark the start of the non connected points
				{
					volumeAStart = pa;
					break;
				}
			}else{
				if(linkedVA == volumeIndexB)//we have entered connected points
					insideConnections=true;
			}
		}
		
		for(int pa=0; pa<volumeASize; pa++)
		{
			int pas = (pa+volumeAStart)%volumeASize;
			
			int paa = volumeA.points[pas];
			int pab = volumeA.points[(pas+1)%volumeASize];
			int linkedVA = GetConnectingVolumeIndex(volumeIndexA,paa,pab);
			if(linkedVA != volumeIndexB)//if we leave the connection - mark the start of the non connected points
			{
				keepPoints.Add(paa);
			}else{
				keepPoints.Add(paa);//add last point
				break;
			}
		}
		outpt = "";
		foreach(int p in keepPoints)
			outpt+=","+p;
		//Debug.Log(outpt);
		
		int volumeBStartIndex = volumeB.IndexOf(keepPoints[keepPoints.Count-1]);
		//Debug.Log("volumeBStartIndex "+volumeBStartIndex);
		for(int pb=1; pb<volumeBSize; pb++)//start at one as we already have the first point logged from volume a
		{
			int pbs = (pb+volumeBStartIndex)%volumeBSize;
			int pba = volumeB.points[pbs];
			int pbb = volumeB.points[(pbs+1)%volumeBSize];
			int linkedVB = GetConnectingVolumeIndex(volumeIndexB,pba,pbb);
			//Debug.Log(pbs+" "+pba+" "+pbb+" "+linkedVB);
			if(linkedVB != volumeIndexA)//if we leave the connection - mark the start of the non connected points
			{
				keepPoints.Add(pba);
			}else{
				break;
			}
		}
		//keepPoints.RemoveAt(keepPoints.Count-1);//remove last one - it is the start of volume a
		
		
		outpt = "";
		foreach(int p in keepPoints)
			outpt+=","+p;
		//Debug.Log(outpt);
		
		//remove all the linked points
		for(int rp=0; rp<volumeASize; rp++)
		{
			int point = volumeA.points[rp];
			if(!keepPoints.Contains(point))
			{
				int keepPointLength = keepPoints.Count;
				for(int ap=0; ap<keepPointLength; ap++)
				{
					int keepPoint = keepPoints[ap];
					if(keepPoint>point)
						keepPoints[ap]--;
				}
				Debug.Log(point);
				RemovePoint(point);
				volumeASize--;
				rp--;
			}
		}
		
		//transfer all the styles across to the one volume
		foreach(int kp in keepPoints)
		{
			BuildrVolumeStylesUnit[] VBStyles = volumeB.styles.GetContents();
			if(!volumeA.Contains(kp))
			{
				foreach(BuildrVolumeStylesUnit style in VBStyles)
				{
					if(style.facadeID == kp)
					{
						volumeA.styles.AddStyle(style.styleID, style.facadeID, style.floors);
					}
				}
			}
		}
		
		//reassign the list of points
		volumeA.points = new List<int>(keepPoints);
		
		//remove old volume
		RemoveVolume(volumeB);
		outpt = "";
		foreach(int p in volumeA.points)
			outpt+=","+p;
		//Debug.Log(outpt);
	}
	
	public void SplitVolume(int volumeIndexA, int pointIndexA, int pointIndexB)
	{
		//add second volume
		int volumeIndexB = AddVolume();
		BuildrVolume volumeA = volumes[volumeIndexA];
		BuildrVolume volumeB = volumes[volumeIndexB];
		int volumeASize = volumeA.Count;
		int volumePointIndexA = volumeA.IndexOf(pointIndexA);
		int volumePointIndexB = volumeA.IndexOf(pointIndexB);
		
		//switchy
		if(volumePointIndexB < volumePointIndexA)
		{
			volumePointIndexA = volumeA.IndexOf(pointIndexB);
			volumePointIndexB = volumeA.IndexOf(pointIndexA);
			pointIndexA = volumeA.points[volumePointIndexA];
			pointIndexB = volumeA.points[volumePointIndexB];
		}
		
		List<int> pointIndices = new List<int>();
		for(int i=0; i<volumeASize; i++)
		{
			pointIndices.Add(volumeA.points[i]);
			
			if(i==volumePointIndexA)
			{
				i=volumePointIndexB;
				pointIndices.Add(volumeA.points[i]);
			}
		}
		
		//Add the points from the first volume to the new volume
		for(int i=0; i<volumeASize; i++)
		{
			int facadeIndex = volumeA.points[i];
			if(!pointIndices.Contains(facadeIndex) || facadeIndex==pointIndexA || facadeIndex==pointIndexB)
			{
				BuildrVolumeStylesUnit[] styleUnits = volumeA.styles.GetContentsByFacade(facadeIndex);
				volumeB.Add(facadeIndex, styleUnits);
			}
		}
		
		//Remove point from the first volume
		for(int i=0; i<volumeASize; i++)
		{
			int pointIndex = volumeA.points[i];
			if(!pointIndices.Contains(pointIndex))
			{
				volumeA.Remove(pointIndex);
				volumeASize--;
				i--;
			}
		}
	}
	
	public void MergePoints(int pointIndex, Vector3 newPosition)
	{
		RemovePoint(pointIndex);
		points[pointIndex].vector3 = newPosition;
	}
	
	public BuildrVolume AddVolume(Vector2z[] newPoints)
	{
		if(!BuildrPolyClockwise.Check(newPoints))
			System.Array.Reverse(newPoints);
		
		int numberOfnewPoints = newPoints.Length;
		int pointIndexBase = points.Count;
		points.AddRange(newPoints);
		
		int newVolumeIndex = AddVolume();
		for(int p=0; p<numberOfnewPoints; p++)
			volumes[newVolumeIndex].Add(p+pointIndexBase);

	    return volumes[newVolumeIndex];
	}
	
	public void AddVolume(Vector3[] newPoints, Vector3 offset)
	{
		int numberOfnewPoints = newPoints.Length;
		Vector2z[] new2VzPoints = new Vector2z[numberOfnewPoints];
		for(int p=0; p<numberOfnewPoints; p++)
			new2VzPoints[p] = new Vector2z(newPoints[p]+offset);
		AddVolume(new2VzPoints);
	}

    public int AddVolume()
	{
		BuildrVolume newVolume = ScriptableObject.CreateInstance<BuildrVolume>();
		newVolume.numberOfFloors = Mathf.FloorToInt(newVolume.height/BuildrMeasurements.FLOOR_HEIGHT_MIN);
		volumes.Add(newVolume);
		newVolume.styles = ScriptableObject.CreateInstance<BuildrVolumeStyles>();
		return numberOfVolumes-1;
	}
	
	public void AddVolume(int a, int b, Vector2z[] newPoints)
	{		
		int volumeID = AddVolume();
		
		int pointBase = points.Count;
		int numberOfNewPoints = newPoints.Length;
		points.AddRange(newPoints);
		
		volumes[volumeID].Add(a);
		for(int np=0; np<numberOfNewPoints; np++)
		{
			volumes[volumeID].Add(pointBase);
			pointBase++;
		}
		volumes[volumeID].Add(b);
	}
	
	private void CleanUpVolumes()
	{
		int numberPoints = numberOfPoints;
		List<int> unusedPoints = new List<int>();
		for(int p=0; p<numberPoints; p++)
			unusedPoints.Add(p);
		int volumeCount = numberOfVolumes;
		for(int v=0; v<volumeCount; v++)
		{
			int volumeLinkCount = volumes[v].Count;
			for(int l=0; l<volumeLinkCount; l++)
			{
				if(unusedPoints.Contains(volumes[v].points[l]))
					unusedPoints.Remove(volumes[v].points[l]);
			}
		}
		int pointReduction = 0;
		foreach(int pointIndex in unusedPoints)
		{
			int removePointIndex = pointIndex+pointReduction;
			points.RemoveAt(removePointIndex);
			for(int v=0; v<volumeCount; v++)
			{
				volumes[v].RemoveAndUpdate(removePointIndex);
			}
			pointReduction--;
		}
	}
	
	private List<int> ConnectedVolumes(int pointID)
	{
		List<int> connectedVolumeIDs = new List<int>();
		
		for(int s=0; s<numberOfVolumes; s++)
		{
			if(volumes[s].Contains(pointID))
				connectedVolumeIDs.Add(s);
		}
		return connectedVolumeIDs;
	}
	
	private void SplitSection(int a, int b)
	{
		List<int> sectionIDs = GetVolumeIDs(a,b);
		if(sectionIDs.Count == 0)
			Debug.LogError("Error: Are not in same section");
		
		if(sectionIDs.Count > 1)
			Debug.LogError("Error: You cannot split through sections");
		
		int sectionID = sectionIDs[0];
		int sectionSize = volumes[sectionID].Count;
		int aIndex = volumes[sectionID].IndexOf(a);
		int bIndex = volumes[sectionID].IndexOf(b);
		
		if(bIndex<aIndex)
		{//switchy
			int temp = aIndex;
			aIndex=bIndex;
			bIndex=temp;
		}
		
		AddVolume();
		int newSectionID = volumes.Count-1;
		for(int sl=0; sl<sectionSize; sl++)
		{
			if(sl > aIndex || sl < bIndex)
			{
				volumes[newSectionID].Add(volumes[sectionID].points[sl]);
				volumes[sectionID].RemoveAt(sl);
			}
		}
	}
	
	/// <summary>
	/// Gets the index of the connecting volume by accepting the point values (not the volume point index value).
	/// </summary>
	/// <returns>
	/// The connecting volume index.
	/// </returns>
	/// <param name='volumeIndex'>
	/// Volume index.
	/// </param>
	/// <param name='a'>
	/// The first point index to consider (not the volume point index value).
	/// </param>
	/// <param name='b'>
	/// The second point index to consider (not the volume point index value).
	/// </param>
	public int GetConnectingVolumeIndex(int volumeIndex, int a, int b)
	{
		List<int> volumeIndices = GetVolumeIDs(a, b);
		if(volumeIndices.Count!=2)
			return -1;
		if(volumeIndices[0]==volumeIndices[1])
			return volumeIndex;
		if(volumeIndices[0] == volumeIndex)
			return volumeIndices[1];
		else
			return volumeIndices[0];
	}
	
	//returns the connected volume id for a given two area point index values (not the volume index values)
	private List<int> GetVolumeIDs(int a, int b)
	{
		List<int> aVolumes = new List<int>();
		int s;
		List<int> returnVolumeIDs = new List<int>();
		for(s=0; s<numberOfVolumes; s++)
		{
			if(volumes[s].points.Contains(a))
				aVolumes.Add(s);
		}
		for(s=0; s<numberOfVolumes; s++)
		{
			if(volumes[s].points.Contains(b) && aVolumes.Contains(s))
				returnVolumeIDs.Add(s);
		}
		return returnVolumeIDs;
	}
	
	private int[] GetVolumeLinks(int volumeA, int volumeB)
	{
		int[] returnIDs = new int[]{-1,-1};
		
		int numberOfLinksA = volumes[volumeA].Count;
		for(int la=0; la<numberOfLinksA; la++)
		{
			if(volumes[volumeB].Contains(volumes[volumeA].points[la]))
			{
				if(returnIDs[0] == -1)
				{
					returnIDs[0] = volumes[volumeA].points[la];
				}else{
					returnIDs[1] = volumes[volumeA].points[la];
					return returnIDs;
				}
			}
		}
		
		return returnIDs;
	}
	
	
	////////////////
	//Triangles, YO!
	
	public int[] triangles
	{
		get
		{
			List<int> output = new List<int>();
			Vector2z[] orderedPoints;
			int[] triangles;
			for(int v=0; v<numberOfVolumes; v++)
			{
				orderedPoints = GetOrderedPoints(v).ToArray();
				triangles = EarClipper.Triangulate(orderedPoints);
				for(int i=0; i<triangles.Length; i++)
					output.Add(points.IndexOf(orderedPoints[triangles[i]]));
			}
			
			return output.ToArray();
		}
	}
	
	public int[] GetTrianglesBySectorBase(int sectorIndex)
	{
		Vector2z[] orderedPoints = GetOrderedPoints(sectorIndex).ToArray();
		return EarClipper.Triangulate(orderedPoints);
	}
	
	public int[] GetTrianglesBySector(int sectorIndex)
	{
		List<int> output = new List<int>();
		int[] triangles = GetTrianglesBySectorBase(sectorIndex);
		Vector2z[] orderedPoints = GetOrderedPoints(sectorIndex).ToArray();
		for(int i=0; i<triangles.Length; i++)//need to convert the triangles back to the point index
			output.Add(points.IndexOf(orderedPoints[triangles[i]]));
		
		return output.ToArray();
	}
	
	public Object[] GetUndoObjects()
	{
		List<Object> undoObjects = new List<Object>();
		undoObjects.Add(this);
		undoObjects.AddRange(volumes.ToArray());
		foreach(BuildrVolume volume in volumes)
			undoObjects.Add(volume.styles);
		return undoObjects.ToArray();
	}

    public BuildrPlan Duplicate()
    {
        BuildrPlan newplan = (BuildrPlan)Instantiate(this);
        return newplan;
    }
}
