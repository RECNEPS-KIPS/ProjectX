// author:KIPKIPS
// date:2024.10.27 11:50
// describe:处理地形工具

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Framework.Common;
using Framework.Core.Manager.ResourcesLoad;
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
        public readonly List<GameObject> terrainList = new();
        public readonly List<GameObject> colliderList = new();

        #endregion

        #region Helper

        public static bool CheckSourceTerrainAsset(string terrainAssetPath)
        {
            return File.Exists(terrainAssetPath);
        }

        public static void FocusTerrain(Transform trs)
        {
            // var trs = terrainRoot.Find(terrainName);
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
        public void LoadSplitTerrain(string worldName, Transform envRoot, Action callback = null)
        {
            var worldDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}";
            if (!Directory.Exists(worldDir))
            {
                LogManager.Log(LOGTag, "There is no split terrain data");
                return;
            }

            var dirInfo = new DirectoryInfo(worldDir);
            var subDirs = dirInfo.GetDirectories();
            foreach (var t in subDirs)
            {
                if (!t.Name.StartsWith("Chunk")) continue;
                var split = t.Name.Split('_');
                var row = int.Parse(split[1]);
                var col = int.Parse(split[2]);
                var saveDir = $"{worldDir}/{t.Name}";
                if (!Directory.Exists(saveDir))
                {
                    LogManager.Log(LOGTag, "There is no split terrain data");
                    continue;
                }
                
                LoadTerrainChunk(worldName, envRoot, row, col);
            }

            callback?.Invoke();
        }

        private void LoadTerrainChunk(string worldName, Transform envRoot, int row, int col)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>($"{saveDir}/Terrain.asset");
            var chunkRoot = new GameObject();
            chunkRoot.transform.SetParent(envRoot);
            chunkRoot.transform.localScale = Vector3.one;
            chunkRoot.transform.localRotation = Quaternion.identity;
            chunkRoot.transform.localPosition = new Vector3(row * td.size.x, 0, col * td.size.z);
            chunkRoot.name = $"Chunk_{row}_{col}";
            
            var go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(chunkRoot.transform);
            go.name = "Terrain";
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.gameObject.isStatic = true;

            terrainList.Add(go);
        }

        public static Terrain LoadSingleTerrain(Transform envRoot, string terrainAssetPath,Action<GameObject> callback = null)
        {
            if (!File.Exists(terrainAssetPath))
            {
                LogManager.Log(LOGTag, "该世界不存在地形资源");
                return null;
            }
            
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainAssetPath);
            var go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(envRoot);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.gameObject.isStatic = true;
            callback?.Invoke(go);
            return go.GetComponent<Terrain>();
        }

        #endregion

        #region Split Terrain

        //分割地形
        public static void SplitTerrain(Terrain terrain, string worldName, int rows, int columns)
        {
            if (terrain == null)
            {
                LogManager.LogError(LOGTag, "Target terrain is null");
                return;
            }

            try
            {
                var terrainData = terrain.terrainData;
                // var heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
                // var originalHeightmapResolution = terrainData.heightmapResolution;
                // 计算适应原地图分辨率的混合贴图分辨率和基础贴图分辨率
                var adaptedAlphamapResolution = terrainData.baseMapResolution / rows;
                var adaptedBaseMapResolution = terrainData.alphamapResolution / rows;
                var heightmapResolution = (terrainData.heightmapResolution - 1) / rows;
                var splatProtos = terrainData.splatPrototypes;

                // 计算子地图的大小
                var originalSize = terrainData.size;
                var tileWidth = originalSize.x / columns;
                var tileLength = originalSize.z / rows;

                //循环宽和长,生成小块地形
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < columns; col++)
                    {
                        //创建资源
                        var chunkDir = $"Chunk{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";

                        EditorUtility.DisplayProgressBar("正在分割地形", chunkDir,(row * rows + col) / (float)(rows * columns));
                        var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
                        if (!Directory.Exists(saveDir))
                        {
                            Directory.CreateDirectory(saveDir);
                        }

                        var assetPath = $"{saveDir}/Terrain.asset";
                        if (File.Exists(assetPath))
                        {
                            File.Delete(assetPath);
                            File.Delete($"{assetPath}.meta");
                        }

                        // 创建一个新的GameObject用于表示子地图
                        var tileObject = Terrain.CreateTerrainGameObject(null);
                        tileObject.name = "Tile_" + row + "_" + col;
                        tileObject.transform.SetParent(terrain.transform);

                        //设置高度
                        var xBase = terrainData.heightmapResolution / rows;
                        var yBase = terrainData.heightmapResolution / rows;
                        var height = terrainData.GetHeights( xBase * row,yBase * col, xBase + 1, yBase + 1);

                        // 添加Terrain组件并设置高度图
                        var tileTerrain = tileObject.GetComponent<Terrain>();
                        var terrainData1 = CreateTerrainData(height, adaptedAlphamapResolution, adaptedBaseMapResolution, heightmapResolution, originalSize, rows, columns);
                        tileTerrain.terrainData = terrainData1;
                        terrainData1.name = tileTerrain.name + "_terrainData";

                        //设置地形原型
                        var newSplats = new SplatPrototype[splatProtos.Length];
                        for (var i = 0; i < splatProtos.Length; ++i)
                        {
                            newSplats[i] = new SplatPrototype
                            {
                                texture = splatProtos[i].texture,
                                tileSize = splatProtos[i].tileSize
                            };

                            var offsetX = (terrainData1.size.x * row) % splatProtos[i].tileSize.x + splatProtos[i].tileOffset.x;
                            var offsetY = (terrainData1.size.z * col) % splatProtos[i].tileSize.y + splatProtos[i].tileOffset.y;
                            newSplats[i].tileOffset = new Vector2(offsetX, offsetY);
                        }

                        terrainData1.splatPrototypes = newSplats;

                        // 调整子地图的大小和位置
                        var data = tileTerrain.terrainData;
                        data.size = new Vector3(tileWidth, originalSize.y, tileLength);
                        tileObject.transform.localPosition = new Vector3(col * tileWidth, 0, row * tileLength);
                        
                        //Tree
                        CopyVegetationData(terrainData, data, row, col, rows, columns);

                        // 设置地形纹理,草地
                        CopyTerrainTextureData(terrain, tileTerrain, row, col, rows, columns);
                        
                        AssetDatabase.CreateAsset(terrainData1, assetPath);

                        
                        AssetDatabase.SaveAssets();
                    }
                }
                
                GenerateWorldData(terrain,worldName,rows, columns,tileWidth,tileLength);
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }

        private static void GenerateWorldData(Terrain terrain, string worldName,int rows, int columns,float tileWidth,float tileLength)
        {
            var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            if(File.Exists(savePath))
            {
                File.Delete(savePath);
                File.Delete($"{savePath}.meta");
            }
            
            try
            {
                var fs = File.Create(savePath);//new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
                var formatter = new BinaryFormatter();
                //序列化对象 生成2进制字节数组 写入到内存流当中
                formatter.Serialize(fs, new WorldData
                {
                    TerrainHeight = terrain.terrainData.size.y,//地形高度
                    ChunkRowCount = rows,
                    ChunkColumnCount = columns,
                    ChunkSizeX = tileWidth,
                    ChunkSizeY = tileLength,
                });
                fs.Close();
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message);
            }

            AssetDatabase.Refresh();
        }

        private static TerrainData CreateTerrainData(float[,] heightmap, int alphamapResolution, int baseMapResolution, int heightmapResolution, Vector3 originalSize, int rows, int columns)
        {
            var terrainData = new TerrainData
            {
                heightmapResolution = heightmapResolution,
                alphamapResolution = alphamapResolution,
                baseMapResolution = baseMapResolution,
                size = new Vector3(originalSize.x / columns, originalSize.y, originalSize.z / rows)
            };

            terrainData.SetHeights(0, 0, heightmap);
            return terrainData;
        }

        private static void CopyVegetationData(TerrainData sourceTerrainData, TerrainData targetTerrainData, int row, int col, int rows, int columns)
        {
            // 获取原始地形的植被数据
            var sourceTreePrototypes = sourceTerrainData.treePrototypes;
            var sourceTreeInstances = sourceTerrainData.treeInstances;

            // 清空目标地形的植被数据
            targetTerrainData.treePrototypes = sourceTreePrototypes;

            // 创建新的 List 以保存调整过的植被实例
            var adjustedTreeInstances = new List<TreeInstance>();

            // 计算子地图的范围
            var tileWidth = sourceTerrainData.size.x / columns;
            var tileLength = sourceTerrainData.size.z / rows;

            var startX = col * tileWidth;
            var endX = (col + 1) * tileWidth;

            var startZ = row * tileLength;
            var endZ = (row + 1) * tileLength;

            // 复制植被实例，并根据地形的缩放和位置调整
            foreach (var sourceTreeInstance in sourceTreeInstances)
            {
                // 百分比坐标转换为真实坐标
                var normalizedX = sourceTreeInstance.position.x * sourceTerrainData.size.x;
                var normalizedZ = sourceTreeInstance.position.z * sourceTerrainData.size.z;

                // 检查植被实例是否在当前子地图的范围内
                if (!(normalizedX >= startX) || !(normalizedX < endX) || !(normalizedZ >= startZ) || !(normalizedZ < endZ))
                {
                    continue;
                }
                // 调整植被实例的位置
                var adjustedTreeInstance = sourceTreeInstance;
                adjustedTreeInstance.position = new Vector3(normalizedX % targetTerrainData.size.x / targetTerrainData.size.x, sourceTreeInstance.position.y, normalizedZ % targetTerrainData.size.z / targetTerrainData.size.z);
                LogManager.Log(LOGTag, targetTerrainData.name + "=>" + adjustedTreeInstance.position);
                // 添加调整过的植被实例到 List 中
                adjustedTreeInstances.Add(adjustedTreeInstance);
            }

            // 将 List 转换为数组并赋值给目标地形的植被数据
            targetTerrainData.treeInstances = adjustedTreeInstances.ToArray();
        }

        private static void CopyTerrainTextureData(Terrain sourceTerrain, Terrain targetTerrain, int row, int col, int rows, int columns)
        {
            var sourceTerrainData = sourceTerrain.terrainData;
            var targetTerrainData = targetTerrain.terrainData;

            // 获取原始地形的纹理数据
            var sourceAlphamaps = sourceTerrainData.GetAlphamaps(0, 0, sourceTerrainData.alphamapResolution, sourceTerrainData.alphamapResolution);

            // 计算子地图的范围
            var startAlphamapX = Mathf.FloorToInt((float)col / columns * sourceTerrainData.alphamapResolution);
            var endAlphamapX = Mathf.FloorToInt((float)(col + 1) / columns * sourceTerrainData.alphamapResolution);
            var startAlphamapY = Mathf.FloorToInt((float)row / rows * sourceTerrainData.alphamapResolution);
            var endAlphamapY = Mathf.FloorToInt((float)(row + 1) / rows * sourceTerrainData.alphamapResolution);

            // 计算目标地图的范围
            const int targetStartX = 0;
            var targetEndX = targetTerrainData.alphamapResolution;
            const int targetStartY = 0;
            var targetEndY = targetTerrainData.alphamapResolution;

            // 复制纹理数据
            var targetAlphamaps = new float[targetEndY - targetStartY, targetEndX - targetStartX, sourceTerrainData.alphamapLayers];
            for (var layer = 0; layer < sourceTerrainData.alphamapLayers; layer++)
            {
                for (int y = startAlphamapY, ty = targetStartY; y < endAlphamapY && ty < targetEndY; y++, ty++)
                {
                    for (int x = startAlphamapX, tx = targetStartX; x < endAlphamapX && tx < targetEndX; x++, tx++)
                    {
                        targetAlphamaps[ty - targetStartY, tx - targetStartX, layer] = sourceAlphamaps[y, x, layer];
                    }
                }
            }

            // 设置目标地形的纹理数据
            targetTerrainData.SetAlphamaps(0, 0, targetAlphamaps);

            // 更新地形纹理贴图
            targetTerrain.terrainData = targetTerrainData; // 更新 TerrainData 引用
            targetTerrain.Flush();
        }

        #endregion

        #region Collider Boxes

        //生成碰撞盒
        public void GenColliderBoxes(Transform envRoot, int rows, int columns, Vector2 chunkSize, Vector2 colliderSize, float terrainHeight, Action callback = null)
        {
            colliderList.Clear();
            for (var row = 0; row < rows; ++row)
            {
                for (var col = 0; col < columns; ++col)
                {
                    InstantiateColliderBox(envRoot, row, col, new Vector3(0.5f * chunkSize.x, 0, 0.5f * chunkSize.y), new Vector3(colliderSize.x, terrainHeight, colliderSize.y));
                }
            }
            callback?.Invoke();
        }

        private void InstantiateColliderBox(Transform envRoot, int row, int col, Vector3 position, Vector3 colliderSize)
        {
            var chunkRoot = envRoot.Find($"Chunk_{row}_{col}");
            var oldTrs = chunkRoot.Find("Collider");
            if (oldTrs)
            {
                Object.DestroyImmediate(oldTrs.gameObject);
            }

            var go = new GameObject();
            go.transform.SetParent(chunkRoot);
            var trs = go.transform;
            trs.name = "Collider";
            trs.localRotation = Quaternion.identity;
            trs.localScale = Vector3.one;
            var collider = go.AddComponent<BoxCollider>();
            trs.localPosition = position;
            collider.size = colliderSize;
            collider.isTrigger = true;
            colliderList.Add(go);
        }

        private void GenColliderBox(Transform envRoot, int row, int col, Vector2 chunkSize, Vector2 colliderSize, float terrainHeight,bool isPosition = false)
        {
            // var nodeName = $"{row}_{col}";
            var chunkRoot = envRoot.Find($"Chunk_{row}_{col}");
            var oldTrs = chunkRoot.Find("Collider");
            if (oldTrs)
            {
                Object.DestroyImmediate(oldTrs.gameObject);
            }

            var go = new GameObject();
            go.transform.SetParent(chunkRoot);
            var trs = go.transform;
            trs.name = "Collider";
            trs.localRotation = Quaternion.identity;
            trs.localScale = Vector3.one;
            var collider = go.AddComponent<BoxCollider>();
            trs.localPosition = new Vector3( isPosition ? chunkSize.x : (0.5f * chunkSize.x), 0,  isPosition ? chunkSize.y : (0.5f * chunkSize.y));
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
        public void LoadColliderBoxes(Transform colliderRoot, string worldName, int rows, int columns, Vector2 chunkSize,Action callback = null)
        {
            colliderList.Clear();
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    try
                    {
                        var chunkDir = $"Chunk{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";
                        var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}/Collider.bytes";
                        if (!File.Exists(savePath))
                        {
                            continue;
                        }
                        var data = BinaryUtils.Bytes2Object<ChunkColiderInfo>(ResourcesLoadManager.LoadAsset<TextAsset>(savePath).bytes);
                        InstantiateColliderBox(colliderRoot, row, col,new Vector3(data.PositionX,data.PositionY,data.PositionZ),new Vector3(data.SizeX,data.SizeY,data.SizeZ));
                    }
                    catch (Exception e)
                    {
                        LogManager.LogError(LOGTag, e.Message);
                    }
                }
            }

            callback?.Invoke();
        }

        public static void SaveColliderBoxes(Transform envRoot, string worldName, int rows, int columns, Action callback = null)
        {
            LogManager.Log(LOGTag,worldName,rows,columns);
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    try
                    {
                        var chunkDir = $"Chunk{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";
                        var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}/Collider.bytes";
                        if(File.Exists(savePath))
                        {
                            File.Delete(savePath);
                            File.Delete($"{savePath}.meta");
                        }
                        var colliderTrs = envRoot.Find(chunkDir).Find("Collider");
                        var collider = colliderTrs.GetComponent<BoxCollider>();
                        
                        var fs = File.Create(savePath);//new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
                        var formatter = new BinaryFormatter();
                        //序列化对象 生成2进制字节数组 写入到内存流当中
                        var localPosition = colliderTrs.localPosition;
                        var size = collider.size;
                        formatter.Serialize(fs, new ChunkColiderInfo
                        {
                            PositionX = localPosition.x,//位置
                            PositionY = localPosition.y,//位置
                            PositionZ = localPosition.z,//位置
                            
                            SizeX = size.x,//尺寸
                            SizeY = size.y,//尺寸
                            SizeZ = size.z,//尺寸
          
                        });
                        fs.Close();
                    }
                    catch (Exception e)
                    {
                        LogManager.LogError(LOGTag, e.Message);
                    }
                }
            }
            callback?.Invoke();
            AssetDatabase.Refresh();
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