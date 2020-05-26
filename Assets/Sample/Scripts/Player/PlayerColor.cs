using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField]
    Renderer art;

    public StateColor Col { get; private set; }

    private void Awake()
    {
        Col = new StateColor();
        Col.OnChanged += Col_OnChanged;
    }

    private void Col_OnChanged(Color current, Color previous)
    {
        art.material.color = current;
    }
}
