using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace Davinet
{
    /// <summary>
    /// Represents either a client or server in the network. Sends and receives messages
    /// with other connected peers. Sends state updates and applies received state updates 
    /// for its <see cref="Remote"/>.
    /// </summary>
    public class Peer
    {
        [Serializable]
        public class Settings
        {
            public bool DiscardOutOfOrderPackets;
            public bool UseJitterBuffer;
            public int JitterBufferDelayFrames;
        }

        public class StatePacket
        {
            public int PeerID;
            public NetPacketReader PacketReader;
        }

        public event Action<int> OnReceivePeerId;
        public event Action<int> OnPeerConnected;

        private Remote remote;
        private NetManager netManager;
        private NetDataWriter netDataWriter;
        private EventBasedNetListener listener;

        private int frame;

        /// <summary>
        /// If this peer receives any state updates before it has correctly
        /// initialized its remote, store them in a queue to apply them after
        /// initialization.
        /// </summary>
        private Queue<StatePacket> queuedStatePackets;
        private Dictionary<int, JitterBuffer> jitterBuffersByPeerId;

        private enum Role { Inactive, Server, Client, ListenClient }
        private Role role;

        private readonly Settings settings;
        private readonly PeerDebug debug;

        public bool HasListenClient { get; set; }

        public Peer()
        {
            listener = new EventBasedNetListener();
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;

            netManager = new NetManager(listener);
            netManager.AutoRecycle = false;

            netDataWriter = new NetDataWriter();
            queuedStatePackets = new Queue<StatePacket>();

            jitterBuffersByPeerId = new Dictionary<int, JitterBuffer>();

            settings = new Settings();
        }

        public Peer(Settings settings) : this()
        {
            this.settings = settings;
        }

        public Peer(Settings settings, PeerDebug debug) : this(settings)
        {
            this.debug = debug;

            if (debug != null)
            {
                netManager.SimulatePacketLoss = debug.settings.simulatePacketLoss;
                netManager.SimulationPacketLossChance = debug.settings.packetLossChance;
            }
        }

        // TODO: Would be nice if server specific logic lived somewhere else.
        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            int id = peer.Id + 1;
            jitterBuffersByPeerId[id] = new JitterBuffer(settings.JitterBufferDelayFrames);

            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)PacketType.Join);
            writer.Put(id);
            writer.Put(StatefulWorld.Instance.Frame);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();

            remote.SynchronizeAll();

            Debug.Log($"Peer index <b>{peer.Id}</b> connected. Assigned global ID {id}.", LogType.Connection);

            OnPeerConnected?.Invoke(id);
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {            
            request.Accept();

            Debug.Log($"Connection requested", LogType.Connection);
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            PacketType packetType = (PacketType)reader.GetByte();

            if (packetType == PacketType.State)
            {
                queuedStatePackets.Enqueue(new StatePacket() { PacketReader = reader, PeerID = peer.Id + 1 });
            }
            // TODO: This is also server only logic.
            else if (packetType == PacketType.Join)
            {
                jitterBuffersByPeerId[peer.Id + 1] = new JitterBuffer(settings.JitterBufferDelayFrames);

                int remoteID = reader.GetInt();
                OnReceivePeerId?.Invoke(remoteID);

                int frame = reader.GetInt();

                StatefulWorld.Instance.Frame = frame;
                remote = new Remote(StatefulWorld.Instance, false, role == Role.ListenClient, remoteID);

                Debug.Log($"Local client assigned id <b>{remoteID}</b>", LogType.Connection);
            }
        }

        private void ProcessStatePacket(StatePacket packet)
        {
            if (debug != null && debug.settings.simulateLatency)
            {
                debug.InsertDelayedReader(UnityEngine.Random.Range(debug.settings.minLatency, debug.settings.maxLatency) / (float)1000, packet);
            }
            else
            {
                ReadStatePacket(packet);
            }
        }

        private void ReadStatePacket(StatePacket packet)
        {
            if (settings.UseJitterBuffer)
            {
                jitterBuffersByPeerId[packet.PeerID].Insert(packet.PacketReader);
            }
            else
            {
                int frame = packet.PacketReader.GetInt();
                remote.ReadState(packet.PacketReader, frame, settings.DiscardOutOfOrderPackets);
            }
        }

        public void Listen(int port)
        {
            if (role != Role.Inactive)
                throw new Exception($"Cannot start listening with peer already performing the {role} role.");

            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            netManager.Start(port);

            role = Role.Server;

            remote = new Remote(StatefulWorld.Instance, true, false, 0);

            StatefulWorld.Instance.Frame = 0;

            OnReceivePeerId?.Invoke(0);
        }

        public void Connect(string address, int port, bool listenClient=false)
        {
            if (role != Role.Inactive)
                throw new Exception($"Cannot connect with peer already performing the {role} role.");

            netManager.Start();
            netManager.Connect(address, port, "Davinet");

            role = listenClient ? Role.ListenClient : Role.Client;
        }

        public void PollEvents()
        {
            frame++;

            netManager.PollEvents();

            while (remote != null && queuedStatePackets.Count > 0)
            {
                StatePacket packet = queuedStatePackets.Dequeue();
                ProcessStatePacket(packet);
            }

            if (role != Role.ListenClient && debug != null && debug.settings.simulateLatency)
            {
                foreach (StatePacket packet in debug.GetAllReadyPackets())
                {
                    ReadStatePacket(packet);
                }
            }

            if (settings.UseJitterBuffer)
            {
                foreach (JitterBuffer jitterBuffer in jitterBuffersByPeerId.Values)
                {
                    foreach (var packet in jitterBuffer.StepBuffer())
                    {
                        remote.ReadState(packet.reader, packet.remoteFrame, settings.DiscardOutOfOrderPackets);
                    }
                }
            }
        }

        public void SendState()
        {
            if (remote != null && role != Role.ListenClient)
            {
                remote.WriteState(netDataWriter);

                if (debug != null && debug.settings.simulateLatency)
                {
                    debug.SendStateDelayed(netDataWriter, netManager, HasListenClient);
                    // TODO: This should probably be pooled when simulating latency.
                    netDataWriter = new NetDataWriter();
                }
                else
                {                    
                    netManager.SendToAll(netDataWriter, DeliveryMethod.ReliableUnordered);
                    netDataWriter.Reset();
                }
            }
        }
    }
}