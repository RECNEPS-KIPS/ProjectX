using UnityEngine;

namespace GameFramework
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_instance;
        private static readonly object m_lock = new object();

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_lock)
                    {
                        m_instance = FindObjectOfType<T>();
                        
                        // 如果场景中没有找到实例，则创建一个新的
                        if (m_instance == null)
                        {
                            GameObject go = new GameObject(typeof(T).Name);
                            m_instance = go.AddComponent<T>();
                            DontDestroyOnLoad(go); // 可选：使单例在场景切换时不被销毁
                        }
                    }
                }
                return m_instance;
            }
        }

        protected virtual void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this as T;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }
    }
}