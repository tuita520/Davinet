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

        public StateInt LocalAuthority;

        public enum AuthorityType { Global, Local }

        private void Awake()
        {
            Authority = new StateInt();
            Owner = new StateInt();

            LocalAuthority = new StateInt();
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

        public void TakeAuthority(int authority)
        {
            if (Owner.Value == 0 || Owner.Value == authority)
            {
                Authority.Value = authority;
                LocalAuthority.Value = authority;
            }
        }

        public bool CanTakeOwnership(int owner)
        {
            return owner == Owner.Value || Owner.Value == 0;
        }

        public bool HasAuthority(int authority)
        {
            AuthorityType type;
            return HasAuthority(authority, out type);
        }

        public bool HasAuthority(int authority, out AuthorityType type)
        {
            if (Authority.Value == authority)
            {
                type = AuthorityType.Global;
                return true;
            }
            else if (LocalAuthority.Value != 0 && LocalAuthority.Value == authority)
            {
                type = AuthorityType.Local;
                return true;
            }

            type = 0;
            return false;
        }

        public bool HasOwnership(int owner)
        {
            return owner == Owner.Value;
        }

        public enum DataType
        {
            Ownership,
            Authority,
            OwnershipAndAuthority
        };

        public void Write(NetDataWriter writer, int id, bool writeEvenIfNotDirty = false)
        {
            if (Owner.IsDirty || Authority.IsDirty)
                writer.Put(id);
            else
                return;

            if (Owner.IsDirty && Authority.IsDirty)
                writer.Put((byte)DataType.OwnershipAndAuthority);
            else if (Owner.IsDirty)
                writer.Put((byte)DataType.Ownership);
            else if (Authority.IsDirty)
                writer.Put((byte)DataType.Authority);

            if (Owner.IsDirty)
            {                
                writer.Put(Owner.Value);
                Owner.IsDirty = false;
            }

            if (Authority.IsDirty)
            {
                writer.Put(Authority.Value);
                Authority.IsDirty = false;
            }
        }

        public void Read(NetDataReader reader, bool arbiter)
        {
            DataType datatype = (DataType)reader.GetByte();

            bool containsOwnership = datatype == DataType.Ownership || datatype == DataType.OwnershipAndAuthority;
            bool containsAuthority = datatype == DataType.Authority || datatype == DataType.OwnershipAndAuthority;

            if (containsOwnership)
            {
                int owner = reader.GetInt();
                Owner.Value = owner;

                if (!arbiter)
                    Owner.IsDirty = false;
            }

            if (containsAuthority)
            {
                int authority = reader.GetInt();

                if (authority != 0 || Authority.Value == 0)
                    Authority.Value = authority;                    

                if (!arbiter)
                {
                    if (authority == 0)
                        LocalAuthority.Value = 0;

                    Authority.IsDirty = false;
                }
            }
        }
    }
}
