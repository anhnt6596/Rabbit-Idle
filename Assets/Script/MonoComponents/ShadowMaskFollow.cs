using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MaskFollowCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float distance = 0.5f;
    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        transform.position = targetCamera.transform.position + targetCamera.transform.forward * distance;
        transform.rotation = targetCamera.transform.rotation;

        float height = Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance * 2f;
        float width = height * targetCamera.aspect;
        transform.localScale = new Vector3(width, height, 1f);
    }

}
