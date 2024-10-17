// author:KIPKIPS
// describe:对象池管理对象约束接口

namespace Framework.Core.Pool
{
    /// <summary>
    /// 可被池管理的对象
    /// </summary>
    public interface IPoolAble
    {
        /// <summary>
        /// 回收操作
        /// </summary>
        void OnRecycled();

        /// <summary>
        /// 是否被回收
        /// </summary>
        bool IsRecycled { get; set; }
    }
}