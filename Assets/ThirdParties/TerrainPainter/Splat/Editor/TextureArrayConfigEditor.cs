//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using Oddworm.EditorFramework;
using UnityEditor.Build;

[InitializeOnLoad]
[CustomEditor(typeof(TextureArrayConfig))]
public class TextureArrayConfigEditor : Editor
{
    static GUIContent CPlatformOverrides = new GUIContent("Platform Compression Overrides", "Override the compression type on a per platform basis");

    static GUIContent CEmisMetalArray = new GUIContent("Emissive/Metal array", "Create a texture array for emissive and metallic materials");

    static GUIContent CDiffuse = new GUIContent("Diffuse", "Diffuse or Albedo texture");
    static GUIContent CNormal = new GUIContent("Normal", "Normal map");
    static GUIContent CAO = new GUIContent("AO", "Ambient Occlusion map");
    static GUIContent CSmoothness = new GUIContent("Smoothness", "Smoothness map, or roughness map with invert on");
    static GUIContent CHeight = new GUIContent("Height", "Height Map");
    static GUIContent CAlpha = new GUIContent("Alpha", "Alpha Map");
    static GUIContent CNoiseNormal = new GUIContent("Noise Normal", "Normal to bend in over a larger area");
    static GUIContent CDetailNoise = new GUIContent("Detail", "Noise texture to blend in when close");
    static GUIContent CDistanceNoise = new GUIContent("Distance", "Noise texture to blend in when far away");

