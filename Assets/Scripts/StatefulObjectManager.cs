using UnityEngine;
using System.Collections.Generic;

public class StatefulObjectManager
{
    private Dictionary<int, IdentifiableObject> registeredPrefabs;

    // TODO: Make input polymorphic.
    private Dictionary<int, PlayerInputController> inputControllers;
    private Dictionary<int, StatefulRigidbody> statefulObjects;

    public StatefulObjectManager(Dictionary<int, IdentifiableObject> registeredPrefabs)
    {
        this.registeredPrefabs = registeredPrefabs;

        statefulObjects = new Dictionary<int, StatefulRigidbody>();
        int i = 0;

        foreach (StatefulRigidbody statefulObject in Object.FindObjectsOfType<StatefulRigidbody>())
        {
            statefulObjects[i] = statefulObject;
            i++;
        }

        inputControllers = new Dictionary<int, PlayerInputController>();
        i = 0;

        foreach (PlayerInputController inputController in Object.FindObjectsOfType<PlayerInputController>())
        {
            inputControllers[i] = inputController;
            i++;
        }
    }
}
