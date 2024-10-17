// author:KIPKIPS
// describe:可存储接口,被存储的对象需要实现该接口

namespace Framework.Core.Manager.Store
{
    /// <summary>
    /// 可存储接口
    /// </summary>
    public interface IStorable
    {
        /// <summary>
        /// 存储的key值
        /// </summary>
        string StoreKey { get; set; }
    }
}