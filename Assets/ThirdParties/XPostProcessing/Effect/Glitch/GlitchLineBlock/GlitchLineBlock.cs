using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace IGG.Rendering.XPostProcessing
{

    [VolumeComponentMenu(VolumeDefine.Glitch + "错位线条故障 (Line Block Glitch)")]
    public class GlitchLineBlock : VolumeSetting
    {
        public override bool IsActive() => frequency.value > 0;

        public DirectionParameter blockDirection = new DirectionParameter(Direction.Horizontal);

        public IntervalTypeParameter intervalType = new IntervalTypeParameter(IntervalType.Random);

        public FloatParameter frequency = new ClampedFloatParameter(0f, 0f, 25f);
        public FloatParameter Amount = new ClampedFloatParameter(0.5f, 0f, 1f);

        public FloatParameter LinesWidth = new ClampedFloatParameter(1f, 0.1f, 10f);

        public FloatParameter Speed = new ClampedFloatParameter(0.8f, 0f, 1f);

        public FloatParameter Offset = new ClampedFloatParameter(1f, 0f, 13f);

        public FloatParameter Alpha = new ClampedFloatParameter(1f, 0f, 1f);


        //新增随机色开关
        public FloatParameter RandomColor = new ClampedFloatParameter(1f, 0f, 1f);
    }


    public class GlitchLineBlockRenderer : VolumeRenderer<GlitchLineBlock>
    {
        private const string PROFILER_TAG = "GlitchLineBlock";
        private Shader shader;
        private Material m_BlitMaterial;

        private float TimeX = 1.0f;
        private float randomFrequency;
        private int frameCount = 0;


        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Glitch/LineBlock");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        static class ShaderIDs
        {
            internal static readonly int Params = Shader.PropertyToID("_Params");
            internal static readonly int Params2 = Shader.PropertyToID("_Params2");
        }


        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;


            cmd.BeginSample(PROFILER_TAG);

            UpdateFrequency(volumeSettings);

            TimeX += Time.deltaTime;
            if (TimeX > 100)
            {
                TimeX = 0;
            }

            m_BlitMaterial.SetVector(ShaderIDs.Params, new Vector3(
                volumeSettings.intervalType.value == IntervalType.Random ? randomFrequency : volumeSettings.frequency.value,
                TimeX * volumeSettings.Speed.value * 0.2f, volumeSettings.Amount.value));
            //新增随机色开关
            m_BlitMaterial.SetVector(ShaderIDs.Params2, new Vector4(volumeSettings.Offset.value, 1 / volumeSettings.LinesWidth.value, volumeSettings.Alpha.value, volumeSettings.RandomColor.value));

            cmd.Blit(source, target, m_BlitMaterial, (int)volumeSettings.blockDirection.value);

            cmd.EndSample(PROFILER_TAG);
        }

        void UpdateFrequency(GlitchLineBlock volumeSettings)
        {
            if (volumeSettings.intervalType.value == IntervalType.Random)
            {
                if (frameCount > volumeSettings.frequency.value)
                {

                    frameCount = 0;
                    randomFrequency = UnityEngine.Random.Range(0, volumeSettings.frequency.value);
                }
                frameCount++;
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