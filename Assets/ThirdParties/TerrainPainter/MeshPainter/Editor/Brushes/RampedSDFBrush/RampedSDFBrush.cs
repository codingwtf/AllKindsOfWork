using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMP.Utils;

namespace UMP
{
    public class RampedSDFBrush : Brush
    {
        public override string name => "SDF";
        protected override Shader shader => Shader.Find("Hidden/OTP/Brush/RampedSDF");

        RampMap rampMap = new RampMap();
        Material rampPreviewMat
        {
            get
            {
                if (m_rampPreviewMat == null)
                    m_rampPreviewMat = new Material(Shader.Find("Hidden/OTP/Brush/RampPreview"));
                return m_rampPreviewMat;
            }
        }
        Material m_rampPreviewMat = null;

        public override void BrushGUILayout(float width)
        {
            EditorGUILayout.BeginHorizontal();
            float brushPreviewSize = EditorGUIUtility.singleLineHeight * 3;
            var rect = EditorGUILayout.BeginVertical(
                GUILayout.Width(brushPreviewSize),
                GUILayout.Height(brushPreviewSize));
            rect.width = brushPreviewSize;
            rect.height = brushPreviewSize;
            rampMap.OnGUI(rect, rampPreviewMat);
            EditorGUILayout.EndVertical();
            rampMap.curve = EditorGUILayout.CurveField(rampMap.curve, GUILayout.Height(brushPreviewSize));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            radius   = Mathf.Max(0f, EditorGUILayout.FloatField("Size", radius));
            strength = EditorGUILayout.Slider("Strength", strength, 0f, 1f);
            EditorGUILayout.EndVertical();
        }

        public override void UpdateMask(DeltaDrag deltaDrag, MeshTile tile, bool isPreview)
        {
            Texture2D ramp = rampMap;
            material.SetTexture("_RampMap", ramp);
            base.UpdateMask(deltaDrag, tile, isPreview);
        }
    }
}