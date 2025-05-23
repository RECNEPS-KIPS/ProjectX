using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStyle
{
    Null,

}
public enum CharacterSoundStyle
{
    Null,
    //按动作分
    DodgeF,DodgeB,Jump
}


[CreateAssetMenu(menuName = "Asset/Audio/CharacterVoiceData")]
public class CharacterVoiceData : ScriptableObject
{ 

    [System.Serializable]
    public class CharacterVoiceInfo
    {
        public CharacterStyle characterStyle;
        public AudioClip[] audioClips;
        public float audioVolume;//0默认为默认设置
        public float lifeTime;//0默认播完自动关闭
    }
    public List<CharacterVoiceInfo> promptToneInfoList = new List<CharacterVoiceInfo>();
   
}
