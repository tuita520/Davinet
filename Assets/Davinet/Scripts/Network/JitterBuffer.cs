using LiteNetLib;
using System.Collections.Generic;

namespace Davinet
{
    /// <summary>
    /// Instead of immediately applying state updates to the world, state packets
    /// instead can be added to a jitter buffer. This buffer allows the world to withdraw
    /// the state packets one per frame. As well, it can hold arriving packets for a short period
    /// (typically for no more than 10ms or so, depending on network conditions); this delay 
    /// allows out-of-order packets to arrive so that the world may withdraw them in a consistent order 
    /// for a smoother simulation.
    /// https://gafferongames.com/post/state_synchronization/
    /// </summary>
    public class JitterBuffer
    {
        public class StatePacket
        {
            public int remoteFrame;
            public int localArrivalFrame;
            public NetPacketReader reader;
        }

        // Contains packets that have arrived, but not yet been sent to the remote.
        // Is sorted when a new element is inserted by the order they were sent.
        private List<StatePacket> buffer;

        private int delayFrames;

        public JitterBuffer(int delayFrames)
        {
            this.delayFrames = delayFrames;

            buffer = new List<StatePacket>();            
        }

        public void Insert(NetPacketReader statePacketReader, int currentFrame)
        {
            // TODO: What to do when the buffer gets too large, for any reason?
            // Maybe fast forward and apply multiple per frame?
            if (buffer.Count > 200)
                return;

            int frame = statePacketReader.GetInt();
            
            StatePacket packet = new StatePacket()
            {
                remoteFrame = frame,
                localArrivalFrame = currentFrame,
                reader = statePacketReader
            };

            // Iterate through the buffer to determine where the new packet should be placed.
            for (int i = 0; i < buffer.Count + 1; i++)
            {
                // If the packet has arrived in order with respect to all elements in the buffer, its
                // remote frame should be larger than all other elements in the buffer, and it will
                // be inserted at the front of the buffer.
                if (i == buffer.Count || packet.remoteFrame > buffer[i].remoteFrame)
                {
                    buffer.Insert(i, packet);
                    break;
                }
                // If the packet arrived out of order with respect to the next packet in the buffer,
                // swap arrival frames with the next packet. This allows the buffer to more accurately represent
                // the cadence that the packets were sent.
                else
                {
                    int tempArrivalFrame = packet.localArrivalFrame;
                    packet.localArrivalFrame = buffer[i].localArrivalFrame;
                    buffer[i].localArrivalFrame = tempArrivalFrame;
                }
            }
        }

        public bool TryGetPacket(out StatePacket reader, int currentFrame)
        {
            if (buffer.Count > 0 && currentFrame - buffer[buffer.Count - 1].localArrivalFrame >= delayFrames)
            {
                reader = buffer[buffer.Count - 1];
                buffer.RemoveAt(buffer.Count - 1);

                return true;
            }

            reader = null;
            return false;
        }
    }
}
