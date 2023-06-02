using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMP.Collections;

namespace UMP
{
    public class TerrainSplatTile : MeshTile
    {
        protected override IEnumerable<(string, RenderTextureFormat, bool)> targets
        {
            get
            {
                yield return ("_IndexTex",  RenderTextureFormat.ARGB32, true);
                yield return ("_WeightTex", RenderTextureFormat.ARGB32, true);
            }
        }

        protected TerrainSplatTile(GameObject gameObject) : base(gameObject) {}
    }
}