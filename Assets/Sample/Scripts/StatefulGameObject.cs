using Davinet;
using UnityEngine;

public class StatefulGameObject : MonoBehaviour
{
    public StateBool Alive { get; private set; }

    private StatefulObject statefulObject;

    private void Awake()
    {
        Alive = new StateBool(true);
        Alive.OnChanged += Alive_OnChanged;

        statefulObject = GetComponent<StatefulObject>();
    }

    private void Alive_OnChanged(bool current, bool previous)
    {
        if (!current)
        {
            gameObject.SetActive(false);
            StatefulWorld.Instance.Remove(statefulObject);
        }
    }
}
