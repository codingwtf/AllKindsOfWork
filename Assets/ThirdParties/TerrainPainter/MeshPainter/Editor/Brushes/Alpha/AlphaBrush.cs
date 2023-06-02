using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UMP
{
    public class AlphaBrush : Brush
    {
        public override string name => "Alpha";

        protected override Shader shader => Shader.Find("Hidden/OTP/Brush/Alpha");

        string alphaLibrary
        {
            get => m_alphaLibrary;
            set
            {
                if(string.IsNullOrEmpty(value))
                    return;
                if (value.StartsWith(Application.dataPath))
                {
                    value = "Assets" + value.Substring(Application.dataPath.Length);
                }
                if (!m_alphaLibrary.Equals(value))
                {
                    m_alphaLibrary = value;
                    ReloadAlphaLibrary();
                }
            }
        }

        private static string m_alphaLibrary;
        Texture[] alphas
        {
            get
            {
                if(m_alphas == null)
                {
                    ReloadAlphaLibrary();
                }
                return m_alphas;
            }
        }
        Texture[] m_alphas;
        int alphaIdx = 0;

        float rotation = 0f;
        bool toggleAngleJitter = false;

        static AlphaBrush()
        {
            string path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("AlphaBrush t:Script")[0]);
            m_alphaLibrary = Path.Combine(Path.GetDirectoryName(path), "AlphaLibrary");
        }
        
        void ReloadAlphaLibrary()
        {
            m_alphas = AssetDatabase.FindAssets("t:Texture", new string[] { alphaLibrary })
                .Select(guid => AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
        }

        float iconSize = 50f;
        public override void BrushGUILayout(float width)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Library: {alphaLibrary}");
            if(GUILayout.Button("Select"))
            {
                alphaLibrary = EditorUtility.OpenFolderPanel("Alpha Library", alphaLibrary, "Assets/");
            }
            EditorGUILayout.EndHorizontal();
            iconSize = EditorGUILayout.Slider("IconSize", iconSize, 32f, Mathf.Min(128f, width));
            int xCount = Mathf.CeilToInt(width / iconSize);
            int yCount = Mathf.CeilToInt(alphas.Length / (float)xCount);
            Rect rect = GUILayoutUtility.GetRect(width, width / xCount * yCount);
            // alphas就是图片集
            alphaIdx = GUI.SelectionGrid(rect, alphaIdx, alphas, xCount);
            radius   = Mathf.Max(0f, EditorGUILayout.FloatField("Size", radius));
            strength = EditorGUILayout.Slider("Strength", strength, 0f, 1f);
            toggleAngleJitter = EditorGUILayout.Toggle("Angle Jitter", toggleAngleJitter);
            EditorGUI.BeginDisabledGroup(toggleAngleJitter);
            rotation = EditorGUILayout.Slider("Rotation", rotation, 0f, 1f);
            EditorGUI.EndDisabledGroup();
        }

        public override void UpdateMask(DeltaDrag deltaDrag, MeshTile tile, bool isPreview)
        {
            if(!isPreview && toggleAngleJitter)
            {
                rotation = Random.Range(0f, 1f);
            }
            Quaternion rot = SceneView.currentDrawingSceneView.rotation;
            Vector3 viewUp = rot * Vector3.up;
            Vector3 viewRight = rot * Vector3.right;
            material.SetVector("_ProjectionUp", Vector3.Cross(deltaDrag.curNorm, viewRight).normalized);
            material.SetVector("_ProjectionRight", Vector3.Cross(deltaDrag.curNorm, viewUp).normalized);
            material.SetTexture("_Alpha", alphas[alphaIdx]);
            material.SetVector("_ChannelMask", new Vector4(0, 0, 0, 1));
            material.SetFloat("_BrushRotation", rotation * Mathf.PI * 2f);
            base.UpdateMask(deltaDrag, tile, isPreview);
        }
    }
}