using System;
using System.Collections.Generic;
using RenderPassPipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Rendering.Pass.Manager
{
    [ExecuteAlways]
    public class LightningPassCom: MonoBehaviour, IAddPassInterface
    {
        [SerializeField]
        private LightningPassInnerSetting _lightningPassInnerSetting = new LightningPassInnerSetting();
        
        private LightningPass _lightningPass;
        
        [SerializeField, HideInInspector] private Shader m_Shader = null;
        
        private Material m_Material;

        private const string k_ShaderName = "Hidden/PPLightning";
        
        public List<AnimationCurve> LightningFlashPatterns = new List<AnimationCurve>();
        AnimationCurve LightningCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        float LightningTime;
        public float LightningCurveMultipler = 1.45f;
        private void Start()
        {
            SwitchCurve();
            OnEnable();
        }

        private void Awake()
        {
            OnEnable();
        }

        void SwitchCurve()
        {
            LightningTime = 0;
            var random = Random.Range(0, LightningFlashPatterns.Count);
            LightningCurve = LightningFlashPatterns[random];
        }

        private void Update()
        {
            LightningTime += Time.deltaTime * LightningCurveMultipler;
            if (LightningTime > 1)
            {
                SwitchCurve();
            }
            
            var LightIntensity = LightningCurve.Evaluate(LightningTime);
            _lightningPassInnerSetting.LightningColor = _lightningPassInnerSetting.fixColor * LightIntensity;
            
        }

        private void OnEnable()
        {
            RenderPassManager.instance.AppendAddPassInterfaces(this);
        }

        public bool PlatformSupported(RuntimePlatform platform)
        {
            return true;
        }

        public void OnRegister()
        {
            if (_lightningPass != null)
                return;
            
            if (m_Material != null)
            {
                return;
            }

            if (m_Shader == null)
            {
                m_Shader = Shader.Find(k_ShaderName);
                if (m_Shader == null)
                {
                    return;
                }
            }

            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            _lightningPass = new LightningPass();
            _lightningPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }
        
        public void OnAddPass(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.renderType != CameraRenderType.Base)
                return;
            
            _lightningPass.SetUp(_lightningPassInnerSetting, m_Material);
            renderer.EnqueuePass(_lightningPass);
        }

        public void OnDispose()
        {
            CoreUtils.Destroy(m_Material);
            m_Material = null;
        }

        public int GetType()
        {
            return (int)RenderPassEnum.lightning;
        }

        public bool IsSingleton()
        {
            return true;
        }
        
        private void OnDisable()
        {
            RenderPassManager.instance.RemoveAddPassInterfaces(this);
        }
        
        [System.Serializable]
        internal class LightningPassInnerSetting
        {
            [Header("Setup")]
            public Shader LightningShader;

            public Color fixColor;
            //[Range(0f, 2f)]
            
            [NonSerialized]
            public Color LightningColor = Color.gray;
            
        }
        
        private class LightningPass : ScriptableRenderPass
        {
            private LightningPassInnerSetting currentLightningPassInnerSetting; 
            
            // Profiling tag
            private static string m_ProfilerTag = "LightningPass";
            private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

            Material LightningMaterial;
            
            
            public LightningPass()
            {
                currentLightningPassInnerSetting = new LightningPassInnerSetting();
                
            }
            
            public void SetUp(LightningPassInnerSetting LightningPassInnerSetting, Material mat)
            {
                
                currentLightningPassInnerSetting = LightningPassInnerSetting;
                LightningMaterial = mat;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                base.Configure(cmd, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                   // cmd.SetGlobalTexture("_MainTex", src);
                   
                    cmd.SetGlobalColor("_LightningColor", currentLightningPassInnerSetting.LightningColor);

                    var camera = renderingData.cameraData.camera;
                    
                    // Prepare for manual blit
                   cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                   cmd.SetViewport(camera.pixelRect);
                    
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, LightningMaterial);
                    
                    cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}

