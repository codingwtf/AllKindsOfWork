using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UMP.Collections;
using UMP.Utils;
using Object = UnityEngine.Object;

namespace UMP
{
    public abstract class MeshPainter : EditorWindow
    {
        protected abstract Shader shader { get; }
        protected Material material
        {
            get
            {
                if (m_material == null)
                {
                    m_material = new Material(shader);
                }
                return m_material;
            }
        }

        protected Material m_material;

        protected Dictionary<GameObject, MeshTile> loadedTiles = new Dictionary<GameObject, MeshTile>();

        protected virtual IRandomAccessEnumerable<int, Brush> brushes
        {
            get;
        }
        protected int brushIdx = 0;
        protected Vector2 scrollPos = Vector2.zero;
        protected bool brushOutline = false;

        bool isDrawing
        {
            get
            {
                Event e = Event.current;
                return e.isMouse && e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag);
            }
        }

        protected abstract MeshTile GetTile(GameObject gameObject);
        
        protected virtual void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            OperationStash.Clear();
        }

        protected virtual void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            foreach (var tile in loadedTiles.Values)
            {
                tile.EndEdit();
            }
            var dirtyTiles = loadedTiles.Values.Where(tile => tile.isDirty);
            if (dirtyTiles.Count() > 0 && EditorUtility.DisplayDialog("Save Tiles", "Save Tiles?", "Yes", "Cancel"))
            {
                foreach (var tile in dirtyTiles)
                {
                    tile.Save();
                }
            }
            OperationStash.Clear();
        }

        bool brushExpanded = true;
        bool paletteExpanded = true;
        bool historyExpanded = true;
        bool tileStatusExpanded = true;
        protected virtual void OnGUI()
        {
            Shortcut();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                fontStyle = FontStyle.Bold,
            };
            brushExpanded = EditorGUILayout.Foldout(brushExpanded, "======Brush======", foldoutStyle);
            if(brushExpanded)
            {
                BrushToolbarLayout();
            }
            paletteExpanded = EditorGUILayout.Foldout(paletteExpanded, "======Palette======", foldoutStyle);
            if(paletteExpanded)
            {
                PaletteLayout();
            }
            historyExpanded = EditorGUILayout.Foldout(historyExpanded, "======History======", foldoutStyle);
            if (historyExpanded)
            {
                UndoRedoLayout();
            }
            tileStatusExpanded = EditorGUILayout.Foldout(tileStatusExpanded, "======Tile Status======", foldoutStyle);
            if(tileStatusExpanded)
            {
                TileStatusLayout();
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual void BrushToolbarLayout()
        {
            EditorGUILayout.BeginVertical();
            brushIdx = GUILayout.Toolbar(brushIdx, brushes.Select(brush => brush.title).ToArray());
            brushes[brushIdx].BrushGUILayout(position.width);
            brushOutline = EditorGUILayout.Toggle("Brush Outline", brushOutline);
            EditorGUILayout.EndVertical();
        }

        protected abstract void PaletteLayout();

        protected bool foldTileList = false;
        protected virtual void TileStatusLayout()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                Selection.objects = loadedTiles
                    .Select(tile => tile.Value.gameObject)
                    .ToArray();
            }
            if (GUILayout.Button("Select Modified"))
            {
                Selection.objects = loadedTiles
                    .Where(tile => tile.Value.isDirty)
                    .Select(tile => tile.Value.gameObject)
                    .ToArray();
            }
            if (GUILayout.Button("Deselect All"))
            {
                Selection.objects = new Object[] { };
            }
            EditorGUILayout.EndHorizontal();
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (foldTileList = EditorGUILayout.Foldout(foldTileList, "Tiles"))
            {
                Color contentColor = GUI.contentColor;
                Color modifiedColor = new Color(1, 0.5f, 0);
                Color savedColor = Color.green;
                foreach (var tile in loadedTiles.Values)
                {
                    bool selected = selectedGameObjects.Contains(tile.gameObject);
                    GUI.contentColor = tile.isDirty ? modifiedColor : savedColor;
                    bool toggle = EditorGUILayout.ToggleLeft($"{tile.gameObject.name}", selected);
                    if (toggle != selected)
                    {
                        if (toggle)
                            SelectionUtility.Add(tile.gameObject);
                        else
                            SelectionUtility.Remove(tile.gameObject);
                    }
                }
                GUI.contentColor = contentColor;
            }
            if (GUILayout.Button("Save Selected"))
            {
                foreach (var tile in loadedTiles.Values.Where(t => selectedGameObjects.Contains(t.gameObject)))
                {
                    tile.Save();
                }
            }
        }

        protected virtual void UndoRedoLayout()
        {
            //GUILayout.Label(OperationStash.status);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Undo"))
            {
                OperationStash.Undo();
            }
            if(GUILayout.Button("Redo"))
            {
                OperationStash.Redo();
            }
            EditorGUILayout.EndHorizontal();
        }

        protected abstract bool ColliderFilter(Collider collider);
        protected abstract void UpdateTile(MeshTile tile);

        bool Raycast(Ray ray, out RaycastHit hit)
        {
            var hits = Physics.RaycastAll(ray)
                .Where(h => ColliderFilter(h.collider))
                .OrderBy(h => h.distance);
            if (hits.Count() != 0)
            {
                hit = hits.First();
                return true;
            }
            else
            {
                hit = default(RaycastHit);
                return false;
            }
        }

        protected Vector3 prevPos;
        protected Vector3 prevNorm;
        protected float prevPressure;
        protected bool isPrevHover = false;

        protected virtual bool Shortcut()
        {
            Event evnt = Event.current;
            if (!evnt.alt)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            switch (evnt.keyCode)
            {
                case KeyCode.LeftBracket:
                {
                    if (evnt.shift && evnt.type == EventType.KeyDown)
                    {
                        Brush.strength = Mathf.Clamp01(Brush.strength - 0.05f);
                    }
                    else
                    {
                        Brush.radius = Brush.radius * 0.95f;
                    }
                    break;
                }
                case KeyCode.RightBracket:
                {
                    if (evnt.shift && evnt.type == EventType.KeyDown)
                    {
                        Brush.strength = Mathf.Clamp01(Brush.strength + 0.05f);
                    }
                    else
                    {
                        Brush.radius = Brush.radius * 1.05f;
                    }
                    break;
                }
                case KeyCode.Z:
                {
                    if(evnt.rawType == EventType.KeyDown)
                    {
                        OperationStash.Undo();
                    }
                    break;
                }
                case KeyCode.Y:
                {
                    if (evnt.rawType == EventType.KeyDown)
                    {
                        OperationStash.Redo();
                    }
                    break;
                }
            }
            return !evnt.alt;
        }

        static Material previewMat
        {
            get
            {
                if (m_previewMat == null)
                    m_previewMat = new Material(Shader.Find("Hidden/OTP/BrushPreview"));
                return m_previewMat;
            }
        }
        static Material m_previewMat = null;
        void DrawPreview(MeshTile tile)
        {
            previewMat.SetTexture("_BrushMask", tile.brushMask);
            previewMat.SetPass(0);
            Graphics.DrawMeshNow(tile.mesh, tile.transform.localToWorldMatrix, 0);
            if(brushOutline)
            {
                previewMat.SetPass(1);
                Graphics.DrawMeshNow(tile.mesh, tile.transform.localToWorldMatrix, 0);
            }
        }

        protected virtual void OnSceneGUI(SceneView sceneView)
        {
            Event evnt = Event.current;
            if(!Shortcut())
            {
                return;
            }
            Vector2 mousePos = evnt.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (evnt.isMouse && evnt.button == 0)
            {
                if (evnt.type == EventType.MouseDown || OperationStash.index == null)
                    OperationStash.BeginOperation();
                if (evnt.type == EventType.MouseUp)
                    OperationStash.EndOperation();
            }

            var brush = brushes[brushIdx];
            RaycastHit hit;
            Brush.DeltaDrag deltaDrag;
            if (Raycast(ray, out hit))
            {
                Vector3 curPos = hit.point;
                Vector3 curNorm = hit.normal;
                float curPressure = evnt.pressure;
                if (!isPrevHover)
                {
                    prevPos = curPos;
                    prevPressure = curPressure;
                    deltaDrag = new Brush.DeltaDrag(
                        curPos, 
                        curNorm, 
                        curPressure, 
                        prevPos, 
                        prevNorm, 
                        prevPressure); 
                }
                else
                {
                    deltaDrag = new Brush.DeltaDrag(
                        curPos, 
                        curNorm, 
                        curPressure, 
                        prevPos, 
                        prevNorm, 
                        prevPressure); 
                    prevPos = curPos;
                    prevPressure = curPressure;
                }
                isPrevHover = true;

                foreach (var collider in Physics.OverlapSphere(curPos, Brush.radius).Where(c => ColliderFilter(c)))
                {
                    var tile = GetTile(collider.gameObject);
                    if (isDrawing)
                    {
                        brush.UpdateMask(deltaDrag, tile, false);
                        OperationStash.Record(tile);
                        UpdateTile(tile);
                    }
                    else
                    {
                        brush.UpdateMask(deltaDrag, tile, true);
                    }
                    DrawPreview(tile);
                }
            }
            else
            {
                isPrevHover = false;
            }
            HandleUtility.Repaint();
            Repaint();
        }

        protected static class OperationStash
        {
            public static readonly LinkedList<Operation> operations = new LinkedList<Operation>();
            public static LinkedListNode<Operation> index { get; private set; }
            static LinkedListNode<Operation> top = null;
            static ObjectPool<Operation> operationPool = new ObjectPool<Operation>(
                () => new Operation(), 
                onDestroy: (op) => op.Dispose(), 
                onRelease: (op) => op.Release());

            public static bool operating = false;

            public static string status
            { 
                get
                {
                    System.Text.StringBuilder status = new System.Text.StringBuilder(OperationStash.operations.Count + 1);
                    foreach (var op in OperationStash.operations)
                    {
                        if (OperationStash.index.Value == op && OperationStash.top?.Value != op)
                        {
                            status.Append('^');
                        }
                        else if (OperationStash.top?.Value != op)
                        {
                            status.Append('+');
                        }
                    }
                    return status.ToString();
                }
            }

            public static void Clear()
            {
                foreach(var op in operations)
                {
                    operationPool.Release(op);
                }
                operations.Clear();
                index = null;
                top = null;
            }

            public static void BeginOperation()
            {
                operationPool.TryGet(out Operation op);
                if(top != null)
                {
                    operations.RemoveLast();
                    operationPool.Release(top.Value);
                    top = null;
                }
                while (operations.Count > 0 && operations.Last != index)
                {
                    var lastOp = operations.Last.Value;
                    operationPool.Release(lastOp);
                    operations.RemoveLast();
                }
                index = operations.AddLast(op);
                operating = true;
            }

            public static void Record(MeshTile tile)
            {
                index.Value.Stash(tile);
            }

            public static void Undo()
            {
                if (operating || index == null)
                    return;
                if(index == operations.Last && top == null)
                {
                    Operation lastOp = index.Value;
                    operationPool.TryGet(out Operation op);
                    foreach (var tile in lastOp.tiles)
                    {
                        op.Stash(tile);
                    }
                    top = operations.AddLast(op);
                }
                index.Value.Pop();
                if (index.Previous != null)
                    index = index.Previous;
            }

            public static void Redo()
            {
                if (operating || index == null)
                    return;
                if (index.Next != null && index.Next != top)
                    index = index.Next;
                index.Next?.Value?.Pop();
            }

            public static void EndOperation()
            {
                if(index.Value.tiles.Count() == 0)
                {
                    operations.RemoveLast();
                }
                operating = false;
            }
        }

        public class Operation : IDisposable
        {
            Dictionary<MeshTile, (PainterTarget, RenderTexture)[]> tileStashes = new Dictionary<MeshTile, (PainterTarget, RenderTexture)[]>();

            public IEnumerable<MeshTile> tiles => tileStashes.Keys;

            public void Stash(MeshTile tile)
            {
                if(tileStashes.ContainsKey(tile))
                {
                    return;
                }
                (PainterTarget, RenderTexture)[] stash = tile.targetTextures.Select((target) =>
                {
                    RenderTexture frontBuffer = target.targetRT.frontBuffer;
                    RenderTexture stashRT = RenderTexture.GetTemporary(frontBuffer.width, frontBuffer.height, frontBuffer.depth, frontBuffer.format);
                    Graphics.Blit(frontBuffer, stashRT);
                    return (target, stashRT);
                }).ToArray();
                tileStashes.Add(tile, stash);
            }

            public void Pop()
            {
                foreach(var tileStash in tileStashes)
                {
                    foreach(var (target, stash) in tileStash.Value)
                    {
                        Graphics.Blit(stash, target.targetRT.frontBuffer);
                    }
                }
            }

            public void Release()
            {
                tileStashes.Clear();
                foreach (var tileStash in tileStashes)
                {
                    foreach (var (_, stash) in tileStash.Value)
                    {
                        RenderTexture.ReleaseTemporary(stash);
                    }
                }
            }

            public void Dispose()
            {
                Release();
            }
        }
    }
}