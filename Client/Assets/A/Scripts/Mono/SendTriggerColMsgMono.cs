using UnityEngine;

namespace GameFramework
{
    public class SendTriggerColMsgMono : MonoBehaviour
    {
        [SerializeField]
        private ReceiverTriggerColMsgMono m_receiverTriggerColMsgMono;
        public GameObject ReceiverObject => m_receiverTriggerColMsgMono != null ? m_receiverTriggerColMsgMono.gameObject : null;

        public void SendTriggerColMsg()
        {
            if (m_receiverTriggerColMsgMono != null)
            {
                m_receiverTriggerColMsgMono.OnReceiveTriggerColMsg(gameObject);
            }
        }
    }
}
