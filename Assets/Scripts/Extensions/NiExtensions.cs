using InfectedRose.Nif;
using UnityEngine;
using UVector3 = UnityEngine.Vector3;
using Vector3 = InfectedRose.Nif.Vector3;

public static class NiExtensions
{
    public static UVector3 ToUVector(this Vector3 vector3)
    {
        return new UVector3(vector3.x, vector3.y, vector3.z);
    }
    
    public static UVector3 ToEulerAngles(this Matrix33 matrix3X3)
    {
        var sy = Mathf.Sqrt(Mathf.Pow(matrix3X3.m11, 2) + Mathf.Pow(matrix3X3.m21, 2));

        var singular = sy < 1e-6;

        UVector3 angles;

        if (!singular)
        {
            angles = new UVector3
            {
                x = Mathf.Atan2(matrix3X3.m32, matrix3X3.m33),
                y = Mathf.Atan2(-matrix3X3.m31, sy),
                z = Mathf.Atan2(matrix3X3.m21, matrix3X3.m11)
            };
        }
        else
        {
            angles = new Vector2
            {
                x = Mathf.Atan2(-matrix3X3.m23, matrix3X3.m22),
                y = Mathf.Atan2(-matrix3X3.m31, sy)
            };
        }

        return angles * 180;
    }
}