using Davinet;
using UnityEngine;

namespace Davinet.Sample
{
    public class OwnershipVisualizer : MonoBehaviour
    {
        [SerializeField]
        float authorityColorStrength = 0.5f;

        private OwnableObject ownable;
        private Renderer thisRenderer;

        private Color defaultColor;

        private void Awake()
        {
            thisRenderer = GetComponent<Renderer>();

            defaultColor = thisRenderer.material.color;
        }

        private void Start()
        {
            ownable = GetComponent<OwnableObject>();
            ownable.Owner.OnChanged += OnOwnableChanged;
            ownable.Authority.OnChanged += OnOwnableChanged;
        }

        private void OnOwnableChanged(int arg1, int arg2)
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            PlayerColor[] playerColors = FindObjectsOfType<PlayerColor>();

            bool isClaimed = false;

            foreach (PlayerColor playerColor in playerColors)
            {
                if (playerColor.GetComponent<OwnableObject>().Owner.Value == 0)
                    continue;

                if (ownable.HasOwnership(playerColor.GetComponent<OwnableObject>().Owner.Value))
                {
                    thisRenderer.material.color = playerColor.Col.Value;
                    isClaimed = true;
                    break;
                }
                else if (ownable.HasAuthority(playerColor.GetComponent<OwnableObject>().Owner.Value))
                {
                    thisRenderer.material.color = Color.Lerp(defaultColor, playerColor.Col.Value, authorityColorStrength);
                    isClaimed = true;
                    break;
                }
            }

            if (!isClaimed)
                thisRenderer.material.color = defaultColor;
        }
    }
}