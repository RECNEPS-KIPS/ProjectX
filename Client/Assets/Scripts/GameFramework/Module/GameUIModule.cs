using System.Collections;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

namespace GameFramework
{
    public enum eUIPanelType
    {
        
    }
    
    public class GameUIModule : GameFrameworkModule
    {
        private Stack<BaseUI> m_uiStack;
        private Transform m_uiRoot;
        
        public GameUIModule()
        {
            m_uiStack = new Stack<BaseUI>();

            // Init UI Root
            var uiRootPrefab  = GameResourceLoader.Instance.LoadResource<GameObject>($"UI/UIRoot");
            var uiRootGO = GameObject.Instantiate(uiRootPrefab, null);
            uiRootGO.transform.localPosition = Vector3.zero;
            m_uiRoot = uiRootGO.transform;
        }

        public void ShowUI(eUIPanelType showUIPanelType)
        {
            if (m_uiStack.Count > 0)
            {
                var topPanel = m_uiStack.Peek();
                if (topPanel.UIPanelType == showUIPanelType)
                {
                    return;
                }
                topPanel.OnViewHide();
            }
    
            var showPanel = GetUIPanel(showUIPanelType);
            if (showPanel == null)
            {
                return;
            }
            
            m_uiStack.Push(showPanel);
            showPanel.OnViewShow();
        }
        
        public void HideUI(eUIPanelType hideUIPanelType)
        {
            if (m_uiStack.Count == 0)
                return;
            
            var topPanel = m_uiStack.Peek();
            if (topPanel.UIPanelType != hideUIPanelType)
            {
                return;
            }
            
            var hidePanel =  m_uiStack.Pop();
            hidePanel.OnViewHide();
        }
    
        private BaseUI GetUIPanel(eUIPanelType uiPanelType)
        {
            var uiPrefab = GameResourceLoader.Instance.LoadResource<GameObject>($"UI/{uiPanelType}");
            var uiGO = GameObject.Instantiate(uiPrefab, m_uiRoot);
            uiGO.transform.localPosition = Vector3.zero;
            BaseUI resultUI = uiGO.GetComponent<BaseUI>();
            if (resultUI != null)
            {
                resultUI.RegisterButtons();
            }
#if UNITY_EDITOR
            if(resultUI == null)
                Debug.LogError("GetUIPanel is null");
#endif
            return resultUI;
        }
    }
}