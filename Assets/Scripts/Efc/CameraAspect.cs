using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAspect : MonoBehaviour
{
    private Camera cam = null;
    private Vector2 lastResolution;

    private Vector2 TargetAspectRatio { get; set; }
    private Vector2 RectCenter { get; set; }

    void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        Init();
    }

    public void LateUpdate()
    {
        Vector2 currentScreenResolution = new Vector2(Screen.width, Screen.height);

        if (lastResolution != currentScreenResolution)
        {
            CalculateCameraRect(currentScreenResolution);
        }

        lastResolution = currentScreenResolution;
    }

    private void Init()
    {
        cam ??= GetComponent<Camera>();

        TargetAspectRatio = new Vector2(9, 16);
        RectCenter = new Vector2(0.5f, 0.5f);
    }

    private void CalculateCameraRect(Vector2 currentScreenResolution)
    {
        Vector2 normalizedAspectRatio = TargetAspectRatio / currentScreenResolution;
        Vector2 size = normalizedAspectRatio / Mathf.Max(normalizedAspectRatio.x, normalizedAspectRatio.y);
        cam.rect = new Rect(default, size) { center = RectCenter };
    }
}
