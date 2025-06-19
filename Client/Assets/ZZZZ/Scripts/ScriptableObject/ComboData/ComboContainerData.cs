using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboContainerData", menuName = "Create/Asset/CoomboContainerData")]
public class ComboContainerData : ScriptableObject
{
    [SerializeField] public List<ComboData> comboDates = new List<ComboData>();

    [SerializeField, Header("DodgeATKData")]
    public ComboData DodgeATKData;

    [SerializeField, Header("BackDodgeATKData")]
    public ComboData BackDodgeATKData;

    private ComboData firstComboData;

    public void Init()
    {
        if (comboDates.Count == 0)
        {
            return;
        }

        firstComboData = comboDates[0];
    }

    public string GetComboName(int index)
    {
        if (comboDates.Count == 0)
        {
            return null;
        }

        if (comboDates[index].comboName == null)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].comboName;
    }

    public void SwitchDodgeATK()
    {
        if (DodgeATKData == null)
        {
            return;
        }

        comboDates[0] = DodgeATKData;
    }

    public void SwitchBackDodgeATK()
    {
        if (BackDodgeATKData == null)
        {
            return;
        }

        comboDates[0] = BackDodgeATKData;
    }

    public void ResetComboDates()
    {
        if (comboDates == null)
        {
            Debug.Log(comboDates + "");
            return;
        }

        if (comboDates[0] != firstComboData)
        {
            comboDates[0] = firstComboData;
            Debug.Log("" + comboDates[0].name);
        }
    }

    public float GetComboColdTime(int index)
    {
        if (comboDates.Count == 0)
        {
            return 0;
        }

        if (comboDates[index].comboColdTime == 0)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].comboColdTime;
    }

    public float GetComboDistance(int index)
    {
        if (comboDates.Count == 0)
        {
            return 0;
        }

        if (comboDates[index].attackDistance == 0)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].attackDistance;
    }

    public float GetComboOffset(int index)
    {
        if (comboDates.Count == 0)
        {
            return 0;
        }

        if (comboDates[index].comboOffset == 0)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].comboOffset;
    }
    //public AudioClip  GetComboSound(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].weaponSound == null) { Debug.LogWarning(index + ""); }
    //    return comboDates[index].weaponSound;
    //}
    //public AudioClip GetCharacterVoice(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].characterVoice == null) { Debug.LogWarning(index + ""); }
    //    return comboDates[index].characterVoice;
    //}
    //public GameObject GetCharacterVoicePrefab(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].characterVoicePrefab == null) { Debug.LogWarning(index + ""); }
    //    return comboDates[index].characterVoicePrefab;
    //}

    //public GameObject GetComboSoundPrefab(int index)
    //{
    //    if (comboDates.Count == 0) { return null; }
    //    if (comboDates[index].weaponSoundPrefab == null) { Debug.LogWarning(index + ""); }
    //    return comboDates[index].weaponSoundPrefab;
    //}

    public string GetComboHitName(int index)
    {
        if (comboDates.Count == 0)
        {
            return null;
        }

        if (comboDates[index].hitName == null)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].hitName;
    }

    public string GetComboParryName(int index)
    {
        if (comboDates.Count == 0)
        {
            return null;
        }

        if (comboDates[index].parryName == null)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].parryName;
    }

    public int GetComboMaxCount()
    {
        if (comboDates.Count == 0)
        {
            return 0;
        }

        return comboDates.Count;
    }

    public float GetComboDamage(int index)
    {
        if (comboDates.Count == 0)
        {
            return 0f;
        }

        if (comboDates[index].comboDamage == 0)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].comboDamage;
    }

    public SoundStyle GetComboSoundStyle(int index)
    {
        if (comboDates[index].comboDamage == 0)
        {
            Debug.LogWarning(index + "");
        }

        return comboDates[index].universalSound;
    }

    public float GetComboShakeForce(int index, int ATKIndex)
    {
        //  Debug.Log("ATKIndexΪ" + ATKIndex);
        // Debug.Log("comboDates[index].shakeForce.LengthΪ" + (comboDates[index].shakeForce.Length ));

        if (comboDates[index].shakeForce == null || ATKIndex > comboDates[index].shakeForce.Length)
        {
            return 0;
        }

        return comboDates[index].shakeForce[ATKIndex - 1];
    }

    public int GetComboATKCount(int index)
    {
        return comboDates[index].ATKCount;
    }

    public float GetPauseFrameTime(int index, int ATKIndex)
    {
        if (comboDates[index].pauseFrameTimeList == null || ATKIndex > comboDates[index].pauseFrameTimeList.Length)
        {
            return GetComboPauseFrameTime(index);
        }

        return comboDates[index].pauseFrameTimeList[ATKIndex - 1];
    }

    private float GetComboPauseFrameTime(int index)
    {
        return comboDates[index].pauseFrameTime;
    }
}