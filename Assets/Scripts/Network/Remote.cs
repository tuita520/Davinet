using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Reflection;

namespace Davinet
{
    public class Remote
    {
        public INetEventListener NetEventListener => eventBasedNetListener;

        private EventBasedNetListener eventBasedNetListener;

        /// <summary>
        /// There is only one arbiter per session. The arbiter by default has authority over all objects.
        /// </summary>
        private bool arbiter;
        private bool listenRemote;

        public NetDataWriter SetOwnershipWriter { get; private set; }

        private int remoteID;
        private Dictionary<int, NetPeer> peersByIndex;
        private RemoteObjects remoteObjects;
        private bool writeAll;

        public Remote(RemoteObjects remoteObjects, bool arbiter, bool listenRemote)
        {            
            this.arbiter = arbiter;
            this.listenRemote = listenRemote;
            this.remoteObjects = remoteObjects;

            eventBasedNetListener = new EventBasedNetListener();
            eventBasedNetListener.NetworkReceiveEvent += Read;
            eventBasedNetListener.ConnectionRequestEvent += EventBasedNetListener_ConnectionRequestEvent;
            eventBasedNetListener.PeerConnectedEvent += EventBasedNetListener_PeerConnectedEvent;

            objectsToWrite = new Dictionary<int, OwnableObject>();
            objectsToSpawn = new Dictionary<int, IdentifiableObject>();

            SetOwnershipWriter = new NetDataWriter();
            peersByIndex = new Dictionary<int, NetPeer>();
        }

        private void EventBasedNetListener_PeerConnectedEvent(NetPeer peer)
        {
            Debug.Log("Peer connected.");

            if (arbiter)
            {
                int id = peersByIndex.Count + 1;
                peersByIndex[id] = peer;

                NetDataWriter writer = new NetDataWriter();
                writer.Put((byte)PacketType.Join);
                writer.Put(id);
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
                writer.Reset();

                foreach (var kvp in remoteObjects.statefulObjects)
                {
                    objectsToSpawn.Add(kvp.Key, kvp.Value.GetComponent<IdentifiableObject>());
                    SetOwnership(kvp.Value.GetComponent<OwnableObject>(), kvp.Value.GetComponent<OwnableObject>().Owner);
                }

                var player = Object.Instantiate((remoteObjects.registeredPrefabs[1717083505]));

                Spawn(player);
                SetOwnership(player.GetComponent<OwnableObject>(), id);

                writeAll = true;
            }
        }

        private void EventBasedNetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log("Connection requested.");
            request.Accept();
        }

        public void Spawn(IdentifiableObject o)
        {
            if (arbiter)
            {
                int id = remoteObjects.statefulObjects.Count + 1;

                remoteObjects.statefulObjects.Add(id, o.GetComponent<StatefulObject>());
                objectsToSpawn.Add(id, o);
            }
        }

        public void SetOwnership(OwnableObject r, int owner)
        {
            if (arbiter)
            {
                if (SetOwnershipWriter.Length == 0)
                    SetOwnershipWriter.Put((byte)PacketType.SetOwnership);

                foreach (var kvp in remoteObjects.statefulObjects)
                {
                    if (kvp.Value.GetComponent<OwnableObject>() == r)
                    {
                        SetOwnershipWriter.Put(owner);
                        SetOwnershipWriter.Put(kvp.Key);
                        r.gameObject.GetComponent<OwnableObject>().SetOwner(owner, owner == remoteID);
                        return;
                    }
                }
            }
        }

        private Dictionary<int, IdentifiableObject> objectsToSpawn;
        private Dictionary<int, OwnableObject> objectsToWrite;

