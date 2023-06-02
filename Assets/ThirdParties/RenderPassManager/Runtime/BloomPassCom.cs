using System;
using RenderPassPipeline;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pass.Manager
{
    [ExecuteAlways]
    public class BloomPassCom : MonoBehaviour, IAddPassInterface
    {
        public bool IsSingleton()
        {
            return true;
        }
        
        public int GetType()
        {
            return (int)RenderPassEnum.Bloom;
        }

        public void OnAddPass(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(!enabled) return;
            pass.Setup(ref BloomSetting);
            renderer.EnqueuePass(pass);
        }

        public bool PlatformSupported(RuntimePlatform platform)
        {
            return true;
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

        public void OnRegister()
        {
            if (pass == null)
            {
                if (BloomMaterial == null)
                {
                    Shader s = Shader.Find("Unlit/BloomForBuiltin");
                    BloomMaterial = new Material(s);
                }
                pass = new BloomPass(BloomMaterial);
            }
        }

        public void OnDispose()
        {
            pass = null;
            if (BloomMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(BloomMaterial);
                }
                else
                {
                    DestroyImmediate(BloomMaterial,true);
                }
            }
        }

        private void OnDisable()
        {
            RenderPassManager.instance.RemoveAddPassInterfaces(this);
        }

        private Material BloomMaterial = null;
        [SerializeField]
        public BloomForBuiltinSetting BloomSetting  = new BloomForBuiltinSetting
        {
            Clamp = 1.0f,
            Scatter = 0.3f,
            Threshold = 0.5f,
            BloomMipmapLevel = 0,
            FastBloom = true,
        };

        [Serializable]
        public struct BloomForBuiltinSetting  
        {
            [Range(0,1)]
            public float Clamp;
            [Range(0,1)]
            public float Threshold;
            [Range(0,1)]
            public float Scatter;
            
            public int BloomMipmapLevel;
            public bool FastBloom;
        }

        private BloomPass pass = null;
        
        
        #region Bloom Pass
        
        private class BloomPass : ScriptableRenderPass
        {
            Material bloomMat = null;
            BloomForBuiltinSetting setting;
            private const int k_MaxPyramidSize = 16;
            private int ShaderParams;
            private int[] bloomMipUp;
            private int[] bloomMipDowm;
            private int sourceTexLowMip;
            private int sourceTex;

            private BuiltinRenderTextureType BlitDstDiscardContent(CommandBuffer cmd, RenderTargetIdentifier rt)
            {
                // We set depth to DontCare because rt might be the source of PostProcessing used as a temporary target
                // Source typically comes with a depth buffer and right now we don't have a way to only bind the color attachment of a RenderTargetIdentifier
                cmd.SetRenderTarget(new RenderTargetIdentifier(rt, 0, CubemapFace.Unknown, -1),
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                return BuiltinRenderTextureType.CurrentActive;
            }
            
            private new void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int passIndex = 0)
            {
                cmd.SetGlobalTexture(sourceTex, source);
                {
                    cmd.Blit(source, destination, material, passIndex);
                }
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cameraRT = renderingData.cameraData.renderer.cameraColorTarget;
                var cmd = CommandBufferPool.Get("BloomForBuiltin");
                var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                // Start at half-res
                int tw = descriptor.width >> 1;
                int th = descriptor.height >> 1;

                // Determine the iteration count
                int maxSize = Mathf.Max(tw, th);
                int iterations = setting.FastBloom? Mathf.FloorToInt(Mathf.Log(maxSize, 4f)) :Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
                int mipCount = Mathf.Clamp(iterations, 1, k_MaxPyramidSize);

                // Pre-filtering parameters
                float clamp = setting.Clamp;
                float threshold = Mathf.GammaToLinearSpace(setting.Threshold);
                float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee

                // Material setup
                float scatter = Mathf.Lerp(0.05f, 0.95f, setting.Scatter);
                bloomMat.SetVector(ShaderParams, new Vector4(scatter, clamp, threshold, thresholdKnee));

                // Prefilter
                cmd.GetTemporaryRT(bloomMipDowm[0], descriptor, FilterMode.Bilinear);
                cmd.GetTemporaryRT(bloomMipUp[0], descriptor, FilterMode.Bilinear);
                Blit(cmd, cameraRT, bloomMipDowm[0], bloomMat, 0);

                // Downsample - gaussian pyramid
                int lastDown = bloomMipDowm[0];
                for (int i = 1; i < mipCount; i++)
                {
                    if (setting.FastBloom)
                    {
                        tw = Mathf.Max(1, tw >> 2);
                        th = Mathf.Max(1, th >> 2);
                        int mipDown = bloomMipDowm[i];
                        int mipUp = bloomMipUp[i];

                        descriptor.width = tw;
                        descriptor.height = th;

                        cmd.GetTemporaryRT(mipDown, descriptor, FilterMode.Bilinear);
                        cmd.GetTemporaryRT(mipUp, descriptor, FilterMode.Bilinear);
                        
                        Blit(cmd, lastDown, mipUp, bloomMat, 5);
                        Blit(cmd, mipUp, mipDown, bloomMat, 6);
                        
                        
                        lastDown = mipDown;
                    }
                    else
                    {
                        tw = Mathf.Max(1, tw >> 1);
                        th = Mathf.Max(1, th >> 1);
                        int mipDown = bloomMipDowm[i];
                        int mipUp = bloomMipUp[i];

                        descriptor.width = tw;
                        descriptor.height = th;

                        cmd.GetTemporaryRT(mipDown, descriptor, FilterMode.Bilinear);
                        cmd.GetTemporaryRT(mipUp, descriptor, FilterMode.Bilinear);

                        // Classic two pass gaussian blur - use mipUp as a temporary target
                        //   First pass does 2x downsampling + 9-tap gaussian
                        //   Second pass does 9-tap gaussian using a 5-tap filter + bilinear filtering
                        Blit(cmd, lastDown, mipUp, bloomMat, 1);
                        Blit(cmd, mipUp, mipDown, bloomMat, 2);

                        lastDown = mipDown;
                    }
                }

                // Upsample (bilinear by default, HQ filtering does bicubic instead
                int target = math.min(setting.BloomMipmapLevel, mipCount);
                for (int i = mipCount - 2; i >= target; i--)
                {
                    int lowMip = (i == mipCount - 2) ? bloomMipDowm[i + 1] : bloomMipUp[i + 1];
                    int highMip = bloomMipDowm[i];
                    int dst = bloomMipUp[i];

                    cmd.SetGlobalTexture(sourceTexLowMip, lowMip);
                    Blit(cmd, highMip, dst, bloomMat, 3);
                }

                
                Blit(cmd,bloomMipUp[target],cameraRT,bloomMat,4); 
                
                // Cleanup
                for (int i = 0; i < mipCount; i++)
                {
                    cmd.ReleaseTemporaryRT(bloomMipDowm[i]);
                    if (i > 0) cmd.ReleaseTemporaryRT(bloomMipUp[i]);
                }
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public BloomPass(Material mat)
            {
                bloomMat = mat;
                ShaderParams = Shader.PropertyToID("_BloomForBuiltinParams");
                bloomMipUp = new int[k_MaxPyramidSize];
                bloomMipDowm = new int[k_MaxPyramidSize];

                for (int i = 0; i < k_MaxPyramidSize; i++)
                {
                    bloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
                    bloomMipDowm[i] = Shader.PropertyToID("_BloomMipDown" + i);
                }

                sourceTexLowMip = Shader.PropertyToID("_SourceTexLowMip");
                sourceTex  =Shader.PropertyToID("_SourceTex");
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }

            public void Setup(ref BloomForBuiltinSetting s)
            {
                setting = s;
            }
        }
        
        #endregion
        
        
        
        
    }
    
    
}

