using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Davinet
{
    public class Network : SingletonBehaviour<Network>
    {
        [SerializeField]
        IdentifiableObject[] registeredPrefabs;

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

            registeredPrefabsMap = new Dictionary<int, IdentifiableObject>();

            foreach (IdentifiableObject registeredPrefab in registeredPrefabs)
            {
                registeredPrefabsMap[registeredPrefab.GUID] = registeredPrefab;
            }
        }

        private Dictionary<int, IdentifiableObject> registeredPrefabsMap;
        private RemoteObjects remoteObjects;

        // Transport layer network manager.
        private NetManager serverManager;
        private NetDataWriter serverWriter;

        private NetManager clientManager;
        private NetDataWriter clientWriter;

        public class NetworkDebug
        {
            public bool simulateLatency;
            public int maxLatency;
            public int minLatency;

            public bool simulatePacketLoss;
            public int packetLossChance;
        }

        public void StartServer(int port, NetworkDebug debug=null)
        {
            remoteObjects = new RemoteObjects(registeredPrefabsMap);

            server = new Remote(remoteObjects, true, false);
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
            if (remoteObjects == null)
                remoteObjects = new RemoteObjects(registeredPrefabsMap);

            client = new Remote(remoteObjects, false, IsServer);
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

        private void FixedUpdate()
        {
            if (IsServer)
            {
                server.WriteSpawns(serverWriter);
                serverManager.SendToAll(serverWriter, DeliveryMethod.ReliableOrdered);
                serverWriter.Reset();

                server.WriteState(serverWriter);
                serverManager.SendToAll(serverWriter, DeliveryMethod.ReliableOrdered);
                serverWriter.Reset();

                if (server.SetOwnershipWriter.Length > 0)
                {
                    serverManager.SendToAll(server.SetOwnershipWriter, DeliveryMethod.ReliableOrdered);
                    server.SetOwnershipWriter.Reset();
                }
            }

            if (IsClient && !IsServer)
            {
                client.WriteState(clientWriter);
                clientManager.SendToAll(clientWriter, DeliveryMethod.ReliableOrdered);
                clientWriter.Reset();
            }

            if (IsServer)
                serverManager.PollEvents();

            if (IsClient)
                clientManager.PollEvents();
        }
    }
}
