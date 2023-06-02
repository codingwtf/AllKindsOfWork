using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Assets/ThirdParties/XPostProcessing/Resources/ShadersAsset", menuName = "ScriptableObjects/ThirdParties/XPostProcessing", order = 1)]
public class ShadersAsset : ScriptableObject
{
    public List<Shader> List = new List<Shader>();
}
