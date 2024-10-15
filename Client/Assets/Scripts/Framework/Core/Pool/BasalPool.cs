// author:KIPKIPS
// describe:普通对象池

namespace Framework.Core.Pool {
    public class BasalPool<T> : Pool<T> where T : IPoolAble, new() {
        public BasalPool() {
            _factory = new ObjectFactory<T>();
        }
        public override T Allocate() {
            T result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }
        public override bool Recycle(T obj) {
            if (obj == null || obj.IsRecycled) {
                return false;
            }
            obj.IsRecycled = true;
            obj.OnRecycled();
            _cacheStack.Push(obj);
            return true;
        }
    }
}