using UnityEngine;

public static class FindShader
{
    public static Shader Find(string name)
    {
        var shader = Shader.Find(name);
#if UNITY_EDITOR
        if (ShadersAsset == null)
        {
            ShadersAsset =
                UnityEditor.AssetDatabase.LoadAssetAtPath<ShadersAsset>(
                    "Assets/ThirdParties/XPostProcessing/Resources/ShadersAsset.asset");
        }
        if (!ShadersAsset.List.Contains(shader))
            ShadersAsset.List.Add(shader);
        UnityEditor.EditorUtility.SetDirty(ShadersAsset);
#endif
        return shader;
    }

    public static ShadersAsset ShadersAsset;
}