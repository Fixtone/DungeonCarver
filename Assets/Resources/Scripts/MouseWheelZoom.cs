using UnityEngine;
using System.Collections;

public class MouseWheelZoom : MonoBehaviour
{

    public float camZoom = 30f;
    public float camZoomSpeed = 2f;

    void Update()
    {
        camZoom -= Input.GetAxis("Mouse ScrollWheel") * camZoomSpeed;

        // Makes the actual change to Field Of View
        Camera.main.orthographicSize = camZoom;
    }
}