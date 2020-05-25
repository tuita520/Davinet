using UnityEngine;

public static class ExtensionsQuaternion
{
    public static bool PerComponentIsEqual(this Quaternion a, Quaternion b, float epsilon)
    {
        return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon && Mathf.Abs(a.w - b.w) < epsilon;
    }
}