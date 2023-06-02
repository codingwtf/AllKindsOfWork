using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IGG.Rendering.XPostProcessing
{
    public class CustomVolumePass : ScriptableRenderPass
    {
        public int DownSample;
        
        private RenderTargetIdentifier _sourceRT;
        private RenderTargetHandle _tempRT0;
        private RenderTargetHandle _tempRT1;
        private List<AbstractVolumeRender> _postProcessingRenderers = new List<AbstractVolumeRender>();
        
        private const string RenderPostProcessingTag = "Custom PostProcessing Pass";
        private static readonly ProfilingSampler ProfilingSampler = new ProfilingSampler(RenderPostProcessingTag);

        public CustomVolumePass()
        {
            _tempRT0.Init("_TempRT0");
            _tempRT1.Init("_TempRT1");
            
            AddRenderer(new DepthFogRenderer());
            AddRenderer(new CloudShadowRenderer());
            AddRenderer(new BlackWhiteRenderer());
            AddRenderer(new SpeedLineRenderer());
            AddRenderer(new GradientRampRenderer());
            //故障
            AddRenderer(new GlitchRGBSplitRenderer());
            AddRenderer(new GlitchRGBSplitV2Renderer());
            AddRenderer(new GlitchRGBSplitV3Renderer());
            AddRenderer(new GlitchRGBSplitV4Renderer());
            AddRenderer(new GlitchRGBSplitV5Renderer());
            AddRenderer(new GlitchDigitalStripeRenderer());
            AddRenderer(new GlitchImageBlockRenderer());
            AddRenderer(new GlitchImageBlockV2Renderer());
            AddRenderer(new GlitchImageBlockV3Renderer());
            AddRenderer(new GlitchImageBlockV4Renderer());
            AddRenderer(new GlitchLineBlockRenderer());
            AddRenderer(new GlitchTileJitterRenderer());
            AddRenderer(new GlitchScanLineJitterRenderer());
            AddRenderer(new GlitchAnalogNoiseRenderer());
            AddRenderer(new GlitchScreenJumpRenderer());
            AddRenderer(new GlitchScreenShakeRenderer());
            AddRenderer(new GlitchWaveJitterRenderer());
            //边缘检测
            AddRenderer(new EdgeDetectionRobertsRenderer());
            AddRenderer(new EdgeDetectionRobertsNeonRenderer());
            AddRenderer(new EdgeDetectionRobertsNeonV2Renderer());
            AddRenderer(new EdgeDetectionScharrRenderer());
            AddRenderer(new EdgeDetectionScharrNeonRenderer());
            AddRenderer(new EdgeDetectionScharrNeonV2Renderer());
            AddRenderer(new EdgeDetectionSobelRenderer());
            AddRenderer(new EdgeDetectionSobelNeonRenderer());
            AddRenderer(new EdgeDetectionSobelNeonV2Renderer());
            //像素化
            AddRenderer(new PixelizeCircleRenderer());
            AddRenderer(new PixelizeDiamondRenderer());
            AddRenderer(new PixelizeHexagonRenderer());
            AddRenderer(new PixelizeHexagonGridRenderer());
            AddRenderer(new PixelizeLeafRenderer());
            AddRenderer(new PixelizeLedRenderer());
            AddRenderer(new PixelizeQuadRenderer());
            AddRenderer(new PixelizeSectorRenderer());
            AddRenderer(new PixelizeTriangleRenderer());


            AddRenderer(new IrisBlurRenderer());
            AddRenderer(new RainRippleRenderer());

            AddRenderer(new SurfaceSnowRenderer());
            AddRenderer(new RaderWaveRenderer());
            AddRenderer(new BulletTimeRenderer());

            //Blur
            AddRenderer(new GaussianBlurRenderer());
            AddRenderer(new BoxBlurRenderer());
            AddRenderer(new KawaseBlurRenderer());
            AddRenderer(new DualBoxBlurRenderer());
            AddRenderer(new DualGaussianBlurRenderer());
            AddRenderer(new DualKawaseBlurRenderer());
            AddRenderer(new DualTentBlurRenderer());
            AddRenderer(new BokehBlurRenderer());
            AddRenderer(new TiltShiftBlurRenderer());
            AddRenderer(new IrisBlurRenderer());
            AddRenderer(new IrisBlurV2Renderer());
            AddRenderer(new GrainyBlurRenderer());
            AddRenderer(new RadialBlurRenderer());
            AddRenderer(new RadialBlurV2Renderer());
            AddRenderer(new DirectionalBlurRenderer());

            //Image Processing
            AddRenderer(new SharpenV1Renderer());
            AddRenderer(new SharpenV2Renderer());
            AddRenderer(new SharpenV3Renderer());


            //Vignette
            AddRenderer(new RapidOldTVVignetteRenderer());
            AddRenderer(new RapidOldTVVignetteV2Renderer());
            AddRenderer(new RapidVignetteRenderer());
            AddRenderer(new RapidVignetteV2Renderer());
            AddRenderer(new AuroraVignetteRenderer());

            //色彩调整
            AddRenderer(new ColorAdjustmentBleachBypassRenderer());
            AddRenderer(new ColorAdjustmentBrightnessRenderer());
            AddRenderer(new ColorAdjustmentContrastRenderer());
            AddRenderer(new ColorAdjustmentContrastV2Renderer());
            AddRenderer(new ColorAdjustmentContrastV3Renderer());
            AddRenderer(new ColorAdjustmentHueRenderer());
            AddRenderer(new ColorAdjustmentLensFilterRenderer());
            AddRenderer(new ColorAdjustmentSaturationRenderer());
            AddRenderer(new ColorAdjustmentTechnicolorRenderer());
            AddRenderer(new ColorAdjustmentTintRenderer());
            AddRenderer(new ColorAdjustmentWhiteBalanceRenderer());
            AddRenderer(new ColorAdjustmentColorReplaceRenderer());
            AddRenderer(new ScreenBinarizationRenderer());
        }

        public void AddRenderer(AbstractVolumeRender renderer)
        {
            _postProcessingRenderers.Add(renderer);
            renderer.Init();
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            cmd.Clear();
            RenderTargetIdentifier buff0, buff1;
            buff0 = _tempRT0.id;
            buff1 = _tempRT1.id;
            using (new ProfilingScope(cmd, ProfilingSampler))
            {
                Blit(cmd, _sourceRT, buff0);
                foreach (var renderer in _postProcessingRenderers)
                {
                    if (renderer.IsActive())
                    {
                        renderer.Render(cmd, buff0, buff1);
                        CoreUtils.Swap(ref buff0, ref buff1);
                    }
                }
                Blit(cmd, buff0, _sourceRT);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Setup(RenderTargetIdentifier sourceRT)
        {
            this._sourceRT = sourceRT;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 32;
            descriptor.width = descriptor.width >> DownSample;
            descriptor.height = descriptor.height >> DownSample;
            
            cmd.GetTemporaryRT(_tempRT0.id, descriptor);
            cmd.GetTemporaryRT(_tempRT1.id, descriptor);
            
            ConfigureTarget(_tempRT0.Identifier());
            ConfigureClear(ClearFlag.None, Color.white);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_tempRT0.id);
            cmd.ReleaseTemporaryRT(_tempRT1.id);
        }
    }
}

