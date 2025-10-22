using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BillboardY : MonoBehaviour
{
    private void LateUpdate()
    {
        var cam = Camera.main;
        if (!cam) return;

        Vector3 dir = cam.transform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
