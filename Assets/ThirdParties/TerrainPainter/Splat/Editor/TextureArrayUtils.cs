using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class TextureArrayUtils
{
    [MenuItem("Assets/Create/TerrainSplat/Texture2DArray",true)]
    private static bool ValidateCreate2DArray()
    {
        return Selection.objects.All(obj => obj is Texture2D);
    }
    [MenuItem("Assets/Create/TerrainSplat/Texture2DArray")]
    static void Create2DArray()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        textures = textures.OrderBy(tex => tex.name).ToArray();
        string path = EditorUtility.SaveFilePanelInProject(
            "Create 2D Array", 
            "New Texture2DArray", 
            "asset", 
            "Create 2D Array");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        CreateTextureArray(textures, path);
    }

    public static void CreateTextureArray(Texture2D[] textures, string path)
    {
        Texture2D first = textures.FirstOrDefault();
        int width = first.width;
        int height = first.height;
        TextureFormat format = first.format;
        int mipmapCount = first.mipmapCount;
        
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        Texture2DArray array = new Texture2DArray(width, height, textures.Length, format, mipmapCount > 1);
        for (int i = 0; i < textures.Length; ++i)
        {
            array.LoadFrame(textures[i], i);
        }
        array.Apply(false);
        AssetDatabase.CreateAsset(array, path);
        AssetDatabase.Refresh();
    }

    static void LoadFrame(this Texture2DArray array, Texture2D src, int element)
    {
        if (array.format != src.format)
        {
            Debug.LogError("Texture Format Mismatch");
            return;
        }
        if (array.width != src.width || array.height != src.height)
        {
            Debug.LogError("Texture Dimension Mismatch");
            return;
        }
        if (array.mipmapCount != src.mipmapCount)
        {
            Debug.LogError("Mipmap Mismatch");
            return;
        }
        
        var tex = src;
        var rawData = tex.GetRawTextureData();
        int mipmapCount = tex.mipmapCount;
        int tileSize = src.format.GetCompressionTileSize();
        int pixelCount = (int)Enumerable.Range(0, mipmapCount).Select(m => Mathf.Max(tileSize, (int)Mathf.Pow(4, m))).Sum();
        for (int mip = 0; mip < mipmapCount; ++mip)
        {
            int pixelOffset = (int)Enumerable.Range(mipmapCount -  mip, mip)
                .Select(m => Mathf.Max((int)Mathf.Pow(4, m), tileSize)).Sum();
            int byteOffset = (int)((long)pixelOffset * rawData.Length / pixelCount);
            array.SetPixelData(rawData, mip, element, byteOffset);
        }
    }

    private static readonly Dictionary<TextureFormat, int> texFormatCompressionTileSize = new Dictionary<TextureFormat, int>()
    {
        { TextureFormat.Alpha8, 1 },
        { TextureFormat.ARGB4444, 1 },
        { TextureFormat.RGB24, 1 },
        { TextureFormat.RGBA32, 1 },
        { TextureFormat.ARGB32, 1 },
        { TextureFormat.RGB565, 1 },
        { TextureFormat.R16, 1 },
        { TextureFormat.DXT1, 16 },
        { TextureFormat.DXT5, 16 },
        { TextureFormat.RGBA4444, 1 },
        { TextureFormat.BGRA32, 1 },
        { TextureFormat.RHalf, 1 },
        { TextureFormat.RGHalf, 1 },
        { TextureFormat.RGBAHalf, 1 },
        { TextureFormat.RFloat, 1 },
        { TextureFormat.RGFloat, 1 },
        { TextureFormat.RGBAFloat, 1 },
        { TextureFormat.YUY2, 1 },
        { TextureFormat.RGB9e5Float, 1 },
        { TextureFormat.BC6H, 16 },
        { TextureFormat.BC7, 16 },
        { TextureFormat.BC4, 16 },
        { TextureFormat.BC5, 16 },
        //{ TextureFormat.DXT1Crunched, 28 },
        //{ TextureFormat.DXT5Crunched, 29 },
        { TextureFormat.PVRTC_RGB2, 32 },
        { TextureFormat.PVRTC_RGBA2, 32 },
        { TextureFormat.PVRTC_RGB4, 16 },
        { TextureFormat.PVRTC_RGBA4, 16 },
        { TextureFormat.ETC_RGB4, 16},
        { TextureFormat.EAC_R, 16 },
        { TextureFormat.EAC_R_SIGNED, 16 },
        { TextureFormat.EAC_RG, 16 },
        { TextureFormat.EAC_RG_SIGNED, 16},
        { TextureFormat.ETC2_RGB, 16 },
        { TextureFormat.ETC2_RGBA1, 16 },
        { TextureFormat.ETC2_RGBA8, 16 },
        { TextureFormat.ASTC_4x4, 16 },
        { TextureFormat.ASTC_5x5, 25 },
        { TextureFormat.ASTC_6x6, 36 },
        { TextureFormat.ASTC_8x8, 64},
        { TextureFormat.ASTC_10x10, 100 },
        { TextureFormat.ASTC_12x12, 144 },
        //[Obsolete("Nintendo 3DS is no longer supported.")]{ TeureFormat.ETC_RGB4_3DS = 60, }
        //[Obsolete("Nintendo 3DS is no longer supported.")]{ TeureFormat.ETC_RGBA8_3DS = 61, }
        { TextureFormat.RG16, 1 },
        { TextureFormat.R8, 1 },
        //{ TextureFormat.ETC_RGB4Crunched, 64 },
        //{ TextureFormat.ETC2_RGBA8Crunched, 65 },
        { TextureFormat.ASTC_HDR_4x4, 16 },
        { TextureFormat.ASTC_HDR_5x5, 25 },
        { TextureFormat.ASTC_HDR_6x6, 36 },
        { TextureFormat.ASTC_HDR_8x8, 64 },
        { TextureFormat.ASTC_HDR_10x10, 100 },
        { TextureFormat.ASTC_HDR_12x12, 144 },
    };

    public static int GetCompressionTileSize(this TextureFormat format)
    {
        if (!texFormatCompressionTileSize.TryGetValue(format, out int tileSize))
        {
            tileSize = 1;
        }
        return tileSize;
    }
}