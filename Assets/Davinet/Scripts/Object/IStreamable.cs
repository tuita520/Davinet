using LiteNetLib.Utils;

namespace Davinet
{
    public interface IStreamable
    {
        void Read(NetDataReader reader);
        void Write(NetDataWriter writer);
        void Pass(NetDataReader reader);
    }
}
