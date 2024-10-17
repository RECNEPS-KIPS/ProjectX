// author:KIPKIPS
// describe:可替换单例类,可被新创建单例替换

using UnityEngine;
using UnityEngine.Serialization;

namespace Framework.Core.Singleton
{
    /// <summary>
    /// 如果跳转到新的场景里已经有了实例，则删除已有示例，再创建新的实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReplaceableMonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        /// <summary>
        /// 初始化的时间
        /// </summary>
        [FormerlySerializedAs("InitializationTime")]
        public float initializationTime;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<T>();
                if (_instance != null) return _instance;
                var obj = new GameObject
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _instance = obj.AddComponent<T>();
                return _instance;
            }
        }

        /// <summary>
        /// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            initializationTime = Time.time;
            DontDestroyOnLoad(this.gameObject);
            // we check for existing objects of the same type
            var check = FindObjectsOfType<T>();
            foreach (var searched in check)
            {
                if (searched == this) continue;
                // if we find another object of the same type (not this), and if it's older than our current object, we destroy it.
                if (searched.GetComponent<ReplaceableMonoSingleton<T>>().initializationTime < initializationTime)
                {
                    Destroy(searched.gameObject);
                }
            }

            if (_instance == null)
            {
                _instance = this as T;
            }
        }
    }
}