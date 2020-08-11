using UnityEngine;

namespace Davinet.Sample
{
    public class PlayerColor : MonoBehaviour
    {
        [SerializeField]
        Renderer art;

        public StatefulEvent<StateColor> InitializeEvent;

        public StateColor Col { get; private set; }

        private void Awake()
        {
            InitializeEvent = new StatefulEvent<StateColor>(Initialize);

            Col = new StateColor();
            Col.OnChanged += Col_OnChanged;
        }

        public void Initialize(StateColor color)
        {
            Col.Value = color.Value;
        }

        private void Col_OnChanged(Color current, Color previous)
        {
            art.material.color = current;
        }
    }
}