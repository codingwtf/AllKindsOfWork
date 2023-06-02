using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMP.Utils
{
    public static class TextureUtils
    {
        public static Vector4 GetTexelSize(this Texture texture)
        {
            return new Vector4(
                1.0f / texture.width, 
                1.0f / texture.height, 
                texture.width, 
                texture.height);
        }
    }
}