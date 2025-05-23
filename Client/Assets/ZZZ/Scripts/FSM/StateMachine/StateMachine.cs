
namespace ZZZ
{
    public abstract class StateMachine
    {

        //这是一个继承了BindableProperty类型的IState字段，获取Istate要通过.Value
        public BindableProperty<IState> currentState = new BindableProperty<IState>();

        /// <summary>
        /// 切换状态的接口API
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(IState newState)
        { 
           
            //可能为空用？逻辑符
            currentState.Value?.Exit();

            currentState.Value = newState;

            currentState.Value.Enter();
        }

        /// <summary>
        /// 处理输入的接口API
        /// </summary>
        public void HandInput()
        {
            //只允许一个状态在这里更新
            currentState.Value?.HandInput();
        }
        /// <summary>
        /// 更新非物理逻辑的接口API
        /// </summary>
        public void Update()
        {
            currentState.Value?.Update();
        }
        /// <summary>
        /// 执行动画事件的接口API
        /// </summary>
        public void OnAnimationTranslateEvent(IState translateState)
        {
            currentState.Value?.OnAnimationTranslateEvent(translateState);
        }
        public void OnAnimationExitEvent() 
        {
            currentState.Value?.OnAnimationExitEvent();
        }
    }
}
