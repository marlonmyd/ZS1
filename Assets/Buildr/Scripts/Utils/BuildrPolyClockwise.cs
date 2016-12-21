// BuildR
// Available on the Unity3D Asset Store
// Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

//checks if the provided points follow a clockwise winding
//used to ensure that generated faces render correctly
public class BuildrPolyClockwise
{

    public static bool Check(Vector2z[] points)
    {
        int numberOfPoints = points.Length;
        int i, j, k;
        int count = 0;
        float z;

        if (numberOfPoints < 3)
            return (false);

        for (i = 0; i < numberOfPoints; i++)
        {
            j = (i + 1) % numberOfPoints;
            k = (i + 2) % numberOfPoints;

            Vector2z pointA = points[i];
            Vector2z pointB = points[j];
            Vector2z pointC = points[k];

            z = (pointB.x - pointA.x) * (pointC.y - pointA.y);
            z -= (pointB.y - pointA.y) * (pointC.x - pointA.x);

            if (z < 0)
                count--;
            else if (z > 0)
                count++;
        }

        if (count > 0)
            return (true);
        else if (count < 0)
            return (false);
        else
            return (false);
    }
}
