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

        // Transport layer network manager.
        private NetManager serverManager;
        private NetDataWriter serverWriter;

        private NetManager clientManager;
        private NetDataWriter clientWriter;

        public void StartServer(int port)
        {
            server = new Remote(registeredPrefabsMap, true);
            serverManager = new NetManager(server.NetEventListener);
            serverManager.Start(port);
            serverWriter = new NetDataWriter();

            enabled = true;
        }

        public void ConnectClient(string address, int port)
        {
            client = new Remote(registeredPrefabsMap, false);
            clientManager = new NetManager(client.NetEventListener);
            clientManager.Start();
            clientManager.Connect(address, port, "DaviNet");
            clientWriter = new NetDataWriter();

            enabled = true;
        }

        private void FixedUpdate()
        {
            if (IsServer)
                serverManager.PollEvents();

            if (IsClient)
                clientManager.PollEvents();

            if (IsServer)
            {
                server.Write(serverWriter);
                serverManager.SendToAll(serverWriter, DeliveryMethod.ReliableOrdered);
                serverWriter.Reset();
            }

            if (IsClient)
            {
                // client.Write(statefulObjects);
            }
        }
    }
}
