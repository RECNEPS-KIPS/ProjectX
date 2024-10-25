using System;

namespace GamePlay.Player
{
    public enum PlayerMainStatus
    {
        NONE,//无状态
        COLD,//冷
        HOT,//热
    }
    public enum PlyaerSubStatus
    {
        NONE,
        INSANE,//精神错乱
    }
    [Serializable]
    public class PlayerStatus
    {
        public PlayerMainStatus mainStatus = PlayerMainStatus.NONE;
        public PlyaerSubStatus subStatus = PlyaerSubStatus.NONE;

        public void InitStatusValues()
        {
            
        }
    }
}