using UnityEngine;

public static class ExtensionsVector3
{
    public static bool PerComponentIsEqual(this Vector3 a, Vector3 b, float epsilon)
    {
        Vector3 c = a - b;

        return Mathf.Abs(c.x) < epsilon && Mathf.Abs(c.y) < epsilon && Mathf.Abs(c.z) < epsilon;
    }
}
