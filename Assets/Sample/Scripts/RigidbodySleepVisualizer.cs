using UnityEngine;

public class RigidbodySleepVisualizer : MonoBehaviour
{
    [SerializeField]
    Material awakeMaterial;

    private Rigidbody rb;

    private Material defaultMaterial;
    private Renderer thisRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        thisRenderer = GetComponent<Renderer>();

        defaultMaterial = thisRenderer.sharedMaterial;
    }

    private void FixedUpdate()
    {
        thisRenderer.sharedMaterial = rb.IsSleeping() ? defaultMaterial : awakeMaterial;
    }
}
