using Framework.Core.Pool;

namespace Framework.Core.Manager.Timer
{
    /// <summary>
    /// 
    /// </summary>
    public struct TimerSlice : IPoolAble
    {
        /// <summary>
        /// 回收函数
        /// </summary>
        public void OnRecycled()
        {
            ID = -1;
            Times = 0;
        }

        /// <summary>
        /// 时间片是否被回收
        /// </summary>
        public bool IsRecycled { get; set; }

        /// <summary>
        /// 定时器id
        /// </summary>
        public int ID { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public int Times { get; internal set; }

        internal TimerSlice(int id, int times)
        {
            ID = id;
            Times = times;
            IsRecycled = false;
        }
    }
}