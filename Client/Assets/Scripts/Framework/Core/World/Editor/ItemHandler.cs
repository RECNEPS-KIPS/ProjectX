// author:KIPKIPS
// date:2024.10.27 11:48
// describe:
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.Common;
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
        private const string logTag = "ItemHandler";
        private FolderLevelNode m_modelAssetRoot;
        public FolderLevelNode modelAssetRoot => m_modelAssetRoot ??= SearchModelAssetDir();
        public readonly Dictionary<string, List<ModelInfo>> chunkItemsDict = new ();

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
            string filePath = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/Chunk_{chunkX}_{chunkY}/data.bytes";
            if (!File.Exists(filePath))
            {
                LogManager.Log(logTag, "The item data file does not exist");
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
                oldData.lightmapScaleOffset = new Vector4(lightmapScaleOffsetX, lightmapScaleOffsetY, lightmapScaleOffsetZ, lightmapScaleOffsetW);
                oldData.materialGUID = reader.ReadString();

                //collider box
                colliderSize.x = reader.ReadInt32();
                colliderSize.y = reader.ReadInt32();
                colliderSize.z = reader.ReadInt32();
            } catch (Exception e)
            {
                LogManager.LogError(logTag, e.Message);
                success = false;
            }
            reader.Close();
            fs.Close();
            if (success)
            {
                Transform chunkRoot = itemRoot.Find($"{chunkX}_{chunkY}");
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
                int cnt = chunkRoot.transform.childCount;
                if (cnt > 0)
                {
                    writer.Write(cnt);
                    Vector3 tempVec;
                    for (int i = 0; i < cnt; i++)
                    {
                        Transform trs = chunkRoot.transform.GetChild(i);
                        tempVec = trs.position;
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
                        MeshRenderer render = trs.gameObject.GetComponent<MeshRenderer>();
                        if (render == null)
                        {
                            writer.Write(-1);
                            writer.Write(1.0f);
                            writer.Write(1.0f);
                            writer.Write(0f);
                            writer.Write(0f);
                        } else
                        {
                            writer.Write(render.lightmapIndex);
                            writer.Write(render.lightmapScaleOffset.x);
                            writer.Write(render.lightmapScaleOffset.y);
                            writer.Write(render.lightmapScaleOffset.z);
                            writer.Write(render.lightmapScaleOffset.w);
                        }
                        Object obj = PrefabUtility.GetCorrespondingObjectFromSource(trs.gameObject);
                        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                        writer.Write(guid);
                        string name = trs.name;
                        writer.Write(name);
                    }
                } else
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

        public void LoadAllItemChunks(Transform itemRoot, string worldName, int xMax, int yMax, Vector2 chunkSize, Action callback = null)
        {
            for (int y = 0; y < yMax; y++)
            {
                for (int x = 0; x < xMax; x++)
                {
                    LoadItemChunk(itemRoot, worldName, x, y, chunkSize);
                }
            }
            callback?.Invoke();
        }
        public void LoadItemChunk(Transform itemRoot, string worldName, int chunkX, int chunkY, Vector2 chunkSize, Action callback = null)
        {
            string nodeName = $"{chunkY}_{chunkX}";
            if (!chunkItemsDict.ContainsKey(nodeName))
            {
                chunkItemsDict.Add(nodeName, new List<ModelInfo>());
            }
            Transform chunkRoot = itemRoot.Find(nodeName);
            if (chunkRoot == null)
            {
                chunkRoot = AddItemChunkRoot(itemRoot, chunkX, chunkY, chunkSize);
            }
            string filePath = $"{DEF.RESOURCES_ASSETS_PATH}Environment/{worldName}/Chunk_{chunkY}_{chunkX}/data.bytes";
            if (!File.Exists(filePath))
            {
                LogManager.Log(logTag, "There is no scene object data");
                return;
            }
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            int cnt;
            try
            {
                //ignore read terrain
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
                //ignore read collider
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();

                //item count
                cnt = reader.ReadInt32();
            } catch (Exception e)
            {
                LogManager.Log(logTag, "There is no scene object data");
                return;
            }
            Vector3 pos;
            Quaternion rotate;
            Vector3 scale;
            int lightingMapIndex;
            Vector4 lightingMapOffsetScale;
            string guid;
            string name;
            for (int i = 0; i < cnt; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                pos = new Vector3(x, y, z);
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                rotate = Quaternion.Euler(x, y, z);
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                scale = new Vector3(x, y, z);
                lightingMapIndex = reader.ReadInt32();
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                float w = reader.ReadSingle();
                lightingMapOffsetScale = new Vector4(x, y, z, w);
                guid = reader.ReadString();
                name = reader.ReadString();
                //加载预制体
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                GameObject go = PrefabUtility.InstantiatePrefab(asset) as GameObject;
                go.isStatic = true;
                go.name = name;
                go.transform.position = pos;
                go.transform.localScale = scale;
                go.transform.rotation = rotate;
                go.transform.SetParent(chunkRoot);
                MeshRenderer rend = go.GetComponent<MeshRenderer>();
                if (rend != null && lightingMapIndex != -1)
                {
                    rend.lightmapIndex = lightingMapIndex;
                    rend.lightmapScaleOffset = lightingMapOffsetScale;
                }
                ModelInfo mi = new ModelInfo();
                mi.go = go;
                mi.suffix = AssetDatabase.GUIDToAssetPath(guid).Split(".").Last().ToLower();
                chunkItemsDict[nodeName].Add(mi);
            }
            reader.Close();
            fs.Close();
            callback?.Invoke();
        }
        public Vector2 CheckParentChunk(Vector3 pos, int xMax, int yMax, Vector2 chunkSize)
        {
            Vector2 parent = new Vector2(-1, -1);
            for (int x = 0; x < xMax; x++)
            {
                for (int y = 0; y < yMax; y++)
                {
                    Bounds bound = new Bounds();
                    bound.center = new Vector3((x + 0.5f) * chunkSize.x, 0, (y + 0.5f) * chunkSize.y);
                    bound.size = new Vector3(chunkSize.x, 2048, chunkSize.y);
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
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            string name = asset.name;
            // Debug.Log(name);
            if (CommonUtils.Find<Transform>(itemRoot, asset.name) != null)
            {
                int num = 1;
                string tempName = asset.name + "_1";
                while (CommonUtils.Find<Transform>(itemRoot, tempName) != default)
                {
                    num++;
                    tempName = asset.name + "_" + num;
                }
                name = tempName;
            }
            GameObject go = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            go.name = name;
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            Vector2 newParent = CheckParentChunk(go.transform.position, xMax, yMax, chunkSize);
            if (newParent.x >= 0 && newParent.y >= 0)
            {
                string nodeName = $"{(int)newParent.y}_{(int)newParent.x}";
                if (!chunkItemsDict.ContainsKey(nodeName))
                {
                    chunkItemsDict.Add(nodeName, new List<ModelInfo>());
                }
                go.transform.SetParent(itemRoot.Find(nodeName));
                ModelInfo mi = new ModelInfo();
                mi.go = go;
                mi.suffix = AssetDatabase.GUIDToAssetPath(assetPath).Split(".").Last().ToLower();
                chunkItemsDict[nodeName].Add(mi);
            }
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
            string rootName = $"{y}_{x}";
            Transform itemChunkTrs = itemRoot.Find(rootName);
            if (chunkItemsDict.ContainsKey(rootName))
            {
                chunkItemsDict[rootName].Clear();
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
            string iName = $"{y}_{x}";
            Transform trs = itemRoot.Find(iName);
            if (trs)
            {
                EditorGUIUtility.PingObject(trs.gameObject);
                Selection.activeGameObject = trs.gameObject;
                // SceneView.lastActiveSceneView.FrameSelected();
                SceneView.FrameLastActiveSceneView();
            } else
            {
                LogManager.LogWarning(logTag, $"Unable to focus because the item chunk[{iName}] does not exist");
            }
        }
        private Transform AddItemChunkRoot(Transform itemRoot, int x, int y, Vector2 chunkSize)
        {
            GameObject go = new GameObject($"{y}_{x}");
            Transform trs = go.transform;
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
            FolderLevelNode root = new FolderLevelNode();
            Dictionary<int, Dictionary<string, FolderLevelNode>> dict = new Dictionary<int, Dictionary<string, FolderLevelNode>>();
            dict.Add(-1, new Dictionary<string, FolderLevelNode>());
            dict[-1].Add("Models", root);
            dict[-1]["Models"].name = "Models";
            dict[-1]["Models"].uid = "Models";
            string rootDir = $"{DEF.RESOURCES_ASSETS_PATH}Models/";
            if (!Directory.Exists(rootDir)) return root;
            DirectoryInfo directoryInfo = new DirectoryInfo(rootDir);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (var t in fileInfos)
            {
                if (t.Name.EndsWith(".meta") || !t.Name.StartsWith("model_")) continue;
                var arr = t.FullName.Replace(@"\", "/").Split(rootDir);
                if (arr.Length != 2) continue;
                string partDir = arr[1];
                string[] splitPart = partDir.Split("/");
                string uid = string.Empty;
                for (int j = 0; j < splitPart.Length; j++)
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
                    FolderLevelNode temp = dict[j][uid];
                    string parentPath = uid.Replace("/" + splitPart[j], "");
                    FolderLevelNode parent = j > 0 ? dict[j - 1][parentPath] : dict[-1]["Models"];
                    if (!parent.subdirectories.Contains(temp))
                    {
                        parent.subdirectories.Add(temp);
                    }
                }
                // LogManager.Log(logTag,partDir);
            }
            return root;
        }

        #endregion
    }
}