using UnityEngine;
using UnityEditor;

public class TileMapEditorWindow : EditorWindow
{
    TileMapAsset map;
    TileType brush = TileType.Grass;
    int brushSize = 1;
    bool eraser = false;
    bool picking = false;
    float groundY = 0f;

    [MenuItem("Tools/TileMap Painter")]
    public static void Open() => GetWindow<TileMapEditorWindow>("TileMap Painter");

    void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    void OnGUI()
    {
        map = (TileMapAsset)EditorGUILayout.ObjectField("Map Asset", map, typeof(TileMapAsset), false);
        groundY = EditorGUILayout.FloatField("Ground Y", groundY);

        if (map == null)
        {
            if (GUILayout.Button("Create New Map Asset"))
            {
                var asset = ScriptableObject.CreateInstance<TileMapAsset>();
                asset.Init(64, 64, 1f, TileType.Grass);
                string path = EditorUtility.SaveFilePanelInProject("Save TileMap", "TileMap", "asset", "Save map asset");
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    map = asset;
                }
            }
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Size / Tile", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Size: {map.width} x {map.height}, TileSize: {map.tileSize}");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
        brush = (TileType)EditorGUILayout.EnumPopup("Tile", brush);
        brushSize = EditorGUILayout.IntSlider("Brush Size", brushSize, 1, 8);
        eraser = EditorGUILayout.ToggleLeft("Eraser (set to Grass)", eraser);
        picking = EditorGUILayout.ToggleLeft("Picker (hold)", picking);

        if (GUILayout.Button("Fill All With Current Brush"))
        {
            Undo.RecordObject(map, "Fill Map");
            for (int y = 0; y < map.height; y++)
                for (int x = 0; x < map.width; x++)
                    map.Set(x, y, eraser ? TileType.Grass : brush);
            EditorUtility.SetDirty(map);
        }

        if (GUILayout.Button("Reinit Map..."))
        {
            if (EditorUtility.DisplayDialog("Reinit", "Reset size & clear map?", "OK", "Cancel"))
            {
                int w = EditorGUILayout.IntField("W", map.width);
                int h = EditorGUILayout.IntField("H", map.height);
            }
        }
    }

    void OnSceneGUI(SceneView sv)
    {
        if (map == null) return;

        Event e = Event.current;
        // Ray hit plane Y=groundY
        Plane plane = new Plane(Vector3.up, new Vector3(0, groundY, 0));
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!plane.Raycast(ray, out float dist)) return;
        Vector3 world = ray.GetPoint(dist);

        // hiển thị con trỏ
        Handles.color = new Color(1, 1, 0, 0.6f);
        Handles.DrawWireDisc(world, Vector3.up, map.tileSize * brushSize);

        // convert world -> cell (X,Z)
        int cx = Mathf.FloorToInt(world.x / map.tileSize);
        int cy = Mathf.FloorToInt(world.z / map.tileSize);

        if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0 && !e.alt)
        {
            PaintArea(cx, cy, e.shift ? Mathf.Max(2, brushSize * 2) : brushSize);
            e.Use();
        }

        // picker bằng chuột phải
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            if (map.InBounds(cx, cy))
            {
                brush = map.Get(cx, cy);
                Repaint();
            }
            e.Use();
        }

        // vẽ lưới gizmo mảnh
        DrawGridGizmos(map, groundY);
    }
    // TileMapEditorWindow.cs

    void PaintArea(int cx, int cy, int size)
    {
        if (map == null) return;
        map.EnsureAlloc();

        Undo.RecordObject(map, "Paint Tiles");

        TileType t = eraser ? TileType.Grass : brush;

        // Tính góc trái-dưới của brush (centered ở (cx, cy))
        int half = size / 2;
        int x0 = cx - half;
        int y0 = cy - half;

        // Clamp vùng vẽ vào biên map
        int xStart = Mathf.Max(0, x0);
        int yStart = Mathf.Max(0, y0);
        int xEnd = Mathf.Min(map.width, x0 + size);
        int yEnd = Mathf.Min(map.height, y0 + size);

        for (int y = yStart; y < yEnd; y++)
            for (int x = xStart; x < xEnd; x++)
                map.Set(x, y, t); // Set() đã tự check biên rồi, nhưng ta vẫn clamp để tối ưu

        EditorUtility.SetDirty(map);

        // Rebuild ngay tất cả renderer dùng map này
#if UNITY_EDITOR
        var rends = Object.FindObjectsOfType<TileMapRenderer>();
        foreach (var r in rends) r.RebuildIfMap(map);

        // vẽ lại Scene
        SceneView.RepaintAll();
#endif
    }

    void PaintAreaCircle(int cx, int cy, int size)
    {
        if (map == null) return;
        map.EnsureAlloc();

        Undo.RecordObject(map, "Paint Tiles");

        TileType t = eraser ? TileType.Grass : brush;
        float r = size * 0.5f;
        float r2 = r * r;

        int x0 = Mathf.FloorToInt(cx - r);
        int y0 = Mathf.FloorToInt(cy - r);
        int x1 = Mathf.CeilToInt(cx + r);
        int y1 = Mathf.CeilToInt(cy + r);

        for (int y = Mathf.Max(0, y0); y < Mathf.Min(map.height, y1); y++)
            for (int x = Mathf.Max(0, x0); x < Mathf.Min(map.width, x1); x++)
            {
                float dx = (x + 0.5f) - cx;
                float dy = (y + 0.5f) - cy;
                if (dx * dx + dy * dy <= r2)
                    map.Set(x, y, t);
            }

        EditorUtility.SetDirty(map);
    }

    void DrawGridGizmos(TileMapAsset m, float y)
    {
        Handles.color = new Color(1, 1, 1, 0.1f);
        int w = m.width; int h = m.height; float s = m.tileSize;
        Vector3 origin = new Vector3(0, y, 0);
        for (int x = 0; x <= w; x++)
            Handles.DrawLine(origin + new Vector3(x * s, 0, 0), origin + new Vector3(x * s, 0, h * s));
        for (int z = 0; z <= h; z++)
            Handles.DrawLine(origin + new Vector3(0, 0, z * s), origin + new Vector3(w * s, 0, z * s));
    }
}
