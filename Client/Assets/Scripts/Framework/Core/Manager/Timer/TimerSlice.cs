using Framework.Core.Pool;

namespace Framework.Core.Manager.Timer {
    public struct TimerSlice : IPoolAble {
        public void OnRecycled() {
            ID = -1;
            Times = 0;
        }
        public bool IsRecycled { get; set; }
        private int id;
        private int times;
        public int ID {
            get => id;
            internal set => id = value;
        }
        public int Times {
            get => times;
            internal set => times = value;
        }
        internal TimerSlice(int _id, int _times) {
            id = _id;
            times = _times;
            IsRecycled = false;
        }
    }
}