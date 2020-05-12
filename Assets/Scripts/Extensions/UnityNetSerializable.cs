using UnityEngine;
using LiteNetLib.Utils;

namespace Davinet
{
    public static class UnityNetSerializable
    {
        public static void Put(this NetDataWriter writer, Vector3 vector)
        {
            writer.Put(vector.x);
            writer.Put(vector.y);
            writer.Put(vector.z);
        }

        public static Vector3 GetVector3(this NetDataReader reader)
        {
            return new Vector3(
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat());
        }

        public static void Put(this NetDataWriter writer, Quaternion quaternion)
        {
            writer.Put(quaternion.x);
            writer.Put(quaternion.y);
            writer.Put(quaternion.z);
            writer.Put(quaternion.w);
        }

        public static Quaternion GetQuaternion(this NetDataReader reader)
        {
            return new Quaternion(
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat());
        }
    }
}
