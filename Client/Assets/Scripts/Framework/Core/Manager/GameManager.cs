using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.Manager
{
    [MonoSingletonPath("[Manager]/CameraManager")]
    public class GameManager : MonoSingleton<GameManager>
    {
        public override void Initialize()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        public void Launch(){}
    }
}