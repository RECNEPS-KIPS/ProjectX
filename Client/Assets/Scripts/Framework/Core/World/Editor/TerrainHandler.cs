// author:KIPKIPS
// date:2024.10.27 11:50
// describe:处理地形工具

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Framework.Common;
using Framework.Core.Manager.ResourcesLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
        private static readonly Dictionary<(TerrainLayer layerSource, Vector2 newTileOffset), TerrainLayer> layerData = new();
        private const float blendStrengthAtSeems = 0.6f;

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
                var split = t.Name.Split(DEF.TerrainSplitChar);
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
            chunkRoot.name = $"Chunk{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";

            var go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(chunkRoot.transform);
            go.name = "Terrain";
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.gameObject.isStatic = true;

            terrainList.Add(go);
        }

        public static Terrain LoadSingleTerrain(Transform envRoot, string terrainAssetPath, Action<GameObject> callback = null)
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
        public static void SplitTerrain(Terrain terrain, string worldName, int piecesPerAxis)
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
                // var adaptedAlphamapResolution = terrainData.baseMapResolution / rows;
                // var adaptedBaseMapResolution = terrainData.alphamapResolution / rows;
                // var heightmapResolution = (terrainData.heightmapResolution - 1) / rows;
                // // var splatProtos = terrainData.splatPrototypes;
                // var terrainLayers = terrainData.terrainLayers;
                // // 计算子地图的大小
                // var originalSize = terrainData.size;
                // var tileWidth = originalSize.x / rows;
                // var tileLength = originalSize.z / columns;
                //
                // //循环宽和长,生成小块地形
                // for (var row = 0; row < rows; row++)
                // {
                //     for (var col = 0; col < columns; col++)
                //     {
                //         //创建资源
                //         var chunkDir = $"Chunk{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";
                //
                //         EditorUtility.DisplayProgressBar("正在分割地形", chunkDir,(row * rows + col) / (float)(rows * columns));
                //         var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
                //         if (!Directory.Exists(saveDir))
                //         {
                //             Directory.CreateDirectory(saveDir);
                //         }
                //
                //         var assetPath = $"{saveDir}/Terrain.asset";
                //         if (File.Exists(assetPath))
                //         {
                //             File.Delete(assetPath);
                //             File.Delete($"{assetPath}.meta");
                //         }
                //
                //         // 创建一个新的GameObject用于表示子地图
                //         var tileObject = Terrain.CreateTerrainGameObject(null);
                //         tileObject.name = $"Tile{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";
                //         tileObject.transform.SetParent(terrain.transform);
                //
                //         //设置高度
                //         var xBase = terrainData.heightmapResolution / rows;
                //         var yBase = terrainData.heightmapResolution / rows;
                //         var height = terrainData.GetHeights( xBase * row,yBase * col, xBase + 1, yBase + 1);
                //
                //         // 添加Terrain组件并设置高度图
                //         var tileTerrain = tileObject.GetComponent<Terrain>();
                //         var terrainData1 = CreateTerrainData(height, adaptedAlphamapResolution, adaptedBaseMapResolution, heightmapResolution, originalSize, rows, columns);
                //         tileTerrain.terrainData = terrainData1;
                //         terrainData1.name = tileTerrain.name + "_terrainData";
                //
                //         // //设置地形原型
                //         // var newSplats = new SplatPrototype[splatProtos.Length];
                //         // for (var i = 0; i < splatProtos.Length; ++i)
                //         // {
                //         //     newSplats[i] = new SplatPrototype
                //         //     {
                //         //         texture = splatProtos[i].texture,
                //         //         tileSize = splatProtos[i].tileSize
                //         //     };
                //         //
                //         //     var offsetX = (terrainData1.size.x * row) % splatProtos[i].tileSize.x + splatProtos[i].tileOffset.x;
                //         //     var offsetY = (terrainData1.size.z * col) % splatProtos[i].tileSize.y + splatProtos[i].tileOffset.y;
                //         //     newSplats[i].tileOffset = new Vector2(offsetX, offsetY);
                //         // }
                //         //
                //         // terrainData1.splatPrototypes = newSplats;
                //
                //         // var newLayers = new TerrainLayer[terrainLayers.Length];
                //         // for (var i = 0; i < terrainLayers.Length; ++i)
                //         // {
                //         //     var offsetX = (terrainData1.size.x * row) % terrainLayers[i].tileSize.x + terrainLayers[i].tileOffset.x;
                //         //     var offsetY = (terrainData1.size.z * col) % terrainLayers[i].tileSize.y + terrainLayers[i].tileOffset.y;
                //         //     newLayers[i] = new TerrainLayer
                //         //     {
                //         //         tileSize = terrainLayers[i].tileSize,
                //         //         tileOffset = new Vector2(offsetX, offsetY),
                //         //         // diffuseTexture = terrainLayers[i].diffuseTexture,
                //         //         // maskMapTexture = terrainLayers[i].maskMapTexture,
                //         //         // normalMapTexture = terrainLayers[i].normalMapTexture,
                //         //         // normalScale = terrainLayers[i].normalScale,
                //         //         // diffuseRemapMin = terrainLayers[i].diffuseRemapMin,
                //         //         // diffuseRemapMax = terrainLayers[i].diffuseRemapMax,
                //         //         // maskMapRemapMin = terrainLayers[i].maskMapRemapMin,
                //         //         // maskMapRemapMax = terrainLayers[i].maskMapRemapMax,
                //         //         // specular = terrainLayers[i].specular,
                //         //         // metallic = terrainLayers[i].metallic,
                //         //         // smoothness = terrainLayers[i].smoothness,
                //         //     };
                //         //     // newLayers[i].tileOffset = new Vector2(offsetX, offsetY);
                //         //     LogManager.Log(LOGTag,"newLayers",terrainData1.size,terrainLayers[i].tileSize,terrainLayers[i].tileOffset,offsetX,offsetY);
                //         // }
                //
                //         terrainData1.terrainLayers = terrainData.terrainLayers;//newLayers;
                //         terrainData1.detailPrototypes = terrainData.detailPrototypes;
                //         // terrainData1.treePrototypes = terrainData.treePrototypes;
                //         // 调整子地图的大小和位置
                //         var data = tileTerrain.terrainData;
                //         data.size = new Vector3(tileWidth, originalSize.y, tileLength);
                //         tileObject.transform.localPosition = new Vector3(row * tileLength, 0,col * tileWidth );
                //         //Tree
                //         CopyVegetationData(terrainData, data, row, col, rows, columns);
                //
                //         // 设置地形纹理,草地
                //         CopyTerrainTextureData(terrain, tileTerrain, row, col, rows, columns);
                //         
                //         AssetDatabase.CreateAsset(terrainData1, assetPath);
                //         AssetDatabase.SaveAssets();
                //     }
                // }
                SplitTerrainTile(piecesPerAxis, terrain, worldName);

                // GenerateWorldData(terrain,worldName,piecesPerAxis,tileWidth,tileLength);
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                terrain.enabled = false;
                // AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }
        }

        public static string GetAssetPathBySliceIndex(string worldName,int sliceIndex)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{sliceIndex}";
            return $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
        }

        private static void SplitTerrainTile(int piecesPerAxis, Terrain sourceTerrain, string worldName)
        {
            TerrainData sourceTerrainData = sourceTerrain.terrainData;
            var terrains = new Terrain[piecesPerAxis * piecesPerAxis];

            var sourceControlTextures = sourceTerrainData.GetAlphamaps(0, 0, sourceTerrainData.alphamapResolution, sourceTerrainData.alphamapResolution);
            SmoothSeems(sourceControlTextures, piecesPerAxis);

            //Split terrain 
            for (var sliceIndex = 0; sliceIndex < terrains.Length; sliceIndex++)
            {
                var targetTerrainData = new TerrainData();
                GameObject terrainGameObject = Terrain.CreateTerrainGameObject(targetTerrainData);

                terrainGameObject.name = $"{sourceTerrain.name}_{sliceIndex}";

                Terrain targetTerrain = terrains[sliceIndex] = terrainGameObject.GetComponent<Terrain>();
                targetTerrain.terrainData = targetTerrainData;
                var chunkDir = $"Chunk{DEF.TerrainSplitChar}{sliceIndex}";
                EditorUtility.DisplayProgressBar("正在分割地形", chunkDir,sliceIndex / (float)terrains.Length);
                var saveDir = GetAssetPathBySliceIndex(worldName,sliceIndex);
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

                // SaveNewAsset(targetTerrainData, assetPath);
                AssetDatabase.CreateAsset(targetTerrainData, assetPath);

                CopyPrototypes(worldName,sourceTerrainData, targetTerrainData, piecesPerAxis, sliceIndex);
                CopyTerrainProperties(sourceTerrain, targetTerrain, piecesPerAxis);
                SetTerrainSlicePosition(sourceTerrain, piecesPerAxis, sliceIndex, terrainGameObject.transform);
                SplitHeightMap(worldName,targetTerrainData, piecesPerAxis, sliceIndex, sourceTerrainData);
                SplitControlTexture(targetTerrainData, piecesPerAxis, sliceIndex, sourceTerrainData, sourceControlTextures);
                SplitDetailMap(targetTerrainData, piecesPerAxis, sliceIndex, sourceTerrainData);

                AssetDatabase.SaveAssets();
            }

            SplitTrees(piecesPerAxis, terrains, sourceTerrain);
            AssetDatabase.SaveAssets();

            layerData.Clear();
        }

        private static void SplitDetailMap(TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex, TerrainData sourceTerrainData)
        {
            var sourceResolution = sourceTerrainData.detailResolution;
            var targetResolution = Math.Max(MINIMAL_DETAIL_RESOLUTION, sourceResolution / piecesPerAxis);
            var detailResolutionPerPatch = Math.Min(targetResolution, Math.Max(MINIMAL_DETAIL_RESOLUTION, sourceTerrainData.detailResolutionPerPatch));

            targetTerrainData.SetDetailResolution(targetResolution, detailResolutionPerPatch);

            var xShift = sliceIndex % piecesPerAxis * sourceResolution / piecesPerAxis;
            var yShift = sliceIndex / piecesPerAxis * sourceResolution / piecesPerAxis;
            var sampleRatio = targetResolution * piecesPerAxis / (float)sourceResolution;

            var sourceResolutionMinus1 = sourceResolution - 1;

            for (var detLay = 0; detLay < sourceTerrainData.detailPrototypes.Length; detLay++)
            {
                var parentDetail = sourceTerrainData.GetDetailLayer(0, 0, sourceResolution, sourceResolution, detLay);

                var detailResolution = targetResolution;
                var pieceDetail = new int[detailResolution, detailResolution];

                for (var x = 0; x < targetResolution; x++)
                {
                    if (x % 100 == 0)
                        EditorUtility.DisplayProgressBar("Split terrain", "Split details", (float)x / targetResolution);

                    var xPos = xShift + x / sampleRatio;
                    for (var y = 0; y < targetResolution; y++)
                    {
                        var yPos = yShift + y / sampleRatio;
                        var ph = Interpolate(parentDetail, xPos, yPos, sourceResolutionMinus1);

                        pieceDetail[x, y] = (int)(ph / sampleRatio);
                    }
                }

                EditorUtility.ClearProgressBar();

                targetTerrainData.SetDetailLayer(0, 0, detLay, pieceDetail);
            }
        }

        private static void SplitHeightMap(string worldName,TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex, TerrainData sourceTerrainData)
        {
            var sourceHeightmapResolutionPlusOne = sourceTerrainData.heightmapResolution;
            var sourceHeightmapResolution = sourceHeightmapResolutionPlusOne - 1;
            var targetHeightmapResolution = NextPowerOf2(sourceHeightmapResolution / piecesPerAxis);
            targetHeightmapResolution = Math.Max(MINIMAL_HEIGHTMAP_RESOLUTION - 1, targetHeightmapResolution);

            targetTerrainData.heightmapResolution = targetHeightmapResolution;
            targetTerrainData.size = new Vector3(sourceTerrainData.size.x / piecesPerAxis, sourceTerrainData.size.y, sourceTerrainData.size.z / piecesPerAxis);

            var pieceHeight = new float[targetHeightmapResolution + 1, targetHeightmapResolution + 1];

            var sampleRatio = targetHeightmapResolution * piecesPerAxis / (float)sourceHeightmapResolution;
            var xShift = sliceIndex % piecesPerAxis * sourceHeightmapResolution / piecesPerAxis;
            var yShift = sliceIndex / piecesPerAxis * sourceHeightmapResolution / piecesPerAxis;

            var parentHeights = sourceTerrainData.GetHeights(0, 0, sourceHeightmapResolutionPlusOne, sourceHeightmapResolutionPlusOne);
            var parentWidth = parentHeights.GetLength(0);
            var parentWidthMinus1 = parentWidth - 1;

            for (var x = 0; x <= targetHeightmapResolution; x++)
            {
                if (x % 100 == 0)
                    EditorUtility.DisplayProgressBar("Split terrain", "Split height", (float)x / targetHeightmapResolution);

                var xPos = xShift + x / sampleRatio;
                for (var y = 0; y <= targetHeightmapResolution; y++)
                {
                    var yPos = yShift + y / sampleRatio;
                    var ph = Interpolate(parentHeights, xPos, yPos, parentWidthMinus1);

                    pieceHeight[x, y] = ph;
                }
            }

            EditorUtility.ClearProgressBar();

            targetTerrainData.SetHeights(0, 0, pieceHeight);
        }

        private static void CopyPrototypes(string worldName,TerrainData sourceTerrainData, TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex)
        {
            targetTerrainData.terrainLayers = sourceTerrainData.terrainLayers
                .Select(x => GetOrCreateTerrainLayer(worldName,sourceTerrainData.size, piecesPerAxis, sliceIndex, x))
                .ToArray();

            targetTerrainData.detailPrototypes = sourceTerrainData.detailPrototypes;
            targetTerrainData.treePrototypes = sourceTerrainData.treePrototypes;
        }

        private static TerrainLayer GetOrCreateTerrainLayer(string worldName,Vector3 sourceSize, int piecesPerAxis, int sliceIndex, TerrainLayer layerSource)
        {
            var targetSize = new Vector2(sourceSize.x / piecesPerAxis, sourceSize.z / piecesPerAxis);
            var shift = ComputeShift(piecesPerAxis, sliceIndex, targetSize.y, targetSize.x);
            Vector2 layerSize = layerSource.tileSize;

            var newTileOffset = new Vector2(
                (layerSource.tileOffset.x + shift.y * piecesPerAxis + layerSize.x) % layerSize.x,
                (layerSource.tileOffset.y + shift.x * piecesPerAxis + layerSize.y) % layerSize.y);

            var assetPath = AssetDatabase.GetAssetPath(layerSource);
            var isSourceLayerAnAsset = !string.IsNullOrEmpty(assetPath);

            if (!isSourceLayerAnAsset)
            {
                if (!AssetDatabase.IsValidFolder("Assets/TerrainLayers"))
                    AssetDatabase.CreateFolder("Assets", "TerrainLayers");

                var layerSourceName = Guid.NewGuid().ToString();
                assetPath = $"{GetAssetPathBySliceIndex(worldName,sliceIndex)}/TerrainLayers/{layerSourceName}.terrainlayer";
                layerSource.name = layerSourceName;
                if (File.Exists(assetPath))
                {
                    File.Delete(assetPath);
                    File.Delete($"{assetPath}.meta");
                }
                // AssetDatabase.SaveNewAsset(layerSource, assetPath);
                AssetDatabase.CreateAsset(layerSource, assetPath);
                layerSource = AssetDatabase.LoadAssetAtPath<TerrainLayer>(assetPath);
            }

            var originalLayer = (layerSource, Vector2.zero);
            if (!layerData.ContainsKey(originalLayer))
                layerData[originalLayer] = layerSource;

            if (!layerData.TryGetValue((layerSource, newTileOffset), out var layer))
            {
                assetPath = CreateLayerPath(assetPath, newTileOffset);

                var terrainLayer = CloneTerrainLayer(layerSource, layerSource.name, newTileOffset);
                if (File.Exists(assetPath))
                {
                    File.Delete(assetPath);
                    File.Delete($"{assetPath}.meta");
                }
                // SaveNewAsset(terrainLayer, assetPath);
                AssetDatabase.CreateAsset(layerSource, assetPath);

                layerData[(layerSource, newTileOffset)] = layer = terrainLayer;
            }

            return layer;
        }

        private static void SmoothSeems(float[,,] sourceControlTextures, int piecesPerAxis)
        {
            var layerCount = sourceControlTextures.GetLength(2);

            var width = sourceControlTextures.GetLength(0);
            int xStep = width / piecesPerAxis;
            var height = sourceControlTextures.GetLength(1);
            int yStep = height / piecesPerAxis;

            for (int x = 1; x < piecesPerAxis; x++)
            {
                int xPos = x * xStep;
                for (int yPos = 0; yPos < height; yPos++)
                {
                    for (int i = 0; i < layerCount; i++)
                    {
                        var c0 = sourceControlTextures[xPos - 1, yPos, i];
                        var c1 = sourceControlTextures[xPos, yPos, i];
                        var mix0 = Mathf.Lerp(c1, c0, blendStrengthAtSeems);
                        var mix1 = Mathf.Lerp(c0, c1, blendStrengthAtSeems);
                        sourceControlTextures[xPos - 1, yPos, i] = mix0;
                        sourceControlTextures[xPos, yPos, i] = mix1;
                    }
                }
            }

            for (int y = 1; y < piecesPerAxis; y++)
            {
                int yPos = y * yStep;
                for (int xPos = 0; xPos < width; xPos++)
                {
                    for (int i = 0; i < layerCount; i++)
                    {
                        var c0 = sourceControlTextures[xPos, yPos - 1, i];
                        var c1 = sourceControlTextures[xPos, yPos, i];
                        var mix0 = Mathf.Lerp(c1, c0, blendStrengthAtSeems);
                        var mix1 = Mathf.Lerp(c0, c1, blendStrengthAtSeems);
                        sourceControlTextures[xPos, yPos - 1, i] = mix0;
                        sourceControlTextures[xPos, yPos, i] = mix1;
                    }
                }
            }
        }

        private static void SplitTrees(int terraPieces, Terrain[] tiles, Terrain sourceTerrain)
        {
            var terrainDataTreeInstances = sourceTerrain.terrainData.treeInstances;

            var stepSize = 1f / terraPieces;

            for (var t = 0; t < terrainDataTreeInstances.Length; t++)
            {
                if (t % 100 == 0)
                    EditorUtility.DisplayProgressBar("Split terrain", "Split trees ", (float)t / terrainDataTreeInstances.Length);

                // Get tree instance					
                TreeInstance ti = terrainDataTreeInstances[t];
                Vector3 treePosition = ti.position;

                for (var i = 0; i < tiles.Length; i++)
                {
                    var splitRect = new Rect(i / terraPieces * stepSize, i % terraPieces * stepSize, stepSize, stepSize);

                    if (!splitRect.Contains(new Vector2(treePosition.x, treePosition.z)))
                        continue;

                    // Recalculate new tree position	
                    ti.position = new Vector3((treePosition.x - splitRect.x) * terraPieces, treePosition.y, (treePosition.z - splitRect.y) * terraPieces);

                    // Add tree instance						
                    tiles[i]?.AddTreeInstance(ti);
                    break;
                }
            }
        }

        private static void CopyTerrainProperties(Terrain sourceTerrain, Terrain targetTerrain, int piecesPerAxis)
        {
            targetTerrain.basemapDistance = sourceTerrain.basemapDistance;
            targetTerrain.shadowCastingMode = sourceTerrain.shadowCastingMode;
            targetTerrain.detailObjectDensity = sourceTerrain.detailObjectDensity;
            targetTerrain.detailObjectDistance = sourceTerrain.detailObjectDistance;
            targetTerrain.heightmapMaximumLOD = sourceTerrain.heightmapMaximumLOD;
            targetTerrain.heightmapPixelError = sourceTerrain.heightmapPixelError;
            targetTerrain.treeBillboardDistance = sourceTerrain.treeBillboardDistance;
            targetTerrain.treeCrossFadeLength = sourceTerrain.treeCrossFadeLength;
            targetTerrain.treeDistance = sourceTerrain.treeDistance;
            targetTerrain.treeMaximumFullLODCount = sourceTerrain.treeMaximumFullLODCount;
            targetTerrain.bakeLightProbesForTrees = sourceTerrain.bakeLightProbesForTrees;
            targetTerrain.drawInstanced = sourceTerrain.drawInstanced;
            targetTerrain.reflectionProbeUsage = sourceTerrain.reflectionProbeUsage;
            targetTerrain.realtimeLightmapScaleOffset = sourceTerrain.realtimeLightmapScaleOffset;
            targetTerrain.lightmapScaleOffset = sourceTerrain.lightmapScaleOffset;

            SetLightmapScale(sourceTerrain, targetTerrain, piecesPerAxis);

            targetTerrain.terrainData.wavingGrassAmount = sourceTerrain.terrainData.wavingGrassAmount;
            targetTerrain.terrainData.wavingGrassSpeed = sourceTerrain.terrainData.wavingGrassSpeed;
            targetTerrain.terrainData.wavingGrassStrength = sourceTerrain.terrainData.wavingGrassStrength;
            targetTerrain.terrainData.wavingGrassTint = sourceTerrain.terrainData.wavingGrassTint;
        }

        private static void SetLightmapScale(Terrain sourceTerrain, Terrain targetTerrain, int piecesPerAxis)
        {
            var sos = new SerializedObject(sourceTerrain);
            var sourceValue = sos.FindProperty("m_ScaleInLightmap").floatValue;
            sos.ApplyModifiedProperties();

            var so = new SerializedObject(targetTerrain);
            so.FindProperty("m_ScaleInLightmap").floatValue = sourceValue / piecesPerAxis;
            so.ApplyModifiedProperties();
        }

        private static void SetTerrainSlicePosition(Terrain sourceTerrain, int piecesPerAxis, int sliceIndex, Transform terrainTransform)
        {
            Vector3 parentPosition = sourceTerrain.GetPosition();

            var sizeY = sourceTerrain.terrainData.size.z;
            var sizeX = sourceTerrain.terrainData.size.x;

            var wShift = ComputeShift(piecesPerAxis, sliceIndex, sizeY, sizeX);

            terrainTransform.position = new Vector3(terrainTransform.position.x + wShift.y, terrainTransform.position.y, terrainTransform.position.z + wShift.x);

            terrainTransform.position = new Vector3(terrainTransform.position.x + parentPosition.x, terrainTransform.position.y + parentPosition.y,
                terrainTransform.position.z + parentPosition.z);
        }

        private static Vector2 ComputeShift(int piecesPerAxis, int sliceIndex, float sizeY, float sizeX)
        {
            var spaceShiftX = sizeY / piecesPerAxis;
            var spaceShiftY = sizeX / piecesPerAxis;

            var xWShift = sliceIndex % piecesPerAxis * spaceShiftX;
            var zWShift = sliceIndex / piecesPerAxis * spaceShiftY;
            return new Vector2(xWShift, zWShift);
        }

        public static bool IsPowerOf2(int value)
        {
            return (value & (value - 1)) == 0;
        }

        public static int ClosestPowerOf2(int value)
        {
            var next = (int)Math.Pow(2, Math.Ceiling(Math.Log(value) / Math.Log(2)));
            return next;
        }

        public static int NextPowerOf2(int value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }

        private const int MINIMAL_HEIGHTMAP_RESOLUTION = 33;
        private const int MINIMAL_CONTROL_TEXTURE_RESOLUTION = 16;
        private const int MINIMAL_BASE_TEXTURE_RESOLUTION = 16;
        private const int MINIMAL_DETAIL_RESOLUTION = 8;

        private static void SplitControlTexture(TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex, TerrainData sourceTerrainData, float[,,] sourceControlTextures)
        {
            var sourceControlTextureResolution = sourceTerrainData.alphamapResolution;
            var sourceControlTextureResolutionMinus1 = sourceControlTextureResolution - 1;
            var sourceBaseMapResolution = sourceTerrainData.baseMapResolution;
            var controlTextureResolution = NextPowerOf2(sourceControlTextureResolution / piecesPerAxis);
            var targetControlTextureResolution = Math.Max(MINIMAL_CONTROL_TEXTURE_RESOLUTION, controlTextureResolution);
            var baseMapResolution = NextPowerOf2(sourceBaseMapResolution / piecesPerAxis);
            var targetBaseMapResolution = Math.Max(MINIMAL_BASE_TEXTURE_RESOLUTION, baseMapResolution);

            targetTerrainData.alphamapResolution = targetControlTextureResolution;
            targetTerrainData.baseMapResolution = targetBaseMapResolution;

            var targetControlTextures = new float[targetControlTextureResolution, targetControlTextureResolution, sourceTerrainData.alphamapLayers];

            var xShift = sliceIndex % piecesPerAxis * sourceControlTextureResolution / piecesPerAxis;
            var yShift = sliceIndex / piecesPerAxis * sourceControlTextureResolution / piecesPerAxis;
            var sampleRatio = targetControlTextureResolution * piecesPerAxis / (float)sourceControlTextureResolution;

            for (var layerIndex = 0; layerIndex < sourceTerrainData.alphamapLayers; layerIndex++)
            {
                for (var x = 0; x < targetControlTextureResolution; x++)
                {
                    if (x % 100 == 0)
                        EditorUtility.DisplayProgressBar("Split terrain", "Split splat", (float)x / targetControlTextureResolution);

                    var xPos = xShift + x / sampleRatio;
                    for (var y = 0; y < targetControlTextureResolution; y++)
                    {
                        var yPos = yShift + y / sampleRatio;
                        var ph = Interpolate(sourceControlTextures, xPos, yPos, layerIndex, sourceControlTextureResolutionMinus1);

                        targetControlTextures[x, y, layerIndex] = ph;
                    }
                }

                StoreLayer(targetControlTextures, targetTerrainData.name, layerIndex);
            }

            EditorUtility.ClearProgressBar();

            targetTerrainData.SetAlphamaps(0, 0, targetControlTextures);
        }

        [Conditional("DEBUG_ALPHAMAPS")]
        private static void StoreLayer(float[,,] targetControlTextures, string name, int layerIndex)
        {
            var width = targetControlTextures.GetLength(0);
            var height = targetControlTextures.GetLength(1);
            var tex = new Texture2D(width, height,
                GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);

            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var intensity = targetControlTextures[x, y, layerIndex];
                    colors[y * width + x] = new Color(intensity, 0, 0, 1);
                }
            }

            tex.SetPixels(colors);
            var pngData = tex.EncodeToPNG();

            var folder = $"{Application.dataPath}\\PNG\\";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllBytes($"{folder}layer_{name}_{layerIndex}.png", pngData);
        }

        private static void GenerateWorldData(Terrain terrain, string worldName, int piecesPerAxis, float tileWidth, float tileLength)
        {
            var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                File.Delete($"{savePath}.meta");
            }

            try
            {
                var fs = File.Create(savePath); //new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
                var formatter = new BinaryFormatter();
                //序列化对象 生成2进制字节数组 写入到内存流当中
                formatter.Serialize(fs, new WorldData
                {
                    TerrainHeight = terrain.terrainData.size.y, //地形高度
                    PiecesPerAxis = piecesPerAxis,
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

        #endregion

        #region Collider Boxes

        //生成碰撞盒
        public void GenColliderBoxes(Transform envRoot, int PiecesPerAxis, Vector2 chunkSize, Vector2 colliderSize, float terrainHeight, Action callback = null)
        {
            colliderList.Clear();
            for (var i = 0; i < PiecesPerAxis * PiecesPerAxis; ++i)
            {
                InstantiateColliderBox(envRoot, i, new Vector3(0.5f * chunkSize.x, 0, 0.5f * chunkSize.y), new Vector3(colliderSize.x, terrainHeight, colliderSize.y));
            }

            callback?.Invoke();
        }

        private void InstantiateColliderBox(Transform envRoot, int PiecesPerAxis, Vector3 position, Vector3 colliderSize)
        {
            var chunkRoot = envRoot.Find($"Chunk{DEF.TerrainSplitChar}{PiecesPerAxis}");
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

        private void GenColliderBox(Transform envRoot, int index, Vector2 chunkSize, Vector2 colliderSize, float terrainHeight, bool isPosition = false)
        {
            // var nodeName = $"{row}{DEF.TerrainSplitChar}{col}";
            var chunkRoot = envRoot.Find($"Chunk{DEF.TerrainSplitChar}{index}");
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
            trs.localPosition = new Vector3(isPosition ? chunkSize.x : (0.5f * chunkSize.x), 0, isPosition ? chunkSize.y : (0.5f * chunkSize.y));
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
        public void LoadColliderBoxes(Transform colliderRoot, string worldName, int PiecesPerAxis, Vector2 chunkSize, Action callback = null)
        {
            colliderList.Clear();
            for (var i = 0; i < PiecesPerAxis * PiecesPerAxis; i++)
            {
                try
                {
                    var chunkDir = $"Chunk{DEF.TerrainSplitChar}{i}";
                    var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}/Collider.bytes";
                    if (!File.Exists(savePath))
                    {
                        continue;
                    }

                    var data = BinaryUtils.Bytes2Object<ChunkColiderInfo>(ResourcesLoadManager.LoadAsset<TextAsset>(savePath).bytes);
                    InstantiateColliderBox(colliderRoot, i, new Vector3(data.PositionX, data.PositionY, data.PositionZ), new Vector3(data.SizeX, data.SizeY, data.SizeZ));
                }
                catch (Exception e)
                {
                    LogManager.LogError(LOGTag, e.Message);
                }
            }

            callback?.Invoke();
        }

        public static void SaveColliderBoxes(Transform envRoot, string worldName, int piecesPerAxis, Action callback = null)
        {
            LogManager.Log(LOGTag, worldName, piecesPerAxis);
            for (var i = 0; i < piecesPerAxis * piecesPerAxis; i++)
            {
                try
                {
                    var chunkDir = $"Chunk{DEF.TerrainSplitChar}{i}";
                    var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}/Collider.bytes";
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                        File.Delete($"{savePath}.meta");
                    }

                    var colliderTrs = envRoot.Find(chunkDir).Find("Collider");
                    var collider = colliderTrs.GetComponent<BoxCollider>();

                    var fs = File.Create(savePath); //new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    var formatter = new BinaryFormatter();
                    //序列化对象 生成2进制字节数组 写入到内存流当中
                    var localPosition = colliderTrs.localPosition;
                    var size = collider.size;
                    formatter.Serialize(fs, new ChunkColiderInfo
                    {
                        PositionX = localPosition.x, //位置
                        PositionY = localPosition.y, //位置
                        PositionZ = localPosition.z, //位置

                        SizeX = size.x, //尺寸
                        SizeY = size.y, //尺寸
                        SizeZ = size.z, //尺寸
                    });
                    fs.Close();
                }
                catch (Exception e)
                {
                    LogManager.LogError(LOGTag, e.Message);
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


        public static Vector2 XZ(Vector3 that)
        {
            return new Vector2(that.x, that.z);
        }

        /// <summary>
        /// If all fields are the same except of the tile offset it is considered similar.
        /// </summary>
        public static bool IsVisuallySame(TerrainLayer that, TerrainLayer other)
        {
            if (that == null && other != null)
                return false;

            if (that != null && other == null)
                return false;

            if (that == null)
                return true;

            return that.diffuseTexture == other.diffuseTexture && that.maskMapTexture == other.maskMapTexture &&
                   that.normalMapTexture == other.normalMapTexture && that.diffuseRemapMax == other.diffuseRemapMax &&
                   that.diffuseRemapMin == other.diffuseRemapMin && that.maskMapRemapMax == other.maskMapRemapMax &&
                   that.maskMapRemapMin == other.maskMapRemapMin && Math.Abs(that.metallic - other.metallic) <= float.Epsilon &&
                   Math.Abs(that.normalScale - other.normalScale) <= float.Epsilon && Math.Abs(that.smoothness - other.smoothness) <= float.Epsilon &&
                   that.specular == other.specular && that.tileSize == other.tileSize;
        }

        public static TerrainLayer CloneTerrainLayer(TerrainLayer layerSource, string layerSourceName, Vector2 newTileOffset)
        {
            var terrainLayer = new TerrainLayer
            {
                tileSize = layerSource.tileSize, diffuseRemapMax = layerSource.diffuseRemapMax,
                diffuseRemapMin = layerSource.diffuseRemapMin, diffuseTexture = layerSource.diffuseTexture,
                maskMapRemapMax = layerSource.maskMapRemapMax, maskMapRemapMin = layerSource.maskMapRemapMin,
                maskMapTexture = layerSource.maskMapTexture, metallic = layerSource.metallic,
                name = layerSourceName, normalMapTexture = layerSource.normalMapTexture, normalScale = layerSource.normalScale,
                smoothness = layerSource.smoothness, specular = layerSource.specular,
                tileOffset = newTileOffset
            };
            return terrainLayer;
        }

        public static string CreateLayerPath(string sourceAssetPath, Vector2 newTileOffset)
        {
            var extension = Path.GetExtension(sourceAssetPath);
            var pathWithoutExtension = Path.ChangeExtension(sourceAssetPath, "");
            pathWithoutExtension = pathWithoutExtension.Substring(0, pathWithoutExtension.Length - 1);
            var newPath = $"{pathWithoutExtension}_{newTileOffset.x}_{newTileOffset.y}{extension}";
            return newPath;
        }

        public static void SaveNewAsset<T>(T asset, string newAssetPath) where T : Object
        {
            Object currentAssetAtPath = null;

            do
            {
                currentAssetAtPath = AssetDatabase.LoadAssetAtPath<Object>(newAssetPath);

                if (currentAssetAtPath != null)
                {
                    var filePath = newAssetPath.Substring(0, newAssetPath.LastIndexOf('/'));
                    var fileName = Path.GetFileNameWithoutExtension(newAssetPath);
                    var fileNameNumber = new string(fileName.Reverse().TakeWhile(x => char.IsDigit(x)).Reverse().ToArray());
                    var fileNameText = fileName.Substring(0, fileName.Length - fileNameNumber.Length);
                    var fileExtension = Path.GetExtension(newAssetPath);

                    int number;
                    fileNameNumber = int.TryParse(fileNameNumber, out number) ? (number + 1).ToString() : "1";

                    newAssetPath = $"{filePath}/{fileNameText}{fileNameNumber}{fileExtension}";
                }
            } while (currentAssetAtPath != null);

            AssetDatabase.CreateAsset(asset, newAssetPath);
        }

        public static float Interpolate(float[,] data, float xPos, float yPos, int parentWidthMinus1)
        {
            var x = parentWidthMinus1 < xPos ? parentWidthMinus1 : xPos;
            var y = parentWidthMinus1 < yPos ? parentWidthMinus1 : yPos;
            var xi = (int)x;
            var yi = (int)y;
            var b = xi + 1;
            var xiPlus1 = parentWidthMinus1 < b ? parentWidthMinus1 : b;
            var b1 = yi + 1;
            var yiPlus1 = parentWidthMinus1 < b1 ? parentWidthMinus1 : b1;

            var c00 = data[xi, yi];
            var c10 = data[xiPlus1, yi];
            var c01 = data[xi, yiPlus1];
            var c11 = data[xiPlus1, yiPlus1];

            var tx = x - xi;
            var ty = y - yi;
            var s = c00 + (c10 - c00) * tx;
            var e = c01 + (c11 - c01) * tx;
            return s + (e - s) * ty;
        }

        public static float Interpolate(int[,] data, float xPos, float yPos, int parentResolutionMinus1)
        {
            var x = parentResolutionMinus1 < xPos ? parentResolutionMinus1 : xPos;
            var y = parentResolutionMinus1 < yPos ? parentResolutionMinus1 : yPos;
            var xi = (int)x;
            var yi = (int)y;
            var b = xi + 1;
            var xiPlus1 = parentResolutionMinus1 < b ? parentResolutionMinus1 : b;
            var b1 = yi + 1;
            var yiPlus1 = parentResolutionMinus1 < b1 ? parentResolutionMinus1 : b1;

            float c00 = data[xi, yi];
            float c10 = data[xiPlus1, yi];
            float c01 = data[xi, yiPlus1];
            float c11 = data[xiPlus1, yiPlus1];

            var tx = x - xi;
            var ty = y - yi;
            var s = c00 + (c10 - c00) * tx;
            var e = c01 + (c11 - c01) * tx;
            return s + (e - s) * ty;
        }

        public static float Interpolate(float[,,] data, float xPos, float yPos, int layerIndex, int parentWidthMinus1)
        {
            float x = parentWidthMinus1 >= xPos ? xPos : parentWidthMinus1;
            float y = parentWidthMinus1 >= yPos ? yPos : parentWidthMinus1;
            int xi = (int)x;
            int yi = (int)y;
            int b = xi + 1;
            int xiPlus1 = parentWidthMinus1 >= b ? b : parentWidthMinus1;
            int b1 = yi + 1;
            int yiPlus1 = parentWidthMinus1 >= b1 ? b1 : parentWidthMinus1;

            float c00 = data[xi, yi, layerIndex];
            float c10 = data[xiPlus1, yi, layerIndex];
            float c01 = data[xi, yiPlus1, layerIndex];
            float c11 = data[xiPlus1, yiPlus1, layerIndex];

            float tx = x - xi;
            float ty = y - yi;
            float s = c00 + (c10 - c00) * tx;
            float e = c01 + (c11 - c01) * tx;
            return s + (e - s) * ty;
        }
    }
}