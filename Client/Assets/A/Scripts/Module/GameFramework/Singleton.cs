namespace GameFramework
{
    // 单例模式
    public class Singleton<T> where T : new()
    {
        private static readonly object m_lockObj = new object();
        private static T m_instance;
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_lockObj)
                    {
                        if (m_instance == null)
                        {
                            m_instance = new T();
                        }
                    }
                }
                return m_instance;
            }
        }
    }
}