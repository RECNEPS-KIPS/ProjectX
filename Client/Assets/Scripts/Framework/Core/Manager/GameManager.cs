using Framework.Core.Singleton;

namespace Framework.Core.Manager
{
    [MonoSingletonPath("[Manager]/GameManager")]
    public class GameManager : MonoSingleton<GameManager>
    {
        public override void Initialize()
        {
            // Cursor.lockState = CursorLockMode.Confined;
            // Cursor.visible = false;
        }

        public void Launch()
        {
            
        }
    }
}