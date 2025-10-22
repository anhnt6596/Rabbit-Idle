// Editor/CreateTexture2DArray.cs (phiên bản robust)
using UnityEngine;
using UnityEditor;

public class CreateTexture2DArray
{
    [MenuItem("Assets/Create/Tile/Texture2DArray")]
    public static void CreateArray()
    {
        var texs = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        if (texs.Length == 0) { Debug.LogError("Select textures first."); return; }

        // Chọn kích thước chuẩn theo texture đầu tiên
        int w = texs[0].width, h = texs[0].height;
        bool genMips = true;

        // Tạo Texture2DArray RGBA32 (sRGB), có mip
        var arr = new Texture2DArray(w, h, texs.Length, TextureFormat.RGBA32, genMips, false);
        arr.wrapMode = TextureWrapMode.Repeat;
        arr.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < texs.Length; i++)
        {
            var src = texs[i];
            // Đảm bảo Read/Write để GetPixels32 hoạt động
            var path = AssetDatabase.GetAssetPath(src);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (!imp.isReadable || imp.textureType != TextureImporterType.Default)
            {
                imp.textureType = TextureImporterType.Default;
                imp.isReadable = true;
                imp.SaveAndReimport();
            }

            // Nếu size khác, scale về w×h qua RenderTexture (giữ sRGB)
            Texture2D readable = src;
            Texture2D tmpScaled = null;
            if (src.width != w || src.height != h)
            {
                var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                Graphics.Blit(src, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                tmpScaled = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                tmpScaled.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tmpScaled.Apply();
                RenderTexture.active = prev;
                rt.Release();
                readable = tmpScaled;
            }

            // Copy pixel vào slice i (mip 0)
            var px = readable.GetPixels32();
            arr.SetPixels32(px, i, 0);

            if (tmpScaled) Object.DestroyImmediate(tmpScaled);
        }

        // Tạo mipmap cho array
        arr.Apply(true, false);

        string pathSave = EditorUtility.SaveFilePanelInProject("Save Texture2DArray", "TilesArray", "asset", "Save array asset");
        if (!string.IsNullOrEmpty(pathSave))
        {
            AssetDatabase.CreateAsset(arr, pathSave);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(arr);
        }

        Debug.Log($"Created Texture2DArray: {texs.Length} layers, {w}x{h}, RGBA32, mips.");
    }
}
