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
                GameEventsManager.MainInstance.CallEvent("");
            }
        }

        private void OnEnable()
        {
            GameEventsManager.MainInstance.AddEventListening("", SendText);
        }

        private void OnDisable()
        {
            GameEventsManager.MainInstance.ReMoveEvent("", SendText);
        }

        private void SendText()
        {
            Debug.Log("");
        }
    }
}