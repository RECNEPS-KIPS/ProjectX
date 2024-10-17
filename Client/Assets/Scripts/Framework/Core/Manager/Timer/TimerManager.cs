// author:KIPKIPS
// describe:定时器管理类

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Core.Singleton;
using Framework.Core.Pool;

namespace Framework.Core.Manager.Timer
{
    /// <summary>
    /// 定时器管理器
    /// </summary>
    [MonoSingletonPath("[Manager]/TimerManager")]
    public class TimerManager : MonoSingleton<TimerManager>
    {
        private const string LOGTag = "TimerManager";
        private readonly Dictionary<int, TimerEntity> _timerEventDict = new Dictionary<int, TimerEntity>();
        private readonly Stack<int> _removeStack = new Stack<int>();
        private readonly BasalPool<TimerEntity> _timerEntityPool = new BasalPool<TimerEntity>();
        private readonly BasalPool<TimerSlice> _timeSlicePool = new BasalPool<TimerSlice>();
        private int _allocateTimerId;

        /// <summary>
        /// 定时器管理器初始化
        /// </summary>
        public override void Initialize()
        {
            StartCoroutine(TriggerTimer());
        }

        internal TimerSlice GetTimeSlice()
        {
            return _timeSlicePool.Allocate();
        }

        internal void RecycleTimeSlice(TimerSlice ts)
        {
            _timeSlicePool.Recycle(ts);
        }

        /// <summary>
        /// 单次定时器
        /// </summary>
        /// <param name="millisecond"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public int SetTimeout(int millisecond, Action<TimerSlice> callback)
        {
            _allocateTimerId++;
            var t = _timerEntityPool.Allocate();
            t.SetTimer(millisecond, callback, _allocateTimerId);
            _timerEventDict[_allocateTimerId] = t;
            return _allocateTimerId;
        }

        /// <summary>
        /// 循环定时器
        /// </summary>
        /// <param name="millisecond"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public int SetInterval(int millisecond, Action<TimerSlice> callback)
        {
            _allocateTimerId++;
            var tp = _timerEntityPool.Allocate();
            tp.SetTimer(millisecond, callback, _allocateTimerId, true);
            _timerEventDict[_allocateTimerId] = tp;
            return _allocateTimerId;
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        /// <param name="timerId"></param>
        public void ClearTimer(int timerId)
        {
            if (_timerEventDict.TryGetValue(timerId, out var value))
            {
                // timerEventDict[timerId].OnRecycled();
                _timerEntityPool.Recycle(value);
            }
            else
            {
                LogManager.Log(LOGTag, "Timer " + timerId + " is not exist !");
            }
        }

        private IEnumerator TriggerTimer()
        {
            while (true)
            {
                foreach (var tp in _timerEventDict.Where(tp => !tp.Value.DoUpdate()))
                {
                    _removeStack.Push(tp.Key);
                }

                if (_removeStack.Count > 0)
                {
                    var count = _removeStack.Count;
                    int i;
                    for (i = 0; i < count; i++)
                    {
                        var id = _removeStack.Pop();
                        if (_timerEventDict.TryGetValue(id, out var value))
                        {
                            // timerEventDict[id].OnRecycled();
                            // timerEventDict.Remove(id);
                            _timerEntityPool.Recycle(value);
                        }
                    }

                    if (_timerEventDict.Count <= 0)
                    {
                        StopCoroutine(TriggerTimer());
                    }
                }

                yield return null;
            }
        }
    }
}