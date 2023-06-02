using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderPassPipeline
{
    public class ExtendRenderFeature : ScriptableRendererFeature
    {    
        public override void Create()
        {
            m_passManager.ReInitialize();            
        }
        private void OnDestroy()
        {
            m_passManager.Release();
        }
        public override void AddRenderPasses(ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            m_passManager.ProcessAddRenderPasses(renderer,
                ref renderingData);
        }
        private RenderPassManager m_passManager = RenderPassManager.instance;
    }
}



