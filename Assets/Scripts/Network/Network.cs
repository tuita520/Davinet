using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Davinet
{
    public class Network : SingletonBehaviour<Network>
    {
        public bool IsServer => server != null;
        public bool IsClient => client != null;

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

        public void StartServer(int port, NetworkDebug debug=null)
        {
            StatefulWorld.Instance.Initialize();

            server = new Remote(StatefulWorld.Instance, true, false);
            serverManager = new NetManager(server.NetEventListener);

            if (debug != null)
            {
                serverManager.SimulateLatency = debug.simulateLatency;
                serverManager.SimulationMaxLatency = debug.maxLatency;
                serverManager.SimulationMinLatency = debug.minLatency;

                serverManager.SimulatePacketLoss = debug.simulatePacketLoss;
                serverManager.SimulationPacketLossChance = debug.packetLossChance;
            }

            serverManager.Start(port);
            serverWriter = new NetDataWriter();

            enabled = true;
        }

        public void ConnectClient(string address, int port, NetworkDebug debug = null)
        {
            if (!IsServer)
                StatefulWorld.Instance.Initialize();

            client = new Remote(StatefulWorld.Instance, false, IsServer);
            clientManager = new NetManager(client.NetEventListener);

            if (debug != null)
            {
                clientManager.SimulateLatency = debug.simulateLatency;
                clientManager.SimulationMaxLatency = debug.maxLatency;
                clientManager.SimulationMinLatency = debug.minLatency;

                clientManager.SimulatePacketLoss = debug.simulatePacketLoss;
                clientManager.SimulationPacketLossChance = debug.packetLossChance;
            }

            clientManager.Start();
            clientManager.Connect(address, port, "DaviNet");
            clientWriter = new NetDataWriter();

            enabled = true;
        }

        private void OnBeforeFrame()
        {
            if (!enabled)
                return;

            StatefulWorld.Instance.Frame++;

            if (IsServer)
                serverManager.PollEvents();

            if (IsClient)
                clientManager.PollEvents();
        }

        private void OnAfterFrame()
        {
            if (!enabled)
                return;

            if (IsServer)
            {
                server.WriteState(serverWriter);
                serverManager.SendToAll(serverWriter, DeliveryMethod.ReliableUnordered);

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

                serverWriter.Reset();
            }

            if (IsClient && !IsServer)
            {
                client.WriteState(clientWriter);
                clientManager.SendToAll(clientWriter, DeliveryMethod.ReliableUnordered);
                clientWriter.Reset();
            }
        }
    }
}
