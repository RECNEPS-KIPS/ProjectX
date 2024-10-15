// author:KIPKIPS
// describe:定时器实例
using UnityEngine;
using System;
using Framework.Core.Pool;

namespace Framework.Core.Manager.Timer {
    public struct TimerEntity : IPoolAble {
        private bool loop;
        private Action<TimerSlice> callback;
        private int runTime;
        private int liveTime;
        private int id;
        private TimerSlice timeSlice;
        public void OnRecycled() {
            TimerManager.Instance.RecycleTimeSlice(timeSlice);
        }
        public bool IsRecycled { get; set; }
        public void SetTimer(int _liveTime, Action<TimerSlice> _func, int _id, bool _loop = false) {
            runTime = _liveTime;
            liveTime = _liveTime;
            id = _id;
            callback = _func;
            loop = _loop;
            timeSlice = TimerManager.Instance.GetTimeSlice();
            timeSlice.ID = _id;
            timeSlice.Times = 0;
        }
        public bool DoUpdate() {
            // Debug.Log(callFunc);
            if (IsRecycled) return false;
            runTime -= (int)(Time.deltaTime * 1000);
            if (runTime <= 0) {
                runTime = liveTime; //重置
                timeSlice.Times++;
                callback?.Invoke(timeSlice);
                if (!loop) return false;
            }
            return true;
        }
    }
}