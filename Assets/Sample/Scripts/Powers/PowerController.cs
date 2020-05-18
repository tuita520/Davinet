using UnityEngine;

public class PowerController : MonoBehaviour
{
    [SerializeField]
    Behaviour[] powers; 

    private StateInt activePower { get; set; }

    private void Awake()
    {
        activePower = new StateInt(0, ActivePower_OnChanged);
    }

    private void ActivePower_OnChanged(int current, int previous)
    {
        for (int i = 0; i < powers.Length; i++)
        {
            powers[i].enabled = i == current;
        }
    }

    public void SetPowerActive(int powerIndex)
    {
        activePower.Value = powerIndex;
    }
}
