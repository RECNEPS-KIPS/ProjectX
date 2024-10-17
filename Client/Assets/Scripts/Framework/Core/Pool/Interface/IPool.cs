// author:KIPKIPS
// describe:对象池接口

namespace Framework.Core.Pool
{
    /// <summary>
    /// 实现对象池接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPool<T>
    {
        /// <summary>
        /// 分配函数
        /// </summary>
        /// <returns></returns>
        T Allocate();

        /// <summary>
        /// 回收函数
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Recycle(T obj);
    }
}