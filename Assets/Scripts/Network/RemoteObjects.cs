using UnityEngine;
using System.Collections.Generic;

namespace Davinet
{
    public class RemoteObjects
    {
        public Dictionary<int, IdentifiableObject> registeredPrefabs;

        // TODO: Make input polymorphic.
        // public Dictionary<int, PlayerInputController> inputControllers;
        public Dictionary<int, StatefulObject> statefulObjects;

        public RemoteObjects(Dictionary<int, IdentifiableObject> registeredPrefabs)
        {
            this.registeredPrefabs = registeredPrefabs;

            statefulObjects = new Dictionary<int, StatefulObject>();
            int i = 0;

            foreach (StatefulObject statefulObject in Object.FindObjectsOfType<StatefulObject>())
            {
                statefulObjects[i] = statefulObject;
                i++;
            }

            // inputControllers = new Dictionary<int, PlayerInputController>();
        }
    }
}
