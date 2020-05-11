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

        private Dictionary<int, IdentifiableObject> registeredPrefabs;

        // TODO: Make input polymorphic.
        private Dictionary<int, PlayerInputController> inputControllers;
        private Dictionary<int, StatefulRigidbody> statefulObjects;

        public Remote(Dictionary<int, IdentifiableObject> registeredPrefabs, bool authoritative)
        {
            this.registeredPrefabs = registeredPrefabs;
            this.authoritative = authoritative;

            eventBasedNetListener = new EventBasedNetListener();
            eventBasedNetListener.NetworkReceiveEvent += Read;
            eventBasedNetListener.ConnectionRequestEvent += EventBasedNetListener_ConnectionRequestEvent;
            eventBasedNetListener.PeerConnectedEvent += EventBasedNetListener_PeerConnectedEvent;

            statefulObjects = new Dictionary<int, StatefulRigidbody>();
            int i = 0;

            foreach (StatefulRigidbody statefulObject in Object.FindObjectsOfType<StatefulRigidbody>())
            {
                statefulObjects[i] = statefulObject;
                i++;
            }

            inputControllers = new Dictionary<int, PlayerInputController>();
            i = 0;

            foreach (PlayerInputController inputController in Object.FindObjectsOfType<PlayerInputController>())
            {
                inputControllers[i] = inputController;
                i++;
            }

            objectsToWrite = new Dictionary<int, StatefulRigidbody>();
            objectsToSpawn = new Dictionary<int, IdentifiableObject>();
        }

        private void EventBasedNetListener_PeerConnectedEvent(NetPeer peer)
        {
            Debug.Log("Peer connected.");

            if (authoritative)
            {
                Spawn(Object.Instantiate((registeredPrefabs[1717083505])));
            }
        }

        private void EventBasedNetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log("Connection requested.");
            request.Accept();
        }

        public void Spawn(IdentifiableObject o)
        {
            if (authoritative)
            {
                int id = statefulObjects.Count + 1;

                statefulObjects.Add(id, o.GetComponent<StatefulRigidbody>());
                objectsToSpawn.Add(id, o);
            }
        }

        private Dictionary<int, IdentifiableObject> objectsToSpawn;
        private Dictionary<int, StatefulRigidbody> objectsToWrite;

        public void Write(NetDataWriter writer)
        {           
            if (authoritative)
            {
                writer.Put(objectsToSpawn.Count);

                foreach (var kvp in objectsToSpawn)
                {
                    writer.Put(kvp.Key);
                    writer.Put(kvp.Value.GUID);
                }

                foreach (var kvp in statefulObjects)
                {
                    if (kvp.Value.ShouldWrite())
                    {
                        objectsToWrite.Add(kvp.Key, kvp.Value);
                    }
                }

                writer.Put(objectsToWrite.Count);

                foreach (var kvp in objectsToWrite)
                {
                    writer.Put(kvp.Key);
                    kvp.Value.Write(writer);
                }
            }

            objectsToSpawn.Clear();
            objectsToWrite.Clear();
        }

        private void Read(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (!authoritative)
            {
                int objectsToSpawnCount = reader.GetInt();

                for (int i = 0; i < objectsToSpawnCount; i++)
                {
                    // Instantiate, etc.
                    int id = reader.GetInt();
                    int GUID = reader.GetInt();

                    // Check if it's already been spawned.
                    if (!statefulObjects.ContainsKey(id))
                    {
                        IdentifiableObject clone = Object.Instantiate(registeredPrefabs[GUID], Vector3.zero, Quaternion.identity);
                        statefulObjects[id] = clone.GetComponent<StatefulRigidbody>();
                    }
                }

                int stateCount = reader.GetInt();

                for (int i = 0; i < stateCount; i++)
                {
                    int id = reader.GetInt();

                    if (statefulObjects.ContainsKey(id))
                    {
                        statefulObjects[id].Read(reader);
                    }
                }
            }
        }
    }
}
