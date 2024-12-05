// author:KIPKIPS
// date:2024.10.27 11:48
// describe:场景物件处理工具

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.Common;
using Framework.Core.Manager.ResourcesLoad;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Framework.Core.World
{
    public class FolderLevelNode
    {
        public readonly List<FolderLevelNode> subdirectories = new List<FolderLevelNode>();
        public string name;
        public string uid; //唯一标识id[这里使用文件路径作为标识]
    }

    public class ItemDataStruct
    {
        public Vector3 pos;
        public Quaternion rotate;
        public Vector3 scale;
        public int lightingMapIndex;
        public Vector4 lightingMapOffsetScale;
        public string guid;
        public string name;
    }

    public class ModelInfo
    {
        public GameObject go;
        public string suffix;
    }

    public class ItemHandler
    {
        private const string LOGTag = "ItemHandler";
        private FolderLevelNode m_modelAssetRoot;
        public FolderLevelNode modelAssetRoot => m_modelAssetRoot ??= SearchModelAssetDir();
        public readonly Dictionary<string, List<ModelInfo>> chunkItemsDict = new();

        #region Save

        public void SaveAllItemChunks(Transform itemRoot, string worldName, int xMax, int yMax, Action callback = null)
        {
            for (int y = 0; y < yMax; y++)
            {
                for (int x = 0; x < xMax; x++)
                {
                    SaveItemChunk(itemRoot, worldName, x, y);
                }
            }

            callback?.Invoke();
        }

        public void SaveItemChunk(Transform itemRoot, string worldName, int chunkX, int chunkY, Action callback = null)
        {
            //record data
            string filePath = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/Chunk{DEF.TerrainSplitChar}{chunkX}{DEF.TerrainSplitChar}{chunkY}/data.bytes";
            if (!File.Exists(filePath))
            {
                LogManager.Log(LOGTag, "The item data file does not exist");
                return;
            }

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            TerrainDataStruct oldData = new TerrainDataStruct();
            Vector3 colliderSize = new Vector3();
            bool success = true;
            try
            {
                //terrain
                float posX = reader.ReadSingle();
                float posY = reader.ReadSingle();
                float posZ = reader.ReadSingle();
                oldData.pos = new Vector3(posX, posY, posZ);
                oldData.sliceSize = reader.ReadInt32();
                oldData.treeDistance = reader.ReadSingle();
                oldData.treeBillboardDistance = reader.ReadSingle();
                oldData.treeCrossFadeLength = reader.ReadInt32();
                oldData.treeMaximumFullLODCount = reader.ReadInt32();
                oldData.detailObjectDistance = reader.ReadSingle();
                oldData.detailObjectDensity = reader.ReadSingle();
                oldData.heightmapPixelError = reader.ReadSingle();
                oldData.heightmapMaximumLOD = reader.ReadInt32();
                oldData.basemapDistance = reader.ReadSingle();
                oldData.shadowCastingMode = (ShadowCastingMode)reader.ReadInt32();
                oldData.lightmapIndex = reader.ReadInt32();
                float lightmapScaleOffsetX = reader.ReadSingle();
                float lightmapScaleOffsetY = reader.ReadSingle();
                float lightmapScaleOffsetZ = reader.ReadSingle();
                float lightmapScaleOffsetW = reader.ReadSingle();
                oldData.lightmapScaleOffset = new Vector4(lightmapScaleOffsetX, lightmapScaleOffsetY,
                    lightmapScaleOffsetZ, lightmapScaleOffsetW);
                oldData.materialGUID = reader.ReadString();

                //collider box
                colliderSize.x = reader.ReadInt32();
                colliderSize.y = reader.ReadInt32();
                colliderSize.z = reader.ReadInt32();
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message);
                success = false;
            }

            reader.Close();
            fs.Close();
            if (success)
            {
                Transform chunkRoot = itemRoot.Find($"{chunkX}{DEF.TerrainSplitChar}{chunkY}");
                if (chunkRoot == null)
                {
                    return;
                }

                File.Delete(filePath);
                fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                BinaryWriter writer = new BinaryWriter(fs);
                Vector3 pos = oldData.pos;
                writer.Write(pos.x);
                writer.Write(pos.y);
                writer.Write(pos.z);
                writer.Write(oldData.sliceSize);
                writer.Write(oldData.treeDistance);
                writer.Write(oldData.treeBillboardDistance);
                writer.Write(oldData.treeCrossFadeLength);
                writer.Write(oldData.treeMaximumFullLODCount);
                writer.Write(oldData.detailObjectDistance);
                writer.Write(oldData.detailObjectDensity);
                writer.Write(oldData.heightmapPixelError);
                writer.Write(oldData.heightmapMaximumLOD);
                writer.Write(oldData.basemapDistance);
                writer.Write((int)oldData.shadowCastingMode);
                writer.Write(oldData.lightmapIndex);
                writer.Write(oldData.lightmapScaleOffset.x);
                writer.Write(oldData.lightmapScaleOffset.y);
                writer.Write(oldData.lightmapScaleOffset.z);
                writer.Write(oldData.lightmapScaleOffset.w);
                writer.Write(oldData.materialGUID);
                writer.Write((int)colliderSize.x);
                writer.Write((int)colliderSize.y);
                writer.Write((int)colliderSize.z);

                //record items data
                var cnt = chunkRoot.transform.childCount;
                if (cnt > 0)
                {
                    writer.Write(cnt);
                    for (var i = 0; i < cnt; i++)
                    {
                        var trs = chunkRoot.transform.GetChild(i);
                        var tempVec = trs.position;
                        writer.Write(tempVec.x);
                        writer.Write(tempVec.y);
                        writer.Write(tempVec.z);
                        tempVec = trs.eulerAngles;
                        writer.Write(tempVec.x);
                        writer.Write(tempVec.y);
                        writer.Write(tempVec.z);
                        tempVec = trs.localScale;
                        writer.Write(tempVec.x);
                        writer.Write(tempVec.y);
                        writer.Write(tempVec.z);
                        var render = trs.gameObject.GetComponent<MeshRenderer>();
                        if (render == null)
                        {
                            writer.Write(-1);
                            writer.Write(1.0f);
                            writer.Write(1.0f);
                            writer.Write(0f);
                            writer.Write(0f);
                        }
                        else
                        {
                            writer.Write(render.lightmapIndex);
                            writer.Write(render.lightmapScaleOffset.x);
                            writer.Write(render.lightmapScaleOffset.y);
                            writer.Write(render.lightmapScaleOffset.z);
                            writer.Write(render.lightmapScaleOffset.w);
                        }

                        Object obj = PrefabUtility.GetCorrespondingObjectFromSource(trs.gameObject);
                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                        writer.Write(guid);
                        var name = trs.name;
                        writer.Write(name);
                    }
                }
                else
                {
                    writer.Write(0);
                }

                callback?.Invoke();
                AssetDatabase.Refresh();
                writer.Flush();
                writer.Close();
                fs.Close();
            }
        }

        #endregion

        #region Load

        // public void LoadAllItemChunks(Transform itemRoot, string worldName, int piecesPerAxis, Vector2 chunkSize, Action callback = null)
        // {
        //     for (int i = 0; i < piecesPerAxis * piecesPerAxis; i++)
        //     {
        //         LoadItemChunk(itemRoot, worldName, i, chunkSize);
        //     }
        //
        //     callback?.Invoke();
        // }

        // public void LoadItemChunk(Transform itemRoot, string worldName, int index, Vector2 chunkSize, Action callback = null)
        // {
        //     // var nodeName = $"{chunkY}{DEF.TerrainSplitChar}{chunkX}";
        //     // if (!chunkItemsDict.ContainsKey(nodeName))
        //     // {
        //     //     chunkItemsDict.Add(nodeName, new List<ModelInfo>());
        //     // }
        //     //
        //     // var chunkRoot = itemRoot.Find(nodeName);
        //     // if (chunkRoot == null)
        //     // {
        //     //     chunkRoot = AddItemChunkRoot(itemRoot, chunkX, chunkY, chunkSize);
        //     // }
        //
        //     // var filePath = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/Chunk{DEF.TerrainSplitChar}{chunkY}{DEF.TerrainSplitChar}{chunkX}/data.bytes";
        //     // if (!File.Exists(filePath))
        //     // {
        //     //     LogManager.Log(LOGTag, "There is no scene object data");
        //     //     return;
        //     // }
        //     //
        //     // var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
        //     // var reader = new BinaryReader(fs);
        //     // int cnt;
        //     // try
        //     // {
        //     //     //ignore read terrain
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadInt32();
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadInt32();
        //     //     reader.ReadInt32();
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadInt32();
        //     //     reader.ReadSingle();
        //     //     reader.ReadInt32();
        //     //     reader.ReadInt32();
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadSingle();
        //     //     reader.ReadString();
        //     //     //ignore read collider
        //     //     reader.ReadInt32();
        //     //     reader.ReadInt32();
        //     //     reader.ReadInt32();
        //     //
        //     //     //item count
        //     //     cnt = reader.ReadInt32();
        //     // }
        //     // catch (Exception)
        //     // {
        //     //     LogManager.Log(LOGTag, "There is no scene object data");
        //     //     return;
        //     // }
        //     //
        //     // for (var i = 0; i < cnt; i++)
        //     // {
        //     //     var x = reader.ReadSingle();
        //     //     var y = reader.ReadSingle();
        //     //     var z = reader.ReadSingle();
        //     //     var pos = new Vector3(x, y, z);
        //     //     x = reader.ReadSingle();
        //     //     y = reader.ReadSingle();
        //     //     z = reader.ReadSingle();
        //     //     var rotate = Quaternion.Euler(x, y, z);
        //     //     x = reader.ReadSingle();
        //     //     y = reader.ReadSingle();
        //     //     z = reader.ReadSingle();
        //     //     var scale = new Vector3(x, y, z);
        //     //     var lightingMapIndex = reader.ReadInt32();
        //     //     x = reader.ReadSingle();
        //     //     y = reader.ReadSingle();
        //     //     z = reader.ReadSingle();
        //     //     var w = reader.ReadSingle();
        //     //     var lightingMapOffsetScale = new Vector4(x, y, z, w);
        //     //     var guid = reader.ReadString();
        //     //     var name = reader.ReadString();
        //     //     //加载预制体
        //     //     var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
        //     //     var go = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        //     //     if (go == null) continue;
        //     //     go.isStatic = true;
        //     //     go.name = name;
        //     //     go.transform.position = pos;
        //     //     go.transform.localScale = scale;
        //     //     go.transform.rotation = rotate;
        //     //     go.transform.SetParent(chunkRoot);
        //     //     var rend = go.GetComponent<MeshRenderer>();
        //     //     if (rend != null && lightingMapIndex != -1)
        //     //     {
        //     //         rend.lightmapIndex = lightingMapIndex;
        //     //         rend.lightmapScaleOffset = lightingMapOffsetScale;
        //     //     }
        //     //
        //     //     var mi = new ModelInfo
        //     //     {
        //     //         go = go,
        //     //         suffix = AssetDatabase.GUIDToAssetPath(guid).Split(".").Last().ToLower()
        //     //     };
        //     //     chunkItemsDict[nodeName].Add(mi);
        //     // }
        //     //
        //     // reader.Close();
        //     // fs.Close();
        //     // callback?.Invoke();
        // }

        public Vector2 CheckParentChunk(Vector3 pos, int xMax, int yMax, Vector2 chunkSize)
        {
            var parent = new Vector2(-1, -1);
            for (var x = 0; x < xMax; x++)
            {
                for (var y = 0; y < yMax; y++)
                {
                    var bound = new Bounds
                    {
                        center = new Vector3((x + 0.5f) * chunkSize.x, 0, (y + 0.5f) * chunkSize.y),
                        size = new Vector3(chunkSize.x, 2048, chunkSize.y)
                    };
                    if (bound.Contains(pos))
                    {
                        return new Vector2(x, y);
                    }
                }
            }

            return parent;
        }

        public void LoadModelPrefab(Transform itemRoot, string assetPath, int xMax, int yMax, Vector2 chunkSize)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            var name = asset.name;
            // Debug.Log(name);
            if (CommonUtils.Find<Transform>(itemRoot, asset.name) != null)
            {
                var num = 1;
                var tempName = asset.name + DEF.TerrainSplitChar + "1";
                while (CommonUtils.Find<Transform>(itemRoot, tempName) != default)
                {
                    num++;
                    tempName = asset.name + DEF.TerrainSplitChar + num;
                }

                name = tempName;
            }

            var go = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (go == null) return;
            go.name = name;
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var newParent = CheckParentChunk(go.transform.position, xMax, yMax, chunkSize);
            if (!(newParent.x >= 0) || !(newParent.y >= 0)) return;
            var nodeName = $"{(int)newParent.y}{DEF.TerrainSplitChar}{(int)newParent.x}";
            if (!chunkItemsDict.ContainsKey(nodeName))
            {
                chunkItemsDict.Add(nodeName, new List<ModelInfo>());
            }

            go.transform.SetParent(itemRoot.Find(nodeName));
            var mi = new ModelInfo
            {
                go = go,
                suffix = AssetDatabase.GUIDToAssetPath(assetPath).Split(".").Last().ToLower()
            };
            chunkItemsDict[nodeName].Add(mi);
        }

        #endregion

        #region Unload

        //卸载所有item chunk
        public void UnloadAllItemChunks(Transform itemRoot, int xMax, int yMax, Action callback = null)
        {
            chunkItemsDict.Clear();
            for (int y = 0; y < yMax; y++)
            {
                for (int x = 0; x < xMax; x++)
                {
                    UnloadItemChunk(itemRoot, x, y);
                }
            }

            callback?.Invoke();
        }

        //卸载指定item chunk
        public void UnloadItemChunk(Transform itemRoot, int x, int y, Action callback = null)
        {
            var rootName = $"{y}{DEF.TerrainSplitChar}{x}";
            var itemChunkTrs = itemRoot.Find(rootName);
            if (chunkItemsDict.TryGetValue(rootName, out var value))
            {
                value.Clear();
            }

            if (itemChunkTrs)
            {
                Object.DestroyImmediate(itemChunkTrs.gameObject);
            }

            callback?.Invoke();
        }

        #endregion

        #region Helper

        public static void FocusItemChunk(Transform itemRoot, int x, int y, Vector2 chunkSize)
        {
            var iName = $"{y}{DEF.TerrainSplitChar}{x}";
            var trs = itemRoot.Find(iName);
            if (trs)
            {
                EditorGUIUtility.PingObject(trs.gameObject);
                Selection.activeGameObject = trs.gameObject;
                // SceneView.lastActiveSceneView.FrameSelected();
                SceneView.FrameLastActiveSceneView();
            }
            else
            {
                LogManager.LogWarning(LOGTag, $"Unable to focus because the item chunk[{iName}] does not exist");
            }
        }

        private Transform AddItemChunkRoot(Transform itemRoot, int x, int y, Vector2 chunkSize)
        {
            var go = new GameObject($"{y}{DEF.TerrainSplitChar}{x}");
            var trs = go.transform;
            trs.SetParent(itemRoot);
            trs.localRotation = Quaternion.identity;
            trs.localPosition = new Vector3((x + 0.5f) * chunkSize.x, 0, (y + 0.5f) * chunkSize.y);
            trs.localScale = Vector3.one;
            return trs;
        }

        #endregion

        #region Model Prefab

        public void ResetModelPrefabs()
        {
            m_modelAssetRoot = null;
        }

        private FolderLevelNode SearchModelAssetDir()
        {
            var root = new FolderLevelNode();
            //第一个元素是-1
            var dict = new Dictionary<int, Dictionary<string, FolderLevelNode>> { { -1, new Dictionary<string, FolderLevelNode>() } };
            dict[-1].Add("Models", root);
            dict[-1]["Models"].name = "Models";
            dict[-1]["Models"].uid = "Models";
            var rootDir = $"{DEF.RESOURCES_ASSETS_PATH}Models/";
            if (!Directory.Exists(rootDir)) return root;
            var directoryInfo = new DirectoryInfo(rootDir);
            var fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (var t in fileInfos)
            {
                if (t.Name.EndsWith(".meta") || !t.Name.StartsWith("model_")) continue;
                var arr = t.FullName.Replace(@"\", "/").Split(rootDir);
                if (arr.Length != 2) continue;
                var partDir = arr[1];
                var splitPart = partDir.Split("/");
                var uid = string.Empty;
                for (var j = 0; j < splitPart.Length; j++)
                {
                    uid += ((j == 0 ? "" : "/") + splitPart[j]);
                    if (!dict.ContainsKey(j))
                    {
                        dict.Add(j, new Dictionary<string, FolderLevelNode>());
                    }

                    if (!dict[j].ContainsKey(uid))
                    {
                        dict[j].Add(uid, new FolderLevelNode());
                        dict[j][uid].name = splitPart[j];
                        dict[j][uid].uid = uid;
                    }

                    var temp = dict[j][uid];
                    var parentPath = uid.Replace("/" + splitPart[j], "");
                    var parent = j > 0 ? dict[j - 1][parentPath] : dict[-1]["Models"];
                    if (!parent.subdirectories.Contains(temp))
                    {
                        parent.subdirectories.Add(temp);
                    }
                }
                // LogManager.Log(LOGTag,partDir);
            }

            return root;
        }

        #endregion

        public void ExportScene(Transform envRoot,string worldName)
        {
            var worldDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            if (!File.Exists(worldDataPath))
            {
                LogManager.Log(LOGTag, $"There is no world data,path:{worldName}");
                return;
            }
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(worldDataPath);
            var data = BinaryUtils.Bytes2Object<WorldData>(assetData.bytes);
            var cnt = data.PiecesPerAxis * data.PiecesPerAxis;

            for (var i = 0; i < cnt; i++)
            {
                var chunkName = $"Chunk{DEF.TerrainSplitChar}{i}";
                var chunkRoot = envRoot.Find(chunkName);
                if (chunkRoot != null)
                {
                    var itemTrs = chunkRoot.Find("[Item]");
                    if (itemTrs != null)
                    {
                        var assetPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkName}/ItemChunk.prefab";
                        if (File.Exists(assetPath))
                        {
                            File.Delete(assetPath);
                            File.Delete($"{assetPath}.meta");
                        }
                        PrefabUtility.SaveAsPrefabAsset(itemTrs.gameObject,assetPath,out var succ);
                        if (!succ)
                        {
                            LogManager.LogWarning(LOGTag, $"Export Chunk:{i} failed reason : PrefabUtility.SaveAsPrefabAsset excute failed");
                        }
                    }
                    else
                    {
                        LogManager.LogWarning(LOGTag, $"Export Chunk:{i} failed,chunk root has not [Item]");
                    }

                }
                else
                {
                    LogManager.LogWarning(LOGTag, $"Export Chunk:{i} failed,not chunkRoot");
                }
            }

        }

        public void RevertSceneItem(Transform envRoot,string worldName)
        {
            var worldDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            if (!File.Exists(worldDataPath))
            {
                LogManager.Log(LOGTag, $"There is no world data,path:{worldName}");
                return;
            }
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(worldDataPath);
            var data = BinaryUtils.Bytes2Object<WorldData>(assetData.bytes);
            var cnt = data.PiecesPerAxis * data.PiecesPerAxis;
            for (var i = 0; i < cnt; i++)
            {
                var chunkName = $"Chunk{DEF.TerrainSplitChar}{i}";
                var chunkRoot = envRoot.Find(chunkName);
                if (chunkRoot != null)
                {
                    var itemTrs = chunkRoot.Find("[Item]");
                    if (itemTrs != null)
                    {
                        var transList = new List<Transform>();
                        for (var j = 0; j < itemTrs.childCount; j++)
                        {
                            var t = itemTrs.GetChild(j);
                            transList.Add(t);
                        }
                        foreach (var t in transList)
                        {
                            t.SetParent(envRoot);
                        }
                    }
                }
            }
        }

        struct Bound
        {
            public float xMin;
            public float zMin;
            public float xMax;
            public float zMax;
        }
        public void SplitSceneItemWithChunk(Transform envRoot,string worldName,TerrainHandler handler)
        {
            var worldDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            if (!File.Exists(worldDataPath))
            {
                LogManager.Log(LOGTag, $"There is no world data,path:{worldName}");
                return;
            }
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(worldDataPath);
            var data = BinaryUtils.Bytes2Object<WorldData>(assetData.bytes);
            var cnt = data.PiecesPerAxis * data.PiecesPerAxis;
            var Bounds = new Bound[cnt];
            var chunkRoots = new Transform[cnt];
            for (var i = 0; i < cnt; i++)
            {
                var chunkName = $"Chunk{DEF.TerrainSplitChar}{i}";
                var chunkRoot = envRoot.Find(chunkName);
                
                var colliderDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkName}/Collider.bytes";
                if (!File.Exists(colliderDataPath))
                {
                    LogManager.Log(LOGTag, $"There is no colliderData,path:{colliderDataPath}");
                    return;
                }
                var colliderAsset = ResourcesLoadManager.LoadAsset<TextAsset>(colliderDataPath);
                var colliderData = BinaryUtils.Bytes2Object<ColliderInfo>(colliderAsset.bytes);
                
                var terrainDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkName}/TerrainInfo.bytes";
                if (!File.Exists(terrainDataPath))
                {
                    LogManager.Log(LOGTag, $"There is no terrainDataPath,path:{terrainDataPath}");
                    return;
                }
                var terrainAsset = ResourcesLoadManager.LoadAsset<TextAsset>(terrainDataPath);
                var terrainData = BinaryUtils.Bytes2Object<TerrainInfo>(terrainAsset.bytes);
                
                if (chunkRoot == null)
                {
                    chunkRoot = new GameObject(chunkName).transform;
                    var transform = chunkRoot.transform;
                    transform.SetParent(envRoot);
                    transform.localScale = Vector3.one;
                    transform.localRotation = Quaternion.identity;
                    chunkRoot.localPosition = new Vector3(terrainData.X, terrainData.Y, terrainData.Z);
                }
                var colliderTrs = chunkRoot.Find("Collider");
                if (colliderTrs == null)
                {
                    colliderTrs = new GameObject("Collider").transform;
                }

                colliderTrs.transform.SetParent(chunkRoot);
                colliderTrs.localRotation = Quaternion.identity;
                colliderTrs.localScale = Vector3.one;
                var collider = colliderTrs.gameObject.AddComponent<BoxCollider>();
                colliderTrs.localPosition = new Vector3(colliderData.PositionX,colliderData.PositionY,colliderData.PositionZ);
                collider.size = new Vector3(colliderData.SizeX,colliderData.SizeY,colliderData.SizeZ);
                collider.isTrigger = true;

                var position = colliderTrs.position;
                Bounds[i] = new Bound
                {
                    xMin = position.x - colliderData.SizeX / 2f,
                    zMin = position.z - colliderData.SizeZ / 2f,
                    xMax = position.x + colliderData.SizeX / 2f,
                    zMax = position.z + colliderData.SizeZ / 2f,
                };
                chunkRoots[i] = chunkRoot;
            }
            
            var prefabChildCnt = 0;
            var transList = new List<Transform>();
            for (var i = 0; i < envRoot.childCount; i++)
            {
                var t = envRoot.GetChild(i);
                if (!CommonEditorUtils.IsPrefabInstance(t.gameObject))
                {
                    LogManager.Log(LOGTag,$"<color=#ff0000>NOT</color> prefab asset:{t.name}");
                    continue;
                }
                transList.Add(t);
                prefabChildCnt++;
                LogManager.Log(LOGTag,$"<color=#00ff00>YES</color> prefab asset:{t.name} childCnt:{prefabChildCnt}");
            }

            foreach (var t in transList)
            {
                var find = false;
                for (var j = 0; j < Bounds.Length; j++)
                {
                    var position = t.position;
                    var inBox = position.x >= Bounds[j].xMin && position.x <= Bounds[j].xMax && position.z >= Bounds[j].zMin && position.z <= Bounds[j].zMax;
                    if (find || !inBox) continue;
                    LogManager.Log(LOGTag,$"{t.name} is in chunk: {j}");
                    var itemTrs = chunkRoots[j].Find("[Item]");
                    if (itemTrs == null)
                    {
                        itemTrs = new GameObject("[Item]").transform;
                        itemTrs.SetParent(chunkRoots[j]);
                        itemTrs.localRotation = Quaternion.identity;
                        itemTrs.localScale = Vector3.one;
                        itemTrs.localPosition = Vector3.zero;
                    }
                    t.SetParent(itemTrs);
                    find = true;
                }
            
                if (!find)
                {
                    LogManager.LogWarning(LOGTag,$"{t.name} is not in any chunk");
                }
            }
            
            LogManager.Log(LOGTag,$"envRoot total child Count:{envRoot.childCount}, prefab count:{prefabChildCnt}");
        }
        
        public void LoadAllItemChunks(Transform envRoot,string worldName, Action callback = null)
        {
            var worldDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(worldDataPath);
            var data = BinaryUtils.Bytes2Object<WorldData>(assetData.bytes);

            for (var i = 0; i < data.PiecesPerAxis * data.PiecesPerAxis; i++)
            {
                LoadItemChunk(envRoot,worldName, i);
            }

            callback?.Invoke();
        }
        private void LoadItemChunk(Transform envRoot,string worldName, int index)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{index}";
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
            var chunkRoot = envRoot.Find(chunkDir);
            if (chunkRoot == null)
            {
                chunkRoot = new GameObject(chunkDir).transform;
            }

            var item = ResourcesLoadManager.LoadAsset<GameObject>($"{saveDir}/ItemChunk.prefab");
            if (item != null)
            {
                var go = Object.Instantiate(item, chunkRoot, true);

                go.name = "[Item]";
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.gameObject.isStatic = true;
                go.layer = LayerMask.NameToLayer("Ground");
            }
        }
    }
}