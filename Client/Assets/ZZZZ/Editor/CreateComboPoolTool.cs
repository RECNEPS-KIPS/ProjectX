#if UNITY_EDITOR
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CreateComboPoolTool : MonoBehaviour
{
    [MenuItem("HuHuTools/Create ComboSoundPool")]
    public static void GeneratePrefabs()
    {
        //在游戏场景中获取Pool的引用
        SFX_PoolManager fX_PoolManager = SFX_PoolManager.MainInstance;
        //创建SerializedObject
        SerializedObject serializedSoundPool = new SerializedObject(fX_PoolManager);
        //获取该池子的SerializedProperty
        SerializedProperty serializedPoolProperty = serializedSoundPool.FindProperty("soundPools");


        //得到所有的ComboData
        string[] guids = AssetDatabase.FindAssets("t: ComboData");
        foreach (string guid in guids)
        {
            //得到路径
            string path = AssetDatabase.GUIDToAssetPath(guid);
            //得到SO
            ComboData comboData  = AssetDatabase.LoadAssetAtPath<ComboData>(path);

            if (comboData.AppAudioPrefab)

            {   if (comboData.weaponSound != null)
                {
                    if (comboData.weaponSound.Length > 0)
                    {
                        CreatePrefabAndAddToPool(comboData, serializedSoundPool, serializedPoolProperty, SoundStyle.WeaponSound);

                    }
                }
                if (comboData.characterVoice != null)
                {
                    if (comboData.characterVoice.Length > 0)
                    {
                        CreatePrefabAndAddToPool(comboData, serializedSoundPool, serializedPoolProperty, SoundStyle.ComboVoice);

                    }
                }
               
                
            }
        }
    }
    private static void CreatePrefabAndAddToPool(ComboData comboData, SerializedObject serializedSoundPool, SerializedProperty serializedPoolProperty,SoundStyle soundStyle)
    {
        //创建预制体
        GameObject go = new GameObject(comboData.name);
        //挂载AudioSource
        AudioSource audioSource = go.AddComponent<AudioSource>();
        //挂载Item脚本
        ComboSFXtem comboCharacterVoiceItem = go.AddComponent<ComboSFXtem>();
        comboCharacterVoiceItem.GetComboData(comboData);
        comboCharacterVoiceItem.SetSoundStyle(soundStyle);
        //将预制体保存到项目中

        string parentPath = "Assets/Prefabs/Audio/ComboAudio";
        if (!System.IO.Directory.Exists(parentPath))
        { 
           System.IO.Directory.CreateDirectory(parentPath);
        }
        //创建文件夹
        string targetPath = parentPath+"/"+ comboData.characterName.ToString();
        if (!System.IO.Directory.Exists(targetPath))
        {
            System.IO.Directory.CreateDirectory(targetPath);
        }
        string prefabPath = targetPath+"/"+ go.name + soundStyle.ToString() + ".prefab";
       GameObject prefab= PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction);


        //注册给通用音效对象池的列表里：
        //在Pool类中创建新的SoundItem
        SFX_PoolManager.SoundItem soundItem = new SFX_PoolManager.SoundItem();
        soundItem.soundName = comboData.comboName;
        soundItem.soundStyle = soundStyle;
        soundItem.soundPrefab = prefab;
        soundItem.soundCount = 4;//先默认为4
        soundItem.ApplyBigCenter = true;
        bool found =false;
        List<int> duplicateIndices = new List<int>();
        //更新相同的元素
        for (int i = 0; i < serializedPoolProperty.arraySize; i++)
        {
            SerializedProperty poolItem = serializedPoolProperty.GetArrayElementAtIndex(i);
            if (poolItem.FindPropertyRelative("soundName").stringValue == soundItem.soundName && poolItem.FindPropertyRelative("soundStyle").enumValueIndex == (int)soundItem.soundStyle)
            {
                if (!found)
                {
                    poolItem.FindPropertyRelative("soundPrefab").objectReferenceValue = soundItem.soundPrefab;
                    poolItem.FindPropertyRelative("soundCount").intValue = soundItem.soundCount;
                    poolItem.FindPropertyRelative("ApplyBigCenter").boolValue = soundItem.ApplyBigCenter;
                    found = true;
                }
                else
                {
                    duplicateIndices.Add(i);
                }
            }
            
        }
        if (!found)
        {
            //更新之前SerializedSoundPool的值
            serializedSoundPool.Update();
            //添加新的条目
            serializedPoolProperty.arraySize++;
            //获取SerializedPoolProperty中最后一个元素
            SerializedProperty newSerializedProperty = serializedPoolProperty.GetArrayElementAtIndex(serializedPoolProperty.arraySize - 1);
            //对最后一个元素进行修改
            newSerializedProperty.FindPropertyRelative("soundName").stringValue = soundItem.soundName;
            newSerializedProperty.FindPropertyRelative("soundStyle").enumValueIndex = (int)soundItem.soundStyle;
            newSerializedProperty.FindPropertyRelative("soundPrefab").objectReferenceValue = soundItem.soundPrefab;
            newSerializedProperty.FindPropertyRelative("soundCount").intValue = soundItem.soundCount;
            newSerializedProperty.FindPropertyRelative("ApplyBigCenter").boolValue = soundItem.ApplyBigCenter;
        }
        else
        {
            for (int i = duplicateIndices.Count - 1; i >= 0; i--)
            {
                serializedPoolProperty.DeleteArrayElementAtIndex(duplicateIndices[i]);
            }
        }
        //保存修改
        serializedSoundPool.ApplyModifiedProperties();
        DestroyImmediate(go);
    }
}
#endif
