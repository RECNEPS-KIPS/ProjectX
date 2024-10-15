// author:KIPKIPS
// describe:对象的创建器

namespace Framework.Core.Pool {
    public class ObjectFactory<T> : IObjectFactory<T> where T : new() {
        public T Create() {
            return new T();
        }
    }
}