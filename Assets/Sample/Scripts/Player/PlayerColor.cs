using UnityEngine;

namespace Davinet.Sample
{
    public class PlayerColor : MonoBehaviour
    {
        [SerializeField]
        Renderer art;

        public StatefulEvent<StateColor> Initialize { get; private set; }
        public StateColor Col { get; private set; }

        private void Awake()
        {
            Initialize = new StatefulEvent<StateColor>(InitializeCallback);

            Col = new StateColor();
            Col.OnChanged += Col_OnChanged;
        }

        private void InitializeCallback(StateColor color)
        {
            Col.Value = color.Value;
        }

        private void Col_OnChanged(Color current, Color previous)
        {
            art.material.color = current;
        }
    }
}