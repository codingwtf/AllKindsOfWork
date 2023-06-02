using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TerrainSplatUtils
{
    [MenuItem("GameObject/TerrainSplat/Init/256x256", false, 10)]
    static void InitTile256() { InitTile(256); }
    [MenuItem("GameObject/TerrainSplat/Init/512x512", false, 10)]
    static void InitTile512() { InitTile(512); }
    [MenuItem("GameObject/TerrainSplat/Init/1024x1024", false, 10)]
    static void InitTile1024() { InitTile(1024); }
    //[MenuItem("GameObject/TerrainSplat/Init/2048x2048", false, 10)]
    static void InitTile2048() { InitTile(2048); }

    static void InitTile(int indexMapSize)
    {
        string configPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:TexturearrayConfig")[0]);
        TextureArrayConfig config = AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(configPath);
        Texture2DArray albedoArray =
            AssetDatabase.LoadAssetAtPath<Texture2DArray>(
                Path.Combine(Path.GetDirectoryName(configPath), config.name + "_diff_tarray.texarray"));
        Texture2DArray normalArray =
            AssetDatabase.LoadAssetAtPath<Texture2DArray>(
                Path.Combine(Path.GetDirectoryName(configPath), config.name +  "_norm_tarray.texarray"));
        string projectPath = Application.dataPath.Replace("Assets", "");
        string splatmapFolder = config.splatmapFolder;
        string matFolder = config.matFolder;
        foreach (var tile in Selection.gameObjects)
        {
            Material mat = new Material(Shader.Find("TerrainSplat/URP/Lit"));
            Texture2D indexMap = new Texture2D(indexMapSize, indexMapSize, TextureFormat.ARGB32, false);
            Texture2D weightMap = new Texture2D(indexMapSize, indexMapSize, TextureFormat.ARGB32, false);
            Color[] indices = new Color[indexMapSize * indexMapSize];
            Color[] weights = new Color[indexMapSize * indexMapSize];
            for (int pix = 0; pix < weights.Length; ++pix)
            {
                indices[pix] = Color.black;
                weights[pix] = Color.red;
            }
            indexMap.SetPixels(indices);
            indexMap.Apply();
            weightMap.SetPixels(weights);
            weightMap.Apply();
            string indexMapPath = Path.Combine(splatmapFolder, tile.name + "_index.tga");
            string weightMapPath = Path.Combine(splatmapFolder, tile.name + "_weight.tga");
            File.WriteAllBytes(Path.Combine(projectPath, indexMapPath), indexMap.EncodeToTGA());
            File.WriteAllBytes(Path.Combine(projectPath, weightMapPath), weightMap.EncodeToTGA());
            Object.DestroyImmediate(indexMap);
            Object.DestroyImmediate(weightMap);
            AssetDatabase.Refresh();
            TextureImporter indexMapImporter = AssetImporter.GetAtPath(indexMapPath) as TextureImporter;
            {
                indexMapImporter.mipmapEnabled = false;
                indexMapImporter.filterMode = FilterMode.Point;
                indexMapImporter.wrapMode = TextureWrapMode.Clamp;
                indexMapImporter.textureCompression = TextureImporterCompression.Uncompressed;
                indexMapImporter.isReadable = false;
                indexMapImporter.sRGBTexture = false;
                indexMapImporter.SaveAndReimport();
            }
            TextureImporter weightMapImporter = AssetImporter.GetAtPath(weightMapPath) as TextureImporter;
            {
                weightMapImporter.mipmapEnabled = false;
                weightMapImporter.filterMode = FilterMode.Bilinear;
                weightMapImporter.wrapMode = TextureWrapMode.Clamp;
                weightMapImporter.textureCompression = TextureImporterCompression.Uncompressed;
                weightMapImporter.isReadable = false;
                weightMapImporter.sRGBTexture = false;
                weightMapImporter.SaveAndReimport();
            }
            indexMap = AssetDatabase.LoadAssetAtPath<Texture2D>(indexMapPath);
            weightMap = AssetDatabase.LoadAssetAtPath<Texture2D>(weightMapPath);
            mat.SetTexture("_IndexTex", indexMap);
            mat.SetTexture("_WeightTex",  weightMap);
            mat.SetTexture("_MainTexArray", albedoArray);
            mat.SetTexture("_NormalTexArray", normalArray);
            string matPath = Path.Combine(matFolder, tile.name + ".mat");
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.Refresh();
            mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            tile.GetComponent<Renderer>().sharedMaterial = mat;
            if (tile.GetComponent<Collider>() == null)
            {
                tile.AddComponent<MeshCollider>().sharedMesh = tile.GetComponent<MeshFilter>().sharedMesh;
            }
        }
    }
    
}
