using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Extra + "屏幕渐变 (GradientRamp)")]
    public class GradientRamp : VolumeSetting
    {
        public override bool IsActive() => Enable.value;

        public BoolParameter Enable = new BoolParameter(false);
        public ColorParameter TintColor = new ColorParameter(Color.white);
        public FloatParameter Threshold = new ClampedFloatParameter(0.5f, 0f, 1f);
        public FloatParameter UVThresholdMin = new ClampedFloatParameter(0.5f, 0f, 1f);
        public FloatParameter UVThresholdMax = new ClampedFloatParameter(0.5f, 0f, 1f);
    }

    public class GradientRampRenderer : VolumeRenderer<GradientRamp>
    {
        private const string PROFILER_TAG = "SpeedLine";
        private Shader shader;
        private Material m_BlitMaterial;
            
        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Other/GradientRamp");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }
        
        static class ShaderIDs
        {
            internal static readonly int Params0ID = Shader.PropertyToID("_Params0");
            public static readonly int ColorID = Shader.PropertyToID("_Color");
        }

        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            cmd.BeginSample(PROFILER_TAG);
            m_BlitMaterial.SetColor(ShaderIDs.ColorID, volumeSettings.TintColor.value);
            m_BlitMaterial.SetVector(ShaderIDs.Params0ID, new Vector4(volumeSettings.Threshold.value, volumeSettings.UVThresholdMin.value, volumeSettings.UVThresholdMax.value, 0));

            cmd.Blit(source, target, m_BlitMaterial, 0);
            
            cmd.EndSample(PROFILER_TAG);
        }

    }
}
