using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Blur + "方向模糊 (Directional Blur)")]
    public class DirectionalBlur : VolumeSetting
    {
        public override bool IsActive() => BlurRadius.value > 0;
        public FloatParameter BlurRadius = new ClampedFloatParameter(0f, 0f, 1f);
        public IntParameter Iteration = new ClampedIntParameter(10, 2, 30);
        public FloatParameter Angle = new ClampedFloatParameter(0.5f, 0f, 6f);
        public FloatParameter RTDownScaling = new ClampedFloatParameter(1f, 1f, 10f);
    }


    public class DirectionalBlurRenderer : VolumeRenderer<DirectionalBlur>
    {
        private const string PROFILER_TAG = "DirectionalBlur";
        private Shader shader;
        private Material m_BlitMaterial;


        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Blur/DirectionalBlur");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        static class ShaderIDs
        {
            internal static readonly int Params = Shader.PropertyToID("_Params");
            internal static readonly int BufferRT = Shader.PropertyToID("_BufferRT");
        }


        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;

            cmd.BeginSample(PROFILER_TAG);

            if (volumeSettings.RTDownScaling.value > 1)
            {
                int RTWidth = (int)(Screen.width / volumeSettings.RTDownScaling.value);
                int RTHeight = (int)(Screen.height / volumeSettings.RTDownScaling.value);
                cmd.GetTemporaryRT(ShaderIDs.BufferRT, RTWidth, RTHeight, 0, FilterMode.Bilinear);
                // downsample screen copy into smaller RT
                cmd.Blit(source, ShaderIDs.BufferRT);
            }

            float sinVal = (Mathf.Sin(volumeSettings.Angle.value) * volumeSettings.BlurRadius.value * 0.05f) / volumeSettings.Iteration.value;
            float cosVal = (Mathf.Cos(volumeSettings.Angle.value) * volumeSettings.BlurRadius.value * 0.05f) / volumeSettings.Iteration.value;
            m_BlitMaterial.SetVector(ShaderIDs.Params, new Vector3(volumeSettings.Iteration.value, sinVal, cosVal));

            if (volumeSettings.RTDownScaling.value > 1)
            {
                cmd.Blit(ShaderIDs.BufferRT, target, m_BlitMaterial, 0);
            }
            else
            {
                cmd.Blit(source, target, m_BlitMaterial, 0);
            }


            cmd.EndSample(PROFILER_TAG);
        }
    }

}