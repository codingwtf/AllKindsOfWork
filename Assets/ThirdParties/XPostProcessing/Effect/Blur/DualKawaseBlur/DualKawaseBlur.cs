using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Blur + "双重Kawase模糊 (Dual Kawase Blur)")]
    public class DualKawaseBlur : VolumeSetting
    {
        public override bool IsActive() => BlurRadius.value > 0;
        public FloatParameter BlurRadius = new ClampedFloatParameter(0f, 0f, 15f);
        public IntParameter Iteration = new ClampedIntParameter(4, 1, 8);
        public FloatParameter RTDownScaling = new ClampedFloatParameter(2f, 1f, 10f);
    }

    public class DualKawaseBlurRenderer : VolumeRenderer<DualKawaseBlur>
    {
        private const string PROFILER_TAG = "DualKawaseBlur";
        private Shader shader;
        private Material m_BlitMaterial;

        // [down,up]
        Level[] m_Pyramid;
        const int k_MaxPyramidSize = 16;

        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Blur/DualKawaseBlur");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);

            m_Pyramid = new Level[k_MaxPyramidSize];

            for (int i = 0; i < k_MaxPyramidSize; i++)
            {
                m_Pyramid[i] = new Level
                {
                    down = Shader.PropertyToID("_BlurMipDown" + i),
                    up = Shader.PropertyToID("_BlurMipUp" + i)
                };
            }
        }

        static class ShaderIDs
        {
            internal static readonly int BlurOffset = Shader.PropertyToID("_BlurOffset");
        }

        struct Level
        {
            internal int down;
            internal int up;
        }


        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;

            cmd.BeginSample(PROFILER_TAG);

            int tw = (int)(Screen.width / volumeSettings.RTDownScaling.value);
            int th = (int)(Screen.height / volumeSettings.RTDownScaling.value);

            m_BlitMaterial.SetFloat(ShaderIDs.BlurOffset, volumeSettings.BlurRadius.value);

            // Downsample
            RenderTargetIdentifier lastDown = source;
            for (int i = 0; i < volumeSettings.Iteration.value; i++)
            {
                int mipDown = m_Pyramid[i].down;
                int mipUp = m_Pyramid[i].up;
                cmd.GetTemporaryRT(mipDown, tw, th, 0, FilterMode.Bilinear);
                cmd.GetTemporaryRT(mipUp, tw, th, 0, FilterMode.Bilinear);

                // cmd.GetTemporaryRT(cmd, mipDown, 0, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw, th);
                // context.GetScreenSpaceTemporaryRT(cmd, mipUp, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw, th);
                cmd.Blit(lastDown, mipDown, m_BlitMaterial);

                lastDown = mipDown;
                tw = Mathf.Max(tw / 2, 1);
                th = Mathf.Max(th / 2, 1);
            }

            // Upsample
            int lastUp = m_Pyramid[volumeSettings.Iteration.value - 1].down;
            for (int i = volumeSettings.Iteration.value - 2; i >= 0; i--)
            {
                int mipUp = m_Pyramid[i].up;
                cmd.Blit(lastUp, mipUp, m_BlitMaterial);
                lastUp = mipUp;
            }


            // Render blurred texture in blend pass
            cmd.Blit(lastUp, target, m_BlitMaterial, 1);

            // Cleanup
            for (int i = 0; i < volumeSettings.Iteration.value; i++)
            {
                if (m_Pyramid[i].down != lastUp)
                    cmd.ReleaseTemporaryRT(m_Pyramid[i].down);
                if (m_Pyramid[i].up != lastUp)
                    cmd.ReleaseTemporaryRT(m_Pyramid[i].up);
            }

            cmd.EndSample(PROFILER_TAG);
        }
    }

}