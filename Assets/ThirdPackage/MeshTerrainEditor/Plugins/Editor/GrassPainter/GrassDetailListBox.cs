using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MTE
{
    internal class GrassDetailListBox : DetailListBox<GrassDetail>
    {
        protected override C DetailType => C.Grass;

        public override void NoDetailGUI()
        {
            EditorGUILayout.LabelField(StringTable.Get(C.Warning_NoGrass));

            if (GUILayout.Button(StringTable.Get(C.LoadFromFile)))
            {
                string path;
                if(Utility.OpenFileDialog(StringTable.Get(C.Open), s_assetFileFilter, out path))
                {
                    LoadDetailListFromAFile(path);
                }
            }
            if (GUILayout.Button(StringTable.Get(C.LoadFromGrassLoader)))
            {
                LoadDetailListFromGrassLoader();
            }
        }

        private static Texture2D grassPrototypeQuadPreviewTexture;
        private static Texture2D grassPrototypeStarPreviewTexture;

        private static void LoadPrototypePreviews()
        {
            var quadMesh = Resources.Load<Mesh>("Grass/Prototype_GrassQuad");
            Utility.LoadMeshPreviewAsync(quadMesh, t => { grassPrototypeQuadPreviewTexture = t; });
            var starMesh = Resources.Load<Mesh>("Grass/Prototype_GrassStar");
            Utility.LoadMeshPreviewAsync(starMesh, t => { grassPrototypeStarPreviewTexture = t; });
        }

        public override void DrawButtonBackground(int detailIndex, Rect buttonRect)
        {
            var detail = this.detailList[detailIndex];

            //draw preview texture
            LoadPrototypePreviews();
            var grassType = detail.GrassType;
            Texture grassMeshPreviewTexture = null;
            switch (grassType)
            {
                case GrassType.OneQuad:
                    grassMeshPreviewTexture = grassPrototypeQuadPreviewTexture;
                    break;
                case GrassType.ThreeQuad:
                    grassMeshPreviewTexture = grassPrototypeStarPreviewTexture;
                    break;
                case GrassType.CustomMesh:
                    Utility.LoadMeshPreviewAsync(detail.GrassMesh, t => grassMeshPreviewTexture = t);
                    break;
            }
            
            var rect = buttonRect;
            rect.min += new Vector2(4,4);
            rect.size = new Vector2(64,64);
            if (grassMeshPreviewTexture)
            {
                GUI.DrawTexture(rect, grassMeshPreviewTexture);
            }

            //draw texture
            var material = detail.Material;
            var texture = material ? material.GetTexture("_MainTex") : null;
            if (texture)
            {
                rect.min += new Vector2(32, 32);
                rect.size = new Vector2(32, 32);
                GUI.DrawTexture(rect, texture);
            }
        }

        protected override void SaveDetailList()
        {
            var path = Res.DetailDir + "SavedGrassDetailList.asset";
            var relativePath = Utility.GetUnityPath(path);
            GrassDetailList obj = ScriptableObject.CreateInstance<GrassDetailList>();
            obj.grassDetailList = (List<GrassDetail>) this.detailList;
            AssetDatabase.CreateAsset(obj, relativePath);
            AssetDatabase.LoadAssetAtPath<GrassDetailList>(relativePath);
            MTEDebug.LogFormat("GrassDetailList saved to {0}", path);
        }

        protected override void AddCallback()
        {
            GrassEditorUtilityWindow window = ScriptableObject.CreateInstance<GrassEditorUtilityWindow>();
            window.titleContent = new GUIContent($"{StringTable.Get(C.Add)} {StringTable.Get(C.Grass)}");
            window.detailList = this.detailList;
            window.IsAdding = true;
            window.OnSave = this.SaveDetailList;
            window.ShowUtility();
        }

        protected override void EditCallback()
        {
            GrassEditorUtilityWindow window = ScriptableObject.CreateInstance<GrassEditorUtilityWindow>();
            window.titleContent = new GUIContent($"{StringTable.Get(C.Edit)} {StringTable.Get(C.Grass)}");
            window.detailList = this.detailList;
            window.editingIndex = selectedIndex;
            window.IsAdding = false;
            window.OnSave = this.SaveDetailList;
            window.ShowUtility();
        }

        public void LoadDetailListFromAFile(string path)
        {
            MTEDebug.LogFormat("Loading grass detail list from <{0}>", path);
            var relativePath = Utility.GetUnityPath(path);
            var obj = AssetDatabase.LoadAssetAtPath<GrassDetailList>(relativePath);
            if (obj)
            {
                this.detailList = obj.grassDetailList;
                MTEDebug.LogFormat("Detail list loaded from {0}", path);
                if (this.detailList.Count == 0)
                {
                    MTEDebug.Log("No detail found in the detail list.");
                }
            }
            else
            {
                this.detailList = new List<GrassDetail>();
                MTEDebug.LogFormat("No detail list found in {0}.", path);
            }
        }

        public void LoadDetailListFromGrassLoader()
        {
            MTEDebug.Log("Loading details(three quad) from existing GrassInstanceList on the GrassLoader...");
            MTEDebug.Log("The min/max width/height of loaded details will use default values because MTE cannot determine them from the grass instances.");

            var grassLoader = MTEContext.TheGrassLoader;
            if (grassLoader == null)
            {
                Debug.LogWarning("No grass loader loaded. Assign it first.");
                return;
            }

            MTEDebug.Log("Remove existing details.");
            this.detailList = new List<GrassDetail>();

            var instanceList = grassLoader.grassInstanceList;
            if (instanceList.grasses != null && instanceList.grasses.Count != 0)
            {
                MTEDebug.Log("Loading details(three quads) from existing GrassInstanceList...");
                foreach (var grassStar in instanceList.grasses)
                {
                    bool found = false;
                    foreach (var d in this.detailList)
                    {
                        var gd = d as GrassDetail;
                        if (gd.Material == grassStar.Material)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }
                    var grassDetail = new GrassDetail
                    {
                        Material = grassStar.Material,
                        MinWidth = GrassDetail.DefaultMinWidth,
                        MaxWidth = GrassDetail.DefaultMaxWidth,
                        MinHeight = GrassDetail.DefaultMinHeight,
                        MaxHeight = GrassDetail.DefaultMaxHeight,
                        GrassType = GrassType.ThreeQuad
                    };
                    this.detailList.Add(grassDetail);
                }
                MTEDebug.LogFormat("{0} detail(s)(three quads) Loaded.", this.detailList.Count);
            }

            if (instanceList.quads != null && instanceList.quads.Count != 0)
            {
                MTEDebug.Log("Loading details(one quad) from existing GrassInstanceList...");
                var oldCount = this.detailList.Count;
                foreach (var quad in instanceList.quads)
                {
                    bool found = false;
                    foreach (var detail in this.detailList)
                    {
                        if (detail.Material == quad.Material)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }
                    var grassDetail = new GrassDetail
                    {
                        Material = quad.Material,
                        MinWidth = GrassDetail.DefaultMinWidth,
                        MaxWidth = GrassDetail.DefaultMaxWidth,
                        MinHeight = GrassDetail.DefaultMinHeight,
                        MaxHeight = GrassDetail.DefaultMaxHeight,
                        GrassType = GrassType.OneQuad
                    };
                    this.detailList.Add(grassDetail);
                }
                MTEDebug.LogFormat("{0} detail(s)(one quad) Loaded.", this.detailList.Count - oldCount);
            }

            if (instanceList.customMeshes != null && instanceList.customMeshes.Count > 0)
            {
                var oldCount = this.detailList.Count;
                foreach (var customMesh in instanceList.customMeshes)
                {
                    bool found = false;
                    foreach (var detail in this.detailList)
                    {
                        if (detail.Material == customMesh.Material)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }
                    var grassDetail = new GrassDetail
                    {
                        GrassMesh = customMesh.Mesh,
                        Material = customMesh.Material,
                        MinWidth = GrassDetail.DefaultMinWidth,
                        MaxWidth = GrassDetail.DefaultMaxWidth,
                        MinHeight = GrassDetail.DefaultMinHeight,
                        MaxHeight = GrassDetail.DefaultMaxHeight,
                        GrassType = GrassType.CustomMesh
                    };
                    detailList.Add(grassDetail);
                }
                MTEDebug.LogFormat("{0} detail(s)(one custom mesh) Loaded.", this.detailList.Count - oldCount);
            }

            //save details to default detail list file
            SaveDetailList();
            GrassPainter.Instance.LoadGrassDetailList();
        }
        
        private static readonly string[] s_assetFileFilter = {"detail list", "asset"};
    }
}