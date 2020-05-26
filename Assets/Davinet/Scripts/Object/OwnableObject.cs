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
        public StateInt Owner { get; set; }
        public StateInt Authority { get; set; }

        public bool CanRelinquishAuthority { get; set; }

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
            if (!CanRelinquishAuthority)
                return;

            if (Owner.Value == 0 && Authority.Value != 0)
                Authority.Value = 0;
        }

        public void TakeAuthority(int authority)
        {
            if (authority != 0 && (Owner.Value == 0 || Owner.Value == authority))
            {
                Authority.Value = authority;
            }
        }

        public bool CanTakeOwnership(int owner)
        {
            return owner == Owner.Value || Owner.Value == 0;
        }

        public bool HasAuthority(int authority)
        {
            return Authority.Value == authority;
        }

        public bool HasOwnership(int owner)
        {
            return Owner.Value == owner;
        }

        public enum DataType
        {
            Ownership,
            Authority,
            OwnershipAndAuthority
        };

        public void Write(NetDataWriter writer, int id, bool arbiter, bool writeEvenIfNotDirty = false)
        {
            bool writeOwner = false;

            if (Owner.IsDirty || writeEvenIfNotDirty)
                writeOwner = true;

            bool writeAuthority = false;

            // Only the arbiter needs to write relinquishes of authority (i.e., authority moving
            // from a client to the server).
            if (Authority.IsDirty && (Authority.Value != 0 || arbiter) || writeEvenIfNotDirty)
                writeAuthority = true;

            if (writeOwner || writeAuthority)
                writer.Put(id);
            else
                return;

            if (writeOwner && writeAuthority)
                writer.Put((byte)DataType.OwnershipAndAuthority);
            else if (writeOwner)
                writer.Put((byte)DataType.Ownership);
            else if (writeAuthority)
                writer.Put((byte)DataType.Authority);

            if (writeOwner)
            {                
                writer.Put(Owner.Value);
                Owner.IsDirty = false;
            }

            if (writeAuthority)
            {
                writer.Put(Authority.Value);
                Authority.IsDirty = false;
            }
        }

        public void Read(NetDataReader reader, bool arbiter)
        {
            DataType datatype = (DataType)reader.GetByte();

            bool readOwnership = datatype == DataType.Ownership || datatype == DataType.OwnershipAndAuthority;
            bool readAuthority = datatype == DataType.Authority || datatype == DataType.OwnershipAndAuthority;

            if (readOwnership)
            {
                int owner = reader.GetInt();
                Owner.Value = owner;

                if (!arbiter)
                    Owner.IsDirty = false;
            }

            if (readAuthority)
            {
                int authority = reader.GetInt();                
                Authority.Value = authority;                    

                if (!arbiter)
                    Authority.IsDirty = false;
            }
        }
    }
}
