using UnityEngine;

public class FireLightFlickerAdvanced : MonoBehaviour
{
    public enum Plane { XY, XZ }              // Sprite nằm trên mặt phẳng nào?
    [Header("General")]
    public Plane plane = Plane.XZ;            // Thường là XZ nếu chiếu xuống mặt đất
    public Color baseColor = new Color(1f, 0.82f, 0.55f);
    public bool colorShift = true;            // đổi nhẹ sắc độ cam-vàng
    [Range(0f, 0.2f)] public float colorShiftStrength = 0.04f;

    [Header("Intensity (brightness)")]
    [Range(0f, 2f)] public float intensityMin = 0.7f;
    [Range(0f, 2f)] public float intensityMax = 1.2f;
    public float intensitySpeed = 8f;
    public float intensitySmooth = 12f;

    [Header("Scale Flicker")]
    [Tooltip("Hệ số nhân scale. 1 = không đổi.")]
    [Range(0.5f, 2f)] public float scaleMin = 0.92f;
    [Range(0.5f, 2f)] public float scaleMax = 1.08f;
    public float scaleSpeed = 4.5f;
    public float scaleSmooth = 10f;
    public bool uniformScale = true;          // true = scale đều 3 trục

    [Header("Position Jitter")]
    [Tooltip("Biên độ dao động vị trí (đơn vị thế giới).")]
    public float posAmplitude = 0.04f;        // 4cm
    public float posSpeed = 3.5f;
    public float posSmooth = 12f;
    public bool useLocalSpace = true;         // dao động theo local hay world

    [Header("Seeds (desync nhiều đốm lửa)")]
    public float seedIntensity = 0f;
    public float seedScale = 11.1f;
    public float seedPosX = 23.7f;
    public float seedPosYorZ = 47.9f;         // Y nếu XY; Z nếu XZ

    private Vector3 basePos;
    private Vector3 baseScale;
    private float curIntensity = 1f;
    private float targetIntensity = 1f;
    private Vector3 posVel;                   // cho SmoothDamp
    private Vector3 scaleVel;                 // cho SmoothDamp
    private Vector3 targetScale;

    void Awake()
    {
        basePos = useLocalSpace ? transform.localPosition : transform.position;
        baseScale = transform.localScale;

        // random seed nhẹ nếu để 0
        if (Mathf.Approximately(seedIntensity, 0f)) seedIntensity = Random.value * 100f;
        if (Mathf.Approximately(seedScale, 0f)) seedScale = Random.value * 100f + 10f;
        if (Mathf.Approximately(seedPosX, 0f)) seedPosX = Random.value * 100f + 20f;
        if (Mathf.Approximately(seedPosYorZ, 0f)) seedPosYorZ = Random.value * 100f + 30f;
    }

    void Update()
    {
        float t = Time.time;

        // ---------- INTENSITY ----------
        float nI = Mathf.PerlinNoise(t * intensitySpeed, seedIntensity);
        targetIntensity = Mathf.Lerp(intensityMin, intensityMax, nI);
        curIntensity = Mathf.Lerp(curIntensity, targetIntensity, Time.deltaTime * intensitySmooth);

        // Áp màu + intensity
        Color c = baseColor * curIntensity;
        if (colorShift)
        {
            float s = Mathf.PerlinNoise(seedIntensity, t * (intensitySpeed * 0.6f)) - 0.5f;
            c.r += s * colorShiftStrength;
            c.g += s * colorShiftStrength * 0.6f;
            c.b -= s * colorShiftStrength; // hơi ngả đỏ -> ấm hơn
        }

        // ---------- SCALE ----------
        float nS = Mathf.PerlinNoise(t * scaleSpeed, seedScale);
        float scaleMul = Mathf.Lerp(scaleMin, scaleMax, nS);
        if (uniformScale)
            targetScale = baseScale * scaleMul;
        else
            targetScale = new Vector3(baseScale.x * scaleMul, baseScale.y, baseScale.z * scaleMul);

        // mượt scale (SmoothDamp)
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVel, 1f / Mathf.Max(0.0001f, scaleSmooth));

        // ---------- POSITION ----------
        // hai noise độc lập cho 2 trục của mặt phẳng
        float nPX = Mathf.PerlinNoise(t * posSpeed, seedPosX) - 0.5f;          // [-0.5..0.5]
        float nPYZ = Mathf.PerlinNoise(seedPosYorZ, t * posSpeed) - 0.5f;      // [-0.5..0.5]
        Vector3 offset = Vector3.zero;
        if (plane == Plane.XY)
            offset = new Vector3(nPX, nPYZ, 0f) * (posAmplitude * 2f);         // XY dao động
        else
            offset = new Vector3(nPX, 0f, nPYZ) * (posAmplitude * 2f);         // XZ dao động

        Vector3 targetPos = basePos + offset;
        if (useLocalSpace)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPos, ref posVel, 1f / Mathf.Max(0.0001f, posSmooth));
        else
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref posVel, 1f / Mathf.Max(0.0001f, posSmooth));
    }

    // Nếu bạn thay đổi vị trí khi chạy và muốn "neo" basePos mới:
    public void ReanchorNow()
    {
        basePos = useLocalSpace ? transform.localPosition : transform.position;
    }
}
