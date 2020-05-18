using UnityEngine;
using System.Collections.Generic;

namespace Davinet
{
    public class StatefulWorld : SingletonBehaviour<StatefulWorld>
    {
        [SerializeField]
        IdentifiableObject[] registeredPrefabs;

        public event System.Action<StatefulObject> OnAdd;
        public event System.Action<StatefulObject> OnRemove;
        public event System.Action<OwnableObject> OnSetOwnership;

        public Dictionary<int, IdentifiableObject> registeredPrefabsMap;
        public Dictionary<int, StatefulObject> statefulObjects;

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
        }

        public void Add(StatefulObject o)
        {
            int id = statefulObjects.Count + 1;

            o.ID = id;
            statefulObjects[id] = o;

            OnAdd?.Invoke(o);
        }

        public void Remove(StatefulObject o)
        {
            statefulObjects.Remove(o.ID);
            OnRemove?.Invoke(o);
        }

        public StatefulObject GetStatefulObject(int id)
        {
            return statefulObjects[id];
        }

        public void SetOwnership(OwnableObject o, int owner)
        {
            o.Owner = owner;
            OnSetOwnership(o);
        }

        public void RelinquishOwnership(OwnableObject o)
        {
            o.Owner = 0;
            OnSetOwnership(o);
        }
    }
}
