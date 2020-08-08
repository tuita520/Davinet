using TMPro;
using UnityEngine;

namespace Davinet.UnityDebug
{
    public class StatefulObjectDebugView : MonoBehaviour
    {
        [SerializeField]
        TextMeshPro text;

        private StatefulObject statefulObject;

        public void Initialize(StatefulObject statefulObject)
        {
            this.statefulObject = statefulObject;
        }

        private void LateUpdate()
        {
            transform.position = statefulObject.transform.position;
        }

        private void FixedUpdate()
        {
            string statefulText = $"ID: {statefulObject.ID}\n" +
                $"Owner: { statefulObject.Ownable.Owner.Value}\n" +
                $"Authority: {statefulObject.Ownable.Authority.Value}";

            string output = statefulText;

            if (statefulObject.TryGetComponent(out Davinet.Sample.OwnableRigidbody ownableRigidbody))
            {
                output += $"\nSleep: {ownableRigidbody.IsSleeping}";
            }

            text.text = output;
        }
    }
}
