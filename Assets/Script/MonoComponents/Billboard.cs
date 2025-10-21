using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        var cam = Camera.main;
        transform.forward = cam.transform.forward;
    }
}
