using System;
using UnityEngine;

namespace Davinet
{
    public class Network : SingletonBehaviour<Network>
    {
        [SerializeField]
        Peer.Settings defaultPeerSettings;

        public event Action<int> OnPlayerJoin;

        private void Awake()
        {
            authorityArbiter = new AuthorityArbiter();

            gameObject.AddComponent<BeforeFixedUpdate>().OnBeforeFixedUpdate += OnBeforeFrame;
            gameObject.AddComponent<AfterFixedUpdate>().OnAfterFixedUpdate += OnAfterFrame;
        }

        private Peer server;
        private Peer client;

        private AuthorityArbiter authorityArbiter;

        public void StartServer(int port, PeerDebug.Settings debugSettings = null)
        {          
            StatefulWorld.Instance.Initialize(authorityArbiter);

            PeerDebug debug = null;

            if (debugSettings != null)
            {
                debug = gameObject.AddComponent<PeerDebug>();
                debug.Initialize(debugSettings);
            }

            server = new Peer(defaultPeerSettings, debug);
            server.OnReceivePeerId += OnReceivePeerId;
            server.Listen(port);

            server.OnPeerConnected += Server_OnPeerConnected;
        }

        public void ConnectClient(string address, int port, PeerDebug.Settings debugSettings = null)
        {
            if (server == null)
                StatefulWorld.Instance.Initialize(authorityArbiter);

            PeerDebug debug = null;

            if (debugSettings != null && server == null)
            {
                debug = gameObject.AddComponent<PeerDebug>();
                debug.Initialize(debugSettings);
            }

            client = new Peer(defaultPeerSettings, debug);
            client.OnReceivePeerId += OnReceivePeerId;
            client.Connect(address, port, server != null);

            if (server != null)
                server.HasListenClient = true;
        }

        private void Server_OnPeerConnected(int id)
        {
            OnPlayerJoin?.Invoke(id);
        }

        private void OnReceivePeerId(int peerId)
        {
            authorityArbiter.AddLocalAuthority(peerId);
        }

        private void OnBeforeFrame()
        {
            StatefulWorld.Instance.Frame++;

            if (server != null)
                server.PollEvents();

            if (client != null)
                client.PollEvents();
        }

        private void OnAfterFrame()
        {
            if (server != null)
                server.SendState();

            if (client != null)
                client.SendState();
        }
    }
}
