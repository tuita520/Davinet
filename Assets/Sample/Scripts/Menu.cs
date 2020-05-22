using UnityEngine;
using UnityEngine.UI;
using System;

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
    Toggle simulateRTT;

    [SerializeField]
    InputField latencyMin;

    [SerializeField]
    InputField latencyMax;

    [SerializeField]
    InputField rtt;

    [SerializeField]
    Slider packetLossPercent;

    public void StartServer()
    {
        Davinet.Network.Instance.StartServer(Convert.ToInt32(port.text), NetworkDebug());

        Destroy(gameObject);
    }

    public void StartClient()
    {
        Davinet.Network.Instance.ConnectClient(address.text, Convert.ToInt32(port.text), NetworkDebug());

        Destroy(gameObject);
    }

    public void StartHost()
    {
        Davinet.Network.Instance.StartServer(Convert.ToInt32(port.text), NetworkDebug());
        Davinet.Network.Instance.ConnectClient(address.text, Convert.ToInt32(port.text), NetworkDebug());

        Destroy(gameObject);
    }

    private Davinet.Network.NetworkDebug NetworkDebug()
    {
        var debug = new Davinet.Network.NetworkDebug()
        {
            simulateLatency = simulateLatency.isOn,
            simulatePacketLoss = simulatePacketLoss.isOn,
            maxLatency = Convert.ToInt32(latencyMax.text),
            minLatency = Convert.ToInt32(latencyMin.text),
            packetLossChance = Mathf.RoundToInt(packetLossPercent.value * 100),
            simulateRTT = simulateRTT.isOn,
            RTT = Convert.ToInt32(rtt.text)
        };

        return debug;
    }
}