        public void WriteState(NetDataWriter writer)
        {
            writer.Put((byte)PacketType.State);

            #region Input
            // Input serialization.
            /*
            writer.Put(inputControllers.Count);

            foreach (var kvp in inputControllers)
            {
                writer.Put(kvp.Key);
                kvp.Value.GetComponent<IStreamable>().Write(writer);
            }
            */
            #endregion

            // Object state serialization.
            foreach (var kvp in remoteObjects.statefulObjects)
            {
                if ((arbiter || kvp.Value.GetComponent<OwnableObject>().Owner == remoteID) && (kvp.Value.GetComponent<IStateful>().ShouldWrite() || writeAll))
                {
                    objectsToWrite.Add(kvp.Key, kvp.Value.GetComponent<OwnableObject>());
                }
            }

            writer.Put(objectsToWrite.Count);

            foreach (var kvp in objectsToWrite)
            {
                writer.Put(kvp.Key);
                kvp.Value.GetComponent<IStateful>().Write(writer);
            }
            
            objectsToWrite.Clear();
            writeAll = false;
        }

        public void WriteStateFields(NetDataWriter writer)
        {
            writer.Put((byte)PacketType.Fields);

            foreach (var kvp in remoteObjects.statefulObjects)
            {
                kvp.Value.WriteStateFields(writer, kvp.Key);
            }
        }

        public void WriteSpawns(NetDataWriter writer)
        {
            if (arbiter)
            {
                writer.Put((byte)PacketType.Spawn);

                foreach (var kvp in objectsToSpawn)
                {
                    writer.Put(kvp.Key);
                    writer.Put(kvp.Value.GUID);
                }
            }

            objectsToSpawn.Clear();
        }

        private void Read(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            PacketType packetType = (PacketType)reader.GetByte();

            switch (packetType)
            {
                case PacketType.State:
                    ReadState(reader);
                    break;
                case PacketType.Fields:
                    ReadStateFields(reader);
                    break;
                case PacketType.Spawn:
                    ReadSpawns(reader);
                    break;
                case PacketType.Join:
                    ReadJoin(reader);
                    break;
                case PacketType.SetOwnership:
                    ReadOwnership(reader);
                    break;
                default:
                    break;
            }
        }

        private void ReadOwnership(NetPacketReader reader)
        {
            while (!reader.EndOfData)
            {
                int owner = reader.GetInt();
                int id = reader.GetInt();

                remoteObjects.statefulObjects[id].GetComponent<OwnableObject>().SetOwner(owner, owner == remoteID);
                Debug.Log($"Granting ownership of {remoteObjects.statefulObjects[id].name} to peer {owner}");
            }
        }

        private void ReadJoin(NetPacketReader reader)
        {            
            remoteID = reader.GetInt();
            Debug.Log($"Local client assigned id {remoteID}");
        }

        private void ReadState(NetPacketReader reader)
        {
            if (listenRemote)
                return;

            #region Input
            // Input deserialization.
            /*
            int inputCount = reader.GetInt();

            for (int i = 0; i < inputCount; i++)
            {
                int id = reader.GetInt();

                if (inputControllers.ContainsKey(id))
                {
                    inputControllers[id].Read(reader);
                }
            }
            */
            #endregion

            int stateCount = reader.GetInt();

            for (int i = 0; i < stateCount; i++)
            {
                int id = reader.GetInt();

                if (remoteObjects.statefulObjects.ContainsKey(id))
                {
                    if (remoteObjects.statefulObjects[id].GetComponent<OwnableObject>().Owner != remoteID)
                        remoteObjects.statefulObjects[id].GetComponent<IStateful>().Read(reader);
                    else
                        remoteObjects.statefulObjects[id].GetComponent<IStateful>().Clear(reader);
                }
            }
        }

        private void ReadStateFields(NetDataReader reader)
        {

        }

        private void ReadSpawns(NetDataReader reader)
        {
            if (listenRemote)
                return;

            while (!reader.EndOfData)
            {
                // Instantiate, etc.
                int id = reader.GetInt();
                int GUID = reader.GetInt();

                // Check if it's already been spawned.
                if (!remoteObjects.statefulObjects.ContainsKey(id))
                {
                    IdentifiableObject clone = Object.Instantiate(remoteObjects.registeredPrefabs[GUID], Vector3.zero, Quaternion.identity);
                    remoteObjects.statefulObjects[id] = clone.GetComponent<StatefulObject>();
                }
            }
        }
    }
}
