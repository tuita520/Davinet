using System;
using LiteNetLib.Utils;

namespace Davinet
{
    public abstract class StatefulEventBase : IAuthorityControllable, IStreamable
    {
        public bool HasPendingCall { get; set; }
        public bool HasControl { get; set; }

        public int LastReadFrame { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected IStateField[] pendingCall;

        protected void StartInvoke(StatefulObject invoker, params IStateField[] args)
        {
            if (!invoker.HasControl)
                return;

            if (HasControl)
            {
                Execute(args);
            }
            else
            {
                pendingCall = args;
                HasPendingCall = true;
            }
        }

        protected abstract void Execute(params IStateField[] args);

        public abstract void Read(NetDataReader reader);
        public abstract void Pass(NetDataReader reader);

        public void Write(NetDataWriter writer)
        {
            for (int i = 0; i < pendingCall.Length; i++)
            {
                pendingCall[i].Write(writer);
            }
        }
    }

    public class StatefulEvent<T> : StatefulEventBase where T : IStateField, new()
    {
        private readonly Action<T> OnInvoke;

        public StatefulEvent(Action<T> callback) : base()
        {
            OnInvoke = callback;
        }

        public void Invoke(StatefulObject invoker, T arg)
        {
            StartInvoke(invoker, arg);
        }

        protected override void Execute(params IStateField[] args)
        {
            OnInvoke.DynamicInvoke(args);
        }

        public override void Read(NetDataReader reader)
        {
            T arg = new T();
            arg.Read(reader);

            pendingCall = new IStateField[] { arg };

            if (HasControl)
                Execute(pendingCall);
        }

        public override void Pass(NetDataReader reader)
        {
            T arg = new T();
            arg.Read(reader);
        }
    }
}
