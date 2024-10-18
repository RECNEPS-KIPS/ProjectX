using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class ClickHandler : MonoBehaviour
    {
        public BaseUI m_uiRoot;

        [HideInInspector]
        public Button btn;
    
        private void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.RemoveListener(OnClick);
            btn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            m_uiRoot.OnViewClick(gameObject);
        }

        public void CheckListener()
        {
            if (btn == null)
            {
                btn = GetComponent<Button>();
            }
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnClick);
                btn.onClick.AddListener(OnClick);
            }
        }
    }
}