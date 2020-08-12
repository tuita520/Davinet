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

        private AuthorityArbiter authorityArbiter;

        private void Awake()
        {
            registeredPrefabsMap = new Dictionary<int, IdentifiableObject>();

            foreach (IdentifiableObject registeredPrefab in registeredPrefabs)
            {
                registeredPrefabsMap[registeredPrefab.GUID] = registeredPrefab;
            }
        }

        // TODO: Allow authority arbiter to be null, or replace with interface.
        // If it is not present, all objects should be permitted to write state fields.
        public void Initialize(AuthorityArbiter authorityArbiter)
        {
            statefulObjects = new Dictionary<int, StatefulObject>();

            foreach (StatefulObject statefulObject in FindObjectsOfType<StatefulObject>())
            {
                Add(statefulObject, true);
            }

            this.authorityArbiter = authorityArbiter;

            OnInitialize?.Invoke();
        }

        public void Add(StatefulObject o, bool silent = false)
        {
            int id = statefulObjects.Count + 1;

            Add(o, id);
        }

        public void Add(StatefulObject o, int id, bool silent=false)
        {
            o.ID = id;
            statefulObjects[id] = o;

            o.Ownable.OnAuthorityChanged += Ownable_OnAuthorityChanged;

            if (!silent)
                OnAdd?.Invoke(o);
        }

        public bool CanTakeAuthority(int authority)
        {
            return authorityArbiter.CanWrite(authority);
        }

        private void Ownable_OnAuthorityChanged(OwnableObject o, int authority)
        {
            o.GetComponent<StatefulObject>().SetControl(authorityArbiter.CanWrite(authority));
        }

        public StatefulObject GetStatefulObject(int id)
        {
            return statefulObjects[id];
        }
    }
}
