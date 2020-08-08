using UnityEngine;

namespace Davinet.Sample
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        IdentifiableObject playerPrefab;

        private void Start()
        {
            Network.Instance.OnPlayerJoin += Instance_OnPlayerJoin;
        }

        private void Instance_OnPlayerJoin(int id)
        {
            var player = Instantiate(playerPrefab);
            StatefulWorld.Instance.Add(player.GetComponent<StatefulObject>());
            player.GetComponent<OwnableObject>().GrantOwnership(id);

            UnityEngine.Debug.Log($"Spawning warrior for {id}.");
        }
    }
}
