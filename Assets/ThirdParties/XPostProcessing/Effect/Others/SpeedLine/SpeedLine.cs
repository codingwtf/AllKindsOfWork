using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Extra + "速度线 (SpeedLine)")]
    public class SpeedLine : VolumeSetting
    {
        public override bool IsActive() => Enable.value;

        public BoolParameter Enable = new BoolParameter(false);
        public ColorParameter TintColor = new ColorParameter(Color.white);

        public TextureParameter NoiseTex = new TextureParameter(null);
        public Vector2Parameter Center = new Vector2Parameter(new Vector2(0.5f, 0.5f));

        public FloatParameter RotateSpeed = new ClampedFloatParameter(0.2f, -30f, 50f);
        public ClampedFloatParameter RayMultiply = new ClampedFloatParameter(10, 0.001f, 50f);
        public FloatParameter RayPower = new ClampedFloatParameter(0.5f, 0f, 50f);
        public FloatParameter Threshold = new ClampedFloatParameter(0.5f, 0f, 1f);
    }

    public class SpeedLineRenderer : VolumeRenderer<SpeedLine>
    {
        private const string PROFILER_TAG = "SpeedLine";
        private Shader shader;
        private Material m_BlitMaterial;
            
        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Other/SpeedLine");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }
        
        static class ShaderIDs
        {
            internal static readonly int Params0ID = Shader.PropertyToID("_Params0");
            
            public static readonly int Params1ID = Shader.PropertyToID("_Params1");
            public static readonly int ColorID = Shader.PropertyToID("_Color");
            public static readonly int NoiseTexID = Shader.PropertyToID("_NoiseTex");
        }

        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            cmd.BeginSample(PROFILER_TAG);
            m_BlitMaterial.SetTexture(ShaderIDs.NoiseTexID, volumeSettings.NoiseTex.value);
            m_BlitMaterial.SetColor(ShaderIDs.ColorID, volumeSettings.TintColor.value);
            m_BlitMaterial.SetVector(ShaderIDs.Params0ID, new Vector4(volumeSettings.Center.value.x, volumeSettings.Center.value.y, 0, 0));
            m_BlitMaterial.SetVector(ShaderIDs.Params1ID, new Vector4(volumeSettings.RotateSpeed.value, volumeSettings.RayMultiply.value, volumeSettings.RayPower.value, volumeSettings.Threshold.value));

            cmd.Blit(source, target, m_BlitMaterial, 0);
            
            cmd.EndSample(PROFILER_TAG);
        }

    }
}
