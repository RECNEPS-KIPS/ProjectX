using Framework.Core.Manager.Input;
using GamePlay.Player;
using GamePlay.Scene;
using UnityEngine;

namespace GamePlay.Character
{
    public class InteractionController:MonoBehaviour
    {
        private Collider collider;
        private const string LOGTag = "InteractionController";
        void Awake()
        {
            collider = GetComponent<Collider>();
        }
        
        void Update()
        {
            if (collider != null && InputManager.Instance.IsPickKeySinglePressed())
            {
                LogManager.Log(LOGTag,"IsPickKeyPressed");
                var list = SceneManager.Instance.CheckBounds(collider.bounds);
                foreach (var item in list)
                {
                    LogManager.Log(LOGTag,$"GameObject in same octree leaf node:{item.SelfTrs.name}");
                }
            }
        }
    }
}