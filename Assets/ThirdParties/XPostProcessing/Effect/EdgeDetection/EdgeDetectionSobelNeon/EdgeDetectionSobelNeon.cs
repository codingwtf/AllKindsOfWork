using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IGG.Rendering.XPostProcessing
{
    [VolumeComponentMenu(VolumeDefine.EdgeDetection + "SobelNeon")]
    public class EdgeDetectionSobelNeon : VolumeSetting
    {
        public override bool IsActive() => EdgeWidth.value > 0;

        public FloatParameter EdgeWidth = new ClampedFloatParameter(0f, 0.05f, 5.0f);
        public FloatParameter BackgroundFade = new ClampedFloatParameter(1f, 0f, 1f);
        public FloatParameter Brigtness = new ClampedFloatParameter(1f, 0.2f, 2.0f);
        public ColorParameter BackgroundColor = new ColorParameter(Color.black, true, true, true);
    }

    public class EdgeDetectionSobelNeonRenderer : VolumeRenderer<EdgeDetectionSobelNeon>
    {
        private const string PROFILER_TAG = "EdgeDetectionSobelNeon";
        private Shader shader;
        private Material m_BlitMaterial;

        public override void Init()
        {
            shader = FindShader.Find("Hidden/PostProcessing/EdgeDetection/SobelNeon");
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        static class ShaderIDs
        {
            internal static readonly int Params = Shader.PropertyToID("_Params");
            internal static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");
        }


        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
        {
            if (m_BlitMaterial == null)
                return;

            cmd.BeginSample(PROFILER_TAG);


            m_BlitMaterial.SetVector(ShaderIDs.Params, new Vector3(volumeSettings.EdgeWidth.value, volumeSettings.Brigtness.value, volumeSettings.BackgroundFade.value));
            m_BlitMaterial.SetColor(ShaderIDs.BackgroundColor, volumeSettings.BackgroundColor.value);

            cmd.Blit(source, target, m_BlitMaterial);

            cmd.EndSample(PROFILER_TAG);
        }
    }

}