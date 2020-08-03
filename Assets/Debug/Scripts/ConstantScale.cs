using UnityEngine;

public class ConstantScale : MonoBehaviour
{
    [SerializeField]
    float defaultScale = 0.1f;

    private void OnEnable()
    {
        Camera.onPreRender += ScaleToCamera;
    }

    private void OnDisable()
    {
        Camera.onPreRender -= ScaleToCamera;
    }

    private void ScaleToCamera(Camera cam)
    {
        float distance = Vector3.Distance(transform.position, cam.transform.position);
        transform.localScale = Vector3.one * defaultScale * distance;
    }
}
