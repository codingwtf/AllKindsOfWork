using RenderPassPipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pass.Manager
{
    [ExecuteAlways]
    public class RainPassCom: MonoBehaviour, IAddPassInterface
    {
        [SerializeField]
        private RainPassInnerSetting _rainPassInnerSetting = new RainPassInnerSetting();
        
        private RainPass _rainPass;
        
        [SerializeField, HideInInspector] private Shader m_Shader = null;
        
        private Material m_Material;

        private const string k_ShaderName = "Hidden/PPRain";
        
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

        public bool PlatformSupported(RuntimePlatform platform)
        {
            return true;
        }

        public void OnRegister()
        {
            if (_rainPass != null)
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

            _rainPass = new RainPass();
            _rainPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }
        
        public void OnAddPass(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.renderType != CameraRenderType.Base)
                return;
            
            _rainPass.SetUp(_rainPassInnerSetting, m_Material);
            renderer.EnqueuePass(_rainPass);
        }

        public void OnDispose()
        {
            CoreUtils.Destroy(m_Material);
            m_Material = null;
        }

        public int GetType()
        {
            return (int)RenderPassEnum.Rain;
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
        internal class RainPassInnerSetting
        {
            [Header("Setup")]
            public Shader rainShader;
            public Mesh rainMesh;

            [Header("Virtual Planes")]
            public Texture2D rainTexture = null;
            public Vector4 rainData0 = new Vector4(1f, 1f, 0f, 4f);

            [Range(0f, 2f)]
            public float rainColorScale = 0.5f;
            public Color rainColor0 = Color.gray;


            [Range(0f, 10f)]
            public float layerDistance0 = 2f;
            [Range(1f, 20f)]
            public float layerDistance1 = 6f;
            
            [Min(-1f)]
            public float forceLayerDistance0 = -1f;


            [Range(0.25f, 4f)]
            public float lightExponent = 1f;
            [Range(0.25f, 4f)]
            public float lightIntensity1 = 1f;
            [Range(0.25f, 4f)]
            public float lightIntensity2 = 1f;

            public Vector3 winDir;
            public float winStrength = 10;
            
            //锥体的缩放值
            public float scale = 1f;
        }
        
        private class RainPass : ScriptableRenderPass
        {
            private RainPassInnerSetting currentRainPassInnerSetting; 
            
            // Profiling tag
            private static string m_ProfilerTag = "RainPass";
            private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

            Material RainMaterial;

            private Matrix4x4 projectionMatrix;
            
            public RainPass()
            {
                currentRainPassInnerSetting = new RainPassInnerSetting();
                
            }
            
            public void SetUp(RainPassInnerSetting rainPassInnerSetting, Material mat)
            {
                
                float fieldOfView = 60f;  // 视角为60度
                float aspect = 16f / 9f;  // 屏幕宽高比为16:9
                float nearPlane = 0.1f;   // 近截面
                float farPlane = 3000f;    // 远截面
                projectionMatrix = Matrix4x4.Perspective(fieldOfView, aspect, nearPlane, farPlane);
                
                currentRainPassInnerSetting = rainPassInnerSetting;
                RainMaterial = mat;
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

                    cmd.SetGlobalTexture("_RainTexture", currentRainPassInnerSetting.rainTexture);
                    cmd.SetGlobalVector("_UVData0", currentRainPassInnerSetting.rainData0);

                    cmd.SetGlobalVector("_RainColor0", currentRainPassInnerSetting.rainColor0 * currentRainPassInnerSetting.rainColorScale);

                    cmd.SetGlobalVector("_LayerDistances0", new Vector4(currentRainPassInnerSetting.layerDistance0, currentRainPassInnerSetting.layerDistance1 - currentRainPassInnerSetting.layerDistance0, 0, 0));


                    cmd.SetGlobalVector("_ForcedLayerDistances", new Vector4(currentRainPassInnerSetting.forceLayerDistance0, -1, -1, -1));

                    cmd.SetGlobalFloat("_LightExponent", currentRainPassInnerSetting.lightExponent);
                    cmd.SetGlobalFloat("_LightIntensity1", currentRainPassInnerSetting.lightIntensity1);
                    cmd.SetGlobalFloat("_LightIntensity2", currentRainPassInnerSetting.lightIntensity2);

                    var camera = renderingData.cameraData.camera;
                    
                    // Prepare for manual blit
                   // cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                  //  cmd.SetViewport(camera.pixelRect);
                    
                   // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, RainMaterial);
                    
                   // cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

                   cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, projectionMatrix);
                   
                   
                    var xform = Matrix4x4.TRS(camera.transform.position, Quaternion.LookRotation(currentRainPassInnerSetting.winDir * currentRainPassInnerSetting.winStrength), new Vector3(1f, 1f, 1f) * currentRainPassInnerSetting.scale );

                    //cmd.SetRenderTarget(dst);
                    cmd.DrawMesh(currentRainPassInnerSetting.rainMesh, xform, RainMaterial);
                    
                    cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                    
                    
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}

