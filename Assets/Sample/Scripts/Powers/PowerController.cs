using UnityEngine;

public class PowerController : MonoBehaviour
{
    [SerializeField]
    Behaviour[] powers; 

    private StateInt activePower { get; set; }

    private PlayerInputController playerInputController;

    private void Awake()
    {
        activePower = new StateInt(0, ActivePower_OnChanged);

        playerInputController = GetComponent<PlayerInputController>();
    }

    private void FixedUpdate()
    {
        if (playerInputController.CurrentInput.setPowerDown != -1)
            activePower.Value = playerInputController.CurrentInput.setPowerDown;
    }

    private void ActivePower_OnChanged(int current, int previous)
    {
        for (int i = 0; i < powers.Length; i++)
        {
            powers[i].enabled = i == current;
        }
    }
}
