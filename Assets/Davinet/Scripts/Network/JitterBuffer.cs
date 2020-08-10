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
            public int bufferFramesRemaining;
            public NetPacketReader reader;
        }

        // Contains packets that have arrived, but not yet been sent to the remote.
        // Is sorted when a new element is inserted by the order they were sent.
        private readonly List<StatePacket> buffer;

        private readonly int delayFrames;

        private readonly List<StatePacket> outputPackets;

        public JitterBuffer(int delayFrames)
        {
            this.delayFrames = delayFrames;

            buffer = new List<StatePacket>();
            outputPackets = new List<StatePacket>();
        }

        public void Insert(NetPacketReader statePacketReader)
        {
            int frame = statePacketReader.GetInt();

            StatePacket packet = new StatePacket()
            {
                remoteFrame = frame,
                bufferFramesRemaining = buffer.Count > 0 ? buffer[0].bufferFramesRemaining + 1 : delayFrames,
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
                    int tempArrivalFrame = packet.bufferFramesRemaining;
                    packet.bufferFramesRemaining = buffer[i].bufferFramesRemaining;
                    buffer[i].bufferFramesRemaining = tempArrivalFrame;
                }
            }

            Debug.Log($"<color=yellow><b>Inserting</b></color> packet with remote frame <b>{packet.remoteFrame}</b> into jitter buffer. " +
                $"<i>(Buffer is now size {buffer.Count}.)</i>", LogType.JitterBuffer);
        }

        public IEnumerable<StatePacket> StepBuffer()
        {
            DecrementBufferFrames();

            outputPackets.Clear();

            // If the buffer is too large, it is better to flush multiple packets all at once
            // to catch up to the sender. This will cause popping, but is preferable to perpetually
            // lagging behind.
            // If this is happening too frequently, the buffer delayFrames should be increased.
            while (buffer.Count > delayFrames)
            {
                StatePacket output = buffer[buffer.Count - 1];

                outputPackets.Add(output);

                Debug.Log($"Buffer is size {buffer.Count}; <b><color=orange>Flushing</color></b> packet with remote frame <b>{output.remoteFrame}</b>. " +
                    $"<i>(Buffer is now size {buffer.Count - 1}.)</i>", LogType.JitterBuffer);

                buffer.RemoveAt(buffer.Count - 1);             
                DecrementBufferFrames();
            }

            if (buffer.Count > 0 && buffer[buffer.Count - 1].bufferFramesRemaining <= 0)
            {
                StatePacket output = buffer[buffer.Count - 1];
                outputPackets.Add(output);
                buffer.RemoveAt(buffer.Count - 1);                

                Debug.Log($"<b><color=cyan>Removing</color></b> packet with remote frame <b>{output.remoteFrame}</b> from jitter buffer. " +
                    $"<i>(Buffer is now size {buffer.Count}.)</i>", LogType.JitterBuffer);
            }

            return outputPackets;
        }

        private void DecrementBufferFrames()
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                buffer[i].bufferFramesRemaining--;
            }
        }
    }
}
