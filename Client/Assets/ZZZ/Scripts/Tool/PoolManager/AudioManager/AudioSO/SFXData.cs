
using System.Collections.Generic;
using UnityEngine;
namespace Test
{
    public enum SFXStyle
    {
        Null =0,
        //UI音效:1-50
        UISound = 1,
        Click , Cancel,

        //角色音效：51-100
        CharacterSound=51,
        jump1,jump2,
        Dodge,Doge1, Doge2,

        //脚步音效：101-120
        FootSound =101,
        AFoot,BFoot,

        //BGM：121-150
        BGM =121,
        BGM_1,

        //环境音：151-180
        EnvironmentSound=151,
        Environment_1 ,

        //其他但不是在这里手动配置的音效181
        OtherSound=181,
        CharacterVoice,EnemyVoice,


    }//考虑配置的方便性：连招音效、连招、技能角色语音等依赖于连招的于此分开，
     //预加载的策略是读取加载的角色的PlayerSO里面的连招SO来进行预加载

    [CreateAssetMenu(menuName = "Asset/Audio/SFXData")]
    public class SFXData : ScriptableObject
    {
        [System.Serializable]
        public class SFXDataInfo
        {
            public SFXStyle sfxStyle;
            public AudioClip[] audioClips;
            public float spatialBlend;//0完全为2D，1完全为3D
            public float audioVolume;//0默认为默认设置
            public float lifeTime;//0默认播完自动关闭
            public float pitch;//0默认为默认配置，音调

        }//上者全部为音效所以AudioMixer设置的都是Effect（音效层）
        public List<SFXDataInfo> SFXDataInfoList = new List<SFXDataInfo>();

    }
}

