using System;
using GameFramework;
using UnityEngine;

namespace GameLogic
{
    public class AnimalCustomerMono : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer m_shapeRenderer;

        private AnimalCustomerInfo m_animalCustomerInfo;

        public void InitData(AnimalCustomerInfo animalCustomerInfo)
        {
            m_animalCustomerInfo = animalCustomerInfo;
        }

        public void ShowShape()
        {
            var gameResourceLoadModule = GameRoot.Instance.GetGameModule<GameResourceLoadModule>();
            m_shapeRenderer.sprite = gameResourceLoadModule?.LoadResource<Sprite>(m_animalCustomerInfo.spriteName);
        }

        public void ShowRequestBubble()
        {
            var gameEventModule = GameRoot.Instance.GetGameModule<GameEventModule>();
            gameEventModule.DispatchEvent(GameEventType.ShowRequestBubble);
        }
    }
}   
