// author:KIPKIPS
// describe:单例对象池

using System;
using Framework.Core.Singleton;

namespace Framework.Core.Pool
{
    /// <summary>
    /// 池对象容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendPool<T> : Pool<T>, ISingleton where T : IPoolAble, new()
    {
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected ExtendPool()
        {
            Factory = new ObjectFactory<T>();
        }

        /// <summary>
        /// 池容器单例
        /// </summary>
        public static ExtendPool<T> Instance => SingletonProperty<ExtendPool<T>>.Instance;

        /// <summary>
        /// 释放对象池
        /// </summary>
        public void Dispose()
        {
            SingletonProperty<ExtendPool<T>>.Dispose();
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="maxCount"></param>
        /// <param name="initCount"></param>
        public void Init(int maxCount, int initCount)
        {
            if (maxCount > 0)
            {
                initCount = Math.Min(maxCount, initCount);
                MaxCount = maxCount;
            }

            if (CurCount >= initCount) return;
            for (var i = CurCount; i < initCount; ++i)
            {
                Recycle(Factory.Create());
            }
        }

        /// <summary>
        /// 最大存储的池对象数
        /// </summary>
        public int MaxCacheCount
        {
            get => MaxCount;
            set
            {
                MaxCount = value;
                if (CacheStack == null || MaxCount <= 0 || MaxCount >= CacheStack.Count) return;
                var removeCount = MaxCount - CacheStack.Count;
                while (removeCount > 0)
                {
                    CacheStack.Pop();
                    --removeCount;
                }
            }
        }

        /// <summary>
        /// 分配实例
        /// </summary>
        /// <returns></returns>
        public override T Allocate()
        {
            T result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }

        /// <summary>
        /// 回收实例
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Recycle(T obj)
        {
            if (obj == null || obj.IsRecycled)
            {
                return false;
            }

            if (MaxCount > 0 && CacheStack.Count >= MaxCount)
            {
                obj.OnRecycled();
                return false;
            }

            obj.IsRecycled = true;
            obj.OnRecycled();
            CacheStack.Push(obj);
            return true;
        }
    }
}