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

        private List<System.Tuple<float, NetPacketReader>> delayedReaders;

        public void Initialize(Settings settings)
        {
            this.settings = settings;

            delayedReaders = new List<System.Tuple<float, NetPacketReader>>();
        }

        public void InsertDelayedReader(float latency, NetPacketReader reader)
        {
            delayedReaders.Add(new System.Tuple<float, NetPacketReader>(Time.time + latency, reader));
        }

        public IEnumerable<NetPacketReader> GetAllReadyReaders()
        {
            List<NetPacketReader> readyReaders = new List<NetPacketReader>();

            for (int i = 0; i < delayedReaders.Count; i++)
            {
                if (Time.time >= delayedReaders[i].Item1)
                {
                    readyReaders.Add(delayedReaders[i].Item2);
                    delayedReaders.RemoveAt(i);
                    i--;
                }
            }

            return readyReaders;
        }

        public void SendStateDelayed(NetDataWriter writer, NetManager manager)
        {
            StartCoroutine(SendStateDelayedRoutine(writer, manager));
        }

        private IEnumerator SendStateDelayedRoutine(NetDataWriter writer, NetManager manager)
        {
            int delayMilliseconds = Random.Range(settings.minLatency, settings.maxLatency);
            yield return new WaitForSeconds(delayMilliseconds / (float)1000);
            manager.SendToAll(writer, DeliveryMethod.ReliableUnordered);
        }
    }
}