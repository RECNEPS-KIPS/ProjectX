// author:KIPKIPS
// describe:对象池数量约束接口

namespace Framework.Core.Pool
{
    /// <summary>
    /// 数量限制对象池接口
    /// </summary>
    public interface ICountObserveAble
    {
        /// <summary>
        /// 当前的数量
        /// </summary>
        int CurCount { get; }
    }
}