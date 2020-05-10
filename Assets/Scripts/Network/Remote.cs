using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;

namespace Davinet
{
    public class Remote
    {
        public INetEventListener NetEventListener => eventBasedNetListener;

        private EventBasedNetListener eventBasedNetListener;
        private bool authoritative;

        private Dictionary<int, StatefulRigidbody> statefulObjects;

        public Remote(bool authoritative)
        {
            this.authoritative = authoritative;

            eventBasedNetListener = new EventBasedNetListener();
            eventBasedNetListener.NetworkReceiveEvent += Read;
            eventBasedNetListener.ConnectionRequestEvent += EventBasedNetListener_ConnectionRequestEvent;
            eventBasedNetListener.PeerConnectedEvent += EventBasedNetListener_PeerConnectedEvent;

            statefulObjects = new Dictionary<int, StatefulRigidbody>();

            int i = 0;

            foreach (StatefulRigidbody statefulObject in UnityEngine.Object.FindObjectsOfType<StatefulRigidbody>())
            {
                statefulObjects[i] = statefulObject;
                i++;
            }
        }

        private void EventBasedNetListener_PeerConnectedEvent(NetPeer peer)
        {
            Debug.Log("Peer connected.");
        }

        private void EventBasedNetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log("Connection requested.");
            request.Accept();
        }

        public void Write(NetDataWriter writer)
        {
            if (authoritative)
            {
                foreach (var kvp in statefulObjects)
                {
                    if (kvp.Value.ShouldWrite())
                    {
                        writer.Put(kvp.Key);
                        kvp.Value.Write(writer);
                    }
                }
            }
        }

        private void Read(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (!authoritative)
            {
                while (!reader.EndOfData)
                {
                    int id = reader.GetInt();
                    statefulObjects[id].Read(reader);
                }
            }
        }
    }
}
