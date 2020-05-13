using UnityEngine;

public class CubeSphereTransformer : MonoBehaviour
{
    [SerializeField]
    GameObject cube;

    [SerializeField]
    GameObject sphere;

    [SerializeField]
    Animator animator;

    private StateField<bool> isCube;

    private void Awake()
    {
        isCube = new StateField<bool>(true, IsCube_OnChanged);

        IsCube_OnChanged(true);
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
