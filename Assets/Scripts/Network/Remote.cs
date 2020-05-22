using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;

namespace Davinet
{
    /// <summary>
    /// Remotes are responsible for serializing and deserializing changes between <see cref="StatefulWorld"/>s.
    /// This is a controller class that does not maintain any data about the world itself.
    /// </summary>
    public class Remote
    {
        public INetEventListener NetEventListener => eventBasedNetListener;

        private EventBasedNetListener eventBasedNetListener;

        /// <summary>
        /// There is only one arbiter per session. The arbiter by default has ownership over all objects.
        /// </summary>
        private bool arbiter;

        /// <summary>
        /// Listen remotes are clients that share their game instance (and stateful world) with the server.
        /// To avoid certain state updates from being applied twice (like spawning), they ignore some updates
        /// from the server.
        /// </summary>
        private bool listenRemote;

        private int remoteID;
        private Dictionary<int, NetPeer> peersByIndex;
        private StatefulWorld world;

        private bool writeAllStates;
        private bool writeAllFields;

        public Remote(StatefulWorld world, bool arbiter, bool listenRemote)
        {
            this.arbiter = arbiter;
            this.listenRemote = listenRemote;
            this.world = world;

            world.OnAdd += StatefulWorld_OnAdd;
            world.OnSetOwnership += StatefulWorld_OnSetOwnership;

            eventBasedNetListener = new EventBasedNetListener();
            eventBasedNetListener.NetworkReceiveEvent += Read;
            eventBasedNetListener.ConnectionRequestEvent += EventBasedNetListener_ConnectionRequestEvent;
            eventBasedNetListener.PeerConnectedEvent += EventBasedNetListener_PeerConnectedEvent;

            objectsToSpawn = new Dictionary<int, IdentifiableObject>();
            ownershipTransfers = new HashSet<StatefulObject>();

            peersByIndex = new Dictionary<int, NetPeer>();
        }

        #region TODO: Move this out of here
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
                writer.Put(world.Frame);
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
                writer.Reset();

                foreach (var kvp in world.statefulObjects)
                {
                    objectsToSpawn.Add(kvp.Key, kvp.Value.GetComponent<IdentifiableObject>());
                    world.SetOwnership(kvp.Value.GetComponent<OwnableObject>(), kvp.Value.GetComponent<OwnableObject>().Owner);
                }

                var player = Object.Instantiate((world.registeredPrefabsMap[1717083505]));
                world.Add(player.GetComponent<StatefulObject>());
                world.SetOwnership(player.GetComponent<OwnableObject>(), id);

