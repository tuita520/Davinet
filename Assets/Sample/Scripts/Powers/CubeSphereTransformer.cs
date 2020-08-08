using UnityEngine;

namespace Davinet.Sample
{
    public class CubeSphereTransformer : MonoBehaviour
    {
        [SerializeField]
        GameObject cube;

        [SerializeField]
        GameObject sphere;

        [SerializeField]
        Animator animator;

        private StateBool isCube { get; set; }

        private PlayerInputController playerInputController;

        private void Awake()
        {
            isCube = new StateBool(true, IsCube_OnChanged);

            playerInputController = GetComponent<PlayerInputController>();
        }

        private void FixedUpdate()
        {
            if (playerInputController.CurrentInput.transformDown)
                isCube.Value = !isCube.Value;
        }

        private void IsCube_OnChanged(bool current, bool previous)
        {
            cube.SetActive(current);
            sphere.SetActive(!current);

            animator.SetBool("IsCube", current);
        }
    }
}
