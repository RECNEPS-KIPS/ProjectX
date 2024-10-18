using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    [System.Serializable]
    public struct SpriteInfo
    {
        public string atlasName;
        public string spriteName;

        public SpriteInfo(string atlasName,string spriteName)
        {
            this.atlasName = atlasName;
            this.spriteName = spriteName;
        }
    }

    public static class UICommon
    {
        public static SpriteInfo CHOOSE_SPRITE_INFO = new SpriteInfo("Icon","CardChoose");
        public static SpriteInfo WITH_CHOOSING_SPRITE_INFO = new SpriteInfo("Icon","CardChoosing");
        public static SpriteInfo NONE_CHOOSE_SPRITE_INFO = new SpriteInfo("Icon","CardBG");
    }
}
