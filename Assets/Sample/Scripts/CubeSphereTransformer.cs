using UnityEngine;

public class CubeSphereTransformer : MonoBehaviour
{
    [SerializeField]
    GameObject cube;

    [SerializeField]
    GameObject sphere;

    [SerializeField]
    Animator animator;

    private StateBool isCube { get; set; }

    private void Awake()
    {
        isCube = new StateBool(true, IsCube_OnChanged);
    }

    private void IsCube_OnChanged(bool current, bool previous)
    {
        cube.SetActive(current);
        sphere.SetActive(!current);

        animator.SetBool("IsCube", current);
    }

    public void Transform()
    {
        isCube.Value = !isCube.Value;
    }
}
