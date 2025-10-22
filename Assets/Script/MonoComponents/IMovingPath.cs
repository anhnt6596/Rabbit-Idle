using UnityEngine;

public interface IMovingPath
{
    public float GetTotalLength();
    public Vector3 GetPointByDistance(float d);
}