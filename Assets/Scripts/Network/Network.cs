using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using UnityEngine;

namespace Davinet
{
    public class Network : SingletonBehaviour<Network>
    {
        [SerializeField]
        bool useJitterBuffer;

        [SerializeField]
        int jitterBufferDelayFrames;

        private void Awake()
        {
            gameObject.AddComponent<BeforeFixedUpdate>().OnBeforeFixedUpdate += OnBeforeFrame;
            gameObject.AddComponent<AfterFixedUpdate>().OnAfterFixedUpdate += OnAfterFrame;

            // bytesPerFrame = new Queue<int>();

            // framesPerSecond = (int)(1 / Time.fixedDeltaTime);
        }

        #region Debug and Diagnostic Tools
        // public int BytesPerSecond { get; private set; }

        // private Queue<int> bytesPerFrame;
        // private int framesPerSecond;

        private PeerDebug networkDebug;
        #endregion

        private Peer server;
        private Peer client;

        public void StartServer(int port, PeerDebug.Settings debugSettings = null)
        {
            StatefulWorld.Instance.Initialize();

            PeerDebug debug = null;

            if (debugSettings != null)
            {
                debug = gameObject.AddComponent<PeerDebug>();
                debug.Initialize(debugSettings);
            }

            server = new Peer(debug);
            server.Listen(port);
        }

        public void ConnectClient(string address, int port, PeerDebug.Settings debugSettings = null)
        {
            if (server == null)
                StatefulWorld.Instance.Initialize();

            PeerDebug debug = null;

            if (debugSettings != null && server == null)
            {
                debug = gameObject.AddComponent<PeerDebug>();
                debug.Initialize(debugSettings);
            }

            client = new Peer(debug);
            client.Connect(address, port, server != null);
        }

        private void OnBeforeFrame()
        {
            StatefulWorld.Instance.Frame++;

            if (server != null)
                server.PollEvents();

            if (client != null)
                client.PollEvents();

            /*
            if (isClient)
            {
                clientManager.PollEvents();

                if (useJitterBuffer)
                {
                    JitterBuffer.StatePacket packet;
                    if (clientBuffer.TryGetPacket(out packet, (int)(Time.fixedTime / Time.fixedDeltaTime)))
                    {
                        client.ReadState(packet.reader);
                    }
                }
            }
            */
        }

        private void OnAfterFrame()
        {
            if (server != null)
                server.SendState();

            if (client != null)
                client.SendState();














            /*
                
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
            */
        }
    }
}
