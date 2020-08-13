using LiteNetLib.Utils;

namespace Davinet
{
    public class HeaderPacketWriter
    {
        private readonly int headerCount;
        private readonly int offset;
        private readonly NetDataWriter writer;

        private int currentHeader;
        private int previousDataLength;

        private const int headerValueSize = sizeof(int);

        public HeaderPacketWriter(int headerCount, NetDataWriter writer)
        {
            this.headerCount = headerCount;
            this.writer = writer;
            offset = writer.Length;

            for (int i = 0; i < headerCount; i++)
            {
                writer.Put(0);
            }

            currentHeader = 0;
            previousDataLength = writer.Length;
        }

        public void WriteCurrentDataSizeToHeader()
        {
            Debug.Assert(currentHeader < headerCount, $"Attempting to write {currentHeader + 1} headers to packet that only supports {headerCount} headers.");

            int length = writer.Length - previousDataLength;

            byte[] lengthBytes = System.BitConverter.GetBytes(length);
            writer.Data[offset + (currentHeader * headerValueSize) + 0] = lengthBytes[0];
            writer.Data[offset + (currentHeader * headerValueSize) + 1] = lengthBytes[1];
            writer.Data[offset + (currentHeader * headerValueSize) + 2] = lengthBytes[2];
            writer.Data[offset + (currentHeader * headerValueSize) + 3] = lengthBytes[3];

            previousDataLength = writer.Length;
            currentHeader++;
        }
    }
}
