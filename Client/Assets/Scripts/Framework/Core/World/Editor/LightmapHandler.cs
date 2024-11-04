// author:KIPKIPS
// date:2024.10.27 11:48
// describe:光照贴图处理工具

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Framework.Core.World
{
    public class LightmapHandler
    {
        // struct TerrainInfo
        // {
        //     public Vector4 lightmapOffsetScale;
        //     public int lightmapIndex;
        //     public string name;
        // }
        // struct RendererInfo
        // {
        //     public int lightIndex;
        //     public Vector4 lightOffsetScale;
        //     public string name;
        //     public Vector3 pos;
        // }
        private const string logTag = "LightmapHandler";
        private Action finishBaked;
        public LightingSettings lightingSettings { get; set; }

        public void GenLightmapData(Transform itemRoot, Transform terrainRoot, string worldName, int xMax, int yMax,
            Action callback = null)
        {
            //设置Lighting光照贴图参数
            LogManager.Log(logTag, "开始烘焙光照贴图,可能需要较长时间,请等待...");
            Lightmapping.lightingSettings = lightingSettings;
            finishBaked = () =>
            {
                var scenePath = Application.dataPath + "/Scenes/" + SceneManager.GetActiveScene().name;
                AssetDatabase.Refresh();
                LogManager.Log(logTag, "烘焙完成,正在写入光照贴图数据");

                //记录光照贴图数据到各个地图切片的二进制文件
                for (var y = 0; y < yMax; y++)
                {
                    for (var x = 0; x < xMax; x++)
                    {
                        RecordChunkLightmapOffset(terrainRoot, itemRoot, worldName, x, y);
                    }
                }

                LogManager.Log(logTag, "写入完成,迁移光照贴图至打包目录");
                //迁移光照贴图
                // var originDir = scenePath;
                var targetDir = $"{Application.dataPath}/ResourcesAssets/Environment/{worldName}/LightmapTextures/";
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                Directory.CreateDirectory(targetDir);
                var files = Directory.GetFiles(scenePath);
                foreach (var t in files)
                {
                    if (t.EndsWith(".asset") || t.EndsWith(".lighting")) continue;
                    var fi = new FileInfo(t);
                    if (t.EndsWith(".meta")) continue;
                    Debug.Log(t);
                    File.Copy(t, $"{targetDir}{fi.Name}");
                }

                var fs = new FileStream($"{targetDir}world.bytes", FileMode.OpenOrCreate, FileAccess.Write);
                var writer = new BinaryWriter(fs);
                writer.Write(LightmapSettings.lightmaps.Length);
                writer.Flush();
                writer.Close();
                fs.Close();
                // Directory.Move(originDir, targetDir);
                try
                {
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("error", e.Message, "ok");
                    LogManager.LogError(logTag, e.StackTrace);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                }

                AssetDatabase.Refresh();
                Lightmapping.bakeCompleted -= finishBaked;
            };
            Lightmapping.bakeCompleted += finishBaked;
            Lightmapping.Bake();
        }

        private void RecordChunkLightmapOffset(Transform terrainRoot, Transform itemRoot, string worldName, int x, int y)
        {
            var filePath = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/Chunk{DEF.TerrainSplitChar}{x}{DEF.TerrainSplitChar}{y}/data.bytes";
            // EditorUtility.DisplayProgressBar("build lightmap", "正在生成lightmap相关配置", 0.1f);
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fs);
            var oldData = new TerrainDataStruct();
            var colliderSize = new Vector3();
            var itemDataStructList = new List<ItemDataStruct>();
            var success = true;
            try
            {
                //terrain
                var posX = reader.ReadSingle();
                var posY = reader.ReadSingle();
                var posZ = reader.ReadSingle();
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
                var lightmapScaleOffsetX = reader.ReadSingle();
                var lightmapScaleOffsetY = reader.ReadSingle();
                var lightmapScaleOffsetZ = reader.ReadSingle();
                var lightmapScaleOffsetW = reader.ReadSingle();
                oldData.lightmapScaleOffset = new Vector4(lightmapScaleOffsetX, lightmapScaleOffsetY,
                    lightmapScaleOffsetZ, lightmapScaleOffsetW);
                oldData.materialGUID = reader.ReadString();

                //collider box
                colliderSize.x = reader.ReadInt32();
                colliderSize.y = reader.ReadInt32();
                colliderSize.z = reader.ReadInt32();

                //item data
                var cnt = reader.ReadInt32();
                for (var i = 0; i < cnt; i++)
                {
                    var ids = new ItemDataStruct();
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
                LogManager.LogError(logTag, e.Message);
                success = false;
            }

            reader.Close();
            fs.Close();
            if (!success) return;
            {
                File.Delete(filePath);
                fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                var writer = new BinaryWriter(fs);
                var pos = oldData.pos;
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
                var terr = terrainRoot.Find($"{x}{DEF.TerrainSplitChar}{y}").GetComponent<Terrain>();
                writer.Write(terr.lightmapIndex);
                writer.Write(terr.lightmapScaleOffset.x);
                writer.Write(terr.lightmapScaleOffset.y);
                writer.Write(terr.lightmapScaleOffset.z);
                writer.Write(terr.lightmapScaleOffset.w);
                writer.Write(oldData.materialGUID);
                writer.Write((int)colliderSize.x);
                writer.Write((int)colliderSize.y);
                writer.Write((int)colliderSize.z);

                //record items data
                var itemTrs = itemRoot.Find($"{x}{DEF.TerrainSplitChar}{y}");
                if (itemTrs == null)
                {
                    return;
                }

                var cnt = itemTrs.transform.childCount;
                if (cnt > 0)
                {
                    writer.Write(cnt);
                    for (var i = 0; i < cnt; i++)
                    {
                        var trs = itemTrs.transform.GetChild(i);
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

                AssetDatabase.Refresh();
                writer.Flush();
                writer.Close();
                fs.Close();
            }
        }

        //Load
        public void LoadLightmapData(Transform terrainRoot, Transform itemRoot, string worldName, int xMax, int yMax,
            Action callback = null)
        {
            var dataPath = $"{Application.dataPath}/ResourcesAssets/Environment/{worldName}/LightmapTextures/";
            var fs = new FileStream($"{dataPath}world.bytes", FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fs);
            var cnt = reader.ReadInt32();
            reader.Close();
            fs.Close();
            //加载光照贴图
            // var lmColors = new string[cnt];
            // var lmDirs = new string[cnt];
            // var lmShadowMarks = new string[cnt];
            var lightmapDataArray = new LightmapData[cnt];
            var texPath = $"Assets/ResourcesAssets/Environment/{worldName}/LightmapTextures/";
            for (var i = 0; i < cnt; i++)
            {
                var data = new LightmapData
                {
                    lightmapColor =
                        AssetDatabase.LoadAssetAtPath<Texture2D>($"{texPath}{DEF.LIGHTMAP_TEXTURE_LIGHT(i)}.exr"),
                    lightmapDir =
                        AssetDatabase.LoadAssetAtPath<Texture2D>($"{texPath}{DEF.LIGHTMAP_TEXTURE_DIR(i)}.png"),
                    shadowMask =
                        AssetDatabase.LoadAssetAtPath<Texture2D>($"{texPath}{DEF.LIGHTMAP_TEXTURE_SHADOWMASK(i)}.png")
                };
                lightmapDataArray[i] = data;
            }

            LightmapSettings.lightmaps = lightmapDataArray;

            //设置光照数据给组件
            for (var x = 0; x < xMax; x++)
            {
                for (var y = 0; y < yMax; y++)
                {
                    LoadLightmapOffsetInfo(terrainRoot, itemRoot, worldName, x, y);
                }
            }

            callback?.Invoke();
        }

        private void LoadLightmapOffsetInfo(Transform terrainRoot, Transform itemRoot, string worldName, int chunkX, int chunkY)
        {
            var filePath = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/Chunk{DEF.TerrainSplitChar}{chunkX}{DEF.TerrainSplitChar}{chunkY}/data.bytes";
            // EditorUtility.DisplayProgressBar("build lightmap", "正在生成lightmap相关配置", 0.1f);
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fs);
            var posX = reader.ReadSingle();
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
            var lmIndex = reader.ReadInt32();
            var lightmapScaleOffsetX = reader.ReadSingle();
            var lightmapScaleOffsetY = reader.ReadSingle();
            var lightmapScaleOffsetZ = reader.ReadSingle();
            var lightmapScaleOffsetW = reader.ReadSingle();
            var terrain = terrainRoot.Find($"{chunkX}{DEF.TerrainSplitChar}{chunkY}").gameObject.GetComponent<Terrain>();
            // terrain.lightmapIndex = lmIndex;
            // terrain.lightmapScaleOffset = new Vector4(lightmapScaleOffsetX, lightmapScaleOffsetY, lightmapScaleOffsetZ, lightmapScaleOffsetW);
            reader.ReadString();

            //collider box
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            var meshRendererMap = new Dictionary<string, MeshRenderer>();
            foreach (var r in itemRoot.Find($"{chunkX}{DEF.TerrainSplitChar}{chunkY}").gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                Debug.Log(r.name + "scene");
                meshRendererMap.Add(r.name, r);
            }

            //item data
            var cnt = reader.ReadInt32();
            for (var i = 0; i < cnt; i++)
            {
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                var lightingMapIndex = reader.ReadInt32();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var z = reader.ReadSingle();
                var w = reader.ReadSingle();
                reader.ReadString();
                var name = reader.ReadString();
                Debug.Log(name + "bytes");
                if (!meshRendererMap.ContainsKey(name)) continue;
                meshRendererMap[name].lightmapIndex = lightingMapIndex;
                meshRendererMap[name].lightmapScaleOffset = new Vector4(x, y, z, w);
            }
        }
    }
}