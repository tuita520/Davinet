﻿using Davinet;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    [SerializeField]
    StatefulObjectDebugView statefulObjectDebugViewPrefab;

    [SerializeField]
    bool generateStateObjectDebugViews;

    [SerializeField]
    Davinet.LogType logLevel;

    private void Start()
    {
        if (logLevel != Davinet.LogType.None)
            Davinet.Debug.RegisterLogger(new UnityDavinetLogger(), logLevel);

        if (generateStateObjectDebugViews)
        {
            StatefulWorld.Instance.OnInitialize += OnInitialize;
            StatefulWorld.Instance.OnAdd += OnAdd;
        }
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
