using LiteNetLib;
using System.Collections.Generic;

namespace Davinet
{
    public class JitterBuffer
    {
        public class StatePacket
        {
            public int remoteFrame;
            public NetPacketReader reader;
        }

        // Contains packets that have arrived, but not yet been sent to the remote.
        // Is sorted by the order they were sent.
        private List<StatePacket> buffer;

        public JitterBuffer()
        {
            buffer = new List<StatePacket>();
        }

        public void Insert(NetPacketReader statePacketReader, int arrivalFrame)
        {
            int frame = statePacketReader.GetInt();

            StatePacket packet = new StatePacket()
            {
                remoteFrame = frame,
                reader = statePacketReader
            };

            // Iterate through the buffer to determine where the new packet should be placed.
            for (int i = 0; i < buffer.Count + 1; i++)
            {
                if (i == buffer.Count || buffer[i].remoteFrame < packet.remoteFrame)
                {
                    buffer.Insert(i, packet);
                }
            }
        }

        public bool TryGetPacket(out StatePacket reader)
        {
            if (buffer.Count > 0)
            {
                reader = buffer[buffer.Count - 1];
                buffer.RemoveAt(buffer.Count - 1);

                return true;
            }
            else
            {
                reader = null;
                return false;
            }
        }
    }
}
