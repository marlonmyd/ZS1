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

public class EarClipper 
{

	public static int[] Triangulate( Vector2z[] points)
	{
		int numberOfPoints = points.Length;
		List<int> usePoints = new List<int>();
		for(int p=0; p<numberOfPoints; p++)
			usePoints.Add(p);
		int numberOfUsablePoints = usePoints.Count;
		List<int> indices = new List<int>();
		
        if (numberOfPoints < 3)
            return indices.ToArray();
		
		bool freeOfIntersections;
		int it=100;
		while(numberOfUsablePoints > 2)
		{
			for(int i=0; i<numberOfUsablePoints; i++)
			{
				int a,b,c;
				
				a = usePoints[i];
				b = usePoints[(i+1)%numberOfUsablePoints];
				c = usePoints[(i+2)%numberOfUsablePoints];
				
				Vector2 pA = points[a].vector2;
				Vector2 pB = points[b].vector2;
				Vector2 pC = points[c].vector2;
				
				float dA = Vector2.Distance(pA,pB);
				float dB = Vector2.Distance(pB,pC);
				float dC = Vector2.Distance(pC,pA);
				
				float angle = Mathf.Acos((Mathf.Pow(dB,2)-Mathf.Pow(dA,2)-Mathf.Pow(dC,2))/(2*dA*dC))*Mathf.Rad2Deg * Mathf.Sign(Sign(points[a],points[b],points[c]));
				if(angle < 0)
				{
					continue;//angle is not reflex
				}
				
				freeOfIntersections = true;
				//check that no point is inside the new triangle
				for(int p=0; p<numberOfUsablePoints; p++)
				{
					int pu = usePoints[p];
					if(pu==a || pu==b || pu==c)
						continue;
					
					if(IntersectsTriangle2(points[a],points[b],points[c],points[pu]))
					{
						freeOfIntersections=false;
						break;
					}
				}
				
				//check that no line midpoint is inside the new triangle
				for(int p=0; p<numberOfUsablePoints; p++)
				{
					int pa = usePoints[p];
					if(pa==a || pa==b || pa==c)
						continue;
					int pb = (p+1)%numberOfPoints;
					Vector2z pab = Vector2z.Lerp(points[pa],points[pb],0.5f);
					
					if(IntersectsTriangle2(points[a],points[b],points[c],pab))
					{
						freeOfIntersections=false;
						break;
					}
				}
				
				if(freeOfIntersections)
				{
					indices.Add(a);
					indices.Add(b);
					indices.Add(c);
					usePoints.Remove(b);
					numberOfUsablePoints = usePoints.Count;
					i--;
					it=100;
					break;
				}
				
			}
			it--;
			if(it<0)
			{
				indices.Reverse();
				return indices.ToArray();
			}
		}
		
		indices.Reverse();
		
		return indices.ToArray();
	}
	
	private static float FindTriAngle(Vector2 pA, Vector2 pB, Vector2 pC)
	{
		float dA = Vector2.Distance(pA,pB);
		float dB = Vector2.Distance(pB,pC);
		float dC = Vector2.Distance(pC,pA);
		return Mathf.Acos((Mathf.Pow(dB,2)-Mathf.Pow(dA,2)-Mathf.Pow(dC,2))/(2*dA*dC))*Mathf.Rad2Deg * Mathf.Sign(Sign(pA,pB,pC));		
	}
	
	private static bool IntersectsTriangle(Vector2z A, Vector2z B, Vector2z C, Vector2z P)
	{
		bool b1, b2, b3;

		b1 = Sign(P, A, B) < 0.0f;
		b2 = Sign(P, B, C) < 0.0f;
		b3 = Sign(P, C, A) < 0.0f;
		
		return ((b1 == b2) && (b2 == b3));
	}
	
	private static float Sign(Vector2z p1, Vector2z p2, Vector2z p3)
	{
		return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
	}
	
	private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
	}
					
	private static bool IntersectsTriangle2(Vector2z A, Vector2z B, Vector2z C, Vector2z P)
	{
			float planeAB = (A.x-P.x)*(B.y-P.y)-(B.x-P.x)*(A.y-P.y);
			float planeBC = (B.x-P.x)*(C.y-P.y)-(C.x - P.x)*(B.y-P.y);
			float planeCA = (C.x-P.x)*(A.y-P.y)-(A.x - P.x)*(C.y-P.y);
			return Sign(planeAB)==Sign(planeBC) && Sign(planeBC)==Sign(planeCA);
	}
	
	private static int Sign(float n) 
	{
		return (int)(Mathf.Abs(n)/n);
	}
	
	public static bool FastLineIntersection(Vector2z a1,Vector2z a2, Vector2z b1, Vector2z b2)
	{
		return (CCW(a1,b1,b2) != CCW(a2,b1,b2)) && (CCW(a1,a2,b1) != CCW(a1,a2,b2));
	}
	
	private static bool CCW(Vector2z p1,Vector2z p2, Vector2z p3)
	{
		return ((p2.x-p1.x)*(p3.y-p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
	}
}
