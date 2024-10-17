// author:KIPKIPS
// describe:mono单例属性

using UnityEngine;

namespace Framework.Core.Singleton
{
    /// <summary>
    /// 继承Mono的属性单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class MonoSingletonProperty<T> where T : MonoBehaviour, ISingleton
    {
        private static T _instance;

        /// <summary>
        /// 唯一实例对象
        /// </summary>
        public static T Instance => _instance ? _instance : SingletonCreator.CreateMonoSingleton<T>();

        /// <summary>
        /// 析构函数
        /// </summary>
        public static void Dispose()
        {
            if (SingletonCreator.IsUnitTestMode)
            {
                Object.DestroyImmediate(_instance.gameObject);
            }
            else
            {
                Object.Destroy(_instance.gameObject);
            }

            _instance = null;
        }
    }
}