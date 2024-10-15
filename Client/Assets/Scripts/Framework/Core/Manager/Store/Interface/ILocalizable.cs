// author:KIPKIPS
// describe:可存储接口,被存储的对象需要实现该接口
namespace Framework.Core.Manager.Store {
    public interface ILocalizable {
        string LocalizeKey { get; set; }
    }
}