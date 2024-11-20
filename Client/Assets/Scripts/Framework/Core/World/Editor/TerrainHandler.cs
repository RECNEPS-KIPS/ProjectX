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
using MathUtils = Framework.Common.MathUtils;
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
            for (var i = 0; i < terrainList.Count; i++)
            {
                GameObject.DestroyImmediate(terrainList[i]);
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
            
            
            var worldDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            if (!File.Exists(worldDataPath))
            {
                LogManager.Log(LOGTag, $"There is no world data,path:{worldName}");
                return;
            }
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(worldDataPath);
            var data = BinaryUtils.Bytes2Object<WorldData>(assetData.bytes);

            var dirInfo = new DirectoryInfo(worldDir);
            var subDirs = dirInfo.GetDirectories();
            foreach (var t in subDirs)
            {
                if (!t.Name.StartsWith("Chunk")) continue;
                var split = t.Name.Split(DEF.TerrainSplitChar);
                var index = int.Parse(split[1]);
                var saveDir = $"{worldDir}/{t.Name}";
                if (!Directory.Exists(saveDir))
                {
                    LogManager.Log(LOGTag, "There is no split terrain data");
                    continue;
                }

                LoadTerrainChunk(worldName, envRoot, data.PiecesPerAxis,index);
            }

            callback?.Invoke();
        }

        private void LoadTerrainChunk(string worldName, Transform envRoot,int piecesPerAxis, int index)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{index}";
            var terrainInfoPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}/TerrainInfo.bytes";
            if (!File.Exists(terrainInfoPath))
            {
                LogManager.Log(LOGTag, $"There is no terrain info,path:{terrainInfoPath}");
                return;
            }
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(terrainInfoPath);
            var data = BinaryUtils.Bytes2Object<TerrainInfo>(assetData.bytes);
            
            
            // var x = Mathf.FloorToInt(index / (float)piecesPerAxis);
            // var y = index - x * piecesPerAxis;
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>($"{saveDir}/Terrain.asset");
            var chunkName = $"Chunk{DEF.TerrainSplitChar}{index}";
            var chunkRoot = envRoot.Find(chunkName);
            if (chunkRoot == null)
            {
                chunkRoot = new GameObject(chunkName).transform;
            }
            chunkRoot.SetParent(envRoot);
            chunkRoot.localScale = Vector3.one;
            chunkRoot.localRotation = Quaternion.identity;
            // chunkRoot.transform.localPosition = new Vector3(x * td.size.x, 0, y * td.size.z);
            chunkRoot.localPosition = new Vector3(data.X, data.Y, data.Z);
            // chunkRoot.name = $"Chunk{DEF.TerrainSplitChar}{index}";

            var go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(chunkRoot);
            go.name = "Terrain";
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.gameObject.isStatic = true;

            terrainList.Add(go);
        }

        public Terrain LoadSingleTerrain(Transform envRoot, string terrainAssetPath, Action<GameObject> callback = null)
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
        public void SplitTerrain(Terrain terrain, string worldName, int piecesPerAxis,Transform envRoot)
        {
            if (terrain == null)
            {
                LogManager.LogError(LOGTag, "Target terrain is null");
                return;
            }

            try
            {
                SplitTerrainTile(piecesPerAxis, terrain, worldName,envRoot);
                var terrainData = terrain.terrainData;
                GenerateWorldData(terrain,worldName,piecesPerAxis,terrainData.size.x / piecesPerAxis,terrainData.size.z / piecesPerAxis);
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                terrain.enabled = false;
                EditorUtility.ClearProgressBar();
            }
        }

        public string GetAssetPathBySliceIndex(string worldName,int sliceIndex)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{sliceIndex}";
            return $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
        }

        private void SplitTerrainTile(int piecesPerAxis, Terrain sourceTerrain, string worldName,Transform envRoot)
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

                terrainGameObject.name = $"Chunk{DEF.TerrainSplitChar}{sliceIndex}";

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
                
                AssetDatabase.CreateAsset(targetTerrainData, assetPath);

                CopyPrototypes(worldName,sourceTerrainData, targetTerrainData, piecesPerAxis, sliceIndex);
                CopyTerrainProperties(sourceTerrain, targetTerrain, piecesPerAxis);
                SetTerrainSlicePosition(worldName,sourceTerrain, piecesPerAxis, sliceIndex, terrainGameObject.transform,envRoot);
                SplitHeightMap(targetTerrainData, piecesPerAxis, sliceIndex, sourceTerrainData);
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

        private static void SplitHeightMap(TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex, TerrainData sourceTerrainData)
        {
            var sourceHeightmapResolutionPlusOne = sourceTerrainData.heightmapResolution;
            var sourceHeightmapResolution = sourceHeightmapResolutionPlusOne - 1;
            var targetHeightmapResolution = MathUtils.NextPowerOf2(sourceHeightmapResolution / piecesPerAxis);
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
                {
                    EditorUtility.DisplayProgressBar("Split terrain", "Split height", (float)x / targetHeightmapResolution);
                }

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

        private void CopyPrototypes(string worldName,TerrainData sourceTerrainData, TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex)
        {
            targetTerrainData.terrainLayers = sourceTerrainData.terrainLayers
                .Select(x => GetOrCreateTerrainLayer(worldName,sourceTerrainData.size, piecesPerAxis, sliceIndex, x))
                .ToArray();

            targetTerrainData.detailPrototypes = sourceTerrainData.detailPrototypes;
            targetTerrainData.treePrototypes = sourceTerrainData.treePrototypes;
        }

        private TerrainLayer GetOrCreateTerrainLayer(string worldName,Vector3 sourceSize, int piecesPerAxis, int sliceIndex, TerrainLayer layerSource)
        {
            var targetSize = new Vector2(sourceSize.x / piecesPerAxis, sourceSize.z / piecesPerAxis);
            var shift = ComputeShift(piecesPerAxis, sliceIndex, targetSize.y, targetSize.x);
            Vector2 layerSize = layerSource.tileSize;

            var newTileOffset = new Vector2((layerSource.tileOffset.x + shift.y * piecesPerAxis + layerSize.x) % layerSize.x, (layerSource.tileOffset.y + shift.x * piecesPerAxis + layerSize.y) % layerSize.y);

            var assetPath = AssetDatabase.GetAssetPath(layerSource);
            var isSourceLayerAnAsset = !string.IsNullOrEmpty(assetPath);

            if (!isSourceLayerAnAsset)
            {
                // if (!AssetDatabase.IsValidFolder($"{GetAssetPathBySliceIndex(worldName,sliceIndex)}/TerrainLayers"))
                // {
                //     AssetDatabase.CreateFolder("Assets", "TerrainLayers");
                // }

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
            layerData.TryAdd(originalLayer, layerSource);

            if (layerData.TryGetValue((layerSource, newTileOffset), out var layer)) return layer;
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
                {
                    EditorUtility.DisplayProgressBar("Split terrain", "Split trees ", (float)t / terrainDataTreeInstances.Length);
                }

                // Get tree instance					
                TreeInstance ti = terrainDataTreeInstances[t];
                Vector3 treePosition = ti.position;

                for (var i = 0; i < tiles.Length; i++)
                {
                    var splitRect = new Rect(i / (float)terraPieces * stepSize, i % terraPieces * stepSize, stepSize, stepSize);

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

            var terrainData = targetTerrain.terrainData;
            var data = sourceTerrain.terrainData;
            terrainData.wavingGrassAmount = data.wavingGrassAmount;
            terrainData.wavingGrassSpeed = data.wavingGrassSpeed;
            terrainData.wavingGrassStrength = data.wavingGrassStrength;
            terrainData.wavingGrassTint = data.wavingGrassTint;
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

        private void SetTerrainSlicePosition(string worldName,Terrain sourceTerrain, int piecesPerAxis, int sliceIndex, Transform terrainTransform,Transform envRoot)
        {
            var parentPosition = sourceTerrain.GetPosition();

            var terrainData = sourceTerrain.terrainData;
            var sizeX = terrainData.size.x;
            var sizeY = terrainData.size.z;

            // var wShift = ComputeShift(piecesPerAxis, sliceIndex, sizeY, sizeX);
            var x = Mathf.FloorToInt(sliceIndex / (float)piecesPerAxis);
            var y = sliceIndex - x * piecesPerAxis;

            var position = terrainTransform.position;
            // position = new Vector3(position.x + wShift.y, position.y, position.z + wShift.x);
            position = new Vector3(position.x + sizeX / piecesPerAxis * x, position.y, position.z + sizeY / piecesPerAxis * y);

            position = new Vector3(position.x + parentPosition.x, position.y + parentPosition.y, position.z + parentPosition.z);
            var chunkName = $"Chunk{DEF.TerrainSplitChar}{sliceIndex}";
            var chunkRoot = envRoot.Find(chunkName);
            if (chunkRoot == null)
            {
                chunkRoot = new GameObject(chunkName).transform;
            }
            chunkRoot.SetParent(envRoot);
            chunkRoot.localScale = Vector3.one;
            chunkRoot.localRotation = Quaternion.identity;
            chunkRoot.localPosition = position;
            // chunkRoot.name = chunkName;

            terrainTransform.name = "Terrain";
            terrainTransform.SetParent(chunkRoot);
            terrainTransform.localPosition = Vector3.zero;
            terrainTransform.localScale = Vector3.one;
            terrainTransform.localRotation = Quaternion.identity;
            terrainList.Add(terrainTransform.gameObject);
            
            
            var savePath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkName}/TerrainInfo.bytes";
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
                var localPosition = chunkRoot.transform.localPosition;
                formatter.Serialize(fs, new TerrainInfo
                {
                    X = localPosition.x,
                    Y = localPosition.y,
                    Z = localPosition.z,
                });
                fs.Close();
            }
            catch (Exception e)
            {
                LogManager.LogError(LOGTag, e.Message);
            }

            AssetDatabase.Refresh();
        }

        private static Vector2 ComputeShift(int piecesPerAxis, int sliceIndex, float sizeY, float sizeX)
        {
            var spaceShiftX = sizeY / piecesPerAxis;
            var spaceShiftY = sizeX / piecesPerAxis;

            var xWShift = sliceIndex % piecesPerAxis * spaceShiftX;
            var zWShift = sliceIndex / (float)piecesPerAxis * spaceShiftY;
            return new Vector2(xWShift, zWShift);
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
            var controlTextureResolution = MathUtils.NextPowerOf2(sourceControlTextureResolution / piecesPerAxis);
            var targetControlTextureResolution = Math.Max(MINIMAL_CONTROL_TEXTURE_RESOLUTION, controlTextureResolution);
            var baseMapResolution = MathUtils.NextPowerOf2(sourceBaseMapResolution / piecesPerAxis);
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
                    {
                        EditorUtility.DisplayProgressBar("Split terrain", "Split splat", (float)x / targetControlTextureResolution);
                    }

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
                InstantiateColliderBox(envRoot, i, new Vector3(0.5f * chunkSize.x, terrainHeight / 2, 0.5f * chunkSize.y), new Vector3(colliderSize.x, terrainHeight, colliderSize.y));
            }

            callback?.Invoke();
        }

        private void InstantiateColliderBox(Transform envRoot, int index, Vector3 position, Vector3 colliderSize)
        {
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
            trs.localPosition = position;
            collider.size = colliderSize;
            collider.isTrigger = true;
            colliderList.Add(go);
        }

        // private void GenColliderBox(Transform envRoot, int index, Vector2 chunkSize, Vector2 colliderSize, float terrainHeight, bool isPosition = false)
        // {
        //     // var nodeName = $"{row}{DEF.TerrainSplitChar}{col}";
        //     var chunkRoot = envRoot.Find($"Chunk{DEF.TerrainSplitChar}{index}");
        //     var oldTrs = chunkRoot.Find("Collider");
        //     if (oldTrs)
        //     {
        //         Object.DestroyImmediate(oldTrs.gameObject);
        //     }
        //
        //     var go = new GameObject();
        //     go.transform.SetParent(chunkRoot);
        //     var trs = go.transform;
        //     trs.name = "Collider";
        //     trs.localRotation = Quaternion.identity;
        //     trs.localScale = Vector3.one;
        //     var collider = go.AddComponent<BoxCollider>();
        //     trs.localPosition = new Vector3(isPosition ? chunkSize.x : (0.5f * chunkSize.x), 0, isPosition ? chunkSize.y : (0.5f * chunkSize.y));
        //     collider.size = new Vector3(colliderSize.x, terrainHeight, colliderSize.y);
        //     collider.isTrigger = true;
        //     colliderList.Add(go);
        // }

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

                    var data = BinaryUtils.Bytes2Object<ColliderInfo>(ResourcesLoadManager.LoadAsset<TextAsset>(savePath).bytes);
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
                    formatter.Serialize(fs, new ColliderInfo
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
            var x = parentWidthMinus1 >= xPos ? xPos : parentWidthMinus1;
            var y = parentWidthMinus1 >= yPos ? yPos : parentWidthMinus1;
            var xi = (int)x;
            var yi = (int)y;
            var b = xi + 1;
            var xiPlus1 = parentWidthMinus1 >= b ? b : parentWidthMinus1;
            var b1 = yi + 1;
            var yiPlus1 = parentWidthMinus1 >= b1 ? b1 : parentWidthMinus1;

            var c00 = data[xi, yi, layerIndex];
            var c10 = data[xiPlus1, yi, layerIndex];
            var c01 = data[xi, yiPlus1, layerIndex];
            var c11 = data[xiPlus1, yiPlus1, layerIndex];

            var tx = x - xi;
            var ty = y - yi;
            var s = c00 + (c10 - c00) * tx;
            var e = c01 + (c11 - c01) * tx;
            return s + (e - s) * ty;
        }
    }
}