using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.Pixelate + "六边形像素化 (PixelizeHexagon)")]
    public class PixelizeHexagon : VolumeSetting
    {
        public override bool IsActive() => pixelSize.value > 0;

        public FloatParameter pixelSize = new ClampedFloatParameter(0f, 0.01f, 1.0f);
        // public FloatParameter gridWidth = new ClampedFloatParameter(1.0f, 0.01f, 5.0f);
        public BoolParameter useAutoScreenRatio = new BoolParameter(false);
        public FloatParameter pixelRatio = new ClampedFloatParameter(1f, 0.2f, 5.0f);

        [Tooltip("像素缩放X")]
        public FloatParameter pixelScaleX = new ClampedFloatParameter(1f, 0.2f, 5.0f);

        [Tooltip("像素缩放Y")]
        public FloatParameter pixelScaleY = new ClampedFloatParameter(1f, 0.2f, 5.0f);
    }

    public class PixelizeHexagonRenderer : VolumeRenderer<PixelizeHexagon>
    {
        private const string PROFILER_TAG = "PixelizeHexagon";
        private Shader shader;
        private Material m_BlitMaterial;


        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/Pixelate/PixelizeHexagon");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        static class ShaderIDs
        {
            internal static readonly int Params = Shader.PropertyToID("_Params");
        }


        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;

            cmd.BeginSample(PROFILER_TAG);

            float size = volumeSettings.pixelSize.value * 0.2f;
            m_BlitMaterial.SetFloat("_PixelSize", size);
            float ratio = volumeSettings.pixelRatio.value;
            if (volumeSettings.useAutoScreenRatio.value)
            {
                ratio = (float)(Screen.width / (float)Screen.height);
                if (ratio == 0)
                {
                    ratio = 1f;
                }
            }

            m_BlitMaterial.SetVector(ShaderIDs.Params, new Vector4(size, ratio, volumeSettings.pixelScaleX.value, volumeSettings.pixelScaleY.value));

            cmd.Blit(source, target, m_BlitMaterial);

            cmd.EndSample(PROFILER_TAG);
        }
    }

}