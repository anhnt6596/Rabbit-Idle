using UnityEngine;
public enum TileType : byte { Grass, Savanna, Rocky, Swamp, Road, Water, Dirt }


[CreateAssetMenu(menuName = "TileMap/Tile Map Asset", fileName = "TileMap")]
public class TileMapAsset : ScriptableObject
{
    public int width = 64;
    public int height = 64;
    public float tileSize = 1f;
    [HideInInspector] public TileType[] tiles;

    void OnValidate() { EnsureAlloc(); }
    void OnEnable() { EnsureAlloc(); }

    public void Init(int w, int h, float size, TileType fill = TileType.Grass)
    {
        width = Mathf.Max(1, w);
        height = Mathf.Max(1, h);
        tileSize = size;
        tiles = new TileType[width * height];
        for (int i = 0; i < tiles.Length; i++) tiles[i] = fill;
    }

    public void EnsureAlloc()
    {
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        if (tiles == null || tiles.Length != width * height)
        {
            var old = tiles;
            tiles = new TileType[width * height];
            if (old != null)
            {
                // copy phần chung khi thay đổi size
                int wmin = Mathf.Min(width, Mathf.Max(1, width));
                int hmin = Mathf.Min(height, Mathf.Max(1, height));
            }
        }
    }

    public bool InBounds(int x, int y) => (uint)x < (uint)width && (uint)y < (uint)height;
    public int Idx(int x, int y) => y * width + x;

    public TileType Get(int x, int y)
    {
        if (!InBounds(x, y)) return TileType.Grass; // hoặc giá trị mặc định
        return tiles[Idx(x, y)];
    }

    public bool Set(int x, int y, TileType t)
    {
        if (!InBounds(x, y)) return false; // <- NGĂN VƯỢT BIÊN Ở ĐÂY
        tiles[Idx(x, y)] = t;
        return true;
    }
}
