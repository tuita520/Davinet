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
            // TODO: Player specific information should be managed elsewhere.
            player.GetComponent<PlayerColor>().Initialize.Invoke(GetComponent<StatefulObject>(), new StateColor(Color.HSVToRGB(Random.Range(0f, 1f), 1, 1)));

            UnityEngine.Debug.Log($"Spawning player for {id}.");
        }
    }
}
