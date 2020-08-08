using System.Collections;
using UnityEngine;

namespace Davinet.Sample
{
    public class PlayerInputController : MonoBehaviour, IInputController
    {
        [SerializeField]
        float maxSpeed = 5;

        public PlayerInput CurrentInput { get; private set; }

        private bool poll;

        private void Awake()
        {
            CurrentInput = new PlayerInput();

            CurrentInput.Clear();
        }

        private void OnEnable()
        {
            StartCoroutine(ClearInput());
        }

        private void Update()
        {
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

            if (Input.GetMouseButtonDown(1))
                CurrentInput.usePowerDown = true;

            CurrentInput.IsDirty = true;
        }

        private IEnumerator ClearInput()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();

                CurrentInput.Clear();
            }
        }

        public void SetEnabled(bool value)
        {
            if (value == poll)
                return;

            if (value)
            {
                FindObjectOfType<SmoothFollowCamera>().target = transform;
                FindObjectOfType<SmoothFollowCamera>().enabled = true;

                // TODO: Player specific information should be managed elsewhere.
                GetComponent<PlayerColor>().Initialize(Color.HSVToRGB(Random.Range(0f, 1f), 1, 1));
            }

            poll = value;
        }
    }
}
