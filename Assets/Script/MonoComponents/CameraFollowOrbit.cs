using UnityEngine;

public class CameraFollowOrbit : MonoBehaviour
{
    public enum CamMode { Focus, Free }

    [Header("Target / Mode")]
    public Transform target;
    public CamMode mode = CamMode.Focus;

    [Header("Distance / Zoom")]
    public float distance = 6f;
    public float minDistance = 2f;
    public float maxDistance = 14f;
    public float scrollSpeed = 3f;

    [Header("Angles")]
    public float yaw = 0f;
    public float pitch = 35f;

    [Header("Smoothing")]
    public float followSmooth = 100f;   // mượt vị trí camera
    public float snapSpeed = 8f;     // mượt xoay Q/E

    [Header("Snap (Q/E)")]
    public float snapStep = 90f;

    [Header("Free Mode - Pan")]
    public float groundY = 0f;
    public float panFriction = 12f;
    public float panSpeed = 1f;

    // internal
    Vector3 pivot, currentPos, desiredPos, pivotVel;
    Vector3 _panTargetPivot, panStartWorld;
    float targetYaw;
    bool isSnapping, isPanning;

    void Start()
    {
        if (!target && mode == CamMode.Focus)
        {
            Debug.LogWarning("No target for Focus mode -> fallback to Free");
            mode = CamMode.Free;
        }

        pivot = (mode == CamMode.Focus && target)
            ? target.position
            : transform.position + transform.forward * distance;

        _panTargetPivot = pivot;
        currentPos = transform.position;
        targetYaw = yaw;

        RecomputeDesired();                 // vị trí mong muốn ban đầu
        currentPos = desiredPos;            // đặt ngay vị trí
        transform.position = currentPos;
        // Rotation sẽ set trong LateUpdate theo mode
    }

    void LateUpdate()
    {
        // Toggle mode (F)
        if (Input.GetKeyDown(KeyCode.F)) ToggleMode();

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
            distance = Mathf.Clamp(distance - scroll * scrollSpeed, minDistance, maxDistance);

        // Snap Q/E
        if (mode == CamMode.Focus)
        {
            if (Input.GetKeyDown(KeyCode.Q)) { targetYaw -= snapStep; isSnapping = true; }
            if (Input.GetKeyDown(KeyCode.E)) { targetYaw += snapStep; isSnapping = true; }
        }

        // Lerp yaw -> targetYaw
        if (isSnapping)
        {
            yaw = Mathf.LerpAngle(yaw, targetYaw, Time.deltaTime * snapSpeed);
            if (Mathf.Abs(Mathf.DeltaAngle(yaw, targetYaw)) < 0.1f)
            { yaw = targetYaw; isSnapping = false; }
        }

        // Update pivot theo mode
        if (mode == CamMode.Focus)
        {
            if (target)
                pivot = Vector3.Lerp(pivot, target.position, Time.deltaTime * followSmooth);
        }
        else // Free
        {
            HandlePan();
            pivot = Vector3.SmoothDamp(pivot, _panTargetPivot, ref pivotVel, 1f / Mathf.Max(0.0001f, panFriction));
        }

        // Tính vị trí camera từ yaw/pitch/distance + pivot
        RecomputeDesired();

        // Mượt vị trí
        currentPos = Vector3.Lerp(currentPos, desiredPos, Time.deltaTime * followSmooth);
        transform.position = currentPos;

        // ---- ROTATION THEO MODE ----
        if (mode == CamMode.Focus)
        {
            // Nhìn vào pivot để theo target mượt, không “nhảy tưng”
            var lookRot = Quaternion.LookRotation(pivot - currentPos, Vector3.up);
            transform.rotation = lookRot;
            // (pitch/yaw lúc này chỉ quyết định vị trí trên quỹ đạo thông qua desiredPos)
        }
        else // Free
        {
            // Giữ góc cố định (không LookAt)
            var rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.rotation = rot;
        }
    }

    void RecomputeDesired()
    {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rot * new Vector3(0, 0, -distance);
        desiredPos = pivot + offset;
    }

    // ======== PAN trong Free mode ========
    void HandlePan()
    {
        var plane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));

        if (Input.GetMouseButtonDown(0) && RayToPlane(Input.mousePosition, plane, out panStartWorld))
            isPanning = true;

        if (isPanning && Input.GetMouseButton(0) && RayToPlane(Input.mousePosition, plane, out var curWorld))
        {
            Vector3 delta = (panStartWorld - curWorld) * panSpeed;
            _panTargetPivot = pivot + delta;
        }

        if (Input.GetMouseButtonUp(0)) isPanning = false;
    }

    bool RayToPlane(Vector3 screenPos, Plane plane, out Vector3 hit)
    {
        var ray = Camera.main.ScreenPointToRay(screenPos);
        if (plane.Raycast(ray, out float dist)) { hit = ray.GetPoint(dist); return true; }
        hit = Vector3.zero; return false;
    }

    public void ToggleMode()
    {
        if (mode == CamMode.Focus)
        {
            mode = CamMode.Free;
            pivot = target ? target.position : pivot;
            _panTargetPivot = pivot;
        }
        else
        {
            mode = CamMode.Focus;
            if (target) { pivot = target.position; _panTargetPivot = pivot; }
        }
    }
}
