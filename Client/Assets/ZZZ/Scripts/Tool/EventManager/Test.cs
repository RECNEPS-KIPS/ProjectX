using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ybh
{
    public class Test : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (CharacterInputSystem.MainInstance.Jump)
            {
                GameEventsManager.MainInstance.CallEvent("事件");
            }
        }
        private void OnEnable()
        {
            GameEventsManager.MainInstance.AddEventListening("事件", SendText);
        }
        private void OnDisable()
        {
            GameEventsManager.MainInstance.ReMoveEvent("事件", SendText);
        }
        private void SendText()
        {
            Debug.Log("事件成功被调用");
        }

    }
}
