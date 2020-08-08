using UnityEngine;

namespace Davinet.UnityDebug
{
    public class Billboard : MonoBehaviour
    {
        private void OnEnable()
        {
            Camera.onPreRender += BillboardToCamera;
        }

        private void OnDisable()
        {
            Camera.onPreRender -= BillboardToCamera;
        }

        private void BillboardToCamera(Camera cam)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.forward);
        }
    }
}