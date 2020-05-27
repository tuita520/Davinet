using LiteNetLib.Utils;

namespace Davinet
{
    public interface IStreamable
    {
        void Read(NetDataReader reader);
        void Write(NetDataWriter writer);

        /// <summary>
        /// Skip the reader ahead of the updates for this object without applying them.
        /// </summary>
        /// <param name="reader"></param>
        void Pass(NetDataReader reader);
    }
}
