using UnityEngine;
using System.Collections.Generic;

namespace Davinet
{
    // Can't spell Singleton without sin.
    /// <summary>
    /// All objects that are part of the game logic should be a part
    /// of the stateful world.
    /// </summary>
    public class StatefulWorld : SingletonBehaviour<StatefulWorld>
    {
        [SerializeField]
        IdentifiableObject[] registeredPrefabs;

        public event System.Action OnInitialize;
        public event System.Action<StatefulObject> OnAdd;

        public Dictionary<int, IdentifiableObject> registeredPrefabsMap;
        public Dictionary<int, StatefulObject> statefulObjects;

        public int Frame { get; set; }

        private void Awake()
        {
            registeredPrefabsMap = new Dictionary<int, IdentifiableObject>();

            foreach (IdentifiableObject registeredPrefab in registeredPrefabs)
            {
                registeredPrefabsMap[registeredPrefab.GUID] = registeredPrefab;
            }
        }

        public void Initialize()
        {
            statefulObjects = new Dictionary<int, StatefulObject>();

            foreach (StatefulObject statefulObject in FindObjectsOfType<StatefulObject>())
            {
                int id = statefulObjects.Count + 1;

                statefulObject.ID = id;
                statefulObjects[id] = statefulObject;
            }

            OnInitialize?.Invoke();
        }

        public void Add(StatefulObject o)
        {
            int id = statefulObjects.Count + 1;

            Add(o, id);
        }

        public void Add(StatefulObject o, int id)
        {
            o.ID = id;
            statefulObjects[id] = o;

            OnAdd?.Invoke(o);
        }

        public StatefulObject GetStatefulObject(int id)
        {
            return statefulObjects[id];
        }
    }
}
