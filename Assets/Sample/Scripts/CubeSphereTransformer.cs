using UnityEngine;

public class CubeSphereTransformer : MonoBehaviour
{
    [SerializeField]
    GameObject cube;

    [SerializeField]
    GameObject sphere;

    [SerializeField]
    Animator animator;

    private StateBool isCube;

    private void Awake()
    {
        isCube = new StateBool(true, IsCube_OnChanged);
    }

    private void IsCube_OnChanged(bool value)
    {
        cube.SetActive(value);
        sphere.SetActive(!value);

        animator.SetBool("IsCube", value);
    }

    public void Transform()
    {
        isCube.Value = !isCube.Value;
    }
}
