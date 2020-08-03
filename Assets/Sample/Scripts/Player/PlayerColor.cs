using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField]
    Renderer art;

    public StateColor Col { get; private set; }

    private bool initialized;

    private void Awake()
    {
        Col = new StateColor();
        Col.OnChanged += Col_OnChanged;
    }

    public void Initialize(Color color)
    {
        Debug.Assert(initialized == false, "Attempting to initialize player color a second time.");

        initialized = true;

        Col.Value = color;
    }

    private void Col_OnChanged(Color current, Color previous)
    {
        art.material.color = current;
    }
}
