// author:KIPKIPS
// date:2024.10.27 11:50
// describe:

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Framework.Core.World
{
    public struct TerrainDataStruct
    {
        public Vector3 pos;
        public int sliceSize;
        public float treeDistance;
        public float treeBillboardDistance;
        public float treeCrossFadeLength;
        public int treeMaximumFullLODCount;
        public float detailObjectDistance;
        public float detailObjectDensity;
        public float heightmapPixelError;
        public int heightmapMaximumLOD;
        public float basemapDistance;
        public ShadowCastingMode shadowCastingMode;
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;
        public string materialGUID;
    }

    public class TerrainHandler
    {
        #region Variable

        private const string LOGTag = "TerrainHandler";
        private const string TerrainSplitChar = "_";
        public readonly List<GameObject> terrainList = new();
        public readonly List<GameObject> colliderList = new();

        #endregion

        #region Helper

        public bool CheckSourceTerrainAsset(string terrainAssetPath)
        {
            return File.Exists(terrainAssetPath);
        }

        public void FocusTerrain(Transform terrainRoot, string terrainName)
        {
            Transform trs = terrainRoot.Find(terrainName);
            if (!trs) return;
            EditorGUIUtility.PingObject(trs.gameObject);
            Selection.activeGameObject = trs.gameObject;
            SceneView.lastActiveSceneView.FrameSelected();
            // SceneView.FrameLastActiveSceneView();
            // FocusDelay();
        }

        //清理切分的地形
        public void ClearSplitTerrains()
        {
            foreach (var t in terrainList)
            {
                Object.DestroyImmediate(t);
            }

            terrainList.Clear();
        }

        #endregion

        #region Load Terrain

        //加载切分的地形
        public void LoadSplitTerrain(string worldName, Transform terrainRoot, Action callback = null)
        {
            string worldDir = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}";
            if (!Directory.Exists(worldDir))
            {
                LogManager.Log(LOGTag, "There is no split terrain data");
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(worldDir);
            DirectoryInfo[] subDirs = dirInfo.GetDirectories();
            foreach (var t in subDirs)
            {
                if (!t.Name.StartsWith("Chunk")) continue;
                var split = t.Name.Split('_');
                int y = int.Parse(split[1]);
                int x = int.Parse(split[2]);
                string saveDir = $"{worldDir}/{t.Name}";
                if (!Directory.Exists(saveDir))
                {
                    LogManager.Log(LOGTag, "There is no split terrain data");
                    continue;
                }

                TerrainDataStruct terrainInfo = LoadTerrainInfo($"{saveDir}/data.bytes");
                LoadTerrainChunk(worldName, terrainRoot, terrainInfo, x, y);
            }

            callback?.Invoke();
        }

        private void LoadTerrainChunk(string worldName, Transform terrainRoot, TerrainDataStruct terrainInfo, int x,
            int y)
        {
            var go = new GameObject($"{y}_{x}");
            terrainList.Add(go);
            go.transform.SetParent(terrainRoot);
            go.transform.localPosition = new Vector3(x * terrainInfo.sliceSize, 0, y * terrainInfo.sliceSize);
            var terrain = go.AddComponent<Terrain>();
#if UNITY_EDITOR
            var chunkDir = $"Chunk{TerrainSplitChar}{y}{TerrainSplitChar}{x}";
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/{chunkDir}";
            terrain.terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>($"{saveDir}/terrain.asset");
#endif
            var collider = go.AddComponent<TerrainCollider>();
            terrain.gameObject.isStatic = true;
            collider.terrainData = terrain.terrainData;
            terrain.reflectionProbeUsage = ReflectionProbeUsage.Off;
            terrain.treeDistance = terrainInfo.treeDistance;
            terrain.treeBillboardDistance = terrainInfo.treeBillboardDistance;
            terrain.treeCrossFadeLength = terrainInfo.treeCrossFadeLength;
            terrain.treeMaximumFullLODCount = terrainInfo.treeMaximumFullLODCount;
            terrain.detailObjectDistance = terrainInfo.detailObjectDistance;
            terrain.detailObjectDensity = terrainInfo.detailObjectDensity;
            terrain.heightmapPixelError = terrainInfo.heightmapPixelError;
            terrain.heightmapMaximumLOD = terrainInfo.heightmapMaximumLOD;
            terrain.basemapDistance = terrainInfo.basemapDistance;
            terrain.shadowCastingMode = terrainInfo.shadowCastingMode;
            terrain.lightmapIndex = terrainInfo.lightmapIndex;
            terrain.lightmapScaleOffset = terrainInfo.lightmapScaleOffset;
#if UNITY_EDITOR
            terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(terrainInfo.materialGUID));
#endif
        }

        public Terrain LoadSingleTerrain(Transform terrainRoot, string terrainAssetPath,
            Action<GameObject> callback = null)
        {
            if (!File.Exists(terrainAssetPath))
            {
                LogManager.Log(LOGTag, "该世界不存在地形资源");
                return null;
            }
            
            // Terrain terrain = go.AddComponent<Terrain>();
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainAssetPath);
            var go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(terrainRoot);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            // #if UNITY_EDITOR
            //             TerrainData td = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainAssetPath);
            //             terrain.terrainData = td;
            // #endif
            //             var collider = go.AddComponent<TerrainCollider>();
            //             collider.terrainData = terrain.terrainData;
            //             terrain.reflectionProbeUsage = ReflectionProbeUsage.Off;
            //             TerrainDataStruct terrainInfo = LoadTerrainInfo();
            //             terrain.treeDistance = terrainInfo.treeDistance;
            //             terrain.treeBillboardDistance = terrainInfo.treeBillboardDistance;
            //             terrain.treeCrossFadeLength = terrainInfo.treeCrossFadeLength;
            //             terrain.treeMaximumFullLODCount = terrainInfo.treeMaximumFullLODCount;
            //             terrain.detailObjectDistance = terrainInfo.detailObjectDistance;
            //             terrain.detailObjectDensity = terrainInfo.detailObjectDensity;
            //             terrain.heightmapPixelError = terrainInfo.heightmapPixelError;
            //             terrain.heightmapMaximumLOD = terrainInfo.heightmapMaximumLOD;
            //             terrain.basemapDistance = terrainInfo.basemapDistance;
            //             terrain.shadowCastingMode = terrainInfo.shadowCastingMode;
            // #if UNITY_EDITOR
            //             terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(terrainInfo.materialGUID));
            // #endif
            go.gameObject.isStatic = true;
            callback?.Invoke(go);
            return go.GetComponent<Terrain>();
        }

        private TerrainDataStruct LoadTerrainInfo(string filePath = null)
        {
            //导入器导入地形资源时,需要生成记录文件
            if (filePath != null && File.Exists(filePath))
            {
                var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
                var reader = new BinaryReader(fs);
                var info = new TerrainDataStruct();
                try
                {
                    var x = reader.ReadSingle();
                    var y = reader.ReadSingle();
                    var z = reader.ReadSingle();
                    info.pos = new Vector3(x, y, z);
                    info.sliceSize = reader.ReadInt32();
                    info.treeDistance = reader.ReadSingle();
                    info.treeBillboardDistance = reader.ReadSingle();
                    info.treeCrossFadeLength = reader.ReadInt32();
                    info.treeMaximumFullLODCount = reader.ReadInt32();
                    info.detailObjectDistance = reader.ReadSingle();
                    info.detailObjectDensity = reader.ReadSingle();
                    info.heightmapPixelError = reader.ReadSingle();
                    info.heightmapMaximumLOD = reader.ReadInt32();
                    info.basemapDistance = reader.ReadSingle();
                    info.shadowCastingMode = (ShadowCastingMode)reader.ReadInt32();
                    info.lightmapIndex = reader.ReadInt32();
                    var lightmapScaleOffsetX = reader.ReadSingle();
                    var lightmapScaleOffsetY = reader.ReadSingle();
                    var lightmapScaleOffsetZ = reader.ReadSingle();
                    var lightmapScaleOffsetW = reader.ReadSingle();
                    info.lightmapScaleOffset = new Vector4(lightmapScaleOffsetX, lightmapScaleOffsetY,lightmapScaleOffsetZ, lightmapScaleOffsetW);
                    info.materialGUID = reader.ReadString();
                }
                catch (Exception e)
                {
                    LogManager.LogError(LOGTag, e.Message + " \n" + e.StackTrace);
                }
                finally
                {
                    reader.Close();
                    fs.Close();
                }

                return info;
            }

            var defaultTerrainInfo = new TerrainDataStruct
            {
                pos = Vector3.zero,
                sliceSize = DEF.CHUNK_SIZE,
                detailObjectDistance = 80,
                detailObjectDensity = 1,
                treeDistance = 5000,
                treeBillboardDistance = 50,
                treeCrossFadeLength = 5,
                treeMaximumFullLODCount = 50,
                heightmapPixelError = 5,
                heightmapMaximumLOD = 0,
                basemapDistance = 1000,
                shadowCastingMode = ShadowCastingMode.TwoSided
            };
            // defaultTerrainInfo.materialGUID = AssetDatabase.AssetPathToGUID($@"{DEF.TERRAIN_ASSETS_PATH}/Environment/Terrains/Materials/mat_default_terrain.mat");
            return defaultTerrainInfo;
        }

        #endregion

        #region Split Terrain

        //分割地形
        public void SplitTerrain(Terrain terrain, string worldName, Vector2 sliceSize)
        {
            if (terrain == null)
            {
                LogManager.LogError(LOGTag, "Target terrain is null");
                return;
            }

            var terrainData = terrain.terrainData;
            var terrainSize = new Vector2(terrainData.size.x, terrainData.size.z);
            var sliceX = (int)(terrainSize.x / sliceSize.x);
            var sliceY = (int)(terrainSize.y / sliceSize.y);
            var oldSize = terrainData.size;

            //得到新地图分辨率
            // var newAlphaMapResolution = terrainData.alphamapResolution / sliceX;
            var splatPrototypes = terrainData.splatPrototypes;
            var detailPrototypes = terrainData.detailPrototypes;
            // var terrainMat = terrain.materialTemplate;
            var treePrototypes = terrainData.treePrototypes;
            var treeInst = terrainData.treeInstances;
            var grassStrength = terrainData.wavingGrassStrength;
            var grassAmount = terrainData.wavingGrassAmount;
            var grassSpeed = terrainData.wavingGrassSpeed;
            var grassTint = terrainData.wavingGrassTint;
            var newDetailResolution = terrainData.detailResolution / sliceX;
            const int resolutionPerPatch = 8;
            //设置高度
            var xBase = terrainData.heightmapResolution / sliceX;
            var yBase = terrainData.heightmapResolution / sliceY;
            var data = new TerrainData[sliceX * sliceY];
            var map = new Dictionary<int, List<TreeInstance>>();
            var arrayPos = 0;
            try
            {
                //循环宽和长,生成小块地形
                for (var x = 0; x < sliceX; ++x)
                {
                    for (var y = 0; y < sliceY; ++y)
                    {
                        //创建资源
                        var newData = new TerrainData();
                        map[arrayPos] = new List<TreeInstance>();
                        data[arrayPos++] = newData;
                        var chunkDir = $"Chunk{TerrainSplitChar}{y}{TerrainSplitChar}{x}";
                        var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/{chunkDir}";
                        if (!Directory.Exists(saveDir))
                        {
                            // Directory.Delete(saveDir, true); //存在文件夹则删除文件夹及其内部文件
                            Directory.CreateDirectory(saveDir);
                        }

                        var assetPath = $"{saveDir}/terrain.asset";
                        if (File.Exists(assetPath))
                        {
                            File.Delete(assetPath);
                        }

                        AssetDatabase.CreateAsset(newData, assetPath);
                        EditorUtility.DisplayProgressBar("正在分割地形", chunkDir,
                            (x * sliceX + y) / (float)(sliceX * sliceY));
                        //设置分辨率参数
                        newData.heightmapResolution = (terrainData.heightmapResolution - 1) / sliceX;
                        newData.alphamapResolution = terrainData.alphamapResolution / sliceX;
                        newData.baseMapResolution = terrainData.baseMapResolution / sliceX;

                        //设置大小
                        newData.size = new Vector3(oldSize.x / sliceX, oldSize.y, oldSize.z / sliceY);

                        //设置地形原型
                        var newSplats = new TerrainLayer[splatPrototypes.Length];
                        for (var i = 0; i < splatPrototypes.Length; ++i)
                        {
                            newSplats[i] = new TerrainLayer
                            {
                                normalMapTexture = splatPrototypes[i].texture,
                                tileSize = splatPrototypes[i].tileSize
                            };
                            var offsetX = (newData.size.x * x) % splatPrototypes[i].tileSize.x +
                                          splatPrototypes[i].tileOffset.x;
                            var offsetY = (newData.size.z * y) % splatPrototypes[i].tileSize.y +
                                          splatPrototypes[i].tileOffset.y;
                            newSplats[i].tileOffset = new Vector2(offsetX, offsetY);
                        }

                        newData.terrainLayers = newSplats;
                        //设置混合贴图
                        var alphaMap = terrainData.GetAlphamaps(x * newData.alphamapWidth, y * newData.alphamapHeight,
                            newData.alphamapWidth, newData.alphamapHeight);
                        newData.SetAlphamaps(0, 0, alphaMap);
                        var height = terrainData.GetHeights(xBase * x, yBase * y, xBase + 1, yBase + 1);
                        newData.SetHeights(0, 0, height);
                        newData.SetDetailResolution(newDetailResolution, resolutionPerPatch);
                        var layers = terrainData.GetSupportedLayers(x * newData.detailWidth - 1,
                            y * newData.detailHeight - 1, newData.detailWidth, newData.detailHeight);
                        var layerLength = layers.Length;
                        var tempDetailPrototype = new DetailPrototype[layerLength];
                        for (var i = 0; i < layerLength; i++)
                        {
                            tempDetailPrototype[i] = detailPrototypes[layers[i]];
                        }

                        newData.detailPrototypes = tempDetailPrototype;
                        for (var i = 0; i < layerLength; i++)
                        {
                            newData.SetDetailLayer(0, 0, i,
                                terrainData.GetDetailLayer(x * newData.detailWidth, y * newData.detailHeight,
                                    newData.detailWidth, newData.detailHeight, layers[i]));
                        }

                        newData.wavingGrassStrength = grassStrength;
                        newData.wavingGrassAmount = grassAmount;
                        newData.wavingGrassSpeed = grassSpeed;
                        newData.wavingGrassTint = grassTint;
                        newData.treePrototypes = treePrototypes;
                        AssetDatabase.SaveAssets();
                    }
                }

                var newWidth = (int)oldSize.x / sliceX;
                var newLength = (int)oldSize.z / sliceY;
                for (var i = 0; i < treeInst.Length; i++)
                {
                    var origPos = Vector3.Scale(new Vector3(oldSize.x, 1, oldSize.z),
                        new Vector3(treeInst[i].position.x, treeInst[i].position.y, treeInst[i].position.z));
                    var column = Mathf.FloorToInt(origPos.x / newWidth);
                    var row = Mathf.FloorToInt(origPos.z / newLength);
                    var tempVector = new Vector3((origPos.x - newWidth * column) / newWidth, origPos.y,
                        (origPos.z - newLength * row) / newWidth);
                    var tempTree = new TreeInstance
                    {
                        position = tempVector,
                        widthScale = treeInst[i].widthScale,
                        heightScale = treeInst[i].heightScale,
                        color = treeInst[i].color,
                        rotation = treeInst[i].rotation,
                        lightmapColor = treeInst[i].lightmapColor
                    };
                    var idx = (column * sliceX) + row;
                    tempTree.prototypeIndex = 0;
                    map[idx].Add(tempTree);
                }

                for (var i = 0; i < sliceX * sliceY; i++)
                {
                    data[i].treeInstances = map[i].ToArray();
                    data[i].RefreshPrototypes();
                }

                for (var x = 0; x < sliceX; ++x)
                {
                    for (var y = 0; y < sliceY; ++y)
                    {
                        SaveDataToBytesFile(terrain, x, y, sliceSize, worldName);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message);
                LogManager.LogError(LOGTag, e.StackTrace);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }

        private void SaveDataToBytesFile(Terrain terrain, int x, int y, Vector2 sliceSize, string worldName)
        {
            var chunkDir = $"Chunk{TerrainSplitChar}{y}{TerrainSplitChar}{x}";
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/{chunkDir}";
            var filePath = $"{saveDir}/data.bytes";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            var writer = new BinaryWriter(fs);
            //这里分割的宽和长度是一样的.这里求出循环次数,TerrainLoad.SIZE要生成的地形宽度,长度相同
            //高度地图的分辨率只能是2的N次幂加1,所以SLICING_SIZE必须为2的N次幂
            var pos = terrain.transform.position;
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);
            writer.Write((int)sliceSize.x);
            writer.Write(terrain.treeDistance);
            writer.Write(terrain.treeBillboardDistance);
            writer.Write(terrain.treeCrossFadeLength);
            writer.Write(terrain.treeMaximumFullLODCount);
            writer.Write(terrain.detailObjectDistance);
            writer.Write(terrain.detailObjectDensity);
            writer.Write(terrain.heightmapPixelError);
            writer.Write(terrain.heightmapMaximumLOD);
            writer.Write(terrain.basemapDistance);
            writer.Write((int)terrain.shadowCastingMode);
            writer.Write(terrain.lightmapIndex);
            writer.Write(terrain.lightmapScaleOffset.x);
            writer.Write(terrain.lightmapScaleOffset.y);
            writer.Write(terrain.lightmapScaleOffset.z);
            writer.Write(terrain.lightmapScaleOffset.w);
            writer.Write(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(terrain.materialTemplate)));

            //default collider boxes size
            writer.Write(sliceSize.x);
            writer.Write(2048);
            writer.Write(sliceSize.y);

            //item data
            writer.Write(0);

            //依赖光照贴图数量
            writer.Write(0);
            writer.Flush();
            writer.Close();
            fs.Close();
        }

        #endregion

        #region Collider Boxes

        //生成碰撞盒
        public void GenColliderBoxes(Transform colliderRoot, int xMax, int yMax, Vector2 chunkSize,
            Vector2 colliderSize, int terrainHeight, Action callback = null)
        {
            for (var y = 0; y < yMax; ++y)
            {
                for (var x = 0; x < xMax; ++x)
                {
                    GenColliderBox(colliderRoot, x, y, chunkSize, colliderSize, terrainHeight);
                }
            }

            callback?.Invoke();
        }

        private void GenColliderBox(Transform colliderRoot, int x, int y, Vector2 chunkSize, Vector2 colliderSize,
            int terrainHeight)
        {
            var nodeName = $"{y}_{x}";
            var oldTrs = colliderRoot.Find(nodeName);
            if (oldTrs)
            {
                Object.DestroyImmediate(oldTrs.gameObject);
            }

            var go = new GameObject(nodeName);
            go.transform.SetParent(colliderRoot);
            var trs = go.transform;
            trs.name = nodeName;
            trs.localRotation = Quaternion.identity;
            trs.localScale = Vector3.one;
            var collider = go.AddComponent<BoxCollider>();
            trs.localPosition = new Vector3((x + 0.5f) * chunkSize.x, 0, (y + 0.5f) * chunkSize.y);
            collider.size = new Vector3(colliderSize.x, terrainHeight, colliderSize.y);
            collider.isTrigger = true;
            colliderList.Add(go);
        }

        //清理碰撞盒
        public void ClearColliderBoxes(Action callback = null)
        {
            foreach (var t in colliderList)
            {
                Object.DestroyImmediate(t);
            }

            colliderList.Clear();
            callback?.Invoke();
        }

        //加载碰撞盒子
        public void LoadColliderBoxes(Transform colliderRoot, string worldName, int xMax, int yMax, Vector2 chunkSize,
            Action callback = null)
        {
            for (var y = 0; y < yMax; y++)
            {
                for (var x = 0; x < xMax; x++)
                {
                    var chunkDir = $"Chunk{TerrainSplitChar}{y}{TerrainSplitChar}{x}";
                    var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/{chunkDir}";
                    var filePath = $"{saveDir}/data.bytes";
                    if (!File.Exists(filePath))
                    {
                        continue;
                    }

                    var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var reader = new BinaryReader(fs);
                    try
                    {
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadString();
                        var colliderX = reader.ReadInt32();
                        var colliderY = reader.ReadInt32();
                        var colliderZ = reader.ReadInt32();
                        GenColliderBox(colliderRoot, x, y, chunkSize, new Vector2(colliderX, colliderZ), colliderY);
                    }
                    catch (Exception e)
                    {
                        LogManager.LogError(LOGTag, e.Message);
                    }

                    reader.Close();
                    fs.Close();
                }
            }

            callback?.Invoke();
        }

        public static void SaveColliderBoxes(Transform colliderRoot, string worldName, int xMax, int yMax,
            Action callback = null)
        {
            for (var y = 0; y < yMax; y++)
            {
                for (var x = 0; x < xMax; x++)
                {
                    var chunkDir = $"Chunk{TerrainSplitChar}{y}{TerrainSplitChar}{x}";
                    var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/{chunkDir}";
                    var filePath = $"{saveDir}/data.bytes";
                    if (!File.Exists(filePath))
                    {
                        continue;
                    }

                    var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var reader = new BinaryReader(fs);
                    var terrainData = new TerrainDataStruct();
                    var itemDataStructList = new List<ItemDataStruct>();
                    var success = true;
                    try
                    {
                        //terrain data
                        var posX = reader.ReadSingle();
                        var posY = reader.ReadSingle();
                        var posZ = reader.ReadSingle();
                        terrainData.pos = new Vector3(posX, posY, posZ);
                        terrainData.sliceSize = reader.ReadInt32();
                        terrainData.treeDistance = reader.ReadSingle();
                        terrainData.treeBillboardDistance = reader.ReadSingle();
                        terrainData.treeCrossFadeLength = reader.ReadInt32();
                        terrainData.treeMaximumFullLODCount = reader.ReadInt32();
                        terrainData.detailObjectDistance = reader.ReadSingle();
                        terrainData.detailObjectDensity = reader.ReadSingle();
                        terrainData.heightmapPixelError = reader.ReadSingle();
                        terrainData.heightmapMaximumLOD = reader.ReadInt32();
                        terrainData.basemapDistance = reader.ReadSingle();
                        terrainData.shadowCastingMode = (ShadowCastingMode)reader.ReadInt32();
                        terrainData.lightmapIndex = reader.ReadInt32();
                        var lightmapScaleOffsetX = reader.ReadSingle();
                        var lightmapScaleOffsetY = reader.ReadSingle();
                        var lightmapScaleOffsetZ = reader.ReadSingle();
                        var lightmapScaleOffsetW = reader.ReadSingle();
                        terrainData.lightmapScaleOffset = new Vector4(lightmapScaleOffsetX, lightmapScaleOffsetY,
                            lightmapScaleOffsetZ, lightmapScaleOffsetW);
                        terrainData.materialGUID = reader.ReadString();
                        //ignore line old collider
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();

                        //item data
                        var cnt = reader.ReadInt32();
                        for (var i = 0; i < cnt; i++)
                        {
                            ItemDataStruct ids = new ItemDataStruct();
                            var tX = reader.ReadSingle();
                            var tY = reader.ReadSingle();
                            var tZ = reader.ReadSingle();
                            ids.pos = new Vector3(tX, tY, tZ);
                            tX = reader.ReadSingle();
                            tY = reader.ReadSingle();
                            tZ = reader.ReadSingle();
                            ids.rotate = Quaternion.Euler(tX, tY, tZ);
                            tX = reader.ReadSingle();
                            tY = reader.ReadSingle();
                            tZ = reader.ReadSingle();
                            ids.scale = new Vector3(tX, tY, tZ);
                            ids.lightingMapIndex = reader.ReadInt32();
                            tX = reader.ReadSingle();
                            tY = reader.ReadSingle();
                            tZ = reader.ReadSingle();
                            var w = reader.ReadSingle();
                            ids.lightingMapOffsetScale = new Vector4(tX, tY, tZ, w);
                            ids.guid = reader.ReadString();
                            ids.name = reader.ReadString();
                            itemDataStructList.Add(ids);
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.LogError(LOGTag, e.Message);
                        success = false;
                    }

                    reader.Close();
                    fs.Close();
                    if (!success) continue;
                    {
                        File.Delete(filePath);
                        fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                        BinaryWriter writer = new BinaryWriter(fs);
                        //恢复记录的地形数据
                        Vector3 pos = terrainData.pos;
                        writer.Write(pos.x);
                        writer.Write(pos.y);
                        writer.Write(pos.z);
                        writer.Write(terrainData.sliceSize);
                        writer.Write(terrainData.treeDistance);
                        writer.Write(terrainData.treeBillboardDistance);
                        writer.Write(terrainData.treeCrossFadeLength);
                        writer.Write(terrainData.treeMaximumFullLODCount);
                        writer.Write(terrainData.detailObjectDistance);
                        writer.Write(terrainData.detailObjectDensity);
                        writer.Write(terrainData.heightmapPixelError);
                        writer.Write(terrainData.heightmapMaximumLOD);
                        writer.Write(terrainData.basemapDistance);
                        writer.Write((int)terrainData.shadowCastingMode);
                        writer.Write(terrainData.lightmapIndex);
                        writer.Write(terrainData.lightmapScaleOffset.x);
                        writer.Write(terrainData.lightmapScaleOffset.y);
                        writer.Write(terrainData.lightmapScaleOffset.z);
                        writer.Write(terrainData.lightmapScaleOffset.w);
                        writer.Write(terrainData.materialGUID);

                        //记录新的碰撞盒数据
                        var nodeName = $"{y}_{x}";
                        var oldTrs = colliderRoot.Find(nodeName);
                        var colliderX = 0;
                        var colliderY = 0;
                        var colliderZ = 0;
                        if (oldTrs)
                        {
                            var collider = oldTrs.GetComponent<BoxCollider>();
                            if (collider)
                            {
                                var v = collider.size;
                                colliderX = (int)v.x;
                                colliderY = (int)v.y;
                                colliderZ = (int)v.z;
                            }
                        }

                        writer.Write(colliderX);
                        writer.Write(colliderY);
                        writer.Write(colliderZ);

                        //恢复item数据
                        writer.Write(itemDataStructList.Count);
                        foreach (var ids in itemDataStructList)
                        {
                            writer.Write(ids.pos.x);
                            writer.Write(ids.pos.y);
                            writer.Write(ids.pos.z);
                            writer.Write(ids.rotate.x);
                            writer.Write(ids.rotate.y);
                            writer.Write(ids.rotate.z);
                            writer.Write(ids.scale.x);
                            writer.Write(ids.scale.y);
                            writer.Write(ids.scale.z);
                            writer.Write(ids.lightingMapIndex);
                            writer.Write(ids.lightingMapOffsetScale.x);
                            writer.Write(ids.lightingMapOffsetScale.y);
                            writer.Write(ids.lightingMapOffsetScale.z);
                            writer.Write(ids.lightingMapOffsetScale.w);
                            writer.Write(ids.guid);
                            writer.Write(ids.name);
                        }

                        writer.Flush();
                        writer.Close();
                        fs.Close();
                    }
                }
            }

            callback?.Invoke();
        }

        #endregion

        #region Convert Terrain

        //To Mesh
        public void Converter()
        {
            if (Selection.objects.Length <= 0)
            {
                LogManager.Log(LOGTag, "Please select the [Terrain] in the [Hierarchy]");
                return;
            }

            var terrainObj = Selection.objects[0] as GameObject;
            if (terrainObj == null)
            {
                LogManager.Log(LOGTag, "Select objects is not [GameObject]");
                return;
            }

            var terrain = terrainObj.GetComponent<Terrain>();
            if (terrain == null)
            {
                LogManager.Log(LOGTag, "Select the object missing [Terrain] component");
                return;
            }

            var terrainData = terrain.terrainData;
            if (terrainData == null)
            {
                LogManager.Log(LOGTag, "Terrain component missing TerrainData");
                return;
            }

            const int vertexCountScale = 4;
            var w = terrainData.heightmapResolution;
            var h = terrainData.heightmapResolution;
            var size = terrainData.size;
            var alphaMapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            var meshScale = new Vector3(size.x / (w - 1f) * vertexCountScale, 1, size.z / (h - 1f) * vertexCountScale);
            // [dev] terrainData.splatPrototypes 有问题,若每个图片大小不一,则出问题
            var uvScale = new Vector2(1f / (w - 1f), 1f / (h - 1f)) * vertexCountScale * (size.x / terrainData.splatPrototypes[0].tileSize.x);
            w = (w - 1) / vertexCountScale + 1;
            h = (h - 1) / vertexCountScale + 1;
            var vertices = new Vector3[w * h];
            var uvs = new Vector2[w * h];
            var alphasWeight = new Vector4[w * h];
            for (var i = 0; i < w; i++)
            {
                for (var j = 0; j < h; j++)
                {
                    var index = j * w + i;
                    var z = terrainData.GetHeight(i * vertexCountScale, j * vertexCountScale);
                    vertices[index] = Vector3.Scale(new Vector3(i, z, j), meshScale);
                    uvs[index] = Vector2.Scale(new Vector2(i, j), uvScale);

                    // alpha map
                    var i2 = (int)(i * terrainData.alphamapWidth / (w - 1f));
                    var j2 = (int)(j * terrainData.alphamapHeight / (h - 1f));
                    i2 = Mathf.Min(terrainData.alphamapWidth - 1, i2);
                    j2 = Mathf.Min(terrainData.alphamapHeight - 1, j2);
                    var alpha0 = alphaMapData[j2, i2, 0];
                    var alpha1 = alphaMapData[j2, i2, 1];
                    var alpha2 = alphaMapData[j2, i2, 2];
                    var alpha3 = alphaMapData[j2, i2, 3];
                    alphasWeight[index] = new Vector4(alpha0, alpha1, alpha2, alpha3);
                }
            }

            /*
             * 三角形
             *     b       c
             *      *******
             *      *   * *
             *      * *   *
             *      *******
             *     a       d
             */
            var triangles = new int[(w - 1) * (h - 1) * 6];
            var triangleIndex = 0;
            for (var i = 0; i < w - 1; i++)
            {
                for (var j = 0; j < h - 1; j++)
                {
                    var a = j * w + i;
                    var b = (j + 1) * w + i;
                    var c = (j + 1) * w + i + 1;
                    var d = j * w + i + 1;
                    triangles[triangleIndex++] = a;
                    triangles[triangleIndex++] = b;
                    triangles[triangleIndex++] = c;
                    triangles[triangleIndex++] = a;
                    triangles[triangleIndex++] = c;
                    triangles[triangleIndex++] = d;
                }
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                triangles = triangles,
                tangents = alphasWeight // 将地形纹理的比重写入到切线中
            };
            const string transName = "[dev]MeshFromTerrainData";
            var t = terrainObj.transform.parent.Find(transName);
            if (t == null)
            {
                var go = new GameObject(transName, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                t = go.transform;
            }

            // 地形渲染
            var mr = t.GetComponent<MeshRenderer>();
            var mat = mr.sharedMaterial;
            if (!mat) mat = new Material(Shader.Find("Custom/Environment/TerrainSimple"));
            for (var i = 0; i < terrainData.splatPrototypes.Length; i++)
            {
                var sp = terrainData.splatPrototypes[i];
                mat.SetTexture("_Texture" + i, sp.texture);
            }

            t.parent = terrainObj.transform.parent;
            t.position = terrainObj.transform.position;
            t.gameObject.layer = terrainObj.layer;
            t.GetComponent<MeshFilter>().sharedMesh = mesh;
            t.GetComponent<MeshCollider>().sharedMesh = mesh;
            mr.sharedMaterial = mat;
            t.gameObject.SetActive(true);
            terrainObj.SetActive(false);
            LogManager.Log(LOGTag, "Convert terrain to mesh finished!");
        }

        #endregion
    }
}