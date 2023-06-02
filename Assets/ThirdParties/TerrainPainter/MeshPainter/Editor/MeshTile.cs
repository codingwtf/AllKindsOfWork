using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UMP.Collections;
using UMP.Utils;

namespace UMP
{
    public abstract class MeshTile
    {
        public virtual GameObject gameObject { get; protected set; }
        public virtual Transform transform { get; protected set; }
        public virtual Renderer renderer { get; protected set; }
        public virtual Material material { get; protected set; }
        public virtual Mesh mesh { get; protected set; }

        protected abstract IEnumerable<(string, RenderTextureFormat, bool)> targets { get; }

        public bool isDirty { get; protected set; }

        protected MeshTile(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
            this.renderer = gameObject?.GetComponent<Renderer>();
            this.material = this.renderer?.sharedMaterial;
            this.mesh = gameObject?.GetComponent<MeshFilter>()?.sharedMesh;
        }
        public virtual IEnumerable<PainterTarget> targetTextures
        {
            get
            {
                if (m_targetTextures == null)
                {
                    m_targetTextures = targets.Select(
                        (target) =>
                            new PainterTarget(
                                target.Item2,
                                target.Item1,
                                material.GetTexture(target.Item1) as Texture2D, 
                                target.Item3)).ToArray();
                }
                return m_targetTextures;
            }
        }
        protected IEnumerable<PainterTarget> m_targetTextures;
        public RenderTexture brushMask = new RenderTexture(512, 512, 32, RenderTextureFormat.R8)
        { 
            name = "Brush Mask", 
            wrapMode = TextureWrapMode.Clamp
        };

        public static T CreateTile<T>(GameObject gameObject) where T : MeshTile
        {
            return typeof(T)
                .GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance, 
                    null, 
                    new System.Type[] { typeof(GameObject) }, 
                    null)
                .Invoke(new object[] { gameObject }) as T;
        }

        public virtual void BeginEdit()
        {
            foreach (var target in targetTextures)
            {
                material.SetTexture(target.propertyName, target.targetRT);
            }
        }

        public virtual void Save()
        {
            RenderTexture prevActive = RenderTexture.active;
            foreach (var target in targetTextures)
            {
                RenderTexture.active = target.targetRT;
                Texture2D src = target.src;
                Texture2D buffer = new Texture2D(src.width, src.height, src.format, false);
                string path = AssetDatabase.GetAssetPath(src);
                buffer.ReadPixels(new Rect(0, 0, buffer.width, buffer.height), 0, 0);
                byte[] rawTexture = null;
                string format = Path.GetExtension(path).ToLower();
                switch (format)
                {
                    case ".tga": rawTexture = buffer.EncodeToTGA(); break;
                    case ".png": rawTexture = buffer.EncodeToPNG(); break;
                    case ".jpg": rawTexture = buffer.EncodeToJPG(); break;
                    case ".exr": rawTexture = buffer.EncodeToEXR(); break;
                }
                if (rawTexture != null)
                    File.WriteAllBytes(path, rawTexture);
                Object.DestroyImmediate(buffer);
            }
            RenderTexture.active = prevActive;
            isDirty = false;
            AssetDatabase.Refresh();
        }

        public virtual void EndEdit()
        {
            foreach (var target in targetTextures)
            {
                material.SetTexture(target.propertyName, target.src);
            }
        }

        public void SetDirty()
        {
            isDirty = true;
        }
    }
}