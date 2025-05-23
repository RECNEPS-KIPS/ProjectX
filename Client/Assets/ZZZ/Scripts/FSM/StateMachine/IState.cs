
namespace ZZZ
{
   
    public interface IState 
    {
        
        public void Enter();

        public void Exit();

        public void HandInput();

        public void Update();

        public void OnAnimationTranslateEvent(IState state);

        public void OnAnimationExitEvent();
    }
}
