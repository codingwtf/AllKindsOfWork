// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering;

// namespace IGG.Rendering.XPostProcessing
// {
//     [VolumeComponentMenu(VolumeDefine.ColorAdjustment + "ColorReplace")]
//     public class ColorAdjustmentColorReplaceV2 : VolumeSetting
//     {
//         public override bool IsActive() => Range.value != 0;

//         public FloatParameter Range = new ClampedFloatParameter(0f, 0f, 1);
//         public FloatParameter Fuzziness = new ClampedFloatParameter(0.5f, 0f, 1f);
//         public GradientParameter FromGradientColor = new GradientParameter(new Color(0.8f, 0.0f, 0.0f, 1), true, true, true);
//         public GradientParameter ToGradientColor = new GradientParameter(new Color(0.0f, 0.8f, 0.0f, 1), true, true, true);
//         public FloatParameter gridentSpeed = new ClampedFloatParameter(0.5f, 0f, 100f);

//     }


//     public class ColorAdjustmentColorReplaceV2Renderer : VolumeRenderer<ColorAdjustmentColorReplaceV2>
//     {
//         private const string PROFILER_TAG = "ColorAdjustmentColorReplaceV2";
//         private Shader shader;
//         private Material m_BlitMaterial;
//         private float TimeX = 1.0f;

//         public override void Init()
//         {
//             shader = XPostProcessingFindShader.Find("Hidden/PostProcessing/ColorAdjustment/ColorReplaceV2");
//             m_BlitMaterial = CoreUtils.CreateEngineMaterial(shader);
//         }

//         static class ShaderIDs
//         {

//             internal static readonly int Range = Shader.PropertyToID("_Range");
//             internal static readonly int Fuzziness = Shader.PropertyToID("_Fuzziness");
//             internal static readonly int FromColor = Shader.PropertyToID("_FromColor");
//             internal static readonly int ToColor = Shader.PropertyToID("_ToColor");
//         }


//         public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target)
//         {
//             if (m_BlitMaterial == null)
//                 return;

//             cmd.BeginSample(PROFILER_TAG);

//             TimeX += (Time.deltaTime * volumeSettings.gridentSpeed.value);
//             if (TimeX > 100)
//             {
//                 TimeX = 0;
//             }

//             if (volumeSettings.FromGradientColor.value != null)
//             {
//                 m_BlitMaterial.SetColor(ShaderIDs.FromColor, volumeSettings.FromGradientColor.value.Evaluate(TimeX * 0.01f));
//             }

//             if (volumeSettings.ToGradientColor.value != null)
//             {
//                 m_BlitMaterial.SetColor(ShaderIDs.ToColor, volumeSettings.ToGradientColor.value.Evaluate(TimeX * 0.01f));
//             }

//             m_BlitMaterial.SetFloat(ShaderIDs.Range, volumeSettings.Range.value);
//             m_BlitMaterial.SetFloat(ShaderIDs.Fuzziness, volumeSettings.Fuzziness.value);

//             cmd.Blit(source, target, m_BlitMaterial);
//             cmd.EndSample(PROFILER_TAG);
//         }
//     }

// }