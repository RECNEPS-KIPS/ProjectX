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
        SFX_PoolManager fX_PoolManager = SFX_PoolManager.MainInstance;
        SerializedObject serializedSoundPool = new SerializedObject(fX_PoolManager);
        SerializedProperty serializedPoolProperty = serializedSoundPool.FindProperty("soundPools");
        
        string[] guids = AssetDatabase.FindAssets("t: ComboData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
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
        GameObject go = new GameObject(comboData.name);

        AudioSource audioSource = go.AddComponent<AudioSource>();
 
        ComboSFXtem comboCharacterVoiceItem = go.AddComponent<ComboSFXtem>();
        comboCharacterVoiceItem.GetComboData(comboData);
        comboCharacterVoiceItem.SetSoundStyle(soundStyle);

        string parentPath = "Assets/Prefabs/Audio/ComboAudio";
        if (!System.IO.Directory.Exists(parentPath))
        { 
           System.IO.Directory.CreateDirectory(parentPath);
        }
  
        string targetPath = parentPath+"/"+ comboData.characterName.ToString();
        if (!System.IO.Directory.Exists(targetPath))
        {
            System.IO.Directory.CreateDirectory(targetPath);
        }
        string prefabPath = targetPath+"/"+ go.name + soundStyle.ToString() + ".prefab";
       GameObject prefab= PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction);
       
        SFX_PoolManager.SoundItem soundItem = new SFX_PoolManager.SoundItem();
        soundItem.soundName = comboData.comboName;
        soundItem.soundStyle = soundStyle;
        soundItem.soundPrefab = prefab;
        soundItem.soundCount = 4;
        soundItem.ApplyBigCenter = true;
        bool found =false;
        List<int> duplicateIndices = new List<int>();

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
            serializedSoundPool.Update();
            serializedPoolProperty.arraySize++;
            SerializedProperty newSerializedProperty = serializedPoolProperty.GetArrayElementAtIndex(serializedPoolProperty.arraySize - 1);
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
        //�����޸�
        serializedSoundPool.ApplyModifiedProperties();
        DestroyImmediate(go);
    }
}
#endif
