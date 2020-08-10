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
        // TODO: When NetDataReader is abstracted to an interface,
        // would be nice to find a way to provide for this functionality.
        // This implementation is not robust to developer error.
        void Pass(NetDataReader reader);

        // TODO: This is required to be set when Read is called,
        // and to be initialized to -1; requiring specific implementations
        // is not a good fit for interfaces. Default implementations are not
        // available in a Unity compatible C#, but this should be moved either way.
        int LastReadFrame { get; set; }
    }
}
