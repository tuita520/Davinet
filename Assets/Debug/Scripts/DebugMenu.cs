using Davinet;
using UnityEngine;
using UnityEngine.UI;

namespace Davinet.UnityDebug
{
    public class DebugMenu : MonoBehaviour
    {
        [SerializeField]
        Text frameText;

        [SerializeField]
        Button buttonAssertStatefulObjectDictionary;

        private void Awake()
        {
            buttonAssertStatefulObjectDictionary.onClick.AddListener(() => AssertStatefulObjectDictionary());
        }

        private void FixedUpdate()
        {
            frameText.text = $"Frame: {StatefulWorld.Instance.Frame}";
        }

        private void AssertStatefulObjectDictionary()
        {
            foreach (var kvp in StatefulWorld.Instance.statefulObjects)
            {
                if (kvp.Key == kvp.Value.ID)
                    UnityEngine.Debug.Log("StatefulObject IDs correctly match their keyed ID.");
                else
                    UnityEngine.Debug.LogError($"StatefulObject ID {kvp.Value.ID} is keyed on {kvp.Key}");
            }
        }
    }
}
