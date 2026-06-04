using UnityEngine;

public class OrthoAspectRatioFitter : MonoBehaviour
{
    [SerializeField] private float _defaultAspectRatio = 0.5625f;
    [SerializeField] private float _defaultOrthoSize = 5f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        ApplyOrthoSize();
    }

    private void ApplyOrthoSize()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        _cam.orthographicSize = _defaultOrthoSize * (_defaultAspectRatio / currentAspectRatio);
    }
}
