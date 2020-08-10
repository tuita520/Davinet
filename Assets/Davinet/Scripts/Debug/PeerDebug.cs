using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Davinet
{
    public class PeerDebug : MonoBehaviour
    {
        public class Settings
        {
            public bool simulateLatency;
            public int maxLatency;
            public int minLatency;

            public bool simulatePacketLoss;
            public int packetLossChance;
        }

        public Settings settings;

        private List<System.Tuple<float, Peer.StatePacket>> delayedPackets;

        public void Initialize(Settings settings)
        {
            this.settings = settings;

            delayedPackets = new List<System.Tuple<float, Peer.StatePacket>>();
        }

        public void InsertDelayedReader(float latency, Peer.StatePacket packet)
        {
            delayedPackets.Add(new System.Tuple<float, Peer.StatePacket>(Time.time + latency, packet));
        }

        public IEnumerable<Peer.StatePacket> GetAllReadyPackets()
        {
            List<Peer.StatePacket> readyPackets = new List<Peer.StatePacket>();

            for (int i = 0; i < delayedPackets.Count; i++)
            {
                if (Time.time >= delayedPackets[i].Item1)
                {
                    readyPackets.Add(delayedPackets[i].Item2);
                    delayedPackets.RemoveAt(i);
                    i--;
                }
            }

            return readyPackets;
        }

        public void SendStateDelayed(NetDataWriter writer, NetManager manager, bool hasListenClient)
        {
            StartCoroutine(SendStateDelayedRoutine(writer, manager, hasListenClient));
        }

        private IEnumerator SendStateDelayedRoutine(NetDataWriter writer, NetManager manager, bool hasListenClient)
        {
            if (hasListenClient)
                manager.ConnectedPeerList.Find(x => x.Id == 0).Send(writer, DeliveryMethod.ReliableOrdered);

            int delayMilliseconds = Random.Range(settings.minLatency, settings.maxLatency);
            yield return new WaitForSeconds(delayMilliseconds / (float)1000);

            for (int i = 0; i < manager.ConnectedPeerList.Count; i++)
            {
                if (!hasListenClient || manager.ConnectedPeerList[i].Id != 0)
                    manager.ConnectedPeerList[i].Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}