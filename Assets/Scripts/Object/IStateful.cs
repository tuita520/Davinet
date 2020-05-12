using LiteNetLib.Utils;

namespace Davinet
{
    public interface IStreamable
    {
        void Read(NetDataReader reader);
        void Write(NetDataWriter writer);
        void Clear(NetDataReader reader);
        bool ShouldWrite();
    }
}
