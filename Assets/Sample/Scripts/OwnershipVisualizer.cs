using Davinet;
using UnityEngine;

public class OwnershipVisualizer : MonoBehaviour
{
    [SerializeField]
    float globalAuthorityColorStrength = 0.5f;

    [SerializeField]
    float localAuthorityColorStrength = 0.5f;

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
        ownable.Owner.OnChanged += Owner_OnChanged;
        ownable.Authority.OnChanged += Authority_OnChanged;
        ownable.LocalAuthority.OnChanged += LocalAuthority_OnChanged;
    }

    private void LocalAuthority_OnChanged(int arg1, int arg2)
    {
        UpdateColor();
    }

    private void Authority_OnChanged(int arg1, int arg2)
    {
        UpdateColor();
    }

    private void Owner_OnChanged(int arg1, int arg2)
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        PlayerColor[] playerColors = FindObjectsOfType<PlayerColor>();

        bool isClaimed = false;

        foreach (PlayerColor playerColor in playerColors)
        {
            OwnableObject.AuthorityType type;

            if (playerColor.GetComponent<OwnableObject>().Owner.Value == 0)
                continue;

            if (ownable.HasOwnership(playerColor.GetComponent<OwnableObject>().Owner.Value))
            {
                thisRenderer.material.color = playerColor.Col.Value;
                isClaimed = true;
                break;
            }   
            else if (ownable.HasAuthority(playerColor.GetComponent<OwnableObject>().Owner.Value, out type))
            {
                if (type == OwnableObject.AuthorityType.Global)
                    thisRenderer.material.color = Color.Lerp(defaultColor, playerColor.Col.Value, globalAuthorityColorStrength);
                else
                    thisRenderer.material.color = Color.Lerp(defaultColor, playerColor.Col.Value, localAuthorityColorStrength);

                isClaimed = true;
                break;
            }
        }

        if (!isClaimed)
            thisRenderer.material.color = defaultColor;
    }
}
