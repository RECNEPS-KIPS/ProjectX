// author:KIPKIPS
// date:2023.04.27 20:00
// describe:拖拽按钮

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framework.Core.Manager.UI
{
    /// <summary>
    /// 支持拖拽的按钮
    /// </summary>
    [AddComponentMenu("LUI/LDragButton", 38), ExecuteAlways]
    public class LDragButton : LButton, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public UnityAction<Vector2> onDragBegin;
        public UnityAction<Vector2> onDrag;
        public UnityAction<Vector2> onDragEnd;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            onDragBegin?.Invoke(eventData.position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(eventData.position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            onDragEnd?.Invoke(eventData.position);
        }
    }
}