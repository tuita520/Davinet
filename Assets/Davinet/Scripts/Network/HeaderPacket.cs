using LiteNetLib.Utils;

namespace Davinet
{
    public class HeaderPacket
    {
        private readonly int offset;
        private readonly NetDataWriter writer;

        private int currentHeader;
        private int previousDataLength;

        public HeaderPacket(int headerCount, NetDataWriter writer)
        {
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
            int length = writer.Length - previousDataLength;

            byte[] lengthBytes = System.BitConverter.GetBytes(length);
            writer.Data[offset + currentHeader + 0] = lengthBytes[0];
            writer.Data[offset + currentHeader + 1] = lengthBytes[1];
            writer.Data[offset + currentHeader + 2] = lengthBytes[2];
            writer.Data[offset + currentHeader + 3] = lengthBytes[3];

            previousDataLength = writer.Length;
            currentHeader++;
        }
    }
}
