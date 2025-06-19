namespace ZZZ
{
    public abstract class StateMachine
    {
        public BindableProperty<IState> currentState = new BindableProperty<IState>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(IState newState)
        {
            currentState.Value?.Exit();

            currentState.Value = newState;

            currentState.Value.Enter();
        }

        /// <summary>
        /// </summary>
        public void HandInput()
        {
            currentState.Value?.HandInput();
        }

        /// <summary>
        /// </summary>
        public void Update()
        {
            currentState.Value?.Update();
        }

        /// <summary>
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