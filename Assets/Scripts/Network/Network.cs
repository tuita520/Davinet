using LiteNetLib;
using LiteNetLib.Utils;

namespace Davinet
{
    public class Network : SingletonBehaviour<Network>
    {
        public event System.Action OnStartNetwork;

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
        }        

        // Transport layer network manager.
        private NetManager netManager;
        private NetDataWriter writer;

        public void StartServer(int port)
        {
            server = new Remote(true);
            netManager = new NetManager(server.NetEventListener);
            netManager.Start(port);
            writer = new NetDataWriter();
            OnStartNetwork?.Invoke();

            enabled = true;
        }

        public void ConnectClient(string address, int port)
        {
            client = new Remote(false);
            netManager = new NetManager(client.NetEventListener);
            netManager.Start();
            netManager.Connect(address, port, "DaviNet");
            writer = new NetDataWriter();

            enabled = true;
            OnStartNetwork?.Invoke();
        }

        private void FixedUpdate()
        {
            netManager.PollEvents();

            if (IsClient)
            {
                // client.Write(statefulObjects);
            }

            if (IsServer)
            {
                server.Write(writer);

                foreach (NetPeer peer in netManager.ConnectedPeerList)
                {
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }

                writer.Reset();
            }
        }
    }
}
