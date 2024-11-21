// author:KIPKIPS
// describe:消息类型枚举

namespace Framework.Core.Manager.Event
{
    /// <summary>
    /// 事件枚举ID
    /// </summary>
    public enum EEvent
    {
        PLAYER_LOAD_FINISHED,//玩家
        PLAYER_ATTR_UPDATE,//玩家属性更新
        
        SCENE_LOAD_FINISHED,//场景加载完成
        SCENE_UNLOAD_FINISHED,//场景卸载完成

        #region 输入响应相关
        
        INPUT_BACKPACK,
        PLAYER_PICK_ITEM,
        
        #endregion
        
        #region 角色动画状态
        ENTER_IDLE,
        ENTER_WALK_RUN,
        ENTER_JUMP,
        #endregion
    }
}