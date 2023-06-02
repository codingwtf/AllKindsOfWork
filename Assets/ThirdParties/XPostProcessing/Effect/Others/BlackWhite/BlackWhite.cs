using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Extra + "黑白闪 (BlackWhite)")]
    public class BlackWhite : VolumeSetting
    {
        public override bool IsActive() => Enable.value;

        public BoolParameter Enable = new BoolParameter(false);
        public ColorParameter TintColor = new ColorParameter(Color.white);

        public TextureParameter NoiseTex = new TextureParameter(null);
        public Vector2Parameter NoiseTexValueTilling = new Vector2Parameter(new Vector2(0.5f, 0.5f));
        public Vector2Parameter NoiseTexValueSpeed = new Vector2Parameter(new Vector2(0.5f, 0.5f));

        public FloatParameter BlurRadius = new ClampedFloatParameter(0f, 0f, 1f);
        public IntParameter Iteration = new ClampedIntParameter(10, 2, 30);
        public FloatParameter RadialCenterX = new ClampedFloatParameter(0.5f, 0f, 1f);
        public FloatParameter RadialCenterY = new ClampedFloatParameter(0.5f, 0f, 1f);

        public ClampedFloatParameter GreyThreshold = new ClampedFloatParameter(0.01f, 0.01f, 0.99f);
        public FloatParameter Luminance = new ClampedFloatParameter(0f, 0f, 1f);
        public FloatParameter Contrast = new ClampedFloatParameter(0f, 0f, 1f);
        public FloatParameter ColorInvert = new ClampedFloatParameter(0f, 0f, 1f);
    }

    public class BlackWhiteRenderer : VolumeRenderer<BlackWhite>
    {
        private const string PROFILER_TAG = "BlackWhite";
        private Shader shader;
        private Material m_BlitMaterial;
            
        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Other/BlackWhite");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }
        
        static class ShaderIDs
        {
            internal static readonly int Params0ID = Shader.PropertyToID("_Params0");
            
            public static readonly int Params1ID = Shader.PropertyToID("_Params1");
            public static readonly int Params2ID = Shader.PropertyToID("_Params2");
            public static readonly int ColorID = Shader.PropertyToID("_Color");
            public static readonly int NoiseTexID = Shader.PropertyToID("_NoiseTex");
        }

        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            cmd.BeginSample(PROFILER_TAG);
            m_BlitMaterial.SetTexture(ShaderIDs.NoiseTexID, volumeSettings.NoiseTex.value);
            m_BlitMaterial.SetColor(ShaderIDs.ColorID, volumeSettings.TintColor.value);
            m_BlitMaterial.SetVector(ShaderIDs.Params0ID, new Vector4(volumeSettings.BlurRadius.value, volumeSettings.Iteration.value, volumeSettings.RadialCenterX.value, volumeSettings.RadialCenterY.value));
            m_BlitMaterial.SetVector(ShaderIDs.Params1ID, new Vector4(volumeSettings.GreyThreshold.value, volumeSettings.Luminance.value, volumeSettings.Contrast.value, volumeSettings.ColorInvert.value));

            m_BlitMaterial.SetVector(ShaderIDs.Params2ID, new Vector4(volumeSettings.NoiseTexValueTilling.value.x,volumeSettings.NoiseTexValueTilling.value.y, volumeSettings.NoiseTexValueSpeed.value.x, volumeSettings.NoiseTexValueSpeed.value.y));

            cmd.Blit(source, target, m_BlitMaterial, 0);
            
            cmd.EndSample(PROFILER_TAG);
        }

    }
}
