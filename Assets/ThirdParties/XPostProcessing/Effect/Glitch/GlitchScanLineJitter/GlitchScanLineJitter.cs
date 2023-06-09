using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Glitch + "扫描线抖动故障 (Scane Line Jitter Glitch)")]
    public class GlitchScanLineJitter : VolumeSetting
    {
        public override bool IsActive() => frequency.value > 0;

        public DirectionParameter JitterDirection = new DirectionParameter(Direction.Horizontal);

        public IntervalTypeParameter intervalType = new IntervalTypeParameter(IntervalType.Random);
        public FloatParameter frequency = new ClampedFloatParameter(0f, 0f, 25f);
        public FloatParameter JitterIndensity = new ClampedFloatParameter(0.1f, 0f, 1f);
    }

    public class GlitchScanLineJitterRenderer : VolumeRenderer<GlitchScanLineJitter>
    {
        private const string PROFILER_TAG = "GlitchScanLineJitter";
        private Shader shader;
        private Material m_BlitMaterial;

        private float randomFrequency;


        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Glitch/ScanLineJitter");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        static class ShaderIDs
        {
            internal static readonly int Params = Shader.PropertyToID("_Params");
            internal static readonly int JitterIndensity = Shader.PropertyToID("_ScanLineJitter");
        }


        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;

            // if (!volumeSettings.IsActive())
            //     return;


            cmd.BeginSample(PROFILER_TAG);

            UpdateFrequency(volumeSettings);

            float displacement = 0.005f + Mathf.Pow(volumeSettings.JitterIndensity.value, 3) * 0.1f;
            float threshold = Mathf.Clamp01(1.0f - volumeSettings.JitterIndensity.value * 1.2f);

            //sheet.properties.SetVector(ShaderIDs.Params, new Vector3(volumeSettings.amount, volumeSettings.speed, );

            m_BlitMaterial.SetVector(ShaderIDs.Params, new Vector3(displacement, threshold, volumeSettings.intervalType.value == IntervalType.Random ? randomFrequency : volumeSettings.frequency.value));

            cmd.Blit(source, target, m_BlitMaterial, (int)volumeSettings.JitterDirection.value);
            // cmd.Blit(target, source);

            cmd.EndSample(PROFILER_TAG);
        }

        void UpdateFrequency(GlitchScanLineJitter volumeSettings)
        {
            if (volumeSettings.intervalType.value == IntervalType.Random)
            {
                randomFrequency = UnityEngine.Random.Range(0, volumeSettings.frequency.value);
            }

            if (volumeSettings.intervalType.value == IntervalType.Infinite)
            {
                m_BlitMaterial.EnableKeyword("USING_FREQUENCY_INFINITE");
            }
            else
            {
                m_BlitMaterial.DisableKeyword("USING_FREQUENCY_INFINITE");
            }
        }
    }

}