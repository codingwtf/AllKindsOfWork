//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

#if !UNITY_WEBGL
#define THREADS_ENABLED
#endif

#if UNITY_SWITCH
#define USE_TRIANGLE_DATA
#endif

using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace FSG.MeshAnimator.Snapshot
{
    /// <summary>
    /// Handles animation playback and swapping of mesh frames on the target MeshFilter
    /// </summary>
    [AddComponentMenu("Miscellaneous/Mesh Animator")]
    [RequireComponent(typeof(MeshFilter))]
    public class SnapshotMeshAnimator : MeshAnimatorBase
    {
        struct LerpMatrix4x4Job : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Matrix4x4> from;
            [ReadOnly] public NativeArray<Matrix4x4> to;
            [ReadOnly] public float delta;
            public NativeArray<Matrix4x4> output;
            public void Execute(int i)
            {
                Matrix4x4 m = output[i];
                for (int j = 0; j < 16; j++)
                    m[j] = Mathf.Lerp(from[i][j], to[i][j], delta);
                output[i] = m;
            }
        }
        struct LerpVector3Job : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> from;
            [ReadOnly] public NativeArray<Vector3> to;
            [ReadOnly] public float delta;
            public NativeArray<Vector3> output;
            public void Execute(int i)
            {
                output[i] = Vector3.Lerp(from[i], to[i], delta);
            }
        }
        struct CalculateBoundsJob : IJob
        {
            [ReadOnly] public NativeArray<Vector3> positions;
            public Bounds bounds;
            public void Execute()
            {
                Vector3 center = Vector3.zero;
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                for (int i = 0; i < positions.Length; i++)
                {
                    Vector3 pos = positions[i];
                    if (pos.x < min.x) min.x = pos.x;
                    if (pos.y < min.y) min.y = pos.y;
                    if (pos.z < min.z) min.z = pos.z;
                    if (pos.x > max.x) max.x = pos.x;
                    if (pos.y > max.y) max.y = pos.y;
                    if (pos.z > max.z) max.z = pos.z;
                    center += pos;
                }
                center /= positions.Length;
                bounds = new Bounds(center, max - min);
            }
        }

#if USE_TRIANGLE_DATA
        private static Dictionary<Mesh, Mesh> s_modifiedMeshCache = new Dictionary<Mesh, Mesh>();
#endif
        // static crossfade pooling
        private static List<Stack<Mesh>> s_crossFadePool = new List<Stack<Mesh>>(10);
        private static Dictionary<Mesh, int> s_crossFadeIndex = new Dictionary<Mesh, int>();

        private struct CrossfadeJobData
        {
            public int framesNeeded;
            public int currentFrame;
            public bool isFading;
            public float endTime;
            public SnapshotMeshFrameData fromFrame;
            public SnapshotMeshFrameData toFrame;
            public LerpVector3Job[] positionJobs;
            public CalculateBoundsJob[] boundsJobs;
            public LerpMatrix4x4Job[] exposedTransformJobs;
            public JobHandle[] positionJobHandles;
            public JobHandle[] boundsJobHandles;
            public JobHandle[] exposedTransformJobHandles;
            public NativeArray<Vector3> from;
            public NativeArray<Vector3> to;
            public NativeArray<Vector3>[] output;
            public NativeArray<Matrix4x4> fromMatrix;
            public NativeArray<Matrix4x4> toMatrix;
            public NativeArray<Matrix4x4>[] outputMatrix;

            private bool isReset;

            public void Reset(bool destroying)
            {
                if (!isReset)
                {
                    ReturnFrame(destroying);
                    isFading = false;
                    endTime = 0;
                    currentFrame = 0;
                    framesNeeded = 0;
                    fromFrame = null;
                    toFrame = null;
                    isReset = true;
                }
            }
            public void StartCrossfade(SnapshotMeshFrameData fromFrame, SnapshotMeshFrameData toFrame)
            {
                Reset(false);
                isReset = false;
                this.fromFrame = fromFrame;
                this.toFrame = toFrame;
                int vertexLength = fromFrame.verts.Length;
                int matrixLength = fromFrame.exposedTransforms != null ? fromFrame.exposedTransforms.Length : 0;

                if (positionJobs == null) positionJobs = AllocatedArray<LerpVector3Job>.Get(framesNeeded);
                if (boundsJobs == null) boundsJobs = AllocatedArray<CalculateBoundsJob>.Get(framesNeeded);
                if (positionJobHandles == null) positionJobHandles = AllocatedArray<JobHandle>.Get(framesNeeded);
                if (boundsJobHandles == null) boundsJobHandles = AllocatedArray<JobHandle>.Get(framesNeeded);
                if (output == null) output = AllocatedArray<NativeArray<Vector3>>.Get(framesNeeded);

                from = new NativeArray<Vector3>(vertexLength, Allocator.Persistent);
                to = new NativeArray<Vector3>(vertexLength, Allocator.Persistent);
                from.CopyFrom(fromFrame.verts);
                to.CopyFrom(toFrame.verts);

                if (matrixLength > 0)
                {
                    if (exposedTransformJobs == null) exposedTransformJobs = AllocatedArray<LerpMatrix4x4Job>.Get(framesNeeded);
                    if (exposedTransformJobHandles == null) exposedTransformJobHandles = AllocatedArray<JobHandle>.Get(framesNeeded);
                    fromMatrix = new NativeArray<Matrix4x4>(vertexLength, Allocator.Persistent);
                    toMatrix = new NativeArray<Matrix4x4>(vertexLength, Allocator.Persistent);
                    fromMatrix.CopyFrom(fromFrame.exposedTransforms);
                    toMatrix.CopyFrom(toFrame.exposedTransforms);
                }

                for (int i = 0; i < framesNeeded; i++)
                {
                    output[i] = new NativeArray<Vector3>(vertexLength, Allocator.Persistent);
                    float delta = i / (float)framesNeeded;
                    var lerpJob = new LerpVector3Job()
                    {
                        from = from,
                        to = to,
                        output = output[i],
                        delta = delta
                    };
                    positionJobs[i] = lerpJob;
                    positionJobHandles[i] = lerpJob.Schedule(vertexLength, 64);

                    var boundsJob = new CalculateBoundsJob() { positions = output[i] };
                    boundsJobs[i] = boundsJob;
                    boundsJobHandles[i] = boundsJob.Schedule(positionJobHandles[i]);

                    if (matrixLength > 0)
                    {
                        outputMatrix[i] = new NativeArray<Matrix4x4>(fromFrame.exposedTransforms.Length, Allocator.Persistent);
                        var matrixJob = new LerpMatrix4x4Job()
                        {
                            from = fromMatrix,
                            to = toMatrix,
                            output = outputMatrix[i],
                            delta = delta
                        };
                        exposedTransformJobs[i] = matrixJob;
                        exposedTransformJobHandles[i] = matrixJob.Schedule(matrixLength, 64);
                    }
                }
            }
            public void ReturnFrame(bool destroying)
            {
                try
                {
                    if (positionJobHandles != null)
                    {
                        for (int i = 0; i < positionJobHandles.Length; i++)
                        {
                            if (destroying || currentFrame <= i)
                            {
                                positionJobHandles[i].Complete();
                            }
                        }
                        AllocatedArray.Return(positionJobHandles, true);
                        positionJobHandles = null;
                    }
                    if (boundsJobHandles != null)
                    {
                        for (int i = 0; i < boundsJobHandles.Length; i++)
                        {
                            if (destroying || currentFrame <= i)
                            {
                                boundsJobHandles[i].Complete();
                            }
                        }
                        AllocatedArray.Return(boundsJobHandles, true);
                        boundsJobHandles = null;
                    }
                    if (exposedTransformJobHandles != null)
                    {
                        for (int i = 0; i < exposedTransformJobHandles.Length; i++)
                        {
                            if (destroying || currentFrame <= i)
                            {
                                exposedTransformJobHandles[i].Complete();
                            }
                        }
                        AllocatedArray.Return(exposedTransformJobHandles, true);
                        exposedTransformJobHandles = null;
                    }
                    if (positionJobs != null)
                    {
                        AllocatedArray.Return(positionJobs, true);
                        positionJobs = null;
                    }
                    if (boundsJobs != null)
                    {
                        AllocatedArray.Return(boundsJobs, true);
                        boundsJobs = null;
                    }
                    if (exposedTransformJobs != null)
                    {
                        AllocatedArray.Return(exposedTransformJobs, true);
                        exposedTransformJobs = null;
                    }

                    if (output != null)
                    {
                        if (from.IsCreated)
                            from.Dispose();
                        if (to.IsCreated)
                            to.Dispose();
                        for (int i = 0; i < output.Length; i++)
                        {
                            if (output[i].IsCreated)
                                output[i].Dispose();
                        }
                        AllocatedArray.Return(output);
                        output = null;
                    }
                    if (outputMatrix != null)
                    {
                        if (fromMatrix.IsCreated)
                            fromMatrix.Dispose();
                        if (toMatrix.IsCreated)
                            toMatrix.Dispose();
                        for (int i = 0; i < outputMatrix.Length; i++)
                        {
                            if (outputMatrix[i].IsCreated)
                                outputMatrix[i].Dispose();
                        }
                        AllocatedArray.Return(outputMatrix);
                        outputMatrix = null;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        public SnapshotMeshAnimation defaultMeshAnimation;
        public SnapshotMeshAnimation[] meshAnimations;
        public bool syncCrossfadeMeshCount = false;
        public bool recalculateCrossfadeNormals = false;
        public override IMeshAnimation defaultAnimation
        {
            get
            {
                return defaultMeshAnimation;
            }

            set
            {
                defaultMeshAnimation = value as SnapshotMeshAnimation;
            }
        }
        public override IMeshAnimation[] animations { get { return meshAnimations; } }

        [SerializeField, HideInInspector]
        private MeshTriangleData[] meshTriangleData;
        [SerializeField, HideInInspector]
        private Vector2[] uv1Data;
        [SerializeField, HideInInspector]
        private Vector2[] uv2Data;
        [SerializeField, HideInInspector]
        private Vector2[] uv3Data;
        [SerializeField, HideInInspector]
        private Vector2[] uv4Data;

        private Mesh crossfadeMesh;
        private CrossfadeJobData currentCrossFade;
        private int crossFadePoolIndex = -1;

        #region Private Methods
#if USE_TRIANGLE_DATA
        // Nintendo Switch currently changes triangle ordering when built
        // so override the mesh triangles when a new instance is created
        private void Awake()
        {
            if (meshTriangleData != null)
            {
                Mesh sourceMesh = baseMesh;
                if (sourceMesh != null)
                {
                    Mesh modifiedMesh = null;
                    if (!s_modifiedMeshCache.TryGetValue(sourceMesh, out modifiedMesh))
                    {
                        modifiedMesh = Instantiate(baseMesh);
                        for (int i = 0; i < meshTriangleData.Length; i++)
                        {
                            modifiedMesh.SetTriangles(meshTriangleData[i].triangles, meshTriangleData[i].submesh);   
                        }
                        if (uv1Data != null)
                            modifiedMesh.uv = uv1Data;
                        if (uv2Data != null)
                            modifiedMesh.uv2 = uv2Data;
                        if (uv3Data != null)
                            modifiedMesh.uv3 = uv3Data;
                        if (uv4Data != null)
                            modifiedMesh.uv4 = uv4Data;
                        baseMesh = modifiedMesh;
                        s_modifiedMeshCache.Add(sourceMesh, baseMesh);
                    }
                    else
                    {
                        baseMesh = modifiedMesh;
                    }
                }
            }
        }
#endif
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ReturnCrossfadeToPool(true);
            if (!s_meshCount.ContainsKey(baseMesh))
            {
                var frames = SnapshotMeshAnimation.GeneratedFrames[baseMesh];
                foreach (var v in frames)
                {
                    for (int i = 0; i < v.Value.Length; i++)
                    {
                        DestroyImmediate(v.Value[i]);
                    }
                }
                SnapshotMeshAnimation.GeneratedFrames.Remove(baseMesh);
                if (crossFadePoolIndex > -1)
                {
                    Stack<Mesh> meshStack = null;
                    lock (s_crossFadePool)
                    {
                        meshStack = s_crossFadePool[crossFadePoolIndex];
                        s_crossFadePool.RemoveAt(crossFadePoolIndex);
                        s_crossFadeIndex.Remove(baseMesh);
                        crossFadePoolIndex = -1;
                    }
                    while (meshStack.Count > 0)
                    {
                        Destroy(meshStack.Pop());
                    }
                }
            }
            else if (syncCrossfadeMeshCount && crossFadePoolIndex > -1)
            {
                Stack<Mesh> meshStack = null;
                lock (s_crossFadePool)
                {
                    meshStack = s_crossFadePool[crossFadePoolIndex];
                }
                int meshCount = s_meshCount[baseMesh];
                while (meshStack.Count > meshCount)
                {
                    Destroy(meshStack.Pop());
                }
            }
        }
        private Mesh GetCrossfadeFromPool()
        {
            if (crossFadePoolIndex > -1)
            {
                lock (s_crossFadePool)
                {
                    Stack<Mesh> meshStack = s_crossFadePool[crossFadePoolIndex];
                    if (meshStack.Count > 0)
                        return meshStack.Pop();
                }
            }
            return (Mesh)Instantiate(baseMesh);
        }
        private void ReturnCrossfadeToPool(bool destroying)
        {
            if (crossfadeMesh != null)
            {
                Stack<Mesh> meshStack = null;
                lock (s_crossFadePool)
                {
                    if (crossFadePoolIndex < 0)
                    {
                        if (!s_crossFadeIndex.TryGetValue(baseMesh, out crossFadePoolIndex))
                        {
                            crossFadePoolIndex = s_crossFadePool.Count;
                            s_crossFadeIndex.Add(baseMesh, crossFadePoolIndex);
                            meshStack = new Stack<Mesh>();
                            s_crossFadePool.Add(meshStack);
                        }
                        else
                        {
                            meshStack = s_crossFadePool[crossFadePoolIndex];
                        }
                    }
                    else
                    {
                        meshStack = s_crossFadePool[crossFadePoolIndex];
                    }
                    meshStack.Push(crossfadeMesh);
                }
                crossfadeMesh = null;
            }
            currentCrossFade.Reset(destroying);
        }
        protected override bool DisplayFrame(int previousFrame)
        {
            if (currentFrame == previousFrame)
                return false;
            if (currentCrossFade.isFading)
            {
                if (currentCrossFade.currentFrame >= currentCrossFade.framesNeeded)
                {
                    ReturnCrossfadeToPool(false);
                    return base.DisplayFrame(previousFrame);
                }
                else
                {
                    // complete any jobs not done for the current frame
                    currentCrossFade.positionJobHandles[currentCrossFade.currentFrame].Complete();
                    currentCrossFade.boundsJobHandles[currentCrossFade.currentFrame].Complete();
                    if (hasExposedTransforms)
                    {
                        currentCrossFade.exposedTransformJobHandles[currentCrossFade.currentFrame].Complete();
                    }

                    if (crossfadeMesh == null)
                        crossfadeMesh = GetCrossfadeFromPool();
                    // copy positions from job
                    var verts = AllocatedArray<Vector3>.Get(currentAnimation.vertexCount);
                    currentCrossFade.positionJobs[currentCrossFade.currentFrame].output.CopyTo(verts);
                    crossfadeMesh.vertices = verts;
                    AllocatedArray.Return(verts);
                    // copy bounds from job
                    crossfadeMesh.bounds = currentCrossFade.boundsJobs[currentCrossFade.currentFrame].bounds;
                    // recalculate normals
                    if (recalculateCrossfadeNormals)
                        crossfadeMesh.RecalculateNormals();
                    // set mesh
                    meshFilter.sharedMesh = crossfadeMesh;

                    // set exposed transforms
                    if (hasExposedTransforms && currentCrossFade.outputMatrix != null)
                    {
                        var exposedTransforms = currentAnimation.ExposedTransforms;
                        Matrix4x4[] positions = AllocatedArray<Matrix4x4>.Get(exposedTransforms.Length);
                        currentCrossFade.outputMatrix[currentCrossFade.currentFrame].CopyTo(positions);
                        for (int i = 0; i < positions.Length; i++)
                        {
                            Transform child = null;
                            if (childMap.TryGetValue(exposedTransforms[i], out child))
                            {
                                MatrixUtils.FromMatrix4x4(child, positions[i]);
                            }
                        }
                    }
                    currentCrossFade.currentFrame++;

                    // apply root motion
                    var fromFrame = currentCrossFade.fromFrame;
                    var toFrame = currentCrossFade.toFrame;
                    bool applyRootMotion = currentAnimation.RootMotionMode == RootMotionMode.AppliedToTransform;
                    if (applyRootMotion)
                    {
                        float delta = currentCrossFade.currentFrame / (float)currentCrossFade.framesNeeded;
                        if (applyRootMotion)
                        {
                            Vector3 pos = Vector3.Lerp(fromFrame.rootMotionPosition, toFrame.rootMotionPosition, delta);
                            Quaternion rot = Quaternion.Lerp(fromFrame.rootMotionRotation, toFrame.rootMotionRotation, delta);
                            transform.Translate(pos, Space.Self);
                            transform.Rotate(rot.eulerAngles * Time.deltaTime, Space.Self);
                        }
                    }
                }
                return false;
            }
            else
            {
                return base.DisplayFrame(previousFrame);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Crossfade an animation by index
        /// </summary>
        /// <param name="index">Index of the animation</param>
        /// <param name="speed">Duration the crossfade will take</param>
        public override void Crossfade(int index, float speed)
        {
            if (index < 0 || animations.Length <= index || currentAnimIndex == index)
                return;
            currentCrossFade.Reset(false);
            currentCrossFade.framesNeeded = (int)(speed * FPS);
            currentCrossFade.isFading = true;
            currentCrossFade.endTime = Time.time + speed;
            if (currentAnimation == null)
            {
                currentCrossFade.fromFrame = defaultAnimation.GetNearestFrame(0) as SnapshotMeshFrameData;
            }
            else
            {
                currentCrossFade.fromFrame = currentAnimation.GetNearestFrame(currentFrame) as SnapshotMeshFrameData;
            }
            Play(index);
            currentCrossFade.toFrame = currentAnimation.GetNearestFrame(0) as SnapshotMeshFrameData;
            currentCrossFade.StartCrossfade(currentCrossFade.fromFrame, currentCrossFade.toFrame);
        }

        /// <summary>
        /// Populates the crossfade pool with the set amount of meshes
        /// </summary>
        /// <param name="count">Amount to fill the pool with</param>
        public override void PrepopulateCrossfadePool(int count)
        {
            Stack<Mesh> pool = null;
            lock (s_crossFadePool)
            {
                if (crossFadePoolIndex > -1)
                {
                    pool = s_crossFadePool[crossFadePoolIndex];
                    count = pool.Count - count;
                    if (count <= 0)
                        return;
                }
            }
            Mesh[] meshes = AllocatedArray<Mesh>.Get(count);
            for (int i = 0; i < count; i++)
            {
                meshes[i] = GetCrossfadeFromPool();
            }
            for (int i = 0; i < count; i++)
            {
                crossfadeMesh = meshes[i];
                ReturnCrossfadeToPool(true);
                meshes[i] = null;
            }
            AllocatedArray<Mesh>.Return(meshes);
        }

        protected override void OnCurrentAnimationChanged(IMeshAnimation meshAnimation) { }
        public override void SetAnimations(IMeshAnimation[] meshAnimations)
        {
            this.meshAnimations = new SnapshotMeshAnimation[meshAnimations.Length];
            for (int i = 0; i < meshAnimations.Length; i++)
            {
                this.meshAnimations[i] = meshAnimations[i] as SnapshotMeshAnimation;
            }
            if (meshAnimations.Length > 0 && defaultMeshAnimation == null)
                defaultMeshAnimation = this.meshAnimations[0];
        }
        public override void StoreAdditionalMeshData(Mesh mesh)
        {
            Vector2[] uv1 = baseMesh.uv;
            Vector2[] uv2 = baseMesh.uv2;
            Vector2[] uv3 = baseMesh.uv3;
            Vector2[] uv4 = baseMesh.uv4;
            if (uv1 != null && uv1.Length > 0)
                uv1Data = uv1;
            if (uv2 != null && uv2.Length > 0)
                uv2Data = uv2;
            if (uv3 != null && uv3.Length > 0)
                uv3Data = uv3;
            if (uv4 != null && uv4.Length > 0)
                uv4Data = uv4;
        }
        #endregion
    }
}