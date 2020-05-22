using UnityEngine;

namespace Davinet
{
    public class OwnableObject : MonoBehaviour
    {
        public int Owner { get; private set; }
        public int Authority { get; private set; }

        public void SetOwner(int owner)
        {
            Owner = owner;
            Authority = owner;
        }

        public void RelinquishOwnership()
        {
            Owner = 0;
        }

        public bool HasOwnership(int owner)
        {
            return owner == Owner;
        }

        public bool HasAuthority(int authority)
        {
            return authority == Authority;
        }
    }
}
