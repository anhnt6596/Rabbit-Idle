using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TileMapRenderer : MonoBehaviour
{
    public TileMapAsset map;
    public Material material; // tạm dùng 1 texture atlas đơn/1 màu theo type
    public Gradient debugColors; // tạm: tô màu theo type để nhìn map

    void Start() => Rebuild();

    public void Rebuild()
    {
        if (!map) return;

        int w = map.width, h = map.height;
        float s = map.tileSize;
        int quads = w * h;

        var verts = new Vector3[quads * 4];
        var uv0 = new Vector2[quads * 4];
        var uv2 = new Vector2[quads * 4];
        var cols = new Color[quads * 4];
        var tris = new int[quads * 6];

        int vi = 0, ti = 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float x0 = x * s, z0 = y * s;
                float x1 = x0 + s, z1 = z0 + s;

                verts[vi + 0] = new Vector3(x0, 0, z0);
                verts[vi + 1] = new Vector3(x1, 0, z0);
                verts[vi + 2] = new Vector3(x1, 0, z1);
                verts[vi + 3] = new Vector3(x0, 0, z1);

                // UV0 (0..1)
                uv0[vi + 0] = new Vector2(0, 0);
                uv0[vi + 1] = new Vector2(1, 0);
                uv0[vi + 2] = new Vector2(1, 1);
                uv0[vi + 3] = new Vector2(0, 1);

                int layer = (int)map.Get(x, y);
                var li = new Vector2(layer, 0);
                uv2[vi + 0] = li;
                uv2[vi + 1] = li;
                uv2[vi + 2] = li;
                uv2[vi + 3] = li;

                // debug: màu theo type (sau này chuyển sang Texture2DArray)
                float t = (int)map.Get(x, y) / Mathf.Max(1f, System.Enum.GetValues(typeof(TileType)).Length - 1);
                var c = debugColors != null ? debugColors.Evaluate(t) : Color.Lerp(Color.green, Color.gray, t);
                cols[vi + 0] = cols[vi + 1] = cols[vi + 2] = cols[vi + 3] = c;

                tris[ti + 0] = vi + 0; tris[ti + 1] = vi + 2; tris[ti + 2] = vi + 1;
                tris[ti + 3] = vi + 0; tris[ti + 4] = vi + 3; tris[ti + 5] = vi + 2;

                vi += 4; ti += 6;
            }

        var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uv0);
        mesh.SetUVs(1, uv2);
        mesh.SetColors(cols);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();

        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        mr.sharedMaterial = material;

        if (overlayMaterial)
        {
            var overlay = BuildOverlayMesh(map);
            var child = transform.Find("Overlay");
            if (!child)
            {
                child = new GameObject("Overlay").transform;
                child.SetParent(transform, false);
                child.gameObject.layer = gameObject.layer;
                child.gameObject.AddComponent<MeshFilter>();
                child.gameObject.AddComponent<MeshRenderer>();
            }
            child.GetComponent<MeshFilter>().sharedMesh = overlay;
            child.GetComponent<MeshRenderer>().sharedMaterial = overlayMaterial;
        }
    }

    [SerializeField] float edgeWidth01 = 0.3f; // 0..1 theo tileSize
    public Material overlayMaterial; // gán shader Tiles/UnlitArray_EdgeBlend

    Mesh BuildOverlayMesh(TileMapAsset map)
    {
        float s = map.tileSize;
        float ew = Mathf.Clamp01(edgeWidth01) * s;

        var verts = new List<Vector3>();
        var uv0 = new List<Vector2>();
        var uv2 = new List<Vector2>(); // center layer
        var uv3 = new List<Vector2>(); // neighbor layer
        var cols = new List<Color>();
        var tris = new List<int>();

        void AddQuad(
            Vector3 a, Vector3 b, Vector3 c, Vector3 d,
            int blendLayer,               // <-- layer của tile "đang đè"
            bool invertY)                 // uv.y = 0 ở phía tile đè; 1 ra ngoài
            {
                int i0 = verts.Count;

                verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);

                if (!invertY)
                {
                    uv0.Add(new Vector2(0, 0)); uv0.Add(new Vector2(1, 0));
                    uv0.Add(new Vector2(1, 1)); uv0.Add(new Vector2(0, 1));
                }
                else
                {
                    uv0.Add(new Vector2(0, 1)); uv0.Add(new Vector2(1, 1));
                    uv0.Add(new Vector2(1, 0)); uv0.Add(new Vector2(0, 0));
                }

                // UV2.x = blendLayer (tile đè)
                Vector2 li = new Vector2(blendLayer, 0f);
                uv2.Add(li); uv2.Add(li); uv2.Add(li); uv2.Add(li);

                cols.Add(Color.white); cols.Add(Color.white);
                cols.Add(Color.white); cols.Add(Color.white);

                tris.Add(i0 + 0); tris.Add(i0 + 2); tris.Add(i0 + 1);
                tris.Add(i0 + 0); tris.Add(i0 + 3); tris.Add(i0 + 2);
            }


        for (int y = 0; y < map.height; y++)
            for (int x = 0; x < map.width; x++)
            {
                // s = tileSize, ew = edgeWidth
                float x0 = x * s, z0 = y * s, x1 = x0 + s, z1 = z0 + s;
                TileType center = map.Get(x, y);
                int lc = (int)center;

                // LEFT (x-1,y) — chỉ tạo khi center đè neighbor
                if (map.InBounds(x - 1, y))
                {
                    var n = map.Get(x - 1, y);
                    if (n != center && ShouldOverlay(center, n))
                    {
                        // strip ở mép trái ô center
                        Vector3 A = new Vector3(x0, 0, z0);
                        Vector3 B = new Vector3(x0 + ew, 0, z0);
                        Vector3 C = new Vector3(x0 + ew, 0, z1);
                        Vector3 D = new Vector3(x0, 0, z1);
                        AddQuad(A, B, C, D, /*blendLayer=*/lc, /*invertY=*/false);
                    }
                }

                // RIGHT (x+1,y)
                if (map.InBounds(x + 1, y))
                {
                    var n = map.Get(x + 1, y);
                    if (n != center && ShouldOverlay(center, n))
                    {
                        Vector3 A = new Vector3(x1 - ew, 0, z0);
                        Vector3 B = new Vector3(x1, 0, z0);
                        Vector3 C = new Vector3(x1, 0, z1);
                        Vector3 D = new Vector3(x1 - ew, 0, z1);
                        AddQuad(A, B, C, D, lc, /*invertY=*/true);
                    }
                }

                // DOWN (x,y-1)
                if (map.InBounds(x, y - 1))
                {
                    var n = map.Get(x, y - 1);
                    if (n != center && ShouldOverlay(center, n))
                    {
                        Vector3 A = new Vector3(x0, 0, z0);
                        Vector3 B = new Vector3(x1, 0, z0);
                        Vector3 C = new Vector3(x1, 0, z0 + ew);
                        Vector3 D = new Vector3(x0, 0, z0 + ew);
                        AddQuad(A, B, C, D, lc, /*invertY=*/false);
                    }
                }

                // UP (x,y+1)
                if (map.InBounds(x, y + 1))
                {
                    var n = map.Get(x, y + 1);
                    if (n != center && ShouldOverlay(center, n))
                    {
                        Vector3 A = new Vector3(x0, 0, z1 - ew);
                        Vector3 B = new Vector3(x1, 0, z1 - ew);
                        Vector3 C = new Vector3(x1, 0, z1);
                        Vector3 D = new Vector3(x0, 0, z1);
                        AddQuad(A, B, C, D, lc, /*invertY=*/true);
                    }
                }
            }

        var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uv0);
        mesh.SetUVs(1, uv2);  // layer index
        mesh.SetColors(cols);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    int Priority(TileType t)
    {
        return (int)t;
    }

    bool ShouldOverlay(TileType center, TileType neigh)
    {
        return Priority(center) < Priority(neigh);
    }




#if UNITY_EDITOR
    public void RebuildIfMap(TileMapAsset changed)
    {
        if (map == changed) Rebuild();
    }
#endif
}