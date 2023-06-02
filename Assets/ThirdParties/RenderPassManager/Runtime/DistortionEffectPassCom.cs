using RenderPassPipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using UnityEngine.Experimental.Rendering;

namespace Rendering.Pass.Manager
{
    [ExecuteAlways]
    public class DistortionEffectPassCom : MonoBehaviour, IAddPassInterface
    {
        [Serializable]
        public struct DistortionSetting
        {
            [Range(0,1)]
            public float Intensity;

            [Range(0, 30)] 
            public float DitheringAngle;
        
            [Range(0, 2)] 
            public float DitheringSpeed;

            [Range(0, 2)] 
            public float Density;
        }
        private void Start()
        {
            OnEnable();
        }
        
        private void Awake()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            RenderPassManager.instance.AppendAddPassInterfaces(this);
        }

        private void OnDisable()
        {
            RenderPassManager.instance.RemoveAddPassInterfaces(this);
        }

        #region interface

        public bool PlatformSupported(RuntimePlatform platform)
        {
            if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16_SNorm, FormatUsage.Render))
            {
                graphicFormat = GraphicsFormat.R16G16_SNorm;
            }
            else if(SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_SNorm, FormatUsage.Render))
            {
                graphicFormat = GraphicsFormat.R8G8_SNorm;
            }
            else if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_SRGB, FormatUsage.Render))
            {
                graphicFormat = GraphicsFormat.R8G8_SRGB;
            }

            return graphicFormat != GraphicsFormat.None;
        }

        public bool IsSingleton()
        {
            return true;
        }
        
        public int GetType()
        {
            return (int)RenderPassEnum.DistortionEffect;
        }

        public void OnAddPass(ScriptableRenderer renderer, ref RenderingData renderingData)
        {            //Distortion依赖uberpost的uv偏移
            if (!renderingData.postProcessingEnabled  ||!renderingData.cameraData.requiresDepthTexture)
                return;

            var despt = renderingData.cameraData.cameraTargetDescriptor;
            if (rt == null || rt.width != despt.width || rt.width != despt.height)
            {
                if (rt != null)
                {
                    RenderTexture.ReleaseTemporary(rt);   
                }
                rt = RenderTexture.GetTemporary(despt.width, despt.height, 0, graphicFormat);
            }

            pass.Setup(renderer,ref renderingData,ref Setting,rt);
            renderer.EnqueuePass(pass);
            renderer.EnqueuePass(blitPass);
        }

        public void OnRegister()
        {
            if (pass == null)
            {
                pass = new DistortionEffectPass();
            }
            //这里的blitpass后续可以直接加到uberpost中
            if (blitPass == null)
            {
                blitPass = new BlitPass();
            }
        }

        public void OnDispose()
        {
            if (rt != null)
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
            }

            if (pass != null)
            {
                pass.Dispose();
                pass = null;
            }

            if (blitPass != null)
            {
                blitPass.Dispose();
                blitPass = null;
            }
        }

        #endregion


        #region variable

        [SerializeField]
        public DistortionSetting Setting = new DistortionSetting
        {
            Intensity = 0.1f,
            DitheringAngle = 5,
            DitheringSpeed = 1.0f,
            Density = 0.5f,
        };
        private DistortionEffectPass pass = null;
        private RenderTexture rt = null;
        private BlitPass blitPass = null;
        private GraphicsFormat graphicFormat = GraphicsFormat.None;
        #endregion

        
        #region pass

        private class DistortionEffectPass :ScriptableRenderPass
    {
        private int _distortionTex = Shader.PropertyToID("_DistortionEffectTexture");
        private int _distortionParams = Shader.PropertyToID("_DistortionParams");
        private ShaderTagId tag = new ShaderTagId("DistortionEffect");
        private RenderTargetIdentifier distortionVecRT;
        //private RenderTargetIdentifier sourceRT;
        private FilteringSettings filter = new FilteringSettings(RenderQueueRange.transparent);
        private DistortionSetting setting;
        private RenderTargetHandle depthHandle;

        public DistortionEffectPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            depthHandle.Init("_CameraDepthTexture");
        }

        public void Setup(ScriptableRenderer renderer, ref RenderingData renderingData,ref DistortionSetting set,RenderTargetIdentifier rt)
        {
            //sourceRT = renderer.cameraColorTarget;
            setting = set;
            distortionVecRT = rt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var depthID = depthHandle.Identifier();
            if (depthID == -1 || depthID == 0)
                return;
            CommandBuffer cmd = CommandBufferPool.Get("DistortionEffect");
            RenderTargetIdentifier sourRT = renderingData.cameraData.renderer.cameraColorTarget;
            RenderTargetIdentifier depthRT = renderingData.cameraData.renderer.cameraDepthTarget;
            cmd.SetRenderTarget(distortionVecRT,depthRT);
            cmd.ClearRenderTarget(false,true,Color.clear);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            DrawingSettings ds = new DrawingSettings(tag,
                new SortingSettings(renderingData.cameraData.camera) { criteria = SortingCriteria.CommonTransparent });
            context.DrawRenderers(renderingData.cullResults,ref ds,ref filter);
            cmd.SetRenderTarget(sourRT,depthRT);
            cmd.SetGlobalTexture(_distortionTex,distortionVecRT);
            cmd.SetGlobalVector(_distortionParams,new Vector4(setting.Intensity,setting.DitheringAngle,setting.DitheringSpeed,setting.Density));
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            Shader.SetGlobalTexture(_distortionTex,null);
            Shader.SetGlobalVector(_distortionParams,Vector4.zero);
        }
    }

    private class BlitPass : ScriptableRenderPass
    {
        private Material mat = null;
        //private RenderTargetIdentifier sourceRT ;
        private int blitRT = Shader.PropertyToID("_BlitDistortionRT");
        public BlitPass()
        {
            Shader blitShader = Shader.Find("Unlit/BlitDistortion");
            mat = new Material(blitShader);
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("BlitDistortion");
            RenderTargetIdentifier sourceRT = renderingData.cameraData.renderer.cameraColorTarget;
            var desp = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(blitRT,desp);
            cmd.Blit(sourceRT,blitRT,mat);
            cmd.Blit(blitRT,sourceRT);
            cmd.ReleaseTemporaryRT(blitRT);
            context.ExecuteCommandBuffer(cmd);
            
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            if (mat != null)
            {
                if(Application.isPlaying)
                    Destroy(mat);
            }
        }
    }

        #endregion
    }
}

