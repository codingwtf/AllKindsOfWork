using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IGG.Rendering.XPostProcessing
{
    public abstract class VolumeRenderer<T> : AbstractVolumeRender where T: VolumeSetting
    {
        public T volumeSettings => VolumeManager.instance.stack.GetComponent<T>();
        public override bool IsActive() => volumeSettings.IsActive();
    }

    public abstract class AbstractVolumeRender
    {
        public abstract bool IsActive();
        public abstract void Init();
        public abstract void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target);
    }
    
    
}
