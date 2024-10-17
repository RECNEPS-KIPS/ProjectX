// author:KIPKIPS
// describe:普通单例属性

namespace Framework.Core.Singleton
{
    /// <summary>
    /// 属性单例类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class SingletonProperty<T> where T : class, ISingleton
    {
        // 静态实例
        private static T _instance;

        // 标签锁
        private static readonly object _lock = new();

        /// <summary>
        /// 静态属性
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= SingletonCreator.CreateSingleton<T>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public static void Dispose()
        {
            _instance = null;
        }
    }
}