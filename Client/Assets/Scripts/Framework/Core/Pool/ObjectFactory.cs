// author:KIPKIPS
// describe:对象的创建器

namespace Framework.Core.Pool
{
    /// <summary>
    /// 对象工厂
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectFactory<T> : IObjectFactory<T> where T : new()
    {
        /// <summary>
        /// 创建对象接口
        /// </summary>
        /// <returns></returns>
        public T Create()
        {
            return new T();
        }
    }
}