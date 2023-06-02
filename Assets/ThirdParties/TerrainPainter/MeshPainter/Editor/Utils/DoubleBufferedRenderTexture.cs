using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UMP.Utils
{
    public class DoubleBufferedRenderTexture
    {
        public RenderTexture backBuffer { get; protected set; }
        public RenderTexture frontBuffer { get; protected set; }

        public DoubleBufferedRenderTexture(RenderTexture src)
        {
            frontBuffer = src;
            backBuffer = new RenderTexture(src);
        }

        public DoubleBufferedRenderTexture(int width, int height, int depth, RenderTextureFormat format, bool linear = false)
        {
            RenderTextureReadWrite rw = linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;
            frontBuffer = new RenderTexture(width, height, depth, format, rw);
            backBuffer = new RenderTexture(frontBuffer);
        }

        public static implicit operator RenderTexture(DoubleBufferedRenderTexture doubleBufferedRT)
        {
            return doubleBufferedRT.frontBuffer;
        }

        public void SwapBuffer()
        {
            RenderTexture tmp = frontBuffer;
            frontBuffer = backBuffer;
            backBuffer = tmp;
        }

        public void Initialize(Texture texture)
        {
            frontBuffer.filterMode = texture.filterMode;
            frontBuffer.wrapMode = texture.wrapMode;
            backBuffer.filterMode = texture.filterMode;
            backBuffer.wrapMode = texture.wrapMode;
            Graphics.Blit(texture, frontBuffer);
            Graphics.Blit(texture, backBuffer);
        }
    }
    public static class DoubleBufferedRenderTextureExtension
    {
        public static void Blit(this IEnumerable<DoubleBufferedRenderTexture> targets, Material material, int pass)
        {
            var activeRT = RenderTexture.active;

            RenderBuffer[] colorBuffers = targets.Select(rt => rt.backBuffer.colorBuffer).ToArray();
            RenderBuffer depthBuffer = targets.First().backBuffer.depthBuffer;
            Graphics.SetRenderTarget(colorBuffers, depthBuffer);

            int i = 0;
            foreach(var target in targets)
            {
                material.SetTexture($"_Src{i}", target.frontBuffer);
                ++i;
            }
            
            if(material.SetPass(pass))
            {
                GL.PushMatrix();
                GL.LoadOrtho();
                GL.Begin(GL.QUADS);

                GL.TexCoord2(0, 0); GL.Vertex3(0, 0, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(0, 1, 0);
                GL.TexCoord2(1, 1); GL.Vertex3(1, 1, 0);
                GL.TexCoord2(1, 0); GL.Vertex3(1, 0, 0);

                GL.End();
                GL.PopMatrix();
            }
            
            RenderTexture.active = activeRT;

            foreach(var target in targets)
            {
                target.SwapBuffer();
            }
        }
    }
}