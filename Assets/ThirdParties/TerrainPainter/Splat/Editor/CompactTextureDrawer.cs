using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CompactTextureDrawer : MaterialPropertyDrawer
{
    protected string tooltipStr;
     
    public CompactTextureDrawer()
    {
        tooltipStr = "";
    }
    public CompactTextureDrawer(string tooltip)
    {
        tooltipStr = tooltip;
    }
 
    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (prop.type != MaterialProperty.PropType.Texture)
            EditorGUI.LabelField(position, "Works only for Texture property.");
 
        var curVal = editor.TexturePropertyMiniThumbnail(position, prop, label, tooltipStr);
        prop.textureValue = curVal;
    }
     
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return  base.GetPropertyHeight(prop, label, editor) + 2.0f;
    }
}
