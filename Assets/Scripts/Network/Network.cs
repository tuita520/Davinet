using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Davinet
{
    public class Network : SingletonBehaviour<Network>
    {
        public Remote Server
        {
            get
            {
                if (server == null)
                    throw new System.Exception("Attempting to access server when server not running.");

                return server;
            }
        }

        private Remote server;

        public Remote Client
        {
            get
            {
                if (Client == null)
                    throw new System.Exception("Attempting to access server when server not running.");

                return Client;
            }
        }

        private Remote client;

        private void Awake()
        {
            enabled = false;

            gameObject.AddComponent<BeforeFixedUpdate>().OnBeforeFixedUpdate += OnBeforeFrame;
            gameObject.AddComponent<AfterFixedUpdate>().OnAfterFixedUpdate += OnAfterFrame;

            bytesPerFrame = new Queue<int>();

            framesPerSecond = (int)(1 / Time.fixedDeltaTime);
        }

        // Transport layer network manager.
        private NetManager serverManager;
        private NetDataWriter serverWriter;

        private NetManager clientManager;
        private NetDataWriter clientWriter;

        private bool isServer;
        private bool isClient;

        #region Debug and Diagnostic Tools
        public int BytesPerSecond { get; private set; }

        private Queue<int> bytesPerFrame;
        private int framesPerSecond;

        public class NetworkDebug
        {
            public bool simulateLatency;
            public int maxLatency;
            public int minLatency;

            public bool simulatePacketLoss;
            public int packetLossChance;

            public bool simulateRTT;
            public int RTT;
        }

        private NetworkDebug networkDebug;
        #endregion

        public void StartServer(int port, NetworkDebug debug=null)
        {
            StatefulWorld.Instance.Initialize();

            EventBasedNetListener serverListener = new EventBasedNetListener();

            server = new Remote(StatefulWorld.Instance, true, false, 0);
            serverManager = new NetManager(serverListener);

            serverListener.NetworkReceiveEvent += ServerListener_NetworkReceiveEvent;
            serverListener.ConnectionRequestEvent += ConnectionRequestEvent;
            serverListener.PeerConnectedEvent += ServerListener_PeerConnectedEvent;

            networkDebug = debug;

            if (networkDebug != null)
            {
                // When LiteNetLib's latency simulator is enabled, packets being latent
                // will block the sending of other packets. We use our own to simulate
                // the delay that occurs while sending over distances (non-LAN).
                /*
                serverManager.SimulateLatency = debug.simulateLatency;
                serverManager.SimulationMaxLatency = debug.maxLatency;
                serverManager.SimulationMinLatency = debug.minLatency;
                */

                serverManager.SimulatePacketLoss = debug.simulatePacketLoss;
                serverManager.SimulationPacketLossChance = debug.packetLossChance;
            }

            serverManager.Start(port);
            serverWriter = new NetDataWriter();

            isServer = true;

            enabled = true;
        }

        public void ConnectClient(string address, int port, NetworkDebug debug = null)
        {
            if (!isServer)
                StatefulWorld.Instance.Initialize();

            EventBasedNetListener clientListener = new EventBasedNetListener();
            
            clientManager = new NetManager(clientListener);
            clientListener.NetworkReceiveEvent += ClientListener_NetworkReceiveEvent;
            clientListener.ConnectionRequestEvent += ConnectionRequestEvent;

            networkDebug = debug;

            if (debug != null)
            {
                /*
                clientManager.SimulateLatency = debug.simulateLatency;
                clientManager.SimulationMaxLatency = debug.maxLatency;
                clientManager.SimulationMinLatency = debug.minLatency;
                */

                clientManager.SimulatePacketLoss = debug.simulatePacketLoss;
                clientManager.SimulationPacketLossChance = debug.packetLossChance;
            }

            clientManager.Start();
            clientManager.Connect(address, port, "DaviNet");
            clientWriter = new NetDataWriter();

            isClient = true;

            enabled = true;
        }

        private void ServerListener_PeerConnectedEvent(NetPeer peer)
        {
            int id = server.AddPeer(peer);

            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)PacketType.Join);
            writer.Put(id);
            writer.Put(StatefulWorld.Instance.Frame);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();

            server.SynchronizeAll();

            var player = Instantiate(StatefulWorld.Instance.registeredPrefabsMap[1717083505]);
            StatefulWorld.Instance.Add(player.GetComponent<StatefulObject>());
            StatefulWorld.Instance.SetOwnership(player.GetComponent<OwnableObject>(), id);
        }

        private void ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log("Connection requested.");
            request.Accept();
        }

        private void ServerListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            NetworkReceiveEvent(reader, server);
        }

        private void ClientListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            NetworkReceiveEvent(reader, client);
        }

        private void NetworkReceiveEvent(NetPacketReader reader, Remote remote)
        {
            PacketType packetType = (PacketType)reader.GetByte();

            switch (packetType)
            {
                case PacketType.State:
                    remote.ReadState(reader);
                    break;
                case PacketType.Join:
                    int remoteID = reader.GetInt();
                    int frame = reader.GetInt();

                    if (!isServer)
                        StatefulWorld.Instance.Frame = frame;

                    client = new Remote(StatefulWorld.Instance, false, isServer, remoteID);

                    Debug.Log($"Local client assigned id {remoteID}");
                    break;
                default:
                    break;
            }
        }

        private void OnBeforeFrame()
        {
            if (!enabled)
                return;

            StatefulWorld.Instance.Frame++;

            if (isServer)
                serverManager.PollEvents();

            if (isClient)
                clientManager.PollEvents();
        }

        private void OnAfterFrame()
        {
            if (!enabled)
                return;

            if (isServer)
            {
                server.WriteState(serverWriter);

                #region Bandwidth Statistics
                if (bytesPerFrame.Count > framesPerSecond)
                    bytesPerFrame.Dequeue();

                bytesPerFrame.Enqueue(serverWriter.Length);
                BytesPerSecond = 0;

                foreach (int byteCount in bytesPerFrame)
                {
                    BytesPerSecond += byteCount;
                }
                #endregion

                if (networkDebug != null && networkDebug.simulateLatency)
                {
                    StartCoroutine(SendStateDelayed(serverWriter, serverManager));

                    // TODO: This should probably be pooled when simulating latency.
                    serverWriter = new NetDataWriter();
                }
                else
                {                    
                    serverManager.SendToAll(serverWriter, DeliveryMethod.ReliableUnordered);
                    serverWriter.Reset();
                }
            }

            if (isClient && !isServer)
            {
                client.WriteState(clientWriter);
                clientManager.SendToAll(clientWriter, DeliveryMethod.ReliableUnordered);
                clientWriter.Reset();
            }
        }

        private IEnumerator SendStateDelayed(NetDataWriter writer, NetManager manager)
        {
            int delayMilliseconds = Random.Range(networkDebug.minLatency, networkDebug.maxLatency);

            yield return new WaitForSeconds(delayMilliseconds / (float)1000);

            manager.SendToAll(writer, DeliveryMethod.ReliableUnordered);
        }
    }
}