                writeAllStates = true;
                writeAllFields = true;
            }
        }

        private void EventBasedNetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log("Connection requested.");
            request.Accept();
        }
        #endregion

        private void StatefulWorld_OnSetOwnership(OwnableObject o)
        {
            SetOwnership(o);
        }

        private void StatefulWorld_OnAdd(StatefulObject obj)
        {
            if (arbiter)
            {
                IdentifiableObject o = obj.GetComponent<IdentifiableObject>();
                objectsToSpawn.Add(obj.ID, o);
            }
        }

        private void ReadJoin(NetPacketReader reader)
        {
            remoteID = reader.GetInt();
            int frame = reader.GetInt();

            if (!listenRemote)
                world.Frame = frame;

            Debug.Log($"Local client assigned id {remoteID}");
        }

        #region Write
        private Dictionary<int, IdentifiableObject> objectsToSpawn;
        private HashSet<StatefulObject> ownershipTransfers;

        public void WriteState(NetDataWriter writer)
        {
            writer.Put((byte)PacketType.State);
            writer.Put(world.Frame);

            int offset = sizeof(byte) + sizeof(int);

            // Reserve positions for the length of the data for
            // spawns, ownership, statefuls, and fields (in order).
            writer.Put(0);
            writer.Put(0);
            writer.Put(0);
            writer.Put(0);

            int beforeLength = writer.Length;
            WriteSpawns(writer);
            int spawnsLength = writer.Length - beforeLength;

            beforeLength = writer.Length;
            WriteOwnership(writer);
            int ownershipLength = writer.Length - beforeLength;

            beforeLength = writer.Length;
            WriteStateful(writer);
            int statefulLength = writer.Length - beforeLength;

            beforeLength = writer.Length;
            WriteFields(writer);
            int fieldsLength = writer.Length - beforeLength;

            // Set the values of the reserved positions.
            byte[] spawnsBytes = System.BitConverter.GetBytes(spawnsLength);
            writer.Data[offset + 0] = spawnsBytes[0];
            writer.Data[offset + 1] = spawnsBytes[1];
            writer.Data[offset + 2] = spawnsBytes[2];
            writer.Data[offset + 3] = spawnsBytes[3];

            byte[] ownershipBytes = System.BitConverter.GetBytes(ownershipLength);
            writer.Data[offset + 4] = ownershipBytes[0];
            writer.Data[offset + 5] = ownershipBytes[1];
            writer.Data[offset + 6] = ownershipBytes[2];
            writer.Data[offset + 7] = ownershipBytes[3];

            byte[] statefulBytes = System.BitConverter.GetBytes(statefulLength);
            writer.Data[offset + 8] = statefulBytes[0];
            writer.Data[offset + 9] = statefulBytes[1];
            writer.Data[offset + 10] = statefulBytes[2];
            writer.Data[offset + 11] = statefulBytes[3];

            byte[] fieldsBytes = System.BitConverter.GetBytes(fieldsLength);
            writer.Data[offset + 12] = fieldsBytes[0];
            writer.Data[offset + 13] = fieldsBytes[1];
            writer.Data[offset + 14] = fieldsBytes[2];
            writer.Data[offset + 15] = fieldsBytes[3];
        }

        private void WriteStateful(NetDataWriter writer)
        {
            // Object state serialization.
            foreach (var kvp in world.statefulObjects)
            {
                if ((arbiter || kvp.Value.Ownable.HasOwnership(remoteID, world.Frame)) && (kvp.Value.GetComponent<IStateful>().ShouldWrite() || writeAllStates))
                {
                    writer.Put(kvp.Key);
                    kvp.Value.GetComponent<IStateful>().Write(writer);
                }
            }

            writeAllStates = false;
        }

        private void WriteFields(NetDataWriter writer)
        {
            foreach (var kvp in world.statefulObjects)
            {
                if (arbiter || kvp.Value.GetComponent<OwnableObject>().Owner == remoteID)
                {
                    kvp.Value.WriteStateFields(writer, kvp.Key, writeAllFields);
                }
            }

            writeAllFields = false;
        }

        private void WriteSpawns(NetDataWriter writer)
        {
            if (arbiter)
            {
                foreach (var kvp in objectsToSpawn)
                {
                    writer.Put(kvp.Key);
                    writer.Put(kvp.Value.GUID);
                }
            }

            objectsToSpawn.Clear();
        }

        private void WriteOwnership(NetDataWriter writer)
        {
            foreach (StatefulObject stateful in ownershipTransfers)
            {
                writer.Put(stateful.ID);
                writer.Put(stateful.Ownable.Owner);                
            }
        }

        private void SetOwnership(OwnableObject o)
        {
            ownershipTransfers.Add(o.GetComponent<StatefulObject>());

            IInputController inputController = world.statefulObjects[o.GetComponent<StatefulObject>().ID].GetComponent<IInputController>();

            if (inputController != null)
                inputController.SetEnabled(o.Owner == remoteID);
        }
        #endregion

        #region Read
        private void Read(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {          
            PacketType packetType = (PacketType)reader.GetByte();

            Debug.Log($"Reading {packetType} at {Time.time:F4}");

            switch (packetType)
            {
                case PacketType.State:
                    ReadState(reader);
                    break;
                case PacketType.Join:
                    ReadJoin(reader);
                    break;
                default:
                    break;
            }
        }

        private void ReadState(NetPacketReader reader)
        {
            int frame = reader.GetInt();

            int spawnsLength = reader.GetInt();
            int ownershipLength = reader.GetInt();
            int statefulsLength = reader.GetInt();
            int fieldsLength = reader.GetInt();

            ReadSpawns(reader, spawnsLength);
            ReadOwnership(reader, ownershipLength);
            ReadStateful(reader, statefulsLength);
            ReadFields(reader, fieldsLength);
        }

        private void ReadOwnership(NetPacketReader reader, int length)
        {
            int startPosition = reader.Position;

            while (reader.Position - startPosition < length)
            {                
                int id = reader.GetInt();
                int owner = reader.GetInt();

                world.SetOwnership(world.GetStatefulObject(id).Ownable, owner, true);
                IInputController inputController = world.statefulObjects[id].GetComponent<IInputController>();

                if (inputController != null)
                    inputController.SetEnabled(owner == remoteID);
            }
        }

        private void ReadStateful(NetPacketReader reader, int length)
        {
            if (listenRemote)
            {
                reader.SkipBytes(length);
                return;
            }

            int startPosition = reader.Position;

            while (reader.Position - startPosition < length)
            {
                int id = reader.GetInt();

                if (world.statefulObjects[id].GetComponent<OwnableObject>().Owner != remoteID)
                    world.statefulObjects[id].GetComponent<IStateful>().Read(reader);
                else
                    world.statefulObjects[id].GetComponent<IStateful>().Pass(reader);
            }
        }

        private void ReadFields(NetDataReader reader, int length)
        {
            if (listenRemote)
            {
                reader.SkipBytes(length);
                return;
            }

            int startPosition = reader.Position;

            // Clear the first byte of the payload. This will be
            // a StatefulObject.DataType.Object enum.
            // TODO: Should not send the packet if no payload.
            if (reader.Position - startPosition < length)
                reader.GetByte();

            while (reader.Position - startPosition < length)
            {
                int id = reader.GetInt();

                if (world.statefulObjects[id].GetComponent<OwnableObject>().Owner != remoteID)
                    world.statefulObjects[id].ReadStateFields(reader);
                else
                    world.statefulObjects[id].ReadStateFields(reader, true);
            }
        }

        private void ReadSpawns(NetDataReader reader, int length)
        {
            if (listenRemote)
            {
                reader.SkipBytes(length);
                return;
            }

            int startPosition = reader.Position;

            while (reader.Position - startPosition < length)
            {
                // Instantiate, etc.
                int id = reader.GetInt();
                int GUID = reader.GetInt();

                // Check if it's already been spawned.
                if (!world.statefulObjects.ContainsKey(id))
                {
                    IdentifiableObject clone = Object.Instantiate(world.registeredPrefabsMap[GUID], Vector3.zero, Quaternion.identity);
                    world.statefulObjects[id] = clone.GetComponent<StatefulObject>();
                }
            }
        }
        #endregion
    }
}
