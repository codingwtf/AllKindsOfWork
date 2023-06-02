using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace IGG.Rendering.XPostProcessing
{
    public class CustomVolumeFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            [Range(0,5)]public int downSample = 1;
        }

        public Settings settings;
        public CustomVolumePass volumePass;
        
        public override void Create()
        {
            if (volumePass == null) volumePass = new CustomVolumePass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.postProcessEnabled)
            {
                volumePass.renderPassEvent = settings.renderPassEvent;
                volumePass.DownSample = settings.downSample;
                volumePass.Setup(renderer.cameraColorTarget);
                renderer.EnqueuePass(volumePass);
            }
        }
        
    }
}

