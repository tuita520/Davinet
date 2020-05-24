using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace Davinet
{
    public class PeerDebug
    {
        public bool simulateLatency;
        public int maxLatency;
        public int minLatency;

        public bool simulatePacketLoss;
        public int packetLossChance;

        private List<Tuple<float, NetPacketReader>> simulatedLatentStateReaders;
        private List<Tuple<float, NetDataWriter>> simulatedLatentStateWriters;

        public PeerDebug()
        {
            simulatedLatentStateReaders = new List<Tuple<float, NetPacketReader>>();
            simulatedLatentStateWriters = new List<Tuple<float, NetDataWriter>>();
        }
    }
}