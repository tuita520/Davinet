using Davinet;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    [SerializeField]
    StatefulObjectDebugView statefulObjectDebugViewPrefab;

    [SerializeField]
    LogLevel logLevel;

    private void Start()
    {
        if (logLevel != LogLevel.None)
            Davinet.Debug.RegisterLogger(new UnityDavinetLogger(), logLevel);

        StatefulWorld.Instance.OnInitialize += OnInitialize;
        StatefulWorld.Instance.OnAdd += OnAdd;
    }

    private void OnInitialize()
    {
        foreach (var kvp in StatefulWorld.Instance.statefulObjects)
        {
            AddDebugView(kvp.Value);
        }
    }

    private void OnAdd(StatefulObject obj)
    {
        AddDebugView(obj);
    }

    private void AddDebugView(StatefulObject obj)
    {
        Instantiate(statefulObjectDebugViewPrefab).Initialize(obj);
    }
}
