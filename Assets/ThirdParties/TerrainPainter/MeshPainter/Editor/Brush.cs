using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMP.Utils;

namespace UMP
{
    public abstract class Brush
    {
        public abstract string name { get; }
        public virtual GUIContent title => new GUIContent(name);
        protected abstract Shader shader { get; }
        protected virtual Material material
        {
            get
            {
                if(m_material == null)
                {
                    m_material = new Material(shader);
                }
                return m_material;
            }
        }
        Material m_material = null;

        public static float radius = 5f;
        public static float strength = 1f;

        public abstract void BrushGUILayout(float width);

        public virtual void UpdateMask(DeltaDrag deltaDrag, MeshTile tile, bool isPreview)
        {
            material.SetVector("_PrevPos", deltaDrag.prevPos);
            material.SetVector("_CurPos", deltaDrag.curPos);
            material.SetVector("_PrevNorm", deltaDrag.prevNorm);
            material.SetVector("_CurNorm", deltaDrag.curNorm);
            material.SetFloat("_PrevIntensity", deltaDrag.prevPressure);
            material.SetFloat("_CurIntensity", deltaDrag.curPressure);
            material.SetFloat("_BrushRadius", radius);
            material.SetFloat("_BrushStrength", strength);
            material.SetVector("_BrushMaskTexelSize", tile.brushMask.GetTexelSize());
            var activeRT = RenderTexture.active;
            RenderTexture.active = tile.brushMask;
            material.SetPass(0);
            GL.Clear(true, false, Color.black);
            Graphics.DrawMeshNow(tile.mesh, tile.transform.localToWorldMatrix, 0);
            RenderTexture.active = activeRT;
        }

        public struct DeltaDrag
        {
            public readonly Vector3 curPos;
            public readonly Vector3 curNorm;
            public readonly float   curPressure;
            
            public readonly Vector3 prevPos;
            public readonly Vector3 prevNorm;
            public readonly float   prevPressure;
            public DeltaDrag(Vector3 curPos, Vector3 curNorm, float curPressure, Vector3 prevPos, Vector3 prevNorm, float prevPressure)
            {
                this.curPos       = curPos;
                this.curNorm      = curNorm;
                this.curPressure  = curPressure;
                this.prevPos      = prevPos;
                this.prevNorm     = prevNorm;
                this.prevPressure = prevPressure;
            }
        }
    }
}