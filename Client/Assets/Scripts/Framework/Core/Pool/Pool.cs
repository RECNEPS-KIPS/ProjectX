// author:KIPKIPS
// describe:对象池类

using System.Collections.Generic;

namespace Framework.Core.Pool
{
    /// <summary>
    /// 对象池类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Pool<T> : IPool<T>
    {
        /// <summary>
        /// Gets the current count.
        /// </summary>
        protected int CurCount => CacheStack.Count;

        /// <summary>
        /// 定义实现接口的类对象
        /// </summary>
        protected IObjectFactory<T> Factory;

        /// <summary>
        /// 存储池对象的容器
        /// </summary>
        protected Stack<T> CacheStack = new Stack<T>();

        /// <summary>
        /// default is 5
        /// </summary>
        protected int MaxCount = 5;

        /// <summary>
        /// 返回申请的对象
        /// </summary>
        /// <returns>T</returns>
        public virtual T Allocate()
        {
            return CacheStack.Count == 0 ? Factory.Create() : CacheStack.Pop();
        }

        /// <summary>
        /// 回收对象到池里
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool Recycle(T obj);
    }
}