using LiteNetLib.Utils;
using UnityEngine;

namespace Davinet
{
    /// <summary>
    /// Peers on the network may have authority over an object,
    /// or own the object, which by default also gives them authority.
    /// When a peer has authority, it will send state updates for that object
    /// to its peers and reject any updates received for that object.
    /// When a peer owns an object, that object cannot be claimed by any other peers.
    /// </summary>
    public class OwnableObject : MonoBehaviour
    {
        public StateInt Owner { get; private set; }
        public StateInt Authority { get; private set; }

        public int EffectiveAuthority { get; private set; }

        private void Awake()
        {
            Authority = new StateInt();
            Owner = new StateInt();
        }

        public void SetOwnership(int owner)
        {
            if (Owner.Value == 0)
            {
                Owner.Value = owner;

                TakeAuthority(owner);                
            }
        }

        public void RelinquishOwnership()
        {
            if (Owner.Value != 0)
                Owner.Value = 0;
        }

        public void RelinquishAuthority()
        {
            if (Owner.Value == 0 && Authority.Value != 0)
                Authority.Value = 0;
        }

        public void AcknowledgeRelinquish()
        {
            if (Owner.Value == 0)
            {
                EffectiveAuthority = 0;
            }
        }

        public void TakeAuthority(int authority)
        {
            if (Owner.Value == 0 || Owner.Value == authority)
            {
                Authority.Value = authority;
                EffectiveAuthority = authority;
            }
        }

        public bool CanTakeOwnership(int owner)
        {
            return owner == Owner.Value || Owner.Value == 0;
        }

        public bool HasAuthority(int authority)
        {
            return Authority.Value == authority || EffectiveAuthority == authority;
        }

        public bool HasOwnership(int owner)
        {
            return owner == Owner.Value;
        }

        public enum DataType
        {
            Ownership,
            Authority
        };

        public void Write(NetDataWriter writer, int id, bool writeEvenIfNotDirty = false)
        {
            if (Owner.IsDirty)
            {
                writer.Put(id);
                writer.Put((byte)DataType.Ownership);
                writer.Put(Owner.Value);

                Owner.IsDirty = false;
            }
            else if (Authority.IsDirty)
            {
                writer.Put(id);
                writer.Put((byte)DataType.Authority);
                writer.Put(Authority.Value);

                Authority.IsDirty = false;
            }
        }

        public void Read(NetDataReader reader, bool sentFromArbiter)
        {
            DataType datatype = (DataType)reader.GetByte();

            if (datatype == DataType.Authority)
            {
                int authority = reader.GetInt();
                Authority.Value = authority;

                if (authority == 0 && sentFromArbiter)
                    EffectiveAuthority = 0;

                Authority.IsDirty = false;
            }
            else if (datatype == DataType.Ownership)
            {
                int owner = reader.GetInt();
                SetOwnership(owner);

                Owner.IsDirty = false;
                Authority.IsDirty = false;
            }
        }
    }
}
