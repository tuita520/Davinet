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
        /// <summary>
        /// There is only one arbiter per session. The arbiter by default has ownership over all objects.
        /// </summary>
        private readonly bool arbiter;

        /// <summary>
        /// Listen remotes are clients that share their game instance (and stateful world) with the server.
        /// To avoid certain state updates from being applied twice (like spawning), they ignore some updates
        /// from the server.
        /// </summary>
        private readonly bool listenRemote;

        private readonly int remoteID;
        private readonly StatefulWorld world;

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
            writer.Put((byte)PacketType.State);
            writer.Put(world.Frame);

            HeaderPacketWriter headerPacket = new HeaderPacketWriter(5, writer);

            WriteSpawns(writer);
            headerPacket.WriteCurrentDataSizeToHeader();

            WriteOwnership(writer);
            headerPacket.WriteCurrentDataSizeToHeader();

            WriteStateful(writer);
            headerPacket.WriteCurrentDataSizeToHeader();

            WriteFields(writer);
            headerPacket.WriteCurrentDataSizeToHeader();

            WriteEvents(writer);
            headerPacket.WriteCurrentDataSizeToHeader();

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

        private void WriteEvents(NetDataWriter writer)
        {
            foreach (var kvp in world.statefulObjects)
            {
                if (!kvp.Value.Ownable.HasAuthority(remoteID))
                {
                    kvp.Value.WriteEvents(writer, kvp.Key);
                }
            }
        }
        #endregion

        #region Read
        public void ReadState(NetPacketReader reader, int frame, bool discardOutOfOrderPackets)
        {
            Debug.Log($"Receiving packet with remote frame <b>{frame}</b> at local frame <b>{world.Frame}</b>", LogType.Packet);

            // TODO: When should clients overwrite their existing frame with the remote's?
            // Should they always use the latest?
            if (frame > world.Frame && !arbiter && !listenRemote)
                world.Frame = frame;

            int spawnsLength = reader.GetInt();
            int ownershipLength = reader.GetInt();
            int statefulsLength = reader.GetInt();
            int fieldsLength = reader.GetInt();
            int eventsLength = reader.GetInt();

            ReadSpawns(reader, spawnsLength);
            ReadOwnership(reader, ownershipLength);
            ReadStateful(reader, statefulsLength, frame, discardOutOfOrderPackets);            
            ReadFields(reader, fieldsLength, frame, discardOutOfOrderPackets);
            ReadEvents(reader, eventsLength);

            if (!reader.EndOfData)
                Debug.LogError($"ReadState method did not read all bytes from the NetPacketReader; it is intended to always consume all bytes, regardless of whether the data is applied to the StatefulWorld.");
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

        private void ReadStateful(NetPacketReader reader, int length, int frame, bool discardOutOfOrderPackets)
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
                {
                    if (world.statefulObjects[id].GetComponent<IStreamable>().LastReadFrame < frame || !discardOutOfOrderPackets)
                    {
                        world.statefulObjects[id].GetComponent<IStreamable>().Read(reader);
                        world.statefulObjects[id].GetComponent<IStreamable>().LastReadFrame = frame;
                    }
                    else
                    {
                        world.statefulObjects[id].GetComponent<IStreamable>().Pass(reader);
                    }
                }
                else
                    world.statefulObjects[id].GetComponent<IStreamable>().Pass(reader);
            }
        }

        private void ReadFields(NetDataReader reader, int length, int frame, bool discardOutOfOrderPackets)
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
                    world.statefulObjects[id].ReadStateFields(reader, startPosition + length, arbiter, frame, discardOutOfOrderPackets, false);
                else
                    world.statefulObjects[id].ReadStateFields(reader, startPosition + length, arbiter, frame, discardOutOfOrderPackets, true);
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

        private void ReadEvents(NetDataReader reader, int length)
        {
            int startPosition = reader.Position;

            // Clear the first byte of the payload. This will be
            // a StatefulObject.DataType.Object enum.
            if (reader.Position - startPosition < length)
                reader.GetByte();

            while (reader.Position - startPosition < length)
            {
                int id = reader.GetInt();
                world.statefulObjects[id].ReadEvents(reader, arbiter);
            }
        }
        #endregion
    }
}
