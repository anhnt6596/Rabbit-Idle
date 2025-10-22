using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TileMapRenderer : MonoBehaviour
{
    public TileMapAsset map;
    public Material material; // tạm dùng 1 texture atlas đơn/1 màu theo type

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
    }

#if UNITY_EDITOR
    public void RebuildIfMap(TileMapAsset changed)
    {
        if (map == changed) Rebuild();
    }
#endif
}