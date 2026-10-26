using UnityEngine;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private Camera _minimapCamera;
    [SerializeField] private float _zoomStep = 2f;
    [SerializeField] private float _minSize = 2f;
    [SerializeField] private float _maxSize = 12f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ZoomIn();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ZoomOut();
        }
    }

    public void ZoomIn()
    {
        float size = _minimapCamera.orthographicSize - _zoomStep;
        _minimapCamera.orthographicSize = Mathf.Max(size, _minSize);
    }

    public void ZoomOut()
    {
        float size = _minimapCamera.orthographicSize + _zoomStep;
        _minimapCamera.orthographicSize = Mathf.Min(size, _maxSize);
    }
}