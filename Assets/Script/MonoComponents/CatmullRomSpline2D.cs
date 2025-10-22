using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CatmullRomSpline2D : MonoBehaviour, IMovingPath
{
    [Header("Control Points (min 4)")]
    public List<Transform> controlPoints = new List<Transform>();

    [Range(4, 500)]
    public int segments = 50;

    private float totalLength;
    private List<float> cumulativeLengths = new List<float>();

    // =====================
    // PUBLIC API
    // =====================

    public Vector3 GetPoint(float t)
    {
        if (controlPoints.Count < 4) return Vector3.zero;

        t = Mathf.Clamp01(t);

        int numSections = controlPoints.Count - 3;
        float scaledT = t * numSections;
        int currPt = Mathf.Min(Mathf.FloorToInt(scaledT), numSections - 1);
        float u = scaledT - currPt;

        Vector3 a = controlPoints[currPt].position;
        Vector3 b = controlPoints[currPt + 1].position;
        Vector3 c = controlPoints[currPt + 2].position;
        Vector3 d = controlPoints[currPt + 3].position;

        return 0.5f * (
            (-a + 3f * b - 3f * c + d) * (u * u * u) +
            (2f * a - 5f * b + 4f * c - d) * (u * u) +
            (-a + c) * u +
            2f * b
        );
    }

    public float GetTotalLength()
    {
        UpdateLengths();
        return totalLength;
    }

    public Vector3 GetPointByDistance(float d)
    {
        UpdateLengths();

        d = Mathf.Clamp(d, 0, totalLength);

        for (int i = 1; i < cumulativeLengths.Count; i++)
        {
            if (d <= cumulativeLengths[i])
            {
                float segLen = cumulativeLengths[i] - cumulativeLengths[i - 1];
                float segT = (d - cumulativeLengths[i - 1]) / segLen;
                float t = (i - 1 + segT) / segments;
                return GetPoint(t);
            }
        }

        return GetPoint(1f);
    }


    // =====================
    // PRIVATE
    // =====================

    private void UpdateLengths()
    {
        if (controlPoints.Count < 4) return;

        cumulativeLengths.Clear();
        cumulativeLengths.Add(0);

        Vector3 prev = GetPoint(0f);
        totalLength = 0;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 curr = GetPoint(t);

            float dx = curr.x - prev.x;
            //float dy = curr.y - prev.y; // Ignored for 2D spline
            float dz = curr.z - prev.z;
            //float segLen = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
            float segLen = Mathf.Sqrt(dx * dx + dz * dz);

            totalLength += segLen;
            cumulativeLengths.Add(totalLength);
            prev = curr;
        }
    }

    // =====================
    // GIZMOS
    // =====================

    private void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Count < 4) return;

        Gizmos.color = Color.green;
        Vector3 prev = GetPoint(0f);

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 curr = GetPoint(t);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }

        Gizmos.color = Color.red;
        foreach (var p in controlPoints)
        {
            if (p != null)
                Gizmos.DrawSphere(p.position, 0.1f);
        }
    }
}
