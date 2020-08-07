using LiteNetLib.Utils;
using UnityEngine;

namespace Davinet
{
    // TODO: Ownership system (distributed authority scheme) is not currently
	// functioning correctly.
	// 1. Has issues when running a listen client (likely caused by
	// the server and client performing different actions when they receive 
	// ownership data.
	// 2. Authority is sometimes not correctly relinquished or transferred
	// between objects in latency situations.
    /// <summary>
    /// Peers on the network may have authority over an object,
    /// or own the object, which by default also gives them authority.
    /// When a peer has authority, it will send state updates for that object
    /// to its peers and reject any updates received for that object.
    /// When a peer owns an object, that object cannot be claimed by any other peers.
    /// </summary>
    public class OwnableObject : MonoBehaviour
    {
        public event System.Action<OwnableObject, int> OnAuthorityChanged;

        public StateInt Owner { get; set; }
        public StateInt Authority { get; set; }

        private int authorityFrameChanged;
        private int ownershipFrameChanged;

        public bool CanRelinquishAuthority { get; set; }

        private void Awake()
        {
            Authority = new StateInt();
            Owner = new StateInt();

            Authority.OnChanged += Authority_OnChanged;
        }

        private void Authority_OnChanged(int current, int previous)
        {
            OnAuthorityChanged(this, current);
        }

        public void GrantOwnership(int owner)
        {
            Owner.Value = owner;
            Authority.Value = owner;

            ownershipFrameChanged = 1;
            authorityFrameChanged = 1;
        }

        public void RelinquishAuthority()
        {
            if (!CanRelinquishAuthority)
                return;

            if (Owner.Value == 0 && Authority.Value != 0)
                Authority.Value = 0;
        }

        public bool TryTakeAuthority(int authority)
        {
            if (authority != 0 && (Authority.Value == 0 || Authority.Value == authority) && StatefulWorld.Instance.CanTakeAuthority(authority))
            {
                Authority.Value = authority;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryTakeOwnership(int owner)
        {
            if (StatefulWorld.Instance.CanTakeAuthority(owner) && (owner == Owner.Value || Owner.Value == 0))
            {
                Owner.Value = owner;
                TryTakeAuthority(owner);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void RelinquishOwnership()
        {
            if (Owner.Value != 0)
                Owner.Value = 0;
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

            int frame;

            if (!arbiter)
                frame = StatefulWorld.Instance.Frame;
            else
            {
                if (writeOwner)
                    frame = ownershipFrameChanged;
                else
                    frame = authorityFrameChanged;
            }

            writer.Put(frame);

            DataType dataType;

            if (writeOwner && writeAuthority)
            {
                dataType = DataType.OwnershipAndAuthority;
                Debug.Log($"Writing ownership to {Owner.Value} and authority to {Authority.Value}.", id, LogType.Ownership);
            }
            else if (writeOwner)
            {
                dataType = DataType.Ownership;
                Debug.Log($"Writing ownership to {Owner.Value}.", id, LogType.Ownership);
            }
            else
            {
                dataType = DataType.Authority;
                Debug.Log($"Writing authority to {Authority.Value}.", id, LogType.Authority);
            }

            writer.Put((byte)dataType);

            if (writeOwner)
            {                
                writer.Put(Owner.Value);
                Owner.IsDirty = false;
                ownershipFrameChanged = frame;
            }

            if (writeAuthority)
            {
                writer.Put(Authority.Value);
                Authority.IsDirty = false;
                authorityFrameChanged = frame;
            }
        }

        public void Read(NetDataReader reader, bool arbiter)
        {
            int frame = reader.GetInt();
            DataType datatype = (DataType)reader.GetByte();

            bool readOwnership = datatype == DataType.Ownership || datatype == DataType.OwnershipAndAuthority;
            bool readAuthority = datatype == DataType.Authority || datatype == DataType.OwnershipAndAuthority;

            if (readOwnership)
            {             
                int owner = reader.GetInt();

                if (frame >= ownershipFrameChanged)
                {
                    Owner.Value = owner;
                    ownershipFrameChanged = frame;

                    if (!arbiter)
                        Owner.IsDirty = false;
                }
            }

            if (readAuthority)
            {
                int authority = reader.GetInt();

                if (frame >= authorityFrameChanged)
                {                   
                    Authority.Value = authority;
                    authorityFrameChanged = frame;

                    if (!arbiter)
                        Authority.IsDirty = false;
                }
            }
        }
    }
}
