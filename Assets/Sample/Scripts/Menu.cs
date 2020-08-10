using UnityEngine;
using UnityEngine.UI;
using System;

namespace Davinet.Sample
{
    public class Menu : MonoBehaviour
    {
        [SerializeField]
        InputField address;

        [SerializeField]
        InputField port;

        [SerializeField]
        Toggle simulatePacketLoss;

        [SerializeField]
        Toggle simulateLatency;

        [SerializeField]
        InputField latencyMin;

        [SerializeField]
        InputField latencyMax;

        [SerializeField]
        Slider packetLossPercent;

        public void StartServer()
        {
            Network.Instance.StartServer(Convert.ToInt32(port.text), NetworkDebug());

            Destroy(gameObject);
        }

        public void StartClient()
        {
            Network.Instance.ConnectClient(address.text, Convert.ToInt32(port.text), NetworkDebug());

            Destroy(gameObject);
        }

        public void StartHost()
        {
            Network.Instance.StartServer(Convert.ToInt32(port.text), NetworkDebug());
            Network.Instance.ConnectClient("localhost", Convert.ToInt32(port.text));

            Destroy(gameObject);
        }

        private PeerDebug.Settings NetworkDebug()
        {
            var debug = new PeerDebug.Settings()
            {
                simulateLatency = simulateLatency.isOn,
                simulatePacketLoss = simulatePacketLoss.isOn,
                maxLatency = Convert.ToInt32(latencyMax.text),
                minLatency = Convert.ToInt32(latencyMin.text),
                packetLossChance = Mathf.RoundToInt(packetLossPercent.value * 100)
            };

            return debug;
        }
    }
}
