// author:KIPKIPS
// describe:对象工厂接口

namespace Framework.Core.Pool
{
    /// <summary>
    /// 创建工厂接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectFactory<T>
    {
        /// <summary>
        /// 创建单例的函数
        /// </summary>
        /// <returns></returns>
        T Create();
    }
}