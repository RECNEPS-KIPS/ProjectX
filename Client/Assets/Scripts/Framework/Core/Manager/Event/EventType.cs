// author:KIPKIPS
// describe:消息类型枚举
namespace Framework.Core.Manager {
    public enum EventType {
        #region PlayerCharacter

        PLAYERCHARACTER_JUMP,
        PLAYERCHARACTER_RUN,

        #endregion

        #region PlayerInput

        LEFTSTICK,
        RIGHTSTICK,
        PADLEFTBUTTON,
        PADRIGHTBUTTON,
        PADUPBUTTON,
        PADDOWNBUTTON,
        SLBUTTON,
        SRBUTTON,
        TLBUTTON,
        TRBUTTON,
        LEFTSTICKBUTTON,
        RIGHTSTICKBUTTON,
        EXITBUTTON,
        STARTBUTTON,
        SCREENSHOTBUTTON,
        ARROWLEFTBUTTON,
        ARROWRIGHTBUTTON,
        ARROWUPBUTTON,
        ARROWDOWNBUTTON,

        #endregion
    }
}