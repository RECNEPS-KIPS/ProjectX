using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName ="ComboContainerData",menuName ="Create/Asset/CoomboContainerData")]
public class ComboContainerData : ScriptableObject
{
    [SerializeField] public List<ComboData> comboDates=new List<ComboData>();
    [SerializeField, Header("闪A")] public ComboData DodgeATKData;
    [SerializeField, Header("后闪A")] public ComboData BackDodgeATKData;

   private ComboData firstComboData;
    
    public void Init()
    {

        if (comboDates.Count == 0) { return; }
        //缓存第一个连招
        firstComboData = comboDates[0];
        Debug.Log("初始化");
    }

    public string GetComboName(int index)
    {
        if (comboDates.Count == 0) { return null; }
        if (comboDates[index].comboName == null) { Debug.LogWarning(index+"的索引值下没有连招名"); }
        return comboDates[index].comboName;
    }
  
    public void SwitchDodgeATK()
    { 
        if (DodgeATKData==null) { return; }
        comboDates[0]= DodgeATKData;
    }
    public void SwitchBackDodgeATK()
    {
        if (BackDodgeATKData == null) { return; }
        comboDates[0] = BackDodgeATKData;
    }
    public void ResetComboDates()
    {
        if (comboDates == null) { Debug.Log(comboDates + "是空的"); return; }
        if (comboDates[0] != firstComboData)
        {
            comboDates[0] = firstComboData;
            Debug.Log("切换的结果为" + comboDates[0].name);
        }
        
    }
    public float GetComboColdTime(int index)
    {
        if (comboDates.Count == 0) { return 0; }
        if (comboDates[index].comboColdTime == 0) { Debug.LogWarning(index + "的索引值下没有设置连招的冷却时间"); }
        return comboDates[index].comboColdTime;
    }

    public float GetComboDistance(int index)
    {
        if (comboDates.Count == 0) { return 0; }
        if (comboDates[index].attackDistance== 0) { Debug.LogWarning(index + "的索引值下没有设置连招的攻击距离"); }
        return comboDates[index].attackDistance;
    }
    public float GetComboOffset(int index)
    {
        if (comboDates.Count == 0) { return 0; }
        if (comboDates[index].comboOffset == 0) { Debug.LogWarning(index + "的索引值下没有设置连招的攻击距离偏差量"); }
        return comboDates[index].comboOffset;
    }
    //public AudioClip  GetComboSound(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].weaponSound == null) { Debug.LogWarning(index + "的索引值下没有设置音效的"); }
    //    return comboDates[index].weaponSound;
    //}
    //public AudioClip GetCharacterVoice(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].characterVoice == null) { Debug.LogWarning(index + "的索引值下没有设置音效的"); }
    //    return comboDates[index].characterVoice;
    //}
    //public GameObject GetCharacterVoicePrefab(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].characterVoicePrefab == null) { Debug.LogWarning(index + "的索引值下没有设置音效的"); }
    //    return comboDates[index].characterVoicePrefab;
    //}

    //public GameObject GetComboSoundPrefab(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].weaponSoundPrefab == null) { Debug.LogWarning(index + "的索引值下没有设置音效的"); }
    //    return comboDates[index].weaponSoundPrefab;
    //}

    public string GetComboHitName(int index)
    {
        if (comboDates.Count == 0) { return null; }
        if (comboDates[index].hitName == null) { Debug.LogWarning(index + "的索引值下没有受伤名"); }
        return comboDates[index].hitName;
    }
    public string GetComboParryName(int index)
    {
        if (comboDates.Count == 0) { return null; }
        if (comboDates[index].parryName == null) { Debug.LogWarning(index + "的索引值下没有格挡名"); }
        return comboDates[index].parryName;
    }
    public int GetComboMaxCount()
    {
        if (comboDates.Count == 0) { return 0; }
        return comboDates.Count;
    } 
    public float GetComboDamage(int index)
    { 
        if(comboDates.Count == 0) { return 0f; }
        if (comboDates[index].comboDamage == 0) { Debug.LogWarning(index + "的索引值下没有伤害"); }
        return comboDates[index].comboDamage;
    }

    public SoundStyle GetComboSoundStyle(int index)
    {
      
        if (comboDates[index].comboDamage == 0) { Debug.LogWarning(index + "的索引值下没有设置音效Style"); }
        return comboDates[index].universalSound;
    }

    public float GetComboShakeForce(int index, int ATKIndex)
    {
        //  Debug.Log("ATKIndex为" + ATKIndex);
        // Debug.Log("comboDates[index].shakeForce.Length为" + (comboDates[index].shakeForce.Length ));
      
        if (comboDates[index].shakeForce == null||ATKIndex > comboDates[index].shakeForce.Length )
        {
         //说明我不设置Force或者没有设置全Force，代表每该ATK都没有震屏
            return 0;
        }
        return comboDates[index].shakeForce[ATKIndex-1];
    }
   
    public int GetComboATKCount(int index)
    {
        return comboDates[index].ATKCount;
    }
    public float GetPauseFrameTime(int index,int ATKIndex)
    {
        if (comboDates[index].pauseFrameTimeList== null|| ATKIndex > comboDates[index].pauseFrameTimeList.Length)
        {
           return GetComboPauseFrameTime(index);
        }
       
        return comboDates[index].pauseFrameTimeList[ATKIndex-1];

    }
    private float GetComboPauseFrameTime(int index)
    {
        return comboDates[index].pauseFrameTime;
    }
}
