using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        var cam = Camera.main;
        if (!cam) return;
        transform.forward = cam.transform.forward;
    }
}
