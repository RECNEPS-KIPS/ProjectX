namespace GameFramework
{
    public interface ICustomInitAndRelease
    {
        void CustomInit();
        void CustomRelease();
    }
    
    public interface ICustomInitAndRelease<T>
    {
        void CustomInit(T t);
        void CustomRelease(T t);
    }

    public interface IActivatable
    {
        bool IsActive { get; set; }
    }
}