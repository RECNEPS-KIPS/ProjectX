using GameFramework;
using UnityEngine;

namespace GameLogic
{
    public class CatChestMono : MonoBehaviour
    {
        [SerializeField]
        private Transform m_openChestRoot;
        [SerializeField]
        private Transform m_backpackRoot;
        [SerializeField]
        private Transform m_exchangeRoot;
        [SerializeField]
        private Transform m_upgradeRoot;

        private GameConfigModule GameConfigModule => GameRoot.Instance.GetGameModule<GameConfigModule>();
        private GameSaveModule GameSaveModule => GameRoot.Instance.GetGameModule<GameSaveModule>();

        public void OpenChest()
        {
            // 动画演出
            var catChestGameSaveVO = GameSaveModule.LoadSaveData<CatChestGameSaveVO>("CatChestGameSaveVO");
            var curLevel = catChestGameSaveVO.Level;
            
        }

    }
    
}
