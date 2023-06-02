using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UMP.Collections;
using UMP.Utils;

namespace UMP
{
    public class TerrainSplat : MeshPainter
    {
        protected override Shader shader => Shader.Find("Hidden/OTP/Painter/TerrainSplat");
        protected override IRandomAccessEnumerable<int, Brush> brushes
        {
            get
            {
                if (m_brushes == null)
                {
                    Brush[] brushArray = new Brush[]
                    {
                        new AlphaBrush(),
                        new RampedSDFBrush()
                    };
                    m_brushes = new RandomAccessCollection<Brush>(brushArray);
                }
                return m_brushes;
            }
        }
        IRandomAccessEnumerable<int, Brush> m_brushes;

        private static TextureArrayConfig layerConfig
        {
            get => m_layerConfig;
            set
            {
                if (value != m_layerConfig)
                {
                    m_previewTextures = null;
                    m_layerConfig = value;
                }
            }
            
        }
        private static TextureArrayConfig m_layerConfig = null;

        static private Texture[] previewTextures
        {
            get
            {
                if (m_previewTextures == null)
                {
                    if (layerConfig == null)
                    {
                        m_previewTextures = new Texture[0];
                    }
                    else
                    {
                        Texture2D[] textures = layerConfig.sourceTextures.Select(tex => tex.diffuse).ToArray();
                        for (int ti = 0; ti < textures.Length; ++ti)
                        {
                            Texture2D src = textures[ti];
                            int width = src.width / 2;
                            int height = src.height / 2;
                            Texture2D preview = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                            Graphics.ConvertTexture(src, preview);
                            preview.ReadPixels(new Rect(0, 0, preview.width, preview.height), 0, 0);
                            Color[] pixels = preview.GetPixels();
                            for (int i = 0; i < pixels.Length; ++i)
                            {
                                pixels[i].a = 1;
                            }
                            preview.SetPixels(pixels);
                            preview.Apply();
                            textures[ti] = preview;
                        }

                        m_previewTextures = textures;
                    }
                }
                return m_previewTextures;
            }
        }
        private static Texture[] m_previewTextures = null;

        static int selectedLayer = 0;

        [MenuItem("Tools/TerrainPainter/Terrain Splat Painter")]
        static void Entry()
        {
            var window = EditorWindow.GetWindow<TerrainSplat>();
            window.titleContent = new GUIContent(typeof(TerrainSplat).Name);
            window.position = new Rect(0, 0, 256, 512);
            window.Show();
        }

        protected override MeshTile GetTile(GameObject gameObject)
        {
            MeshTile tile = null;
            if (!loadedTiles.TryGetValue(gameObject, out tile))
            {
                tile = MeshTile.CreateTile<TerrainSplatTile>(gameObject);
                tile.BeginEdit();
                loadedTiles.Add(gameObject, tile);
            }
            return tile;
        }

        protected override bool ColliderFilter(Collider collider)
        {
            var shader = collider.gameObject.GetComponent<Renderer>()?.sharedMaterial?.shader;
            return shader == null ? false : shader.name.Contains("TerrainSplat");
        }

        float iconSize = 50f;
        protected override void PaletteLayout()
        {
            layerConfig = (TextureArrayConfig)EditorGUILayout.ObjectField("Layer Config", layerConfig, typeof(TextureArrayConfig), false);
            splatMode = (SplatMode)EditorGUILayout.EnumPopup("SplatMode", splatMode);
            if (layerConfig != null)
            {
                iconSize = EditorGUILayout.Slider("IconSize", iconSize, 32f, Mathf.Min(128f, position.width));
                int xCount = Mathf.CeilToInt(position.width / iconSize);
                int yCount = Mathf.CeilToInt(previewTextures.Length / (float)xCount);
                Rect rect = GUILayoutUtility.GetRect(position.width, position.width / xCount * yCount);
                selectedLayer = GUI.SelectionGrid(rect, selectedLayer, previewTextures, xCount);
            }
        }

        enum SplatMode
        {
            splatX2,
            splatX3
        }
        static SplatMode splatMode = SplatMode.splatX2;

        protected override void UpdateTile(MeshTile tile)
        {
            Material mat = material;
            mat.SetTexture("_BrushMask", tile.brushMask);
            mat.SetInt("_Index", selectedLayer);
            switch (splatMode)
            {
                case SplatMode.splatX2:
                {
                    mat.EnableKeyword("_SPLAT_X2");
                    mat.DisableKeyword("_SPLAT_X3");
                    tile.material.SetFloat("_SPLAT", 0);
                    tile.material.EnableKeyword("_SPLAT_X2");
                    tile.material.DisableKeyword("_SPLAT_X3");
                    break;
                }
                case SplatMode.splatX3:
                {
                    mat.EnableKeyword("_SPLAT_X3");
                    mat.DisableKeyword("_SPLAT_X2");
                    tile.material.SetFloat("_SPLAT", 1);
                    tile.material.EnableKeyword("_SPLAT_X3");
                    tile.material.DisableKeyword("_SPLAT_X2");
                    break;
                }
            }
            tile.targetTextures.Select(t => t.targetRT).Blit(mat, 0);
            foreach(var target in tile.targetTextures)
            {
                tile.material.SetTexture(target.propertyName, target.targetRT);
            }
            tile.SetDirty();
        }
    }
}