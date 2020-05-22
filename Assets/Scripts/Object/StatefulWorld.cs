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

        public event System.Action<StatefulObject> OnAdd;
        public event System.Action<StatefulObject> OnRemove;
        public event System.Action<OwnableObject> OnSetOwnership;

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

        public void SetOwnership(OwnableObject o, int owner, bool silent=false)
        {
            o.SetOwner(owner);

            if (!silent)
                OnSetOwnership(o);
        }

        public void RelinquishOwnership(OwnableObject o, bool silent=false)
        {
            o.RelinquishOwnership();

            if (!silent)
                OnSetOwnership(o);
        }

        public void RelinquishAuthority(OwnableObject o)
        {

        }
    }
}
