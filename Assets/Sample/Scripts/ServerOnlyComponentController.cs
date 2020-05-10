using UnityEngine;

public class ServerOnlyComponentController : MonoBehaviour
{
    [SerializeField]
    Component[] serverOnlyComponents;

    private void Awake()
    {
        Davinet.Network.Instance.OnStartNetwork += Instance_OnStartNetwork;
    }

    private void Instance_OnStartNetwork()
    {
        if (Davinet.Network.Instance.IsClient)
        {
            foreach(Component component in serverOnlyComponents)
            {
                Destroy(component);
            }
        }
    }
}
