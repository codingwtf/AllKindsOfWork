//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace FSG.MeshAnimator.ShaderAnimated
{
    [AddComponentMenu("Mesh Animator/GPU Shader Animated")]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class ShaderMeshAnimator : MeshAnimatorBase
    {
        private static readonly int _animTimeProp = Shader.PropertyToID("_AnimTimeInfo");
        private static readonly int _animInfoProp = Shader.PropertyToID("_AnimInfo");
        private static readonly int _animScalarProp = Shader.PropertyToID("_AnimScalar");
        private static readonly int _animTexturesProp = Shader.PropertyToID("_AnimTextures");
        private static readonly int _animTextureIndexProp = Shader.PropertyToID("_AnimTextureIndex");
        private static readonly int _crossfadeAnimInfoProp = Shader.PropertyToID("_CrossfadeAnimInfo");
        private static readonly int _crossfadeAnimScalarProp = Shader.PropertyToID("_CrossfadeAnimScalar");
        private static readonly int _crossfadeAnimTextureIndexProp = Shader.PropertyToID("_CrossfadeAnimTextureIndex");
        private static readonly int _crossfadeStartTimeProp = Shader.PropertyToID("_CrossfadeStartTime");
        private static readonly int _crossfadeEndTimeProp = Shader.PropertyToID("_CrossfadeEndTime");

        private static Dictionary<Mesh, Texture2DArray> _animTextures = new Dictionary<Mesh, Texture2DArray>();

        private int pixelsPerTexture = 2;
        private int textureStartIndex = 0;
        private int framesPerTexture = 1000;
        private int textureSizeX;
        private int textureSizeY;
        private MaterialPropertyBlock materialPropertyBlock;
        private Vector4 propertyBlockData;
        private Vector4 timeBlockData;

        public ShaderMeshAnimation defaultMeshAnimation;
        public ShaderMeshAnimation[] meshAnimations;

        public override IMeshAnimation defaultAnimation
        {
            get
            {
                return defaultMeshAnimation;
            }
            set
            {
                defaultMeshAnimation = value as ShaderMeshAnimation;
            }
        }
        public override IMeshAnimation[] animations { get { return meshAnimations; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetupTextureData();
        }
        protected override void Start()
        {
            base.Start();
            SetupTextureData();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!s_meshCount.ContainsKey(baseMesh) && _animTextures.ContainsKey(baseMesh))
            {
                DestroyImmediate(_animTextures[baseMesh]);
                _animTextures.Remove(baseMesh);
            }
        }
        protected override bool DisplayFrame(int previousFrame)
        {
            // Doesn't need to call base, as it does nothing in this implementation
            if (currentFrame == previousFrame || currentAnimIndex < 0)
                return false;
            float currentStartTime = timeBlockData.z;
            float currentEndTime = timeBlockData.w;
            float newEndTime = currentStartTime + (currentAnimation.length / (speed * currentAnimation.playbackSpeed));
            if (currentEndTime != newEndTime)
            {
                timeBlockData.w = newEndTime;
                meshRenderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetVector(_animTimeProp, timeBlockData);
                meshRenderer.SetPropertyBlock(materialPropertyBlock);
            }
            return true;
        }
        protected override void OnAnimationCompleted(bool stopUpdate)
        {
            base.OnAnimationCompleted(stopUpdate);
            if (stopUpdate)
            {
                // set time info
                float startTime = Time.time;
                int frames = currentAnimation.TotalFrames;
                timeBlockData.x = frames;
                timeBlockData.y = frames;
                timeBlockData.z = startTime;
                timeBlockData.w = startTime;
                meshRenderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetVector(_animTimeProp, timeBlockData);
                // set property block
                meshRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
        protected override void OnCurrentAnimationChanged(IMeshAnimation meshAnimation)
        {
            ShaderMeshAnimation currentAnimation = meshAnimation as ShaderMeshAnimation;
            CreatePropertyBlock();
            meshRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetVector(_animScalarProp, currentAnimation.animScalar);
            propertyBlockData.y = currentAnimation.vertexCount;
            pixelsPerTexture = currentAnimation.textureSize.x * currentAnimation.textureSize.y;
            framesPerTexture = Mathf.FloorToInt(pixelsPerTexture / (float)(currentAnimation.vertexCount * 2));
            textureStartIndex = 0;
            for (int i = 0; i < currentAnimIndex; i++)
            {
                textureStartIndex += meshAnimations[i].textureCount;
            }
            textureSizeX = currentAnimation.textureSize.x;
            textureSizeY = currentAnimation.textureSize.y;
            // set animation info
            propertyBlockData.x = textureStartIndex;
            propertyBlockData.z = textureSizeX;
            propertyBlockData.w = textureSizeY;
            materialPropertyBlock.SetVector(_animInfoProp, propertyBlockData);
            // set texture index
            materialPropertyBlock.SetFloat(_animTextureIndexProp, textureStartIndex);
            // set time info
            float startTime = Time.time;
            timeBlockData = new Vector4(
                0,
                currentAnimation.TotalFrames - 1,
                startTime,
                startTime + (currentAnimation.length / (speed * currentAnimation.playbackSpeed)));
            materialPropertyBlock.SetVector(_animTimeProp, timeBlockData);
            // set property block
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        public override void Crossfade(int index, float speed)
        {
            if (currentAnimIndex < 0)
            {
                Play(index);
                return;
            }
            ShaderMeshAnimation currentAnimation = this.currentAnimation as ShaderMeshAnimation;
            CreatePropertyBlock();
            meshRenderer.GetPropertyBlock(materialPropertyBlock);
            Vector4 crossfadeInfo = new Vector4(propertyBlockData.x, propertyBlockData.y, propertyBlockData.z, propertyBlockData.w);

            int framesPerTexture = Mathf.FloorToInt((textureSizeX * textureSizeY) / (currentAnimation.vertexCount * 2));
            int localOffset = Mathf.FloorToInt(currentFrame / (float)framesPerTexture);
            int textureIndex = textureStartIndex + localOffset;
            int frameOffset = Mathf.FloorToInt(currentFrame % framesPerTexture);
            int pixelOffset = currentAnimation.vertexCount * 2 * frameOffset;

            crossfadeInfo.x = pixelOffset;
            materialPropertyBlock.SetFloat(_crossfadeAnimTextureIndexProp, textureIndex);
            materialPropertyBlock.SetVector(_crossfadeAnimScalarProp, currentAnimation.animScalar);
            materialPropertyBlock.SetVector(_crossfadeAnimInfoProp, crossfadeInfo);
            materialPropertyBlock.SetFloat(_crossfadeStartTimeProp, Time.time);
            materialPropertyBlock.SetFloat(_crossfadeEndTimeProp, Time.time + speed);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
            Play(index);
        }
        public override void PrepopulateCrossfadePool(int count) { }
        public override void SetTime(float time, bool instantUpdate = false)
        {
            base.SetTime(time, instantUpdate);
            RefreshTimeData();
        }
        public override void SetTimeNormalized(float time, bool instantUpdate = false)
        {
            base.SetTimeNormalized(time, instantUpdate);
            RefreshTimeData();
        }
        public override void SetAnimations(IMeshAnimation[] meshAnimations)
        {
            this.meshAnimations = new ShaderMeshAnimation[meshAnimations.Length];
            for (int i = 0; i < meshAnimations.Length; i++)
            {
                this.meshAnimations[i] = meshAnimations[i] as ShaderMeshAnimation;
            }
            if (meshAnimations.Length > 0 && defaultMeshAnimation == null)
                defaultMeshAnimation = this.meshAnimations[0];
        }
        public override void StoreAdditionalMeshData(Mesh mesh) { }

        private void RefreshTimeData()
        {
            float normalized = currentAnimTime / currentAnimation.length;
            // set time info
            float startTime = Time.time - (currentAnimation.length * normalized);
            timeBlockData = new Vector4(
                0,
                currentAnimation.TotalFrames - 1,
                startTime,
                startTime + (currentAnimation.length / (speed * currentAnimation.playbackSpeed)));
            materialPropertyBlock.SetVector(_animTimeProp, timeBlockData);
            // set property block
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
        private void SetupTextureData()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (baseMesh == null || meshAnimations == null || meshAnimations.Length == 0)
                    return;
            }
#endif
            CreatePropertyBlock();
            if (!_animTextures.ContainsKey(baseMesh))
            {
                int totalTextures = 0;
                Vector2Int texSize = Vector2Int.zero;
                for (int i = 0; i < meshAnimations.Length; i++)
                {
                    var anim = meshAnimations[i];
                    totalTextures += anim.textures.Length;
                    for (int t = 0; t < anim.textures.Length; t++)
                    {
                        if (anim.textures[t].width > texSize.x)
                            texSize.x = anim.textures[t].width;

                        if (anim.textures[t].height > texSize.y)
                            texSize.y = anim.textures[t].height;
                    }
                }
                Texture2DArray texture2DArray = new Texture2DArray(texSize.x, texSize.y, totalTextures, meshAnimations[0].textures[0].format, false, false);
                texture2DArray.filterMode = FilterMode.Point;
                int index = 0;
                for (int i = 0; i < meshAnimations.Length; i++)
                {
                    var anim = meshAnimations[i];
                    for (int t = 0; t < anim.textures.Length; t++)
                    {
                        var tex = anim.textures[t];
                        Graphics.CopyTexture(tex, 0, 0, texture2DArray, index, 0);
                        index++;
                    }
                    totalTextures += anim.textures.Length;
                }
                _animTextures.Add(baseMesh, texture2DArray);
                if (meshRenderer == null)
                    meshRenderer = GetComponent<MeshRenderer>();
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].SetTexture(_animTexturesProp, _animTextures[baseMesh]);
                }
                meshRenderer.sharedMaterials = materials;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                currentAnimation = defaultMeshAnimation ?? meshAnimations[0];
                currentFrame = Mathf.Clamp(currentFrame, 0, currentAnimation.Frames.Length - 1);
                for (int i = 0; i < meshAnimations.Length; i++)
                {
                    if (meshAnimations[i] == currentAnimation)
                        currentAnimIndex = i;
                }
                if (currentAnimation != null)
                {
                    OnCurrentAnimationChanged(defaultMeshAnimation ?? meshAnimations[0]);
                    DisplayFrame(Random.Range(-2, -10000));
                }
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].SetTexture(_animTexturesProp, _animTextures[baseMesh]);
                }
                meshRenderer.sharedMaterials = materials;
                meshRenderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetFloat(_animTextureIndexProp, 0);
                meshRenderer.SetPropertyBlock(materialPropertyBlock);
            }
#endif
        }
        private void CreatePropertyBlock()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            if (materialPropertyBlock == null)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(materialPropertyBlock);
            }
        }
    }
}