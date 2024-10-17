// author:KIPKIPS
// describe:mono单例路径

using System;

namespace Framework.Core.Singleton
{
    /// <summary>
    /// MonoSingleton路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)] //这个特性只能标记在Class上
    public class MonoSingletonPath : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pathInHierarchy">Hierarchy路径</param>
        public MonoSingletonPath(string pathInHierarchy)
        {
            PathInHierarchy = pathInHierarchy;
        }

        /// <summary>
        /// Hierarchy路径
        /// </summary>
        public string PathInHierarchy { get; }
    }
}