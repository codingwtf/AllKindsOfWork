using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UMP.Utils
{
    public class RampMap
    {
        public AnimationCurve curve
        {
            get => m_curve;
            set
            {
                if (value != m_curve && value != null)
                {
                    m_curve = value;
                }
                UpdateTexture();
            }
        }
        Texture2D texture;
        AnimationCurve m_curve;


        public int resolution => texture == null ? 0 : texture.width;

        public RampMap(int resolution = 64)
        {
            m_curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            texture = new Texture2D(resolution, 1, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            UpdateTexture();
        }

        public void UpdateTexture()
        {
            for (int u = 0; u < texture.width; ++u)
            {
                float val = curve.Evaluate((u + 0.5f) / texture.width);
                texture.SetPixel(u, 0, new Color(val, val, val));
            }
            texture.Apply();
        }

        public static implicit operator Texture2D(RampMap rampMap)
        {
            return rampMap?.texture;
        }

        public void OnGUI(Rect rect, Material mat = null)
        {
            GUILayout.Box("", GUILayout.Width(rect.width), GUILayout.Height(rect.height));
            curve = EditorGUI.CurveField(rect, curve);
            if (mat == null)
                EditorGUI.DrawPreviewTexture(rect, texture);
            else
                EditorGUI.DrawPreviewTexture(rect, texture, mat);
        }
    }
}