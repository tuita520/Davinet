using System;
using System.Collections.Generic;

namespace Davinet
{
    public abstract class StatefulEventBase
    {
        private List<IStateField[]> pendingCalls;

        public StatefulEventBase()
        {
            pendingCalls = new List<IStateField[]>();
        }

        protected void StartInvoke(params IStateField[] args)
        {
            // pendingCalls.Add(args);

            Execute(args);
        }

        protected abstract void Execute(params IStateField[] args);
    }

    public class StatefulEvent<T> : StatefulEventBase where T : IStateField
    {
        public event Action<T> OnInvoke;

        public StatefulEvent() : base() { }

        public StatefulEvent(Action<T> callback) : this()
        {
            OnInvoke += callback;
        }

        public void Invoke(T arg)
        {
            StartInvoke(arg);
        }

        protected override void Execute(params IStateField[] args)
        {
            OnInvoke.DynamicInvoke(args);
        }
    }
}
