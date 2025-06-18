#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using static SFX_PoolManager;

public class CreateSoundPoolItem : Editor
{
    [MenuItem("HuHuTools/Create SoundDataPool")]
    public static void CreateSoundDataPool()
    { 
        //得到池子
        SFX_PoolManager fX_PoolManager =SFX_PoolManager.MainInstance;
        //创建SerializedObject
        SerializedObject serializedSoundPool = new SerializedObject(fX_PoolManager);
        //获取池子的属性
        SerializedProperty serializedPoolProperty = serializedSoundPool.FindProperty("soundPools");

        string[] guids = AssetDatabase.FindAssets("t:SoundData");
        foreach (string guid in guids) 
        {
            //获得路径
           string path =AssetDatabase.GUIDToAssetPath(guid);
            //得到SO文件
            SoundData soundData = AssetDatabase.LoadAssetAtPath<SoundData>(path);
           
            if (soundData.soundInfoList.Count == 0) { return; }
            for (int i = 0; i < soundData.soundInfoList.Count; i++) 
            {
                //创建预制体
                GameObject go = new GameObject(soundData.soundInfoList[i].soundStye.ToString());
                AudioSource audioSource = go.AddComponent<AudioSource>();
                SoundItem soundItem =go.AddComponent<SoundItem>();
                //传递SO引用
                soundItem.GetSoundData(soundData);
                //修改脚本上的SoundStyle
                soundItem.SetSoundStyle(soundData.soundInfoList[i].soundStye);
                soundItem.SetCharacterName(soundData.soundInfoList[i].characterName);
                //生成路径
                string parentPath = "Assets/Prefabs/Audio";
                if(!System.IO.Directory.Exists(parentPath))
                { 
                    System.IO.Directory.CreateDirectory(parentPath);
                }
                string targetPath= parentPath+"/"+ soundData.soundInfoList[i].soundStye.ToString();
                if (!System.IO.Directory.Exists(targetPath))
                {
                    System.IO.Directory.CreateDirectory(targetPath);
                }
                string prefabPath;
                //规定命名，避免不同角色也能在同一个文件夹生成不同的预制体
                if (soundData.soundInfoList[i].characterName != ZZZ.CharacterNameList.Null)
                {
                    prefabPath = targetPath + "/" + soundData.soundInfoList[i].soundStye.ToString() + soundData.soundInfoList[i].characterName.ToString() + ".prefab";
                }
                else
                {
                    prefabPath = targetPath + "/" + soundData.soundInfoList[i].soundStye.ToString() + ".prefab";
                }
                //生成预制体文件
                GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction);

                //注册对象池
                 SFX_PoolManager.SoundItem soundItem1 =new SFX_PoolManager.SoundItem();
                soundItem1.soundStyle = soundData.soundInfoList[i].soundStye;
                soundItem1.soundName = soundData.soundInfoList[i].characterName.ToString();
                soundItem1.soundPrefab = prefab;
                soundItem1.soundCount = 4;
                if (soundData.soundInfoList[i].characterName == ZZZ.CharacterNameList.Null)
                {
                    soundItem1.ApplyBigCenter = false;
                }
                else
                {
                    soundItem1.ApplyBigCenter = true;
                }

                //更新对象池
                serializedSoundPool.Update();

                bool found = false;
                List<int> duplicateIndices = new List<int>();
                //更新原来的项目
                UpdateOldPoolItem(serializedPoolProperty, soundItem1,ref found, duplicateIndices, soundItem1.ApplyBigCenter);
                if (!found)
                {
                    serializedPoolProperty.arraySize++;
                    //添加新的列表
                    SerializedProperty newSoundItem = serializedPoolProperty.GetArrayElementAtIndex(serializedPoolProperty.arraySize - 1);
                    //然后对最后一个值进行修改;
                    newSoundItem.FindPropertyRelative("soundStyle").enumValueIndex = (int)soundItem1.soundStyle;
                    newSoundItem.FindPropertyRelative("soundName").stringValue = soundItem1.soundName;
                    newSoundItem.FindPropertyRelative("soundPrefab").objectReferenceValue = soundItem1.soundPrefab;
                    newSoundItem.FindPropertyRelative("soundCount").intValue = soundItem1.soundCount;
                    newSoundItem.FindPropertyRelative("ApplyBigCenter").boolValue = soundItem1.ApplyBigCenter;
                }
                else
                {
                    for (int k = duplicateIndices.Count - 1; k >= 0; k--)
                    {
                        serializedPoolProperty.DeleteArrayElementAtIndex(duplicateIndices[k]);
                    }
                }


                serializedSoundPool.ApplyModifiedProperties();
                //销毁场景中的对象
                DestroyImmediate(go);


            }

        }
    }

    public static void UpdateOldPoolItem(SerializedProperty serializedPoolProperty, SFX_PoolManager.SoundItem soundItem1,ref bool found,List<int> duplicateIndices, bool ApplyBigCenter)
    {
        for (int j = 0; j < serializedPoolProperty.arraySize; j++)
        {
            SerializedProperty SoundItem = serializedPoolProperty.GetArrayElementAtIndex(j);
            if (SoundItem.FindPropertyRelative("soundStyle").enumValueIndex == (int)soundItem1.soundStyle && (!ApplyBigCenter || SoundItem.FindPropertyRelative("soundName").stringValue == soundItem1.soundName))
            {
                if (!found)
                {
                    SoundItem.FindPropertyRelative("soundName").stringValue = soundItem1.soundName;
                    SoundItem.FindPropertyRelative("soundPrefab").objectReferenceValue = soundItem1.soundPrefab;
                    SoundItem.FindPropertyRelative("soundCount").intValue = soundItem1.soundCount;
                    SoundItem.FindPropertyRelative("ApplyBigCenter").boolValue = soundItem1.ApplyBigCenter;
                    found = true;
                }
                else
                {
                    //记录重复元素的索引值
                    duplicateIndices.Add(j);
                }
            }
        }
    }


}
#endif
