using UnityEngine;

namespace Davinet
{
    public class OwnableObject : MonoBehaviour
    {
        public int Owner { get; private set; }
        public int OwnerFrame { get; private set; }

        private int previousOwner;

        public void SetOwner(int owner, int frame)
        {
            previousOwner = Owner;

            Owner = owner;
            OwnerFrame = frame;
        }

        public bool HasOwnership(int owner, int currentFrame)
        {
            if (currentFrame == OwnerFrame)
            {
                if (previousOwner == owner)
                    return true;
            }
            else if (Owner == owner)
            {
                return true;
            }

            return false;
        }
    }
}
