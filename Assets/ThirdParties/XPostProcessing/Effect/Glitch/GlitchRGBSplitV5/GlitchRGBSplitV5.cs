using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Glitch + "RGB颜色分离V5 (RGB SplitV5)")]
    public class GlitchRGBSplitV5 : VolumeSetting
    {
        public override bool IsActive() => Amplitude.value > 0;
        public FloatParameter Amplitude = new ClampedFloatParameter(0f, 0f, 5f);
        public FloatParameter Speed = new ClampedFloatParameter(0.1f, 0f, 1f);

    }


    public sealed class GlitchRGBSplitV5Renderer : VolumeRenderer<GlitchRGBSplitV5>
    {
        private const string PROFILER_TAG = "GlitchRGBSplitV5";
        private Shader shader;
        private Material m_BlitMaterial;
        private Texture2D m_NoiseTex;

        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Glitch/RGBSplitV5");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);

            m_NoiseTex = Resources.Load("X-Noise256") as Texture2D;
        }

        static class ShaderIDs
        {
            internal static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");
            internal static readonly int Params = Shader.PropertyToID("_Params");
        }


        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;

            // if (!volumeSettings.IsActive())
            //     return;


            cmd.BeginSample(PROFILER_TAG);

            m_BlitMaterial.SetVector(ShaderIDs.Params, new Vector2(volumeSettings.Amplitude.value, volumeSettings.Speed.value));
            if (m_NoiseTex != null)
            {
                m_BlitMaterial.SetTexture(ShaderIDs.NoiseTex, m_NoiseTex);
            }

            cmd.Blit(source, target, m_BlitMaterial);
            // cmd.Blit(target, source);

            cmd.EndSample(PROFILER_TAG);
        }
    }

}