using UnityEngine;

public class PlayerInput
{
    public Vector3 moveInput;
    public bool transformDown;
    public bool usePowerDown;
    public int setPowerDown;
    public Ray mouseRay;

    public void Clear()
    {
        moveInput = Vector3.zero;
        transformDown = false;
        usePowerDown = false;
        setPowerDown = -1;
    }
}
