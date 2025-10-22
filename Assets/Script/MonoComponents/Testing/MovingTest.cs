using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTest : MonoBehaviour
{
    public CatmullRomSpline2D movingPath;

    [SerializeField] float speed = 0.1f;
    [SerializeField] float distance = 0;
    [SerializeField] Animator animator;
    float pathLength;
    private void Awake()
    {
        pathLength = movingPath.GetTotalLength();
        animator.SetInteger("State", 1);
    }
    private void Update()
    {
        distance += speed * Time.deltaTime;
        if (distance > pathLength) distance -= pathLength;
        var last = transform.position;
        transform.position = movingPath.GetPointByDistance(distance);
        var dir = (int)MovingUtils.GetDirection4Index(transform.position - last, Camera.main.transform);
        animator.SetInteger("Dir", dir);
    }
}
