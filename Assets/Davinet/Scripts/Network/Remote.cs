﻿using UnityEngine;
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

        private bool writeAll;

        public Remote(StatefulWorld world, bool arbiter, bool listenRemote, int remoteID)
        {
            this.arbiter = arbiter;
            this.listenRemote = listenRemote;
            this.world = world;
            this.remoteID = remoteID;

            world.OnAdd += StatefulWorld_OnAdd;

            if (arbiter)
            {
                foreach (var kvp in world.statefulObjects)
                {
                    kvp.Value.Ownable.CanRelinquishAuthority = true;
                }
            }

            objectsToSpawn = new Dictionary<int, IdentifiableObject>();
            peersByIndex = new Dictionary<int, NetPeer>();
        }

        /// <summary>
        /// Writes all data about all objects (whether dirty or not) to the next state update.
        /// Can be used to ensure the world is fully in sync when new peers join.
        /// </summary>
        public void SynchronizeAll()
        {
            foreach (var kvp in world.statefulObjects)
            {
                objectsToSpawn.Add(kvp.Key, kvp.Value.GetComponent<IdentifiableObject>());
                kvp.Value.GetComponent<OwnableObject>().GrantOwnership(kvp.Value.Ownable.Owner.Value);
            }

            writeAll = true;
        }

        public int AddPeer(NetPeer peer)
        {
            int id = peersByIndex.Count + 1;
            peersByIndex[id] = peer;

            return id;
        }

        private void StatefulWorld_OnAdd(StatefulObject obj)
        {
            if (obj.IsDirty && arbiter)
            {
                IdentifiableObject o = obj.GetComponent<IdentifiableObject>();

                // Only the arbiter is permitted to relinquish an object's authority;
                // non-arbiter remotes will hang on to authority until they receive confirmation
                // from the arbiter that the object has relinquished.
                o.GetComponent<StatefulObject>().Ownable.CanRelinquishAuthority = true;
                objectsToSpawn.Add(obj.ID, o);
            }
        }

        #region Write
        private Dictionary<int, IdentifiableObject> objectsToSpawn;

        public void WriteState(NetDataWriter writer)
        {
            // TODO: This packet structure requires lots of hard coding, making it not
            // very error resistent. Should either find a better structure, or encapsulate
            // common functionality.
            writer.Put((byte)PacketType.State);
            writer.Put(world.Frame);

            int offset = sizeof(byte) + sizeof(int);

            // Reserve positions for the length of the data for spawns, ownership, statefuls, 
            // and fields (in order).
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

            writeAll = false;
        }

        private void WriteStateful(NetDataWriter writer)
        {
            foreach (var kvp in world.statefulObjects)
            {
                if (arbiter || kvp.Value.Ownable.HasAuthority(remoteID))
                {
                    IStreamable streamable = kvp.Value.GetComponent<IStreamable>();

                    if (streamable != null)
                    {
                        writer.Put(kvp.Key);
                        streamable.Write(writer);
                    }
                }
            }
        }

        private void WriteFields(NetDataWriter writer)
        {
            foreach (var kvp in world.statefulObjects)
            {
                if (arbiter || kvp.Value.Ownable.HasAuthority(remoteID))
                {
                    kvp.Value.WriteStateFields(writer, kvp.Key, writeAll);
                }
            }
        }

        private void WriteSpawns(NetDataWriter writer)
        {
            if (arbiter)
            {
                foreach (var kvp in objectsToSpawn)
                {
                    writer.Put(kvp.Key);
                    writer.Put(kvp.Value.GUID);

                    kvp.Value.GetComponent<StatefulObject>().IsDirty = false;
                }
            }

            objectsToSpawn.Clear();
        }

        private void WriteOwnership(NetDataWriter writer)
        {
            foreach (var kvp in world.statefulObjects)
            {
                kvp.Value.GetComponent<OwnableObject>().Write(writer, kvp.Key, arbiter, writeAll);
            }
        }
        #endregion

        #region Read
        public void ReadState(NetPacketReader reader)
        {
            int frame = reader.GetInt();

            if (!arbiter && !listenRemote)
                world.Frame = frame;

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

                IInputController inputController = world.statefulObjects[id].GetComponent<IInputController>();

                // If this remote is not the arbiter, the one sending the data must be.
                world.GetStatefulObject(id).Ownable.Read(reader, arbiter);

                if (inputController != null)
                    inputController.SetEnabled(world.GetStatefulObject(id).Ownable.HasOwnership(remoteID));
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

                if (!world.GetStatefulObject(id).Ownable.HasAuthority(remoteID))
                    world.statefulObjects[id].GetComponent<IStreamable>().Read(reader);
                else
                    world.statefulObjects[id].GetComponent<IStreamable>().Pass(reader);
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
            if (reader.Position - startPosition < length)
                reader.GetByte();

            while (reader.Position - startPosition < length)
            {
                int id = reader.GetInt();

                if (!world.statefulObjects[id].Ownable.HasAuthority(remoteID))
                    world.statefulObjects[id].ReadStateFields(reader, arbiter);
                else
                    world.statefulObjects[id].ReadStateFields(reader, arbiter, true);
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
                    clone.GetComponent<StatefulObject>().IsDirty = false;
                    world.Add(clone.GetComponent<StatefulObject>(), id);
                }
            }
        }
        #endregion
    }
}
