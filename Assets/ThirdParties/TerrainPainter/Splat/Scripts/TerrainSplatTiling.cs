#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class TerrainSplatTiling : MonoBehaviour
{
    [SerializeField] bool isGlobal = true;
    [SerializeField] float[] tiling = new float[16] { 
        8f, 8f, 8f, 8f, 
        8f, 8f, 8f, 8f, 
        8f, 8f, 8f, 8f, 
        8f, 8f, 8f, 8f};

    void Start()
    {
        ApplyTiling();
    }

#if UNITY_EDITOR
    void Update()
    {
        if(!EditorApplication.isPlaying)
        {
            ApplyTiling();
        }
    }
#endif

    public void ApplyTiling()
    {
        if(isGlobal)
        {
            Shader.SetGlobalFloatArray("_TerrainSplatTiling", tiling);
        }
        else
        {
            GetComponent<Renderer>()?.sharedMaterial?.SetFloatArray("_TerrainSplatTiling", tiling);
        }
    }
}
