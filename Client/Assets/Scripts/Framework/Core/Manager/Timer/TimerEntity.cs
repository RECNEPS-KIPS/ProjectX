// author:KIPKIPS
// describe:定时器实例

using UnityEngine;
using System;
using Framework.Core.Pool;

namespace Framework.Core.Manager.Timer
{
    /// <summary>
    /// 定时器实例
    /// </summary>
    public struct TimerEntity : IPoolAble
    {
        private bool _loop;
        private Action<TimerSlice> callback;
        private int _runTime;
        private int _liveTime;
        private int _id;
        private TimerSlice _timeSlice;

        /// <summary>
        /// 回收函数
        /// </summary>
        public void OnRecycled()
        {
            TimerManager.Instance.RecycleTimeSlice(_timeSlice);
        }

        /// <summary>
        /// 是否被回收
        /// </summary>
        public bool IsRecycled { get; set; }

        /// <summary>
        /// 设置定时器数据
        /// </summary>
        /// <param name="liveTime"></param>
        /// <param name="func"></param>
        /// <param name="id"></param>
        /// <param name="loop"></param>
        public void SetTimer(int liveTime, Action<TimerSlice> func, int id, bool loop = false)
        {
            _runTime = liveTime;
            _liveTime = liveTime;
            _id = id;
            callback = func;
            _loop = loop;
            _timeSlice = TimerManager.Instance.GetTimeSlice();
            _timeSlice.ID = _id;
            _timeSlice.Times = 0;
        }

        /// <summary>
        /// 更新定时器
        /// </summary>
        /// <returns></returns>
        public bool DoUpdate()
        {
            // Debug.Log(callFunc);
            if (IsRecycled) return false;
            _runTime -= (int)(Time.deltaTime * 1000);
            if (_runTime > 0) return true;
            _runTime = _liveTime; //重置
            _timeSlice.Times++;
            callback?.Invoke(_timeSlice);
            return _loop;
        }
    }
}