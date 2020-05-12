using UnityEngine;

namespace Davinet
{
    public class OwnableObject : MonoBehaviour
    {
        public int Owner { get; private set; }

        public void SetOwner(int owner, bool locallyOwned)
        {
            Owner = owner;

            IInputController inputController = GetComponent<IInputController>();

            if (inputController != null)
                inputController.SetEnabled(locallyOwned);
        }
    }
}
