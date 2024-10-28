using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using Framework.Core.Manager.Config;
using UnityEngine.SceneManagement;

namespace Framework.Core.World
{
    public class WorldEditorWindow : EditorWindow
    {
        class ModelSceneInfo
        {
            public Vector3 pos; //位置
            public string parent; //父节点
        }

        #region const value

        private const string LOGTag = "WorldEditorWindow";
        private const string editorAstPath = "Assets/Scripts/Framework/Core/World/Editor/Style/";
        private const int leftBorder = 2;
        private const int rightBorder = 2;
        private const int normalSpace = 20;
        private const int border = leftBorder + rightBorder;
        private const int verticalScrollBar = 16;
        private const int helpBoxHeight = 40;
        private const int rightHandlePanelWidth = 200;

        #endregion

        #region enum

        enum ToolEnum
        {
            Terrain = 0, //处理地形
            SceneItem = 1, //场景物件
            Lightmap = 2, //光照贴图
            Postprocessing = 3, //后处理
        }

        private readonly int[] sliceChunkSizeList = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };

        private readonly string[] sliceChunkSizeDisplayList =
        {
            "32 x 32", "64 x 64", "128 x 128", "256 x 256", "512 x 512", "1024 x 1024", "2048 x 2048", "4096 x 4096"
        };

        private enum ChunkStatus
        {
            Fold = 0,
            Visible = 1,
        }

        #endregion

        #region window asset

        private Texture _terrainIcon;
        private Texture terrainIcon => _terrainIcon ??= GetTexture("editor_terrain");
        private Texture _lightmapIcon;
        private Texture lightmapIcon => _lightmapIcon ??= GetTexture("editor_lightmap");
        private Texture _postProcessIcon;
        private Texture postProcessIcon => _postProcessIcon ??= GetTexture("editor_post_process");
        private Texture _sceneItemIcon;
        private Texture sceneItemIcon => _sceneItemIcon ??= GetTexture("editor_scene_items");
        private GUIStyle _strictButton;

        private GUIStyle strictButton
        {
            get
            {
                _strictButton ??= new GUIStyle("flow node 6");
                _strictButton.alignment = TextAnchor.MiddleCenter;
                _strictButton.fontStyle = FontStyle.Bold;
                _strictButton.wordWrap = true;
                _strictButton.contentOffset = new Vector2(0, -14);
                return _strictButton;
            }
        }

        #endregion

        #region window data

        private int selectIndex = -1;
        private List<dynamic> _config;
        private List<dynamic> config => _config ??= LoadConfig();

        private GizmosHandler gizmosHandler;

        private dynamic worldConfig => selectIndex >= 0 ? config[selectIndex] : null;
        
        private WorldData worldData;

        private WorldData LoadWorldData()
        {
            var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bin";
            if(!File.Exists(savePath))
            {
                return null;
            }
            
            var data = new WorldData();
            var fs = new FileStream(savePath, FileMode.Open);
            var reader = new BinaryReader(fs);
            try
            {
                data.TerrainHeight = reader.ReadSingle();//地形高度
                data.TerrainRowCount = reader.ReadInt32();//行数
                data.TerrainColumnCount = reader.ReadInt32();//列数
                data.TerrainChunkWidth = reader.ReadSingle();//地形块尺寸宽
                data.TerrainChunkLength = reader.ReadSingle();//地形块尺寸高
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message);
            }

