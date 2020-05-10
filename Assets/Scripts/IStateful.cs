using LiteNetLib.Utils;

namespace Davinet
{
    public interface IStateful
    {
        void Read(NetDataReader reader);
        void Write(NetDataWriter writer);
        bool ShouldWrite();
    }
}
