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

/// <summary>
/// A custom Vector2 class that deals in an XZ plane rather than the default XY. Simplifies a lot of code. Most of the functionality of Vector2 has been implimented here
/// </summary>

[System.Serializable]
public class Vector2z
{

    /// <summary>
    /// Returns an empty Vector2z
    /// </summary>
    public static Vector2z zero
    {
        get { return new Vector2z(0, 0); }
    }

    public static Vector2z Lerp(Vector2z a, Vector2z b, float val)
    {
        Vector2z lerped = new Vector2z();
        lerped.vector2 = Vector2.Lerp(a.vector2, b.vector2, val);
        return lerped;
    }

    public float x = 0;
    public float z = 0;

    public Vector2z()
    {
        //nothing
    }

    public Vector2z(float _x, float _z)
    {
        x = _x;
        z = _z;
    }

    public Vector2z(string _x, string _z)
    {
        x = float.Parse(_x);
        z = float.Parse(_z);
    }

    public Vector2z(Vector2 v2)
    {
        x = v2.x;
        z = v2.y;
    }

    public Vector2z(Vector3 v3)
    {
        x = v3.x;
        z = v3.z;
    }

    public Vector3 vector3
    {
        get { return new Vector3(x, 0, z); }

        set
        {
            x = value.x;
            z = value.z;
        }
    }

    public Vector2 vector2
    {
        get { return new Vector3(x, z); }

        set
        {
            x = value.x;
            z = value.y;
        }
    }

    public float y
    {
        get { return z; }
        set { z = value; }
    }

    public static Vector2z operator +(Vector2z a, Vector2z b)
    {
        return new Vector2z(a.x + b.x, a.z + b.z);
    }

    public static Vector2z operator -(Vector2z a, Vector2z b)
    {
        return new Vector2z(a.x - b.x, a.z - b.z);
    }

    public static Vector2z operator *(Vector2z a, Vector2z b)
    {
        return new Vector2z(a.x * b.x, a.z * b.z);
    }

    public static Vector2z operator /(Vector2z a, Vector2z b)
    {
        return new Vector2z(a.x / b.x, a.z / b.z);
    }

    public bool Equals(Object a)
    {
        return base.Equals(a);// (Dot(this, a) > 0.999f);
    }

    public override bool Equals(object a)
    {
        return base.Equals(a);// (Dot(this, a) > 0.999f);
    }

    public bool Equals(Vector2z a)
    {
        return (Dot(this, a) > 0.999f);
    }

    public override int GetHashCode()
    {
        return Mathf.RoundToInt(Mathf.Pow(x,z));
    }

    public static bool operator ==(Vector2z a, Vector2z b)
    {
        return a.vector2 == b.vector2;
    }

    public static bool operator !=(Vector2z a, Vector2z b)
    {
        return a.vector2 != b.vector2;
    }

    public static float Distance(Vector2z a, Vector2z b)
    {
        return Mathf.Sqrt(SqrMag(a,b));
    }

    public static float SqrMag(Vector2z a, Vector2z b)
    {
        float x = Mathf.Abs(b.x - a.x);
        float z = Mathf.Abs(b.z - a.z);
        return (x * x + z * z);
    }

    public static float Dot(Vector2z a, Vector2z b)
    {
        return Vector2.Dot(a.vector2, b.vector2);
    }

    override public string ToString()
    {
        return ("("+x.ToString("F2")+" , "+z.ToString("F2")+")");
    }
}