    void DrawHeader(TextureArrayConfig cfg)
    {
        if (cfg.textureMode != TextureArrayConfig.TextureMode.Basic)
        {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("", GUILayout.Width(30));
        EditorGUILayout.LabelField("Channel to extract:", GUILayout.Width(120));
        EditorGUILayout.EndVertical();
        EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(20));
        EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(64));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(CHeight, GUILayout.Width(64));
        cfg.allTextureChannelHeight = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelHeight, GUILayout.Width(64));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(CSmoothness, GUILayout.Width(64));
        cfg.allTextureChannelSmoothness = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelSmoothness, GUILayout.Width(64));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (cfg.IsAdvancedDetail())
        {
            EditorGUILayout.LabelField(CAlpha, GUILayout.Width(64));
            cfg.allTextureChannelAlpha = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelAlpha, GUILayout.Width(64));
        }
        else
        {         
            EditorGUILayout.LabelField(CAO, GUILayout.Width(64));
            cfg.allTextureChannelAO = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelAO, GUILayout.Width(64));
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        GUILayout.Box(Texture2D.blackTexture, GUILayout.Height(3), GUILayout.ExpandWidth(true));
        }
    }

    void DrawAntiTileEntry(TextureArrayConfig cfg, TextureArrayConfig.TextureEntry e, int i)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField(CNoiseNormal, GUILayout.Width(92));
        e.noiseNormal = (Texture2D)EditorGUILayout.ObjectField(e.noiseNormal, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(CDetailNoise, GUILayout.Width(92));
        e.detailNoise = (Texture2D)EditorGUILayout.ObjectField(e.detailNoise, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
        e.detailChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.detailChannel, GUILayout.Width(64));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(CDistanceNoise, GUILayout.Width(92));
        e.distanceNoise = (Texture2D)EditorGUILayout.ObjectField(e.distanceNoise, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
        e.distanceChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.distanceChannel, GUILayout.Width(64));
        EditorGUILayout.EndVertical();


        EditorGUILayout.EndHorizontal();

    }

    void SwapEntry(TextureArrayConfig cfg, int src, int targ)
    {
        if (src >= 0 && targ >= 0 && src < cfg.sourceTextures.Count && targ < cfg.sourceTextures.Count)
        {
        {
            var s = cfg.sourceTextures[src];
            cfg.sourceTextures[src] = cfg.sourceTextures[targ];
            cfg.sourceTextures[targ] = s;
        }
        if (cfg.sourceTextures2.Count == cfg.sourceTextures.Count)
        {
            var s = cfg.sourceTextures2[src];
            cfg.sourceTextures2[src] = cfg.sourceTextures2[targ];
            cfg.sourceTextures2[targ] = s;
        }
        if (cfg.sourceTextures3.Count == cfg.sourceTextures.Count)
        {
            var s = cfg.sourceTextures3[src];
            cfg.sourceTextures3[src] = cfg.sourceTextures3[targ];
            cfg.sourceTextures3[targ] = s;
        }

        }
         
    }

    bool DrawTextureEntry(TextureArrayConfig cfg, TextureArrayConfig.TextureEntry e, int i, bool controls = true)
    {
        bool ret = false;
        if (controls)
        {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));

               
        if (e.HasTextures())
        {
               
            EditorGUILayout.LabelField(e.diffuse != null ? e.diffuse.name : "empty");
            ret = GUILayout.Button("Clear Entry");
        }
        else
        {
//            EditorGUILayout.HelpBox("Removing an entry completely can cause texture choices to change on existing terrains. You can leave it blank to preserve the texture order and MicroSplat will put a dummy texture into the array.", MessageType.Warning);
            ret = (GUILayout.Button("Delete Entry"));
        }

        if (GUILayout.Button("↑", GUILayout.Width(40)))
        {
            SwapEntry(cfg, i, i - 1);
        }
        if (GUILayout.Button("↓", GUILayout.Width(40)))
        {
            SwapEntry(cfg, i, i + 1);
        }
        EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        if (cfg.textureMode == TextureArrayConfig.TextureMode.PBR)
        {
#if !UNITY_2017_3_OR_NEWER
        EditorGUILayout.BeginVertical();
        if (controls)
        {
            EditorGUILayout.LabelField(new GUIContent("Substance"), GUILayout.Width(64));
        }
        e.substance = (ProceduralMaterial)EditorGUILayout.ObjectField(e.substance, typeof(ProceduralMaterial), false, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.EndVertical();
#endif
#if SUBSTANCE_PLUGIN_ENABLED
        EditorGUILayout.BeginVertical();
        if (controls)
        {
            EditorGUILayout.LabelField(new GUIContent("Substance"), GUILayout.Width(64));
        }
        e.substance = (Substance.Game.Substance)EditorGUILayout.ObjectField(e.substance, typeof(Substance.Game.Substance), false, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.EndVertical();
#endif
        }

        EditorGUILayout.BeginVertical();
        if (controls)
        {
        EditorGUILayout.LabelField(CDiffuse, GUILayout.Width(64));
        }
        e.diffuse = (Texture2D)EditorGUILayout.ObjectField(e.diffuse, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (controls)
        {
        EditorGUILayout.LabelField(CNormal, GUILayout.Width(64));
        }
        e.normal = (Texture2D)EditorGUILayout.ObjectField(e.normal, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.EndVertical();

        if (cfg.textureMode == TextureArrayConfig.TextureMode.PBR || cfg.IsAdvancedDetail())
        {
        EditorGUILayout.BeginVertical();
        if (controls)
        {
            EditorGUILayout.LabelField(CHeight, GUILayout.Width(64));
        }
        e.height = (Texture2D)EditorGUILayout.ObjectField(e.height, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
        if (cfg.allTextureChannelHeight == TextureArrayConfig.AllTextureChannel.Custom)
        {
            e.heightChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.heightChannel, GUILayout.Width(64));
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (controls)
        {
            EditorGUILayout.LabelField(CSmoothness, GUILayout.Width(64));
        }
        e.smoothness = (Texture2D)EditorGUILayout.ObjectField(e.smoothness, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
        if (cfg.allTextureChannelSmoothness == TextureArrayConfig.AllTextureChannel.Custom)
        {
            e.smoothnessChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.smoothnessChannel, GUILayout.Width(64));
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Invert", GUILayout.Width(44));
        e.isRoughness = EditorGUILayout.Toggle(e.isRoughness, GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (cfg.IsAdvancedDetail())
        {
            if (controls)
            {
                EditorGUILayout.LabelField(CAlpha, GUILayout.Width(64));
            }
            e.alpha = (Texture2D)EditorGUILayout.ObjectField(e.alpha, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
            if (cfg.allTextureChannelAlpha == TextureArrayConfig.AllTextureChannel.Custom)
            {
                e.alphaChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.alphaChannel, GUILayout.Width(64));
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Normalize", GUILayout.Width(64));
            e.normalizeAlpha = EditorGUILayout.Toggle(e.normalizeAlpha, GUILayout.Width(44));
            EditorGUILayout.EndHorizontal();               
        }
        else
        {
            if (controls)
            {
                EditorGUILayout.LabelField(CAO, GUILayout.Width(64));
            }
            e.ao = (Texture2D)EditorGUILayout.ObjectField(e.ao, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
            if (cfg.allTextureChannelAO == TextureArrayConfig.AllTextureChannel.Custom)
            {
                e.aoChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.aoChannel, GUILayout.Width(64));
            }
        }
        EditorGUILayout.EndVertical();

        if (!cfg.IsAdvancedDetail() && cfg.emisMetalArray)
        {
            EditorGUILayout.BeginVertical();
            if (controls)
            {
                EditorGUILayout.LabelField(new GUIContent("Emissive"), GUILayout.Width(64));
            }
            e.emis = (Texture2D)EditorGUILayout.ObjectField(e.emis, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (controls)
            {
                EditorGUILayout.LabelField(new GUIContent("Metal"), GUILayout.Width(64));
            }
            e.metal = (Texture2D)EditorGUILayout.ObjectField(e.metal, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
            e.metalChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.metalChannel, GUILayout.Width(64));

            EditorGUILayout.EndVertical();
        }
        }
        EditorGUILayout.EndHorizontal();
        //e.tiling = Mathf.Max(float.Epsilon, EditorGUILayout.FloatField("Tiling", e.tiling));

        return ret;
    }

    public static bool GetFromTerrain(TextureArrayConfig cfg, Terrain t)
    {
        if (t != null && cfg.sourceTextures.Count == 0 && t.terrainData != null)
        {
#if UNITY_2018_3_OR_NEWER
        int count = t.terrainData.terrainLayers.Length;
        for (int i = 0; i < count; ++i)
        {
            // Metalic, AO, Height, Smooth
            var proto = t.terrainData.terrainLayers[i];
            var e = new TextureArrayConfig.TextureEntry();
            e.diffuse = proto.diffuseTexture;
            e.normal = proto.normalMapTexture;
            e.metal = proto.maskMapTexture;
            e.metalChannel = TextureArrayConfig.TextureChannel.R;
            e.height = proto.maskMapTexture;
            e.heightChannel = TextureArrayConfig.TextureChannel.B;
            e.smoothness = proto.maskMapTexture;
            e.smoothnessChannel = TextureArrayConfig.TextureChannel.A;
            e.ao = proto.maskMapTexture;
            e.aoChannel = TextureArrayConfig.TextureChannel.G;
            cfg.sourceTextures.Add(e);
        }
        return true;
#else
        int count = t.terrainData.splatPrototypes.Length;
        for (int i = 0; i < count; ++i)
        {
            var proto = t.terrainData.splatPrototypes[i];
            var e = new TextureArrayConfig.TextureEntry();
            e.diffuse = proto.texture;
            e.normal = proto.normalMap;
            cfg.sourceTextures.Add(e);
        }
        return true;
#endif
        }
        return false;
    }

    void Remove(TextureArrayConfig cfg, int i)
    {
        cfg.sourceTextures.RemoveAt(i);
        cfg.sourceTextures2.RemoveAt(i);
        cfg.sourceTextures3.RemoveAt(i);
    }

    void Reset(TextureArrayConfig cfg, int i)
    {
        cfg.sourceTextures[i].Reset();
        cfg.sourceTextures2[i].Reset();
        cfg.sourceTextures3[i].Reset();
    }

    static void MatchArrayLength(TextureArrayConfig cfg)
    {
        int srcCount = cfg.sourceTextures.Count;
        bool change = false;
        while (cfg.sourceTextures2.Count < srcCount)
        {
        var entry = new TextureArrayConfig.TextureEntry();
        entry.aoChannel = cfg.sourceTextures[0].aoChannel;
        entry.heightChannel = cfg.sourceTextures[0].heightChannel;
        entry.smoothnessChannel = cfg.sourceTextures[0].smoothnessChannel;
        cfg.sourceTextures2.Add(entry);
        change = true;
        }

        while (cfg.sourceTextures3.Count < srcCount)
        {
        var entry = new TextureArrayConfig.TextureEntry();
        entry.aoChannel = cfg.sourceTextures[0].aoChannel;
        entry.heightChannel = cfg.sourceTextures[0].heightChannel;
        entry.smoothnessChannel = cfg.sourceTextures[0].smoothnessChannel;
        cfg.sourceTextures3.Add(entry);
        change = true;
        }

        while (cfg.sourceTextures2.Count > srcCount)
        {
        cfg.sourceTextures2.RemoveAt(cfg.sourceTextures2.Count - 1);
        change = true;
        }
        while (cfg.sourceTextures3.Count > srcCount)
        {
        cfg.sourceTextures3.RemoveAt(cfg.sourceTextures3.Count - 1);
        change = true;
        }
        if (change)
        {
        EditorUtility.SetDirty(cfg);
        }
    }

    void DrawOverrideGUI(TextureArrayConfig cfg)
    {
        //var prop = serializedObject.FindProperty("platformOverrides");
        //EditorGUILayout.PropertyField(prop, CPlatformOverrides, true);
    }

    public override void OnInspectorGUI()
    {
        var cfg = target as TextureArrayConfig;
        serializedObject.Update();
        MatchArrayLength(cfg);
        EditorGUI.BeginChangeCheck();
        //cfg.textureMode = (TextureArrayConfig.TextureMode)EditorGUILayout.EnumPopup(CTextureMode, cfg.textureMode);
        //cfg.packingQuality = (TextureArrayConfig.PackingQuality)EditorGUILayout.EnumPopup(CPackingMode, cfg.packingQuality);
//        cfg.sourceTextureSize = (TextureArrayConfig.SourceTextureSize)EditorGUILayout.EnumPopup(CSourceTextureSize, cfg.sourceTextureSize);
//        cfg.atlasDilation = EditorGUILayout.IntField("Atlas Dilation", cfg.atlasDilation);
        cfg.atlasLayout = (TextureArrayConfig.AtlasLayout)EditorGUILayout.EnumPopup("Atlas Layout", cfg.atlasLayout);
        cfg.packingMode = (TextureArrayConfig.PackingMode)EditorGUILayout.EnumPopup("Packing Mode", cfg.packingMode);

        cfg.splatmapFolder = EditorGUILayout.TextField("Splatmap Folder", cfg.splatmapFolder);
        cfg.matFolder = EditorGUILayout.TextField("Material Folder", cfg.matFolder);

        if (cfg.IsAdvancedDetail())
        {
        cfg.clusterMode = TextureArrayConfig.ClusterMode.None;
        }
        else
        {
        #if __MICROSPLAT_DETAILRESAMPLE__
        cfg.antiTileArray = EditorGUILayout.Toggle(CAntiTileArray, cfg.antiTileArray);
        #endif

        if (cfg.textureMode == TextureArrayConfig.TextureMode.PBR)
        {
            cfg.emisMetalArray = EditorGUILayout.Toggle(CEmisMetalArray, cfg.emisMetalArray);
        }

        #if __MICROSPLAT_TEXTURECLUSTERS__
        if (!cfg.IsAdvancedDetail())
        {
            cfg.clusterMode = (TextureArrayConfig.ClusterMode)EditorGUILayout.EnumPopup(CClusterMode, cfg.clusterMode);
        }
        #endif
        }

        var root = serializedObject.FindProperty("defaultTextureSettings");

        //EditorGUILayout.PropertyField(root);
        if (root.isExpanded)
        {
        EditorGUI.indentLevel++;
        //EditorGUILayout.PropertyField(root.FindPropertyRelative("diffuseSettings"), true);
        //EditorGUILayout.PropertyField(root.FindPropertyRelative("normalSettings"), true);

        if (cfg.textureMode != TextureArrayConfig.TextureMode.Basic)
        {
            if (cfg.packingQuality == TextureArrayConfig.PackingQuality.Quality)
            {
                EditorGUILayout.PropertyField(root.FindPropertyRelative("smoothSettings"), true);
            }
            if (cfg.antiTileArray)
            {
                EditorGUILayout.PropertyField(root.FindPropertyRelative("antiTileSettings"), true);
            }
            if (cfg.emisMetalArray)
            {
                EditorGUILayout.PropertyField(root.FindPropertyRelative("emissiveSettings"), true);
            }
        }
        else 
        {
            EditorGUILayout.HelpBox("Select PBR mode to provide custom height, smoothness, and ao textures to greatly increase quality!", MessageType.Info);
        }

        EditorGUI.indentLevel--;
        }

        DrawOverrideGUI(cfg);


        //if (MicroSplatUtilities.DrawRollup("Textures", true))
        {
            //EditorGUILayout.HelpBox("Don't have a normal map? Any missing textures will be generated automatically from the best available source texture", MessageType.Info);
            bool disableClusters = cfg.IsAdvancedDetail();
            DrawHeader(cfg);
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    bool remove = (DrawTextureEntry(cfg, cfg.sourceTextures[i], i));


                    if (cfg.clusterMode != TextureArrayConfig.ClusterMode.None && !disableClusters)
                    {
                        DrawTextureEntry(cfg, cfg.sourceTextures2[i], i, false);
                    }
                    if (cfg.clusterMode == TextureArrayConfig.ClusterMode.ThreeVariations && !disableClusters)
                    {
                        DrawTextureEntry(cfg, cfg.sourceTextures3[i], i, false);
                    }
                    if (remove)
                    {
                        var e = cfg.sourceTextures[i];
                        if (!e.HasTextures())
                        {
                        Remove(cfg, i);
                        i--;
                        }
                        else
                        {
                        Reset(cfg, i);
                        }
                    }

                    if (cfg.antiTileArray)
                    {
                        DrawAntiTileEntry(cfg, cfg.sourceTextures[i], i);
                    }

                    GUILayout.Box(Texture2D.blackTexture, GUILayout.Height(3), GUILayout.ExpandWidth(true));
                }
            }
            if (GUILayout.Button("Add Texture"))
            {
                if (cfg.sourceTextures.Count > 0)
                {
                    var entry = new TextureArrayConfig.TextureEntry();
                    entry.aoChannel = cfg.sourceTextures[0].aoChannel;
                    entry.heightChannel = cfg.sourceTextures[0].heightChannel;
                    entry.smoothnessChannel = cfg.sourceTextures[0].smoothnessChannel;
                    entry.alphaChannel = cfg.sourceTextures[0].alphaChannel;
                    cfg.sourceTextures.Add(entry);
                }
                else
                {
                    var entry = new TextureArrayConfig.TextureEntry();
                    entry.aoChannel = TextureArrayConfig.TextureChannel.G;
                    entry.heightChannel = TextureArrayConfig.TextureChannel.G;
                    entry.smoothnessChannel = TextureArrayConfig.TextureChannel.G;
                    entry.alphaChannel = TextureArrayConfig.TextureChannel.G;
                    cfg.sourceTextures.Add(entry);
                }
            }
        }
//         if (GUILayout.Button("Grab From Scene Terrain"))
//         {
//            cfg.sourceTextures.Clear();
//            GetFromTerrain(cfg);
//         }
        if (GUILayout.Button("Create"))
        {
            staticConfig = cfg;
            if (cfg.packingMode == TextureArrayConfig.PackingMode.Array)
            {
                CreateArray(cfg);
            }
            else
            {
                CreateAtlas(cfg);
            }
            serializedObject.ApplyModifiedProperties();
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(cfg);
        }
    }

    static Texture2D ResizeTexture(Texture2D source, int width, int height, bool linear)
    {
        RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
        rt.DiscardContents();
        GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear) && !linear;
        Graphics.Blit(source, rt);
        GL.sRGBWrite = false;
        RenderTexture.active = rt;
        Texture2D ret = new Texture2D(width, height, TextureFormat.ARGB32, true, linear);
        ret.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        ret.Apply(true);
        RenderTexture.active = null;
        rt.Release();
        DestroyImmediate(rt);
        return ret;
    }

    public static TextureFormat GetTextureFormat(TextureArrayConfig cfg, TextureArrayConfig.Compression cmp)
    {
        if (cmp == TextureArrayConfig.Compression.ForceETC2)
        {
        return TextureFormat.ETC2_RGBA8;
        }
        else if (cmp == TextureArrayConfig.Compression.ForcePVR)
        {
        return TextureFormat.PVRTC_RGBA4;
        }
        else if (cmp == TextureArrayConfig.Compression.ForceASTC)
        {
        return TextureFormat.ASTC_4x4;
        }
        else if (cmp == TextureArrayConfig.Compression.ForceDXT)
        {
        return TextureFormat.DXT5;
        }
        else if (cmp == TextureArrayConfig.Compression.ForceCrunch)
        {
        return TextureFormat.DXT5Crunched;
        }
        else if (cmp == TextureArrayConfig.Compression.Uncompressed)
        {
        return TextureFormat.ARGB32;
        }
        var platform = EditorUserBuildSettings.activeBuildTarget;
        if (platform == BuildTarget.Android)
        {
        return TextureFormat.ETC2_RGBA8;
        }
        else if (platform == BuildTarget.iOS)
        {
        return TextureFormat.PVRTC_RGBA4;
        }
        else
        {
        return TextureFormat.DXT5;
        }
    }

    static Texture2D RenderMissingTexture(Texture2D src, string shaderPath, int width, int height, int channel = -1)
    {
        Texture2D res = new Texture2D(width, height, TextureFormat.ARGB32, true, true);
        RenderTexture resRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        resRT.DiscardContents();
        Shader s = Shader.Find(shaderPath);
        if (s == null)
        {
        Debug.LogError("Could not find shader " + shaderPath);
        res.Apply();
        return res;
        }
        Material genMat = new Material(Shader.Find(shaderPath));
        if (channel >= 0)
        {
        genMat.SetInt("_Channel", channel);
        }

        GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);
        Graphics.Blit(src, resRT, genMat);
        GL.sRGBWrite = false;

        RenderTexture.active = resRT;
        res.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        res.Apply();
        RenderTexture.active = null;
        resRT.Release();
        DestroyImmediate(resRT);
        DestroyImmediate(genMat);
        return res;
    }

    static TextureArrayConfig staticConfig;

    static string GetAtlasPath(TextureArrayConfig cfg)
    {
        string path = AssetDatabase.GetAssetPath(cfg);
        // create array path
        path = path.Replace("\\", "/");
    return path;
    }
    static string GetDiffPath(TextureArrayConfig cfg, string ext)
    {
        string path = AssetDatabase.GetAssetPath(cfg);
        // create array path
        path = path.Replace("\\", "/");
        return path.Replace(".asset", "_diff" + ext + "_tarray.texarray");
    }

    static string GetNormPath(TextureArrayConfig cfg, string ext)
    {
        string path = AssetDatabase.GetAssetPath(cfg);
        // create array path
        path = path.Replace("\\", "/");
        return path.Replace(".asset", "_norm" + ext + "_tarray.texarray");
    }

    static int SizeToMipCount(int size)
    {
        int mips = 11;
        if (size == 4096)
        mips = 13;
        else if (size == 2048)
        mips = 12;
        else if (size == 1024)
        mips = 11;
        else if (size == 512)
        mips = 10;
        else if (size == 256)
        mips = 9;
        else if (size == 128)
        mips = 8;
        else if (size == 64)
        mips = 7;
        else if (size == 32)
        mips = 6;
        return mips;
    }

    static void ShrinkSourceTexture(Texture2D tex, TextureArrayConfig.SourceTextureSize stz)
    {
        if (tex != null)
        {
        AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
        TextureImporter ti = (TextureImporter)ai;
        if (ti != null && ti.maxTextureSize != (int)stz)
        {
            ti.maxTextureSize = (int)stz;
            ti.SaveAndReimport();
        }
        }
    }

    static void ShrinkSourceTextures(List<TextureArrayConfig.TextureEntry> textures, TextureArrayConfig.SourceTextureSize stz)
    {
        if (textures == null)
        return;
        if (stz == TextureArrayConfig.SourceTextureSize.Unchanged)
        return;
        foreach (var t in textures)
        {
        ShrinkSourceTexture(t.ao, stz);
        ShrinkSourceTexture(t.alpha, stz);
        ShrinkSourceTexture(t.diffuse, stz);
        ShrinkSourceTexture(t.distanceNoise, stz);
        ShrinkSourceTexture(t.normal, stz);
        ShrinkSourceTexture(t.noiseNormal, stz);
        ShrinkSourceTexture(t.detailNoise, stz);
        ShrinkSourceTexture(t.emis, stz);
        ShrinkSourceTexture(t.height, stz);
        ShrinkSourceTexture(t.metal, stz);
        ShrinkSourceTexture(t.smoothness, stz);
        }
    }
         

    static void RestoreSourceTexture(Texture2D tex)
    {
        if (tex != null)
        {
        AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
        TextureImporter ti = ai as TextureImporter;
        if (ti != null && ti.maxTextureSize <= 256)
        {
            ti.maxTextureSize = 4096;
            ti.SaveAndReimport();
        }

        }
    }

    static void RestoreSourceTextures(List<TextureArrayConfig.TextureEntry> textures, TextureArrayConfig.SourceTextureSize stz)
    {
        if (textures == null)
        return;

        foreach (var t in textures)
        {
        RestoreSourceTexture(t.ao);
        RestoreSourceTexture(t.alpha);
        RestoreSourceTexture(t.diffuse);
        RestoreSourceTexture(t.distanceNoise);
        RestoreSourceTexture(t.normal);
        RestoreSourceTexture(t.noiseNormal);
        RestoreSourceTexture(t.detailNoise);
        RestoreSourceTexture(t.emis);
        RestoreSourceTexture(t.height);
        RestoreSourceTexture(t.metal);
        RestoreSourceTexture(t.smoothness);
        }
    }

    public static TextureArrayConfig.TextureArrayGroup GetSettingsGroup(TextureArrayConfig cfg, BuildTarget t)
    { 
        foreach (var g in cfg.platformOverrides)
        {
        if (g.platform == t)
        {
            return g.settings;
        }
        }
        return cfg.defaultTextureSettings;
    }

    static void CompileConfig(TextureArrayConfig cfg, 
        List<TextureArrayConfig.TextureEntry> src,
        string ext, 
        bool isCluster = false,
        bool isAtlas = false)
    {

        RestoreSourceTextures(src, cfg.sourceTextureSize);

        bool diffuseIsLinear = false;

        var settings = GetSettingsGroup(cfg, UnityEditor.EditorUserBuildSettings.activeBuildTarget);


        int diffuseWidth =   (int)settings.diffuseSettings.textureSize;
        int diffuseHeight =  (int)settings.diffuseSettings.textureSize;
        int normalWidth =    (int)settings.normalSettings.textureSize;
        int normalHeight =   (int)settings.normalSettings.textureSize;

        int diffuseAnisoLevel = settings.diffuseSettings.Aniso;
        int normalAnisoLevel = settings.normalSettings.Aniso;

        FilterMode diffuseFilter = settings.diffuseSettings.filterMode;
        FilterMode normalFilter = settings.normalSettings.filterMode;

        int diffuseMipCount = SizeToMipCount(diffuseWidth);
        int normalMipCount = SizeToMipCount(normalWidth);

        int texCount = src.Count;
        if (texCount < 1)
        texCount = 1;

        //array 
        Texture2DArray diffuseArray = null;
        Texture2DArray normalSAOArray = null;
        //atlas
        int COLUMN_AND_ROW_COUNT = 0;
        int texSize = 0;
        int dilation = 0;
        int AtlasResolution = 0;

        if (isAtlas)
        {
            COLUMN_AND_ROW_COUNT = (int)cfg.atlasLayout;
            AtlasResolution = diffuseWidth * COLUMN_AND_ROW_COUNT;
            dilation = cfg.atlasDilation;
            texSize = diffuseWidth;
            if (src.Count > COLUMN_AND_ROW_COUNT * COLUMN_AND_ROW_COUNT)
            {
                EditorUtility.DisplayDialog("Error", "Source textures count exceeds atlas cell count", "OK");
                return;
            }

            RenderTexture rtArray = RenderTexture.GetTemporary(texSize, texSize, 24);
            Shader shader = Shader.Find("Hidden/UnlitShader");
            Material material = new Material(shader);

            cfg.diffuseHeightAtlas = new Texture2D(AtlasResolution, AtlasResolution, TextureFormat.ARGB32, false);
            cfg.normalSAOAtlas = new Texture2D(AtlasResolution, AtlasResolution, TextureFormat.ARGB32, false);
        }
        else
        { 
            // diffuse
            diffuseArray = new Texture2DArray(diffuseWidth, diffuseHeight, texCount,
            GetTextureFormat(cfg, settings.diffuseSettings.compression),
            true, diffuseIsLinear);

            diffuseArray.wrapMode = TextureWrapMode.Repeat;
            diffuseArray.filterMode = diffuseFilter;
            diffuseArray.anisoLevel = diffuseAnisoLevel;


            // normal
            var nmlcomp = GetTextureFormat(cfg, settings.normalSettings.compression);
            normalSAOArray = new Texture2DArray(normalWidth, normalHeight, texCount, nmlcomp, true, true);

            normalSAOArray.wrapMode = TextureWrapMode.Repeat;
            normalSAOArray.filterMode = normalFilter;
            normalSAOArray.anisoLevel = normalAnisoLevel;
        }


        for (int i = 0; i < src.Count; ++i)
        {
        try
        {
            EditorUtility.DisplayProgressBar("Packing textures...", "", (float)i / (float)src.Count);

            // first, generate any missing data. We generate a full NSAO map from diffuse or height map
            // if no height map is provided, we then generate it from the resulting or supplied normal. 
            var e = src[i];
            Texture2D diffuse = e.diffuse;
            if (diffuse == null)
            {
                diffuse = Texture2D.whiteTexture;
            }

            // resulting maps
            Texture2D diffuseHeightTex = ResizeTexture(diffuse, diffuseWidth, diffuseHeight, diffuseIsLinear);
            Texture2D normalSAOTex = null;

            // copy, but go ahead and generate other channels in case they aren't provided later.
            normalSAOTex = RenderMissingTexture(e.normal, "Unlit/Texture", normalWidth, normalHeight);

            if (cfg.packingQuality == TextureArrayConfig.PackingQuality.Quality)
            {
                // clear non-normal data to help compression quality
                Color[] cols = normalSAOTex.GetPixels();
                /*for (int x = 0; x < cols.Length; ++x)
                {
                    Color c = cols[x];
                    c.r = 0;
                    c.b = 0;
                    cols[x] = c;
                }*/
                normalSAOTex.SetPixels(cols);
            }

#if UNITY_2018_3_OR_NEWER
            int tq = (int)UnityEditor.TextureCompressionQuality.Normal;
#else
            var tq = UnityEngine.TextureCompressionQuality.Normal;
#endif

            if (settings.normalSettings.compression != TextureArrayConfig.Compression.Uncompressed)
            {
                EditorUtility.CompressTexture(normalSAOTex, GetTextureFormat(cfg, settings.normalSettings.compression), tq);
            }

            if (settings.diffuseSettings.compression != TextureArrayConfig.Compression.Uncompressed)
            {
                EditorUtility.CompressTexture(diffuseHeightTex, GetTextureFormat(cfg, settings.diffuseSettings.compression), tq);
            }

            normalSAOTex.Apply();
            diffuseHeightTex.Apply();

            if (isAtlas)
            {
                int columnNum = i % COLUMN_AND_ROW_COUNT;
                int rowNum = (i % (COLUMN_AND_ROW_COUNT * COLUMN_AND_ROW_COUNT)) / COLUMN_AND_ROW_COUNT;
                int startWidth = columnNum * texSize;
                int startHeight = rowNum * texSize;
                for (int j = 0; j < texSize; j++)
                {
                    for (int k = 0; k < texSize; k++)
                    {
                        Color color = GetPixelColor(j, k, diffuseHeightTex, AtlasResolution, dilation, COLUMN_AND_ROW_COUNT);
                        cfg.diffuseHeightAtlas.SetPixel(startWidth + j, startHeight + k, color);

                        Color normalColor = (e.normal == null) ? new Color(0.5f, 0.5f, 1) : GetPixelColor(j, k, normalSAOTex, AtlasResolution, dilation, COLUMN_AND_ROW_COUNT);
                        cfg.normalSAOAtlas.SetPixel(startWidth + j, startHeight + k, normalColor);
                    }
                }
            }
            else
            {
                if (diffuseArray != null)
                {
                    for (int mip = 0; mip < diffuseMipCount; ++mip)
                    {
                        Graphics.CopyTexture(diffuseHeightTex, 0, mip, diffuseArray, i, mip);
                    }
                }
                if (normalSAOArray != null)
                {
                    for (int mip = 0; mip < normalMipCount; ++mip)
                    {
                        Graphics.CopyTexture(normalSAOTex, 0, mip, normalSAOArray, i, mip);
                    }
                }
            }


            DestroyImmediate(diffuseHeightTex);
            DestroyImmediate(normalSAOTex);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        }
        EditorUtility.ClearProgressBar();

        if (isAtlas)
        {
            cfg.diffuseHeightAtlas.Apply();
            byte[] bytes = cfg.diffuseHeightAtlas.EncodeToTGA();
            string atlasDir = GetAtlasPath(cfg);
            string diffuseAtlasPath = atlasDir.Replace(".asset", "_diff_atlas.tga");
            File.WriteAllBytes(diffuseAtlasPath, bytes);
            AssetDatabase.ImportAsset(diffuseAtlasPath, ImportAssetOptions.ForceUpdate);
            cfg.diffuseHeightAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(diffuseAtlasPath);

            cfg.normalSAOAtlas.Apply();
            byte[] normalBytes = cfg.normalSAOAtlas.EncodeToTGA();
            string normalAtlasPath = atlasDir.Replace(".asset", "_norm_atlas.tga");
            File.WriteAllBytes(normalAtlasPath, normalBytes);
            AssetDatabase.ImportAsset(normalAtlasPath, ImportAssetOptions.ForceUpdate);
            cfg.normalSAOAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(normalAtlasPath);

            if (cfg.smoothAOAtlas != null)
            {
                cfg.smoothAOAtlas.Apply();
                byte[] SAOBytes = cfg.smoothAOAtlas.EncodeToTGA();
                string saoAtlasPath = atlasDir.Replace(".asset", "_sao_atlas.tga");
                File.WriteAllBytes(saoAtlasPath, SAOBytes);
                AssetDatabase.ImportAsset(saoAtlasPath, ImportAssetOptions.ForceUpdate);
                cfg.smoothAOAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(saoAtlasPath);
            }
            if (cfg.emisAtlas != null)
            {
                cfg.emisAtlas.Apply();
                byte[] emisBytes = cfg.emisAtlas.EncodeToTGA();
                string emisAtlasPath = atlasDir.Replace(".asset", "_emis_atlas.tga");
                File.WriteAllBytes(emisAtlasPath, emisBytes);
                AssetDatabase.ImportAsset(emisAtlasPath, ImportAssetOptions.ForceUpdate);
                cfg.emisAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(emisAtlasPath);
            }
        }
        else
        {
            diffuseArray.Apply(false, true);
            normalSAOArray.Apply(false, true);

            string diffPath = GetDiffPath(cfg, ext);
            string normSAOPath = GetNormPath(cfg, ext);
            {
                var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(diffPath);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(diffuseArray, existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(diffuseArray, diffPath);
                }
            }

            {
                var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(normSAOPath);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(normalSAOArray, existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(normalSAOArray, normSAOPath);
                }
            }
        }

        EditorUtility.SetDirty(cfg);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        ShrinkSourceTextures(src, cfg.sourceTextureSize);
    }

    public static void CreateArray(TextureArrayConfig cfg)
    {
        Texture2D[] diffuses = cfg.sourceTextures.Select(src => src.diffuse).ToArray();
        Texture2D[] normals = cfg.sourceTextures.Select(src => src.normal).ToArray();
        string diffPath = GetDiffPath(cfg, "");
        string normPath = GetNormPath(cfg, "");
        string dataPath = Application.dataPath.Replace(@"/Assets", "");
        if(File.Exists(Path.Combine(dataPath, diffPath)))
            File.Delete(Path.Combine(dataPath, diffPath));
        if(File.Exists(Path.Combine(dataPath, normPath)))
            File.Delete(Path.Combine(dataPath, normPath));
        AssetDatabase.CreateAsset(new Texture2DArray(1, 1, 1, TextureFormat.RGB24, false), diffPath);
        AssetDatabase.CreateAsset(new Texture2DArray(1, 1, 1, TextureFormat.RGB24, false), normPath);
        var diffImporter = AssetImporter.GetAtPath(diffPath) as Texture2DArrayImporter;
        var normImporter = AssetImporter.GetAtPath(normPath) as Texture2DArrayImporter;
        diffImporter.textures = diffuses;
        normImporter.textures = normals;
        EditorUtility.SetDirty(diffImporter);
        EditorUtility.SetDirty(normImporter);
        diffImporter.SaveAndReimport();
        normImporter.SaveAndReimport();
        AssetDatabase.Refresh();
    }

    void CreateAtlas(TextureArrayConfig cfg)
    {
        MatchArrayLength(cfg);

        CompileConfig(cfg, cfg.sourceTextures, "", false, true);

        cfg.diffuseArray = null;
        cfg.normalSAOArray = null;
        cfg.smoothAOArray = null;
        cfg.emisArray = null;
        

        EditorUtility.SetDirty(cfg);
        if (!TextureArrayPreProcessor.sIsPostProcessing)
        {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        Debug.Log("Atlas Exported: " + AssetDatabase.GetAssetPath(cfg.diffuseHeightAtlas));
    }

    static Color GetPixelColor(int rowNum, int columnNum, Texture2D oriTex, int atlasDim, int dilation, int gridSize)
    {

        int COLUMN_AND_ROW_COUNT = gridSize;

        Color oriColor;
        int minNum = dilation;
        int maxNum = atlasDim / COLUMN_AND_ROW_COUNT - minNum + 1;

        if (rowNum <= minNum && columnNum <= minNum)
        {
            oriColor = oriTex.GetPixel(minNum, minNum);
        }
        else if (rowNum <= minNum && columnNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(minNum, maxNum);
        }
        else if (rowNum >= maxNum && columnNum <= minNum)
        {
            oriColor = oriTex.GetPixel(maxNum, minNum);
        }
        else if (rowNum >= maxNum && columnNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(maxNum, maxNum);
        }

        else if (rowNum <= minNum)
        {
            oriColor = oriTex.GetPixel(minNum, columnNum);
        }
        else if (rowNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(maxNum, columnNum);
        }
        else if (columnNum <= minNum)
        {
            oriColor = oriTex.GetPixel(rowNum, minNum);
        }
        else if (columnNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(rowNum, maxNum);
        }

        else
        {
            oriColor = oriTex.GetPixel(rowNum, columnNum);
        }
    //        oriColor = oriTex.GetPixel(rowNum, columnNum);
        return oriColor;
    }

    public class ActiveBuildTargetListener : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;
        public void OnActiveBuildTargetChanged(BuildTarget prevTarget, BuildTarget newTarget)
        {
            foreach (var cfg in AssetDatabase.FindAssets("t:TextureArrayConfig")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(path)))
            {
                CreateArray(cfg);
            }
        }
    }
}