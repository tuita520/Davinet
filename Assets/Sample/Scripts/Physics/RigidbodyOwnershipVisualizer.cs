using Davinet;
using UnityEngine;

public class RigidbodyOwnershipVisualizer : MonoBehaviour
{
    [SerializeField]
    float globalAuthorityValue = 0.5f;

    [SerializeField]
    float localAuthorityValue = 0.5f;

    private Rigidbody rb;
    private OwnableObject ownable;
    private Renderer thisRenderer;

    private Color defaultColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
        //UpdateColor();
    }

    private void Authority_OnChanged(int arg1, int arg2)
    {
        //UpdateColor();
    }

    private void Owner_OnChanged(int arg1, int arg2)
    {
        //UpdateColor();
    }

    private void FixedUpdate()
    {
        PlayerColor[] playerColors = FindObjectsOfType<PlayerColor>();

        bool isClaimed = false;

        foreach (PlayerColor playerColor in playerColors)
        {
            float h, s, v;
            Color.RGBToHSV(playerColor.Col.Value, out h, out s, out v);
            OwnableObject.AuthorityType type;

            if (ownable.HasOwnership(playerColor.GetComponent<OwnableObject>().Owner.Value))
            {
                thisRenderer.material.color = Color.HSVToRGB(h, s, v);
                isClaimed = true;
                break;
            }   
            else if (ownable.HasAuthority(playerColor.GetComponent<OwnableObject>().Owner.Value, out type))
            {
                if (type == OwnableObject.AuthorityType.Global)
                {
                    thisRenderer.material.color = Color.HSVToRGB(h, s * globalAuthorityValue, v * localAuthorityValue);
                }
                else
                {
                    thisRenderer.material.color = Color.HSVToRGB(h, s * localAuthorityValue, v * localAuthorityValue);
                }

                isClaimed = true;
                break;
            }
        }

        if (!isClaimed)
        {
            thisRenderer.material.color = defaultColor;
        }
    }
}
