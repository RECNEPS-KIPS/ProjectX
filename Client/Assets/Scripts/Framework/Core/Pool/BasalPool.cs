// author:KIPKIPS
// describe:普通对象池

namespace Framework.Core.Pool
{
    /// <summary>
    /// 简单对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasalPool<T> : Pool<T> where T : IPoolAble, new()
    {
        /// <summary>
        /// 构造函数,初始化工厂
        /// </summary>
        public BasalPool()
        {
            Factory = new ObjectFactory<T>();
        }

        /// <summary>
        /// 分配对象
        /// </summary>
        /// <returns></returns>
        public override T Allocate()
        {
            var result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Recycle(T obj)
        {
            if (obj == null || obj.IsRecycled)
            {
                return false;
            }

            obj.IsRecycled = true;
            obj.OnRecycled();
            CacheStack.Push(obj);
            return true;
        }
    }
}