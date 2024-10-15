// author:KIPKIPS
// describe:对象工厂接口
namespace Framework.Core.Pool {
    public interface IObjectFactory<T> {
        T Create();
    }
}