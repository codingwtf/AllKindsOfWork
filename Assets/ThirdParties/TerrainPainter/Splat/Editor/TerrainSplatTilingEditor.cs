using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainSplatTiling))]
public class TerrainSplatTilingEditor : Editor
{
    static TextureArrayConfig config
    {
        get
        {
            if(m_config == null)
            {
                string guid = AssetDatabase.FindAssets("t:TextureArrayConfig").FirstOrDefault();
                if (!string.IsNullOrEmpty(guid))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    m_config = AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(path);
                }
            }
            return m_config;
        }
    }
    static TextureArrayConfig m_config = null;

    Texture GetIcon(int index)
    {
        var cfg = config;
        if(cfg == null || cfg.sourceTextures == null || cfg.sourceTextures.Count() <= index)
        {
            return null;
        }
        var icon = cfg.sourceTextures[index].diffuse;
        if(icon != null)
        {
            icon = AssetPreview.GetAssetPreview(icon);
        }
        return icon;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        SerializedProperty pIsGlobal = serializedObject.FindProperty("isGlobal");
        EditorGUILayout.PropertyField(pIsGlobal);
        SerializedProperty pTilings = serializedObject.FindProperty("tiling");
        pTilings.isExpanded = EditorGUILayout.Foldout(pTilings.isExpanded, "Tilings");
        if(pTilings.isExpanded)
        {
            int arraySize = EditorGUILayout.IntField(pTilings.arraySize);
            pTilings.arraySize = arraySize;
            for (int i = 0; i < arraySize; ++i)
            {
                SerializedProperty pTiling = pTilings.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                Texture icon = GetIcon(i);
                float iconSize = EditorGUIUtility.singleLineHeight * 2f;
                GUILayout.Box(icon, GUILayout.Height(iconSize), GUILayout.Width(iconSize));
                pTiling.floatValue = EditorGUILayout.FloatField("Tiling", pTiling.floatValue);
                EditorGUILayout.EndHorizontal();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
