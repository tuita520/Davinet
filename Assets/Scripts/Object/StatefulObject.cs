using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using LiteNetLib.Utils;

namespace Davinet
{
    public class StatefulObject : MonoBehaviour
    {
        public enum DataType
        {
            Object,
            Behaviour,
            Field
        };

        // TODO: Make this a custom data type for ease of use.
        private List<KeyValuePair<MonoBehaviour, List<PropertyInfo>>> stateFieldsByMonoBehaviour;

        private void Awake()
        {
            stateFieldsByMonoBehaviour = new List<KeyValuePair<MonoBehaviour, List<PropertyInfo>>>();
        }

        private void Start()
        {
            MonoBehaviour[] monoBehaviours = GetComponentsInChildren<MonoBehaviour>();

            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                List<PropertyInfo> propertyInfos = null;

                foreach (PropertyInfo propertyInfo in
                    monoBehaviour.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (propertyInfo is IStateField)
                    {
                        if (propertyInfos == null)
                            propertyInfos = new List<PropertyInfo>();

                        propertyInfos.Add(propertyInfo);
                    }
                }

                if (propertyInfos != null)
                    stateFieldsByMonoBehaviour.Add(new KeyValuePair<MonoBehaviour, List<PropertyInfo>>(monoBehaviour, propertyInfos));
            }
        }

        public void WriteStateFields(NetDataWriter writer, int id)
        {
            bool objectHeaderWritten, behaviourHeaderWritten;

            objectHeaderWritten = false;

            for (int i = 0; i < stateFieldsByMonoBehaviour.Count; i++)
            {
                behaviourHeaderWritten = false;

                var kvp = stateFieldsByMonoBehaviour[i];

                for (int j = 0; j < kvp.Value.Count; j++)
                {
                    PropertyInfo info = kvp.Value[j];

                    IStateField field = (IStateField)info.GetValue(kvp.Key);

                    if (field.IsDirty)
                    {
                        // If this is the first field written for this object,
                        // we need to write in this object's ID.
                        if (!objectHeaderWritten)
                        {
                            writer.Put((byte)DataType.Object);
                            writer.Put(id);
                            objectHeaderWritten = true;
                        }

                        // Same as above, but per-behaviour.
                        if (!behaviourHeaderWritten)
                        {
                            writer.Put((byte)DataType.Behaviour);
                            writer.Put(i);
                            behaviourHeaderWritten = true;
                        }

                        writer.Put((byte)DataType.Field);
                        writer.Put(j);
                        field.Write(writer);

                        field.IsDirty = false;
                    }
                }
            }
        }

        public void ReadStateFields(NetDataReader reader)
        {
            KeyValuePair<MonoBehaviour, List<PropertyInfo>> selectedBehaviour = default;

            while (true)
            {
                DataType datatype = (DataType)reader.GetByte();

                // We have reader all of the fields for every behaviour
                // on this object.
                if (datatype == DataType.Object)
                {
                    return;
                } 
                else if (datatype == DataType.Behaviour)
                {
                    int behaviourIndex = reader.GetInt();
                    selectedBehaviour = stateFieldsByMonoBehaviour[behaviourIndex];
                }
                else if (datatype == DataType.Field)
                {
                    int fieldIndex = reader.GetInt();
                    IStateField field = (IStateField)selectedBehaviour.Value[fieldIndex].GetValue(selectedBehaviour.Key);
                    field.Read(reader);
                }
            }
        }
    }
}
