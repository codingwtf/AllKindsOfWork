using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.ColorAdjustment + "屏幕灰化 (Screen Binarization)")]
    public class ScreenBinarization : VolumeSetting
    {
        public override bool IsActive() => intensity.value > 0;

        [Tooltip("强度")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0.00f, 1f, true);
    }

    public class ScreenBinarizationRenderer : VolumeRenderer<ScreenBinarization>
    {
        private Shader shader;
        private Material m_BlitMaterial;
        private const string PROFILER_TAG = "Screen Binarization";

        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/ScreenBinarization");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        static class ShaderIDs
        {
            public static readonly int BinarizationAmountPID = Shader.PropertyToID("_BinarizationAmount");
        }

        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;


            cmd.BeginSample(PROFILER_TAG);

            m_BlitMaterial.SetFloat(ShaderIDs.BinarizationAmountPID, volumeSettings.intensity.value);
            cmd.Blit(source, target, m_BlitMaterial);
            // cmd.Blit(target, source);

            cmd.EndSample(PROFILER_TAG);

        }
    }

}