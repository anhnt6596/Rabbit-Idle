using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LightCamera : MonoBehaviour
{
    public Camera mainCam;
    public Camera lightCam;
    private void LateUpdate()
    {
        if (!mainCam) mainCam = Camera.main;
        if (!lightCam) lightCam = GetComponent<Camera>();

        transform.position = mainCam.transform.position;
        transform.rotation = mainCam.transform.rotation;
        float baseFOV = mainCam.fieldOfView;
        float aspect = mainCam.aspect;

        float fovRad = baseFOV * Mathf.Deg2Rad;
        float expandedFovRad = Mathf.Atan(Mathf.Tan(fovRad * 0.5f) * aspect) * 2f;
        
        lightCam.fieldOfView = expandedFovRad * Mathf.Rad2Deg;
    }
}
