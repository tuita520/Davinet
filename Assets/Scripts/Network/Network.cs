using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
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

            server = new Remote(StatefulWorld.Instance, true, false);
            serverManager = new NetManager(server.NetEventListener);

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

            enabled = true;
        }

        public void ConnectClient(string address, int port, NetworkDebug debug = null)
        {
            if (!IsServer)
                StatefulWorld.Instance.Initialize();

            client = new Remote(StatefulWorld.Instance, false, IsServer);
            clientManager = new NetManager(client.NetEventListener);

            networkDebug = debug;

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

            if (IsClient && !IsServer)
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
