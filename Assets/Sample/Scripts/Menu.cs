using UnityEngine;
using UnityEngine.UI;
using System;

public class Menu : MonoBehaviour
{
    [SerializeField]
    InputField address;

    [SerializeField]
    InputField port;

    public void StartServer()
    {
        Davinet.Network.Instance.StartServer(Convert.ToInt32(port.text));

        Destroy(gameObject);
    }

    public void StartClient()
    {
        Davinet.Network.Instance.ConnectClient(address.text, Convert.ToInt32(port.text));

        Destroy(gameObject);
    }
}
