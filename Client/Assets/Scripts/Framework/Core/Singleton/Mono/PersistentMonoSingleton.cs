// author:KIPKIPS
// describe:唯一单例,一经创建不再销毁

using UnityEngine;

namespace Framework.Core.Singleton
{
    /// <summary>
    /// 如果跳转到新的场景里已经有了实例，则不创建新的单例(或者创建新的单例后会销毁掉新的单例)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PersistentMonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        /// <summary>
        /// 是否Enabled
        /// </summary>
        protected bool Enabled;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        public static T Instance => _instance = (_instance ??= FindObjectOfType<T>()) ?? new GameObject().AddComponent<T>();

        /// <summary>
        /// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_instance == null)
            {
                //If I am the first instance, make me the Singleton
                _instance = this as T;
                DontDestroyOnLoad(transform.gameObject);
                Enabled = true;
            }
            else
            {
                //If a Singleton already exists and you find
                //another reference in scene, destroy it!
                if (this != _instance)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}