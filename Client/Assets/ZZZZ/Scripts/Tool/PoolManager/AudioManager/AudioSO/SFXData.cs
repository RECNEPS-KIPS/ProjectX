using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public enum SFXStyle
    {
        Null = 0,
        UISound = 1,
        Click,
        Cancel,

        //51-100
        CharacterSound = 51,
        jump1,
        jump2,
        Dodge,
        Doge1,
        Doge2,

        //101-120
        FootSound = 101,
        AFoot,
        BFoot,

        //BGM 121-150
        BGM = 121,
        BGM_1,

        //151-180
        EnvironmentSound = 151,
        Environment_1,

        //181
        OtherSound = 181,
        CharacterVoice,
        EnemyVoice,
    }

    [CreateAssetMenu(menuName = "Asset/Audio/SFXData")]
    public class SFXData : ScriptableObject
    {
        [System.Serializable]
        public class SFXDataInfo
        {
            public SFXStyle sfxStyle;
            public AudioClip[] audioClips;
            public float spatialBlend;
            public float audioVolume;
            public float lifeTime;
            public float pitch;
        }

        public List<SFXDataInfo> SFXDataInfoList = new List<SFXDataInfo>();
    }
}