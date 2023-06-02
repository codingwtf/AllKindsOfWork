using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMP.Utils;

namespace UMP
{
    public class PainterTarget
    {
        public readonly DoubleBufferedRenderTexture targetRT;
        public readonly string propertyName;
        public readonly Texture2D src;
        
        public PainterTarget(RenderTextureFormat format, string propertyName, Texture2D src, bool linear = false)
        {
            this.src = src;
            this.propertyName = propertyName;
            targetRT = new DoubleBufferedRenderTexture(src.width, src.height, 0, format, linear);
            targetRT.Initialize(src);
        }
    }
}