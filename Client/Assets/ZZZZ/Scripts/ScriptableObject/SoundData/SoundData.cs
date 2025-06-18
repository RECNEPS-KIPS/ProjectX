
using System.Collections.Generic;
using UnityEngine;
using ZZZ;

[CreateAssetMenu(fileName ="SoundData",menuName ="Create/Asset/SoundData")]
public class SoundData : ScriptableObject
{
    [System.Serializable]
    public class SoundInfo
    {
        public SoundStyle soundStye;
        public CharacterNameList characterName;
        public AudioClip[] clips;
    }
    [SerializeField]public List<SoundInfo> soundInfoList = new List<SoundInfo>();
    public AudioClip GetAudioClip(SoundStyle soundStye,CharacterNameList characterName)
    {
        if (characterName == CharacterNameList.Null)
        {
            for (int i = 0; i < soundInfoList.Count; i++)
            {
                if (soundStye == soundInfoList[i].soundStye)
                {
                    return soundInfoList[i].clips[Random.Range(0, soundInfoList[i].clips.Length)];
                }
            }

        }
        else
        {
            SoundInfo targetSound = soundInfoList.Find(i => i.soundStye == soundStye && i.characterName == characterName);
             return targetSound.clips[Random.Range(0, targetSound.clips.Length)];
        }
        return null;
       
    }
  
}
