using UnityEngine;

public class CameraFollowOrbit : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset Settings")]
    public float distance = 5f;          // khoảng cách mặc định
    public float minDistance = 2f;       // giới hạn zoom gần nhất
    public float maxDistance = 10f;      // giới hạn zoom xa nhất
    public float scrollSpeed = 3f;       // tốc độ zoom chuột

    [Header("Angle Settings")]
    public float yaw = 0f;               // xoay quanh trục Y (ngang)
    public float pitch = 30f;            // góc nhìn từ trên xuống
    public float rotateSmooth = 5f;      // độ mượt khi xoay
    public bool followRotation = false;  // nếu true, camera xoay theo target

    [Header("Follow Smoothing")]
    public float followSmooth = 10f;     // độ mượt khi follow

    private Vector3 currentPosition;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollowOrbit: No target assigned!");
            enabled = false;
            return;
        }
        currentPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- zoom bằng cuộn chuột ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * scrollSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // --- tính vị trí camera theo góc & khoảng cách ---
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 desiredPos = target.position + offset;

        // --- di chuyển mượt ---
        currentPosition = Vector3.Lerp(currentPosition, desiredPos, Time.deltaTime * followSmooth);
        transform.position = currentPosition;

        // --- luôn nhìn về target ---
        transform.LookAt(target.position);

        // --- nếu muốn camera xoay theo hướng target ---
        if (followRotation)
        {
            yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, Time.deltaTime * rotateSmooth);
        }
    }
}
