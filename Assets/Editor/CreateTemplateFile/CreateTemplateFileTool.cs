using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CreateTemplateFileTool
{
    private static readonly string DefaultUrpNewShaderName = "NewUrpShader.shader";
    private static readonly string DefaultUrpHlslName = "UrpHlsl.hlsl";
    
    [MenuItem("Assets/Create/Shader/Universal Render Pipeline/NewUrpShader")]
    internal static void CreateNewUrpShader()
    {
        string templatePath = AssetDatabase.GUIDToAssetPath(TemplateFileResourceGuid.NewUrpShaderGuid);
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, DefaultUrpNewShaderName);
    }
    
    [MenuItem("Assets/Create/Shader/Universal Render Pipeline/NewUrpHlsl")]
    internal static void CreateNewUrpHlsl()
    {
        string templatePath = AssetDatabase.GUIDToAssetPath(TemplateFileResourceGuid.NewUrpHlslGuid);
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, DefaultUrpHlslName);
    }

    static class TemplateFileResourceGuid
    {
        public static readonly string NewUrpShaderGuid = "afbb681673fc0604c88ffc0e6897026d";
        public static readonly string NewUrpHlslGuid = "7c238044bb949964ab1f6b1c0abf2937";
    }
}
