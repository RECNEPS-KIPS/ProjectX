using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public abstract class BaseUI : MonoBehaviour
    {
        public abstract eUIPanelType UIPanelType { get; }
        public virtual void OnViewShow()
        {
            gameObject.SetActive(true);
        }
    
        public virtual void OnViewHide()
        {
            gameObject.SetActive(false);
        }
    
        public virtual void OnViewClick(GameObject clickObject)
        {
            
        }

        protected virtual void OnViewUpdate()
        {
            
        }

        private void Update()
        {
            OnViewUpdate();
        }

        public virtual void RegisterButtons()
        {
            // Button
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                var ch = b.GetComponent<ClickHandler>();
                if (ch == null) ch = b.gameObject.AddComponent<ClickHandler>();
                ch.m_uiRoot = this;
            }
        }
        
        public void RegisterButtons(Transform item)
        {
            Button[] buttons = item.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                var ch = b.GetComponent<ClickHandler>();
                if (ch == null)
                {
                    ch = b.gameObject.AddComponent<ClickHandler>();
                }
                else
                {
                    ch.CheckListener();
                }
                ch.m_uiRoot = this;
            }
        }
    
        protected void ShowUI(eUIPanelType showUIPanel)
        {
            GameRoot.Instance.GetModule<GameUIModule>().ShowUI(showUIPanel);
        }
        
        protected void HideUI(eUIPanelType hideUIPanel)
        {
            GameRoot.Instance.GetModule<GameUIModule>().HideUI(hideUIPanel);
        }
    }

}
