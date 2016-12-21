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

public class BuildrUtils {

	public static Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point)
	{
    	Vector3 v1 = point - a;
    	Vector3 v2 = (b - a).normalized;
		float distance = Vector3.Distance(a, b);
		float t = Vector3.Dot(v2, v1);
		
		if (t <= 0)
			return a;
		if (t >= distance)
			return b;
		Vector3 v3 = v2 * t;
		Vector3 closestPoint = a + v3;
		return closestPoint;
	}
	
	public static bool FastLineIntersection(Vector2z a1,Vector2z a2, Vector2z b1, Vector2z b2)
    {
        if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            return false;
		return (CCW(a1,b1,b2) != CCW(a2,b1,b2)) && (CCW(a1,a2,b1) != CCW(a1,a2,b2));
	}
	
	public static bool FastLineIntersection(Vector2 a1,Vector2 a2, Vector2 b1, Vector2 b2)
	{
        if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            return false;
		return (CCW(a1,b1,b2) != CCW(a2,b1,b2)) && (CCW(a1,a2,b1) != CCW(a1,a2,b2));
	}
	
	private static bool CCW(Vector2z p1,Vector2z p2, Vector2z p3)
	{
		return ((p2.x-p1.x)*(p3.y-p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
	}
	
	private static bool CCW(Vector2 p1,Vector2 p2, Vector2 p3)
	{
		return ((p2.x-p1.x)*(p3.y-p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
	}

    public static Vector2 FindIntersection(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
    {
        // if abs(angle)==1 then the lines are parallel,  
        // so no intersection is possible  
        if(Mathf.Abs(Vector2.Dot(lineA, lineB)) == 1.0f) return Vector2.zero;

        Vector2 intersectionPoint = IntersectionPoint(lineA, originA, lineB, originB);

        if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
        {
            //flip the second line to find the intersection point
            intersectionPoint = IntersectionPoint(lineA, originA, -lineB, originB);
        }

        if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
        {
//            Debug.Log(intersectionPoint.x+" "+intersectionPoint.y);
            intersectionPoint = originA+lineA;
        }

        return intersectionPoint;
    }

    private static Vector2 IntersectionPoint(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
    {

        float xD1, yD1, xD2, yD2, xD3, yD3;
        float ua, div;

        // calculate differences  
        xD1 = lineA.x;
        xD2 = lineB.x;
        yD1 = lineA.y;
        yD2 = lineB.y;
        xD3 = originA.x - originB.x;
        yD3 = originA.y - originB.y;

        // find intersection Pt between two lines    
        Vector2 pt = new Vector2(0, 0);
        div = yD2 * xD1 - xD2 * yD1;
        ua = (xD2 * yD3 - yD2 * xD3) / div;
        pt.x = originA.x + ua * xD1;
        pt.y = originA.y + ua * yD1;

        // return the valid intersection  
        return pt;
    }

    private static Vector2 IntersectionPoint2(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
    {

        Vector2 lineA2 = lineA + originA;
        Vector2 lineB2 = lineB + originB;

        Vector3 crossA = Vector3.Cross(new Vector3(lineA.x, lineA.y, 1), new Vector3(lineA2.x, lineA2.y, 1));
        Vector3 crossB = Vector3.Cross(new Vector3(lineB.x, lineB.y, 1), new Vector3(lineB2.x, lineB2.y, 1));
        Vector3 crossAB = Vector3.Cross(crossA, crossB);

        Vector2 pt = new Vector2(0, 0);
        pt.x = crossAB.x / crossAB.z;
        pt.x = crossAB.y / crossAB.z;

        // return the valid intersection  
        return pt;
    } 
 
    public static Vector2z FindIntersection(Vector2z lineA, Vector2z originA, Vector2z lineB, Vector2z originB)
    {
        Vector2 returnPoint = FindIntersection(lineA.vector2, originA.vector2, lineB.vector2, originB.vector2);
        return new Vector2z(returnPoint);
    }

    public static bool PointInsidePoly(Vector2z point, Vector2z[] poly)
    {
        Rect polyBounds = new Rect(0,0,0,0);
        foreach(Vector2z polyPoint in poly)
        {
            if(polyBounds.xMin > polyPoint.x)
                polyBounds.xMin = polyPoint.x;
            if(polyBounds.xMax < polyPoint.x)
                polyBounds.xMax = polyPoint.x;
            if(polyBounds.yMin > polyPoint.z)
                polyBounds.yMin = polyPoint.z;
            if(polyBounds.yMax < polyPoint.z)
                polyBounds.yMax = polyPoint.z;
        }
        if(!polyBounds.Contains(point.vector2))
            return false;

        Vector2z pointRight = point + new Vector2z(polyBounds.width, 0);

        int numberOfPolyPoints = poly.Length;
        int numberOfCrossOvers = 0;
        for(int i = 0; i < numberOfPolyPoints; i++)
        {
            Vector2z p0 = poly[i];
            Vector2z p1 = poly[(i + 1) % numberOfPolyPoints];
            if (FastLineIntersection(point, pointRight, p0, p1))
                numberOfCrossOvers++;
        }

        return numberOfCrossOvers % 2 != 0;
    }
}
