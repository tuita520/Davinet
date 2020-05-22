using UnityEngine;

public class PlayerInputController : MonoBehaviour, IInputController
{
    [SerializeField]
    float maxSpeed = 5;

    public PlayerInput CurrentInput { get; private set; }

    private bool hasFixedUpdateRun;
    private bool poll;

    private void Awake()
    {
        CurrentInput = new PlayerInput();

        CurrentInput.Clear();
    }

    private void Update()
    {
        if (hasFixedUpdateRun)
        {
            CurrentInput.Clear();
            hasFixedUpdateRun = false;
        }

        if (!poll)
            return;

        CurrentInput.mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            float d;
            plane.Raycast(CurrentInput.mouseRay, out d);

            if (d > 0)
            {
                Vector3 target = CurrentInput.mouseRay.origin + CurrentInput.mouseRay.direction * d;

                CurrentInput.moveInput = Vector3.ClampMagnitude(target - transform.position, maxSpeed);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
            CurrentInput.transformDown = true;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            CurrentInput.setPowerDown = 0;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            CurrentInput.setPowerDown = 1;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            CurrentInput.setPowerDown = 2;

        if (Input.GetMouseButtonDown(1))
            CurrentInput.usePowerDown = true;

        CurrentInput.IsDirty = true;
    }

    private void FixedUpdate()
    {
        hasFixedUpdateRun = true;
    }

    public void SetEnabled(bool value)
    {
        poll = value;

        if (poll)
        {
            FindObjectOfType<SmoothFollowCamera>().target = transform;
            FindObjectOfType<SmoothFollowCamera>().enabled = true;
        }
    }
}