            reader.Close();
            fs.Close();
            AssetDatabase.Refresh();
            return data;
        }
        private string terrainName => worldConfig != null ? $"Terrain_{worldConfig["id"]}" : "UnknownTerrain";
        
        private bool hasSourceTerrain;
        
        private string worldName => worldConfig != null ? worldConfig["worldName"] : "UnknownWorld";
        
        private Terrain terrain;
        private Vector2 colliderSize;
        private Vector2 chunkSize;
        private Vector2 worldScrollPosition;
        private Vector2 sceneScrollPosition;
        private float colliderHeight;
        private bool showColliderSettings;
        private int chunkSizeIndex = 3; //默认[256 x 256]
        private bool hasTerrainChunks;
        private bool drawColliderBoxesGizmos;
        private Color colliderBoxesGizmosColor = new(1, 0.4f, 0.7f, 1);
        private bool showSceneScroll;
        private bool showModelPrefabsScroll;
        private List<GameObject> _itemList;
        private Vector2 modelPrefabsScrollPosition;
        private Dictionary<int, Dictionary<string, int[]>> _worldChunkStatusDict;
        private Dictionary<int, Dictionary<string, int[]>> worldChunkStatusDict => _worldChunkStatusDict ??= new Dictionary<int, Dictionary<string, int[]>>();
        private GUISkin _guiSkin;
        private GUISkin guiSkin => _guiSkin ??= (GUISkin)AssetDatabase.LoadAssetAtPath($"{editorAstPath}gui_skin.guiskin", typeof(GUISkin));
        private static float screenScale => Screen.dpi / DEF.SYSTEM_STANDARD_DPI;
        private Rect _windowSize;
        private static Rect windowSize => new Rect(0, 0, Screen.width / screenScale, Screen.height / screenScale);
        private bool startEdit;
        private static WorldEditorWindow window;
        private int selectToolId;
        private bool showSourceTerrain;
        private bool showSlicedTerrainChunks;
        private bool showSliceTerrain;
        private bool showWorldList = true;
        private bool showWorldInfo;
        private bool showWorldCleaner;
        private bool showTerrainChunkList;
        private Vector2 terrainChunksPosition;
        private readonly Dictionary<string, bool> prefabModelsFoldStatus = new();
        private readonly Dictionary<string, List<ModelInfo>> markDeleteDict = new();
        private int drawPrefabModelsTimes;
        private readonly Dictionary<ModelInfo, ModelSceneInfo> modelInfoDict = new();
        private readonly List<ModelInfo> modifyModelList = new();
        private bool modelSwitchFoldStatus;
        private bool modelSwitchFoldStatusTrigger;
        private bool onSceneHandle;
        private bool itemChunksLoaded;

        #endregion

        #region scene node

        private Transform _envRoot;
        private Transform envRoot
        {
            get
            {
                GameObject root = GameObject.Find(DEF.ENV_ROOT);
                _envRoot = root == null ? CreateNode(DEF.ENV_ROOT) : root.transform;
                if (_envRoot.GetComponent<GizmosHandler>() == null)
                {
                    gizmosHandler = _envRoot.gameObject.AddComponent<GizmosHandler>();
                }
                return _envRoot;
            }
        }
        // private Transform _colliderRoot;
        // private Transform colliderRoot
        // {
        //     get
        //     {
        //         Transform trs = envRoot.Find(DEF.COLLIDER_ROOT);
        //         _colliderRoot = trs == null ? CreateNode(DEF.COLLIDER_ROOT, envRoot) : trs;
        //         return _colliderRoot;
        //     }
        // }
        // private Transform _itemRoot;
        // private Transform itemRoot
        // {
        //     get
        //     {
        //         Transform trs = envRoot.Find(DEF.ITEM_ROOT);
        //         _itemRoot = trs == null ? CreateNode(DEF.ITEM_ROOT, envRoot) : trs;
        //         return _itemRoot;
        //     }
        // }
        // private Transform _terrainRoot;
        // private Transform terrainRoot
        // {
        //     get
        //     {
        //         Transform trs = envRoot.Find(DEF.TERRAIN_ROOT);
        //         _terrainRoot = trs == null ? CreateNode(DEF.TERRAIN_ROOT, envRoot) : trs;
        //         return _terrainRoot;
        //     }
        // }

        #endregion

        #region hangler

        private TerrainHandler _terrainHandler;
        private TerrainHandler terrainHandler => _terrainHandler ??= new TerrainHandler();
        private ItemHandler _itemHandler;
        private ItemHandler itemHandler => _itemHandler ??= new ItemHandler();
        private LightmapHandler _lightmapHandler;
        private LightmapHandler lightmapHandler => _lightmapHandler ??= new LightmapHandler();

        #endregion

        [MenuItem("Tools/世界编辑器")]
        public static void OpenWorldEditorWindow()
        {
            window = GetWindow<WorldEditorWindow>("World Editor");
            window.Show();
            // var position = window.position;
            window.minSize = new Vector2(560, 510);
            window.Init();
        }
        // private Editor editor;
        // private Transform transform;
        private void Init()
        {
            //打开WorldEditorScene
            const string path = "Assets/Scenes/WorldEditor.unity";
            var sceneName = Path.GetFileNameWithoutExtension(path);
            var bIsCurScene = SceneManager.GetActiveScene().name.Equals(sceneName);//是否为当前场景
            if (!bIsCurScene)
            {
                EditorSceneManager.OpenScene(path);
            }
            terrain = null;
            _config = null;
            selectIndex = -1;
            // _terrainRoot = null;
            // _itemRoot = null;
            // _colliderRoot = null;
            _envRoot = null;
            _terrainHandler = null;
            _lightmapHandler = null;
            
            SceneView.duringSceneGui += OnSceneGUI;
            // transform = target as Transform;
            // editor = Editor.CreateEditor(target, Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.TransformInspector",true));
        }

        #region Render Tool View

        //地形工具
        private void DrawTerrainToolPanel()
        {
            GUILayout.BeginVertical();
            showSourceTerrain = EditorGUILayout.BeginFoldoutHeaderGroup(showSourceTerrain, "Source Terrain");
            if (showSourceTerrain)
            {
                if (hasSourceTerrain)
                {
                }
                //加载地形
                GUILayout.BeginHorizontal();
                GUILayout.Space(normalSpace);
                if (GUILayout.Button("Load Source Terrain", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                {
                    // LogManager.Log(LOGTag,skin.button.normal.background);
                    void Callback(GameObject go)
                    {
                        go.name = terrainName;
                        LogManager.Log(LOGTag, "Load terrain finished");
                        hasSourceTerrain = true;
                        showSliceTerrain = true;
                    }

                    terrain = TerrainHandler.LoadSingleTerrain(envRoot, worldConfig["terrainAssetPath"],(Action<GameObject>)Callback);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(normalSpace);
                //删除地形
                if (GUILayout.Button("Unload Source Terrain", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                {
                    hasSourceTerrain = false;
                    if (terrain != null)
                    {
                        DestroyImmediate(terrain.gameObject);
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            var usedHeight = 5 + 45 + (showSourceTerrain ? 70 : 25) + (hasSourceTerrain ? (showSliceTerrain ? 71 : 28) : 9) + (showSlicedTerrainChunks ? 70 : 25) + (showColliderSettings ? 195 : 27) + ((showColliderSettings && drawColliderBoxesGizmos) ? 25 : 5) + 25;
            if (hasSourceTerrain)
            {
                showSliceTerrain = EditorGUILayout.BeginFoldoutHeaderGroup(showSliceTerrain, "Slice Terrain");
                if (showSliceTerrain)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(normalSpace);
                    GUILayout.Label("Chunk Size");
                    chunkSizeIndex = EditorGUILayout.Popup(chunkSizeIndex, sliceChunkSizeDisplayList);
                    GUILayout.EndHorizontal();
                    //拆分地形
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(normalSpace);
                    if (GUILayout.Button("Slice Source Terrain", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                    {
                        if (chunkSizeIndex >= 0 && chunkSizeIndex <= sliceChunkSizeList.Length - 1)
                        {
                            var size = sliceChunkSizeList[chunkSizeIndex];
                            var terrainData = terrain.terrainData;
                            var rows = (int)terrainData.size.x / size;
                            var columns = (int)terrainData.size.z / size;
                            terrainHandler.SplitTerrain(terrain, worldName, rows,columns);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            showSlicedTerrainChunks = EditorGUILayout.BeginFoldoutHeaderGroup(showSlicedTerrainChunks, "Sliced Terrain Chunks");
            if (showSlicedTerrainChunks)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(normalSpace);
                //加载拆分地形
                if (GUILayout.Button("Load All Terrain Chunks", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                {
                    if (!hasTerrainChunks)
                    {
                        terrainHandler.LoadSplitTerrain(worldName, envRoot, () =>
                        {
                            LogManager.Log(LOGTag, "All terrain chunks load finished");
                            if (terrain != null)
                            {
                                terrain.gameObject.SetActive(false);
                            }
                            hasTerrainChunks = true;
                            colliderSize = new Vector2(0,0);
                            colliderHeight = worldData.TerrainHeight;
                            chunkSize = new Vector2(worldData.TerrainChunkWidth, worldData.TerrainChunkLength);
                        });
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(normalSpace);
                //加载拆分地形
                if (GUILayout.Button("Unload All Terrain Chunks", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                {
                    if (hasTerrainChunks)
                    {
                        terrainHandler.ClearSplitTerrains();
                        hasTerrainChunks = false;
                        colliderSize = Vector2.zero;
                        chunkSize = Vector2.zero;
                        colliderHeight = 0;
                        terrainHandler.ClearColliderBoxes();
                        gizmosHandler.colliderList.Clear();
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (hasTerrainChunks)
            {
                showTerrainChunkList = EditorGUILayout.BeginFoldoutHeaderGroup(showTerrainChunkList, "Terrain Chunks");
                if (showTerrainChunkList)
                {
                    if (_terrainHandler != null && _terrainHandler.terrainList != null && _terrainHandler.terrainList.Count > 0)
                    {
                        terrainChunksPosition = GUILayout.BeginScrollView(terrainChunksPosition, false, true, GUILayout.Height(windowSize.height - usedHeight));
                        var c = GUI.backgroundColor;
                        for (var i = 0; i < _terrainHandler.terrainList.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUI.backgroundColor = i % 2 == 0 ? Color.white : Color.black;
                            GUILayout.Space(normalSpace);
                            var tName = _terrainHandler.terrainList[i].name;
                            GUILayout.Box(new GUIContent(EditorGUIUtility.IconContent("d_Terrain Icon").image), guiSkin.box, GUILayout.Width(16), GUILayout.Height(20));
                            GUILayout.Box("", guiSkin.box, GUILayout.Width(4), GUILayout.Height(20));
                            GUILayout.Label(tName, guiSkin.box, GUILayout.Width(windowSize.width - 20 - 2 * normalSpace - verticalScrollBar), GUILayout.Height(20));
                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_ToolHandleCenter").image, "Focus Terrain Chunk"), guiSkin.box, GUILayout.Height(normalSpace), GUILayout.Width(normalSpace)))
                            {
                                TerrainHandler.FocusTerrain(_terrainHandler.terrainList[i].transform);
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUI.backgroundColor = c;
                        GUILayout.EndScrollView();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (worldData != null)
                {
                    // LogManager.Log(LOGTag,worldData.ToString());
                    showColliderSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showColliderSettings, "Collider Settings");
                    if (showColliderSettings)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        colliderSize = EditorGUILayout.Vector2Field("Collider Size", colliderSize);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        colliderHeight = EditorGUILayout.FloatField("Collider Height", colliderHeight);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        drawColliderBoxesGizmos = EditorGUILayout.Toggle("Draw Boxes Gizmos", drawColliderBoxesGizmos);
                        gizmosHandler.drawColliderBoxesGizmos = drawColliderBoxesGizmos;
                        GUILayout.EndHorizontal();
                        if (drawColliderBoxesGizmos)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(normalSpace);
                            colliderBoxesGizmosColor = EditorGUILayout.ColorField("Gizmos color", colliderBoxesGizmosColor);
                            gizmosHandler.colliderBoxesGizmosColor = colliderBoxesGizmosColor;
                            GUILayout.EndHorizontal();
                        }
                        if (colliderHeight < worldData.TerrainHeight)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(normalSpace);
                            EditorGUILayout.HelpBox("Collider height must be larger than the terrain height!", MessageType.Warning, true);
                            GUILayout.EndHorizontal();
                        }
                        if (colliderSize.x < chunkSize.x || colliderSize.y < chunkSize.y)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(normalSpace);
                            EditorGUILayout.HelpBox("Collider size must be larger than the terrain chunk size!", MessageType.Warning, true);
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        //生成碰撞盒
                        if (GUILayout.Button("Create Collider Boxes", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                        {
                            terrainHandler.GenColliderBoxes(envRoot, worldData.TerrainRowCount, worldData.TerrainColumnCount, chunkSize, colliderSize, worldData.TerrainHeight, () =>
                            {
                                LogManager.Log(LOGTag, "Create collider boxes finished");
                                gizmosHandler.colliderList = new List<GameObject>(terrainHandler.colliderList);
                            });
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        if (GUILayout.Button("Load Collider Boxes", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                        {
                            terrainHandler.LoadColliderBoxes(envRoot, worldName, worldData.TerrainRowCount, worldData.TerrainColumnCount, chunkSize, () =>
                            {
                                LogManager.Log(LOGTag, "Load collider boxes form bytes file finished");
                                gizmosHandler.colliderList = new List<GameObject>(terrainHandler.colliderList);
                            });
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        if (GUILayout.Button("Save Collider Data", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                        {
                            TerrainHandler.SaveColliderBoxes(envRoot, worldName, worldData.TerrainRowCount, worldData.TerrainColumnCount, () => { LogManager.Log(LOGTag, "Save collider data finished"); });
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        if (GUILayout.Button("Clear Collider Boxes", GUILayout.Width(windowSize.width - normalSpace - border), GUILayout.Height(20)))
                        {
                            terrainHandler.ClearColliderBoxes(() =>
                            {
                                LogManager.Log(LOGTag, "Clear collider boxes finished");
                                gizmosHandler.colliderList = new List<GameObject>(terrainHandler.colliderList);
                            });
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
            GUILayout.EndVertical();
        }

        //Item工具
        private void DrawItemToolPanel()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(windowSize.width - rightHandlePanelWidth));
            showSceneScroll = EditorGUILayout.BeginFoldoutHeaderGroup(showSceneScroll, "Scene Items");
            var usableHeight = (int)windowSize.height - 20 * 6 - 12 - ((!hasTerrainChunks && !hasSourceTerrain) ? helpBoxHeight : 0);
            if (showSceneScroll)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(normalSpace);
                sceneScrollPosition = GUILayout.BeginScrollView(sceneScrollPosition, false, true, GUILayout.Width(windowSize.width - normalSpace - rightHandlePanelWidth), GUILayout.Height(showModelPrefabsScroll ? usableHeight / 2f : usableHeight));
                if (!worldChunkStatusDict.ContainsKey(selectIndex))
                {
                    worldChunkStatusDict.Add(selectIndex, new Dictionary<string, int[]>());
                }
                var chunkStatusDict = worldChunkStatusDict[selectIndex];
                var c = GUI.backgroundColor;
                var line = 0;
                for (var row = 0; row < worldData.TerrainRowCount; row++)
                {
                    for (var col = 0; col < worldData.TerrainColumnCount; col++)
                    {
                        GUILayout.BeginHorizontal();
                        var chunkName = $"Chunk_{row}_{col}";
                        if (!chunkStatusDict.ContainsKey(chunkName))
                        {
                            chunkStatusDict.Add(chunkName, new int[2]);
                        }
                        line++;
                        GUI.backgroundColor = line % 2 == 0 ? Color.white : Color.black;
                        var unfold = chunkStatusDict[chunkName][(int)ChunkStatus.Fold] == DEF.TRUE;
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(unfold ? "IN foldout on" : "IN foldout").image), guiSkin.box, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            chunkStatusDict[chunkName][(int)ChunkStatus.Fold] = 1 - chunkStatusDict[chunkName][(int)ChunkStatus.Fold];
                        }
                        if (GUILayout.Button(chunkName, guiSkin.box, GUILayout.Height(20), GUILayout.Width(windowSize.width - 20 * 4 - rightBorder - 9.4f - normalSpace - border - rightHandlePanelWidth)))
                        {
                            chunkStatusDict[chunkName][(int)ChunkStatus.Fold] = 1 - chunkStatusDict[chunkName][(int)ChunkStatus.Fold];
                        }
                        GUILayout.FlexibleSpace();
                        var visible = chunkStatusDict[chunkName][(int)ChunkStatus.Visible] == DEF.TRUE;
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(visible ? "d_animationvisibilitytoggleon" : "d_animationvisibilitytoggleoff").image, "Display or Hide Chunk Items"), guiSkin.box, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            chunkStatusDict[chunkName][(int)ChunkStatus.Visible] = 1 - chunkStatusDict[chunkName][(int)ChunkStatus.Visible];
                            if (visible)
                            {
                                itemHandler.UnloadItemChunk(envRoot, row, col,() => { LogManager.Log(LOGTag, $"{chunkName} items unload finished"); });
                            } else
                            {
                                itemHandler.LoadItemChunk(envRoot, worldName, row, col, chunkSize, () => { LogManager.Log(LOGTag, $"{chunkName} items load finished"); });
                            }
                        }
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_ToolHandleCenter").image, "Focus Chunk Items"), guiSkin.box, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            ItemHandler.FocusItemChunk(envRoot, row, col, chunkSize);
                        }
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("SceneSaveGrey").image, "Save Chunk Items"), guiSkin.box, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            itemHandler.SaveItemChunk(envRoot, worldName, row, col, () => { LogManager.Log(LOGTag, $"{chunkName} items save success"); });
                        }
                        GUILayout.Space(rightBorder);
                        GUILayout.EndHorizontal();
                        if (chunkStatusDict[chunkName][(int)ChunkStatus.Fold] != DEF.TRUE) continue;
                        var itemName = $"{row}_{col}";
                        if (!itemHandler.chunkItemsDict.ContainsKey(itemName))
                        {
                            itemHandler.chunkItemsDict.Add(itemName, new List<ModelInfo>());
                        }
                        var usableW = windowSize.width - normalSpace * 4 - 17.5f - verticalScrollBar - rightHandlePanelWidth;
                        for (var i = 0; i < itemHandler.chunkItemsDict[itemName].Count; i++)
                        {
                            line++;
                            GUI.backgroundColor = line % 2 == 0 ? Color.white : Color.black;
                            GUILayout.BeginHorizontal();
                            GUILayout.Box("", guiSkin.box, GUILayout.Height(normalSpace), GUILayout.Width(normalSpace));
                            var mi = itemHandler.chunkItemsDict[itemName][i];
                            GUILayout.Box(new GUIContent(EditorGUIUtility.IconContent(mi.suffix == "fbx" ? "d_PrefabModel Icon" : "d_Prefab Icon")), guiSkin.box, GUILayout.Width(18), GUILayout.Height(normalSpace));
                            if (mi.go != null)
                            {
                                var go = mi.go;
                                GUILayout.Box(go.name, guiSkin.box, GUILayout.Width(usableW), GUILayout.Height(normalSpace));
                            } else
                            {
                                if (!markDeleteDict.ContainsKey(itemName))
                                {
                                    markDeleteDict.Add(itemName, new List<ModelInfo>());
                                }
                                if (!markDeleteDict[itemName].Contains(mi))
                                {
                                    markDeleteDict[itemName].Add(mi);
                                }
                            }
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_ToolHandleCenter")), guiSkin.box, GUILayout.Width(normalSpace), GUILayout.Height(normalSpace)))
                            {
                                if (mi.go != null)
                                {
                                    Selection.activeGameObject = mi.go;
                                    SceneView.FrameLastActiveSceneView();
                                }
                            }
                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_winbtn_win_close@2x")), guiSkin.box, GUILayout.Width(normalSpace), GUILayout.Height(normalSpace)))
                            {
                                if (mi.go != null)
                                {
                                    var m = itemHandler.chunkItemsDict[itemName].Find(obj => obj.go == mi.go);
                                    itemHandler.chunkItemsDict[itemName].Remove(m);
                                    DestroyImmediate(mi.go);
                                }
                            }
                            GUILayout.Space(2);
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUI.backgroundColor = c;
                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(rightHandlePanelWidth));
            showSceneScroll = EditorGUILayout.BeginFoldoutHeaderGroup(showSceneScroll, "Global Handler");
            if (showSceneScroll)
            {
                if (GUILayout.Button("Load All", GUILayout.Width(rightHandlePanelWidth - border * 2)))
                {
                    if (!itemChunksLoaded)
                    {
                        itemHandler.LoadAllItemChunks(envRoot, worldName, worldData.TerrainRowCount, worldData.TerrainColumnCount, chunkSize, () =>
                        {
                            LogManager.Log(LOGTag, "All item chunks load finished!");
                            itemChunksLoaded = true;
                        });
                        if (!worldChunkStatusDict.ContainsKey(selectIndex))
                        {
                            worldChunkStatusDict.Add(selectIndex, new Dictionary<string, int[]>());
                        }
                        var chunkStatusDict = worldChunkStatusDict[selectIndex];
                        for (var row = 0; row < worldData.TerrainRowCount; row++)
                        {
                            for (var col = 0; col < worldData.TerrainColumnCount; col++)
                            {
                                var chunkName = $"Chunk_{row}_{col}";
                                if (!chunkStatusDict.ContainsKey(chunkName))
                                {
                                    chunkStatusDict.Add(chunkName, new int[2]);
                                }
                                chunkStatusDict[chunkName][(int)ChunkStatus.Visible] = 1;
                            }
                        }
                    }
                }
                if (GUILayout.Button("Clear All", GUILayout.Width(rightHandlePanelWidth - border * 2)))
                {
                    itemChunksLoaded = false;
                    itemHandler.UnloadAllItemChunks(envRoot, worldData.TerrainRowCount, worldData.TerrainColumnCount, () => { LogManager.Log(LOGTag, "All item chunks unload finished!"); });
                    var chunkStatusDict = worldChunkStatusDict[selectIndex];
                    for (var row = 0; row < worldData.TerrainRowCount; row++)
                    {
                        for (var col = 0; col < worldData.TerrainColumnCount; col++)
                        {
                            var chunkName = $"Chunk_{row}_{col}";
                            if (!chunkStatusDict.ContainsKey(chunkName))
                            {
                                chunkStatusDict.Add(chunkName, new int[2]);
                            }
                            chunkStatusDict[chunkName][(int)ChunkStatus.Visible] = 0;
                        }
                    }
                }
                if (GUILayout.Button("Save All", GUILayout.Width(rightHandlePanelWidth - border * 2)))
                {
                    itemHandler.SaveAllItemChunks(envRoot, worldName, worldData.TerrainRowCount, worldData.TerrainColumnCount, () => { LogManager.Log(LOGTag, "Save succeed!"); });
                }
                if (GUILayout.Button("Switch All Fold", GUILayout.Width(rightHandlePanelWidth - border * 2)))
                {
                    if (worldChunkStatusDict.ContainsKey(selectIndex))
                    {
                        foreach (var kvp in worldChunkStatusDict[selectIndex])
                        {
                            kvp.Value[(int)ChunkStatus.Fold] = 1 - kvp.Value[(int)ChunkStatus.Fold];
                        }
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //模型预制体
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(windowSize.width - rightHandlePanelWidth));
            showModelPrefabsScroll = EditorGUILayout.BeginFoldoutHeaderGroup(showModelPrefabsScroll, "Model Prefabs");
            if (showModelPrefabsScroll)
            {
                modelPrefabsScrollPosition = GUILayout.BeginScrollView(modelPrefabsScrollPosition, false, true, GUILayout.Width(windowSize.width - rightHandlePanelWidth), GUILayout.Height(showSceneScroll ? usableHeight / 2f : usableHeight));
                var c = GUI.backgroundColor;
                var temp = itemHandler.modelAssetRoot;
                DrawPrefabModelsFold(temp, 0);
                modelSwitchFoldStatusTrigger = false;
                GUI.backgroundColor = c;
                GUILayout.EndScrollView();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(rightHandlePanelWidth));
            showModelPrefabsScroll = EditorGUILayout.BeginFoldoutHeaderGroup(showModelPrefabsScroll, "Model Prefabs Handler");
            if (showModelPrefabsScroll)
            {
                if (GUILayout.Button("Refresh Models", GUILayout.Width(rightHandlePanelWidth - border * 2)))
                {
                    // itemHandler.LoadAllItems(itemRoot,worldName,chunkSliceX,chunkSliceY);
                    itemHandler.ResetModelPrefabs();
                }
                if (GUILayout.Button("Switch All Fold", GUILayout.Width(rightHandlePanelWidth - border * 2)))
                {
                    modelSwitchFoldStatus = !modelSwitchFoldStatus;
                    modelSwitchFoldStatusTrigger = true;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (!hasTerrainChunks && !hasSourceTerrain)
            {
                EditorGUILayout.HelpBox("There is no terrain in the scene!", MessageType.Warning, true);
            }
            GUILayout.EndVertical();
        }

        //Lightmap工具
        private void DrawLightmapToolPanel()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lighting Settings Asset");
            lightmapHandler.lightingSettings = (LightingSettings)EditorGUILayout.ObjectField(lightmapHandler.lightingSettings, typeof(LightingSettings));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Generate Lightmap Data"))
            {
                // lightmapHandler.GenLightmapData(terrainRoot, itemRoot, worldName, worldData.TerrainRowCount, worldData.TerrainColumnCount, () => { LogManager.Log(LOGTag, "光照数据生成完成"); });
            }
            if (GUILayout.Button("Load Lightmap Data"))
            {
                // lightmapHandler.LoadLightmapData(terrainRoot, itemRoot, worldName, worldData.TerrainRowCount, worldData.TerrainColumnCount, () => { LogManager.Log(LOGTag, "光照数据加载完成"); });
            }
            GUILayout.EndVertical();
        }
        //后处理工具面板
        private void DrawPostprocessingToolPanel()
        {
        }
        private void OnSceneGUI(SceneView sv)
        {
            if (Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDrag:
                        CheckAndRecordPositionChange();
                        break;
                    case EventType.MouseUp:
                        ApplyPositionChangeAndUpdateParent();
                        break;
                }
            }
            onSceneHandle = true;
        }
        
        //场景内物件位置数据发生变化
        private void CheckAndRecordPositionChange()
        {
            if (worldData == null)
            {
                return;
            }
            for (var row = 0; row < worldData.TerrainRowCount; row++)
            {
                for (var col = 0; col < worldData.TerrainColumnCount; col++)
                {
                    var chunkName = $"{row}_{col}";
                    if (!itemHandler.chunkItemsDict.ContainsKey(chunkName))
                    {
                        itemHandler.chunkItemsDict.Add(chunkName, new List<ModelInfo>());
                    }
                    for (var i = 0; i < itemHandler.chunkItemsDict[chunkName].Count; i++)
                    {
                        var mi = itemHandler.chunkItemsDict[chunkName][i];
                        if (mi.go == null) continue;
                        if (!modelInfoDict.ContainsKey(mi))
                        {
                            modelInfoDict.Add(mi, new ModelSceneInfo());
                        }
                        if (mi.go.transform.position == modelInfoDict[mi].pos) continue;
                        modelInfoDict[mi].pos = mi.go.transform.position;
                        modelInfoDict[mi].parent = mi.go.transform.parent.name;
                        if (!modifyModelList.Contains(mi))
                        {
                            modifyModelList.Add(mi);
                        }
                    }
                }
            }
        }
        
        //(数据层)应用位置的变更,更新位置变更之后的新父节点
        private void ApplyPositionChangeAndUpdateParent()
        {
            foreach (var mi in modifyModelList)
            {
                if (!modelInfoDict.ContainsKey(mi)) continue;
                Vector2 newParent = itemHandler.CheckParentChunk(modelInfoDict[mi].pos, worldData.TerrainRowCount, worldData.TerrainColumnCount, chunkSize);
                if (!(newParent.x >= 0) || !(newParent.y >= 0)) continue;
                string np = $"{(int)newParent.y}_{(int)newParent.x}";
                if (modelInfoDict[mi].parent == np) continue;
                itemHandler.chunkItemsDict[np].Add(mi);
                itemHandler.chunkItemsDict[modelInfoDict[mi].parent].Remove(mi);
                modelInfoDict[mi].parent = np;
                // mi.go.transform.SetParent(itemRoot.Find(np));
            }
            modifyModelList.Clear();
        }
        private void OnInspectorUpdate()
        {
            if (!onSceneHandle)
            {
                CheckAndRecordPositionChange();
                ApplyPositionChangeAndUpdateParent();
            }
            onSceneHandle = false;
        }
        //绘制模型文件夹
        private void DrawPrefabModelsFold(FolderLevelNode node, int subLev)
        {
            drawPrefabModelsTimes++;
            var c = drawPrefabModelsTimes % 2 == 0 ? Color.white : Color.black;
            GUI.backgroundColor = c;
            var isRoot = subLev == 0;
            subLev++;
            var prefixSpaceWidth = (subLev - 2) * normalSpace;
            if (node.subdirectories.Count > 0)
            {
                bool showChild;
                if (!isRoot)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(normalSpace);
                    GUILayout.Box("", guiSkin.box, GUILayout.Height(normalSpace), GUILayout.Width(prefixSpaceWidth));
                    prefabModelsFoldStatus.TryAdd(node.uid, false);
                    // var fold = prefabModelsFoldStatus[node.uid];
                    if (modelSwitchFoldStatusTrigger)
                    {
                        prefabModelsFoldStatus[node.uid] = modelSwitchFoldStatus;
                    }
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(prefabModelsFoldStatus[node.uid] ? "IN foldout on" : "IN foldout").image), guiSkin.box, GUILayout.Height(normalSpace), GUILayout.Width(15)))
                    {
                        prefabModelsFoldStatus[node.uid] = !prefabModelsFoldStatus[node.uid];
                    }
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(prefabModelsFoldStatus[node.uid] ? "d_FolderOpened Icon" : "d_Folder Icon").image), guiSkin.box, GUILayout.Height(normalSpace), GUILayout.Width(normalSpace)))
                    {
                        prefabModelsFoldStatus[node.uid] = !prefabModelsFoldStatus[node.uid];
                    }
                    if (GUILayout.Button(node.name, guiSkin.box, GUILayout.Width(windowSize.width - subLev * normalSpace - rightHandlePanelWidth - normalSpace - 8), GUILayout.Height(normalSpace)))
                    {
                        prefabModelsFoldStatus[node.uid] = !prefabModelsFoldStatus[node.uid];
                    }
                    showChild = prefabModelsFoldStatus[node.uid];
                    GUILayout.EndHorizontal();
                } else
                {
                    showChild = true;
                }
                if (showChild)
                {
                    foreach (var subNode in node.subdirectories)
                    {
                        DrawPrefabModelsFold(subNode, subLev);
                    }
                }
            } else
            {
                if (!isRoot)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(normalSpace);
                    GUILayout.Box("", guiSkin.box, GUILayout.Height(normalSpace), GUILayout.Width(prefixSpaceWidth));
                    var splits = node.name.Split('.');
                    var suffix = splits.Last().ToLower();
                    GUILayout.Box(new GUIContent(EditorGUIUtility.IconContent(suffix == "fbx" ? "d_PrefabModel Icon" : "d_Prefab Icon").image), guiSkin.box, GUILayout.Width(18), GUILayout.Height(normalSpace));
                    GUILayout.Box(node.name, guiSkin.box, GUILayout.Width(windowSize.width - normalSpace - rightHandlePanelWidth - verticalScrollBar - 15 - 20 - prefixSpaceWidth), GUILayout.Height(normalSpace));
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("ToolBar Plus").image, "Add model to scene"), guiSkin.box, GUILayout.Height(normalSpace), GUILayout.Width(normalSpace)))
                    {
                        //add to scene [screen pos => world pos]
                        var assetPath = $"{DEF.RESOURCES_ASSETS_PATH}Models/{node.uid}";
                        // itemHandler.LoadModelPrefab(itemRoot, assetPath, worldData.TerrainRowCount, worldData.TerrainColumnCount, chunkSize);
                    }
                    // GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            GUI.backgroundColor = c;
        }
        private void OnGUI()
        {
            if (startEdit && selectIndex >= 0)
            {
                // 选项组,Terrain,Lightmap,Item
                selectToolId = GUILayout.Toolbar(selectToolId, new[] { terrainIcon, sceneItemIcon, lightmapIcon, postProcessIcon }, GUILayout.Width(windowSize.width - 6), GUILayout.Height(40));
                switch (selectToolId)
                {
                    case (int)ToolEnum.Terrain:
                        DrawTerrainToolPanel();
                        break;
                    case (int)ToolEnum.SceneItem:
                        DrawItemToolPanel();
                        break;
                    case (int)ToolEnum.Lightmap:
                        DrawLightmapToolPanel();
                        break;
                    case (int)ToolEnum.Postprocessing:
                        DrawPostprocessingToolPanel();
                        break;
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear Scene", GUILayout.Width(windowSize.width - 6), GUILayout.Height(normalSpace)))
                {
                    ClearScene();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("Back To Select World", GUILayout.Width(windowSize.width - 6), GUILayout.Height(normalSpace)))
                {
                    startEdit = false;
                }
                GUILayout.Space(5);
            } 
            else
            {
                GUILayout.BeginVertical();
                showWorldList = EditorGUILayout.BeginFoldoutHeaderGroup(showWorldList, "World List");
                var warn = false;
                if (selectIndex >= 0 && showWorldInfo)
                {
                    warn = !TerrainHandler.CheckSourceTerrainAsset(worldConfig["terrainAssetPath"]);
                }
                if (showWorldList)
                {
                    var color = GUI.backgroundColor;
                    var usableHeight = 20 + (selectIndex >= 0 ? (showWorldCleaner ? 42 : 20) : 0) + (showWorldInfo ? 60 : 0) + 30 + (selectIndex > 0 ? 48 : (helpBoxHeight + 28)) + (warn ? helpBoxHeight : 0);
                    worldScrollPosition = GUILayout.BeginScrollView(worldScrollPosition, false, true, GUILayout.Width(windowSize.width - border), GUILayout.Height(windowSize.height - usableHeight));
                    for (var i = 0; i < config.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        GUI.backgroundColor = selectIndex == i ? Color.green : Color.white;
                        if (GUILayout.Button($"{config[i]["worldName"]} - {config[i]["id"].ToString()}", GUILayout.Width(windowSize.width - normalSpace - border - verticalScrollBar), GUILayout.Height(normalSpace)))
                        {
                            selectIndex = i;
                            showWorldInfo = true;
                            worldData = LoadWorldData();
                        }
                        GUILayout.EndHorizontal();
                        GUI.backgroundColor = color;
                    }
                    GUI.backgroundColor = color;
                    GUILayout.EndScrollView();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (selectIndex >= 0)
                {
                    showWorldInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showWorldInfo, "World Info");
                    if (showWorldInfo)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        GUILayout.Label($"Select World: ");
                        GUILayout.TextField($"{worldConfig["worldName"]} - {worldConfig["id"].ToString()}", GUILayout.MinWidth(80));
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        GUILayout.Label($"Terrain Path : ");
                        GUILayout.TextField(worldConfig["terrainAssetPath"], GUILayout.Height(18));
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        if (warn)
                        {
                            EditorGUILayout.HelpBox("The world has no terrain resources!", MessageType.Warning, true);
                        }
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    showWorldCleaner = EditorGUILayout.BeginFoldoutHeaderGroup(showWorldCleaner, "World Cleaner");
                    if (showWorldCleaner)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(normalSpace);
                        if (GUILayout.Button("Delete World Data", strictButton, GUILayout.Height(20), GUILayout.Width(windowSize.width - normalSpace - rightBorder)))
                        {
                            var worldDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}";
                            if (!Directory.Exists(worldDir))
                            {
                                return;
                            }
                            if (EditorUtility.DisplayDialog($"Warning!", $"Are you sure you want to delete {worldName} world data?", "Yes", "No"))
                            {
                                //删除chunks
                                var dirInfo = new DirectoryInfo(worldDir);
                                var subDirs = dirInfo.GetDirectories();
                                foreach (var t in subDirs)
                                {
                                    if (t.Name == "RawInfo") continue;
                                    LogManager.Log(LOGTag,$"Delete dir:{worldDir}/{t.Name}");
                                    Directory.Delete($"{worldDir}/{t.Name}",true);
                                    File.Delete($"{worldDir}/{t.Name}.meta");
                                    AssetDatabase.Refresh();
                                }
                                //删除WorldData.bin
                                var binPath = $"{worldDir}/WorldData.bin";
                                if (File.Exists(binPath))
                                {
                                    File.Delete(binPath);
                                }
                                AssetDatabase.Refresh();
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(2);
                    }
                } else
                {
                    EditorGUILayout.HelpBox("Please select world to edit!", MessageType.Info, true);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear Scene", GUILayout.Width(windowSize.width - border - 1), GUILayout.Height(30)))
                {
                    ClearScene();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("Start Edit World", GUILayout.Width(windowSize.width - border - 1), GUILayout.Height(30)))
                {
                    if (selectIndex >= 0)
                    {
                        startEdit = true;
                    }
                }
                GUILayout.Space(5);
                // GUILayout.Box(authorTex,guiSkin.box,GUILayout.Width(windowSize.width),GUILayout.Height(windowSize.width / 10));
                GUILayout.EndVertical();
            }
            if (Event.current.type != EventType.Repaint) return;
            if (markDeleteDict.Count <= 0) return;
            foreach (var kvp in markDeleteDict)
            {
                if (!itemHandler.chunkItemsDict.ContainsKey(kvp.Key)) continue;
                foreach (var mi in kvp.Value)
                {
                    if (itemHandler.chunkItemsDict[kvp.Key].Contains(mi))
                    {
                        itemHandler.chunkItemsDict[kvp.Key].Remove(mi);
                    }
                }
            }
            markDeleteDict.Clear();
        }

        #endregion

        #region Helper

        private static Texture GetTexture(string texName)
        {
            return (Texture)AssetDatabase.LoadAssetAtPath($"{editorAstPath}{texName}.png", typeof(Texture));
        }
        //加载配置表
        private static List<dynamic> LoadConfig()
        {
            ConfigManager.AnalyticsConfig();
            var cfList = ConfigManager.GetConfig(EConfig.World);
            return cfList;
        }
        private static Transform CreateNode(string nodeName, Transform parent = null)
        {
            var go = new GameObject(nodeName);
            var trs = go.transform;
            if (parent)
            {
                trs.SetParent(parent);
            }
            trs.localPosition = Vector3.zero;
            trs.localScale = Vector3.one;
            trs.localRotation = Quaternion.identity;
            return trs;
        }
        // private void OnDestroy()
        // {
        //     ClearScene();
        // }
        // private void OnDisable()
        // {
        //     ClearScene();
        // }
        private void ClearScene()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (FindObjectsOfType(typeof(GameObject), true) is not GameObject[] objs) return;
            // Undo.RecordObjects(objs,"Clear Objs");
            foreach (var obj in objs)
            {
                if (obj.transform.parent == null)
                {
                    Undo.DestroyObjectImmediate(obj);
                }
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            // EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        #endregion
    }
}