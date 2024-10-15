// author:KIPKIPS
// describe:定时器管理类
using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Core.Singleton;
using Framework.Core.Pool;

namespace Framework.Core.Manager.Timer {
    [MonoSingletonPath("[Manager]/TimerManager")]
    public class TimerManager : MonoSingleton<TimerManager> {
        private string logTag = "TimerManager";
        private Dictionary<int, TimerEntity> _timerEventDict = new Dictionary<int, TimerEntity>();
        private Stack<int> _removeStack = new Stack<int>();
        private BasalPool<TimerEntity> _timerEntityPool = new BasalPool<TimerEntity>();
        private BasalPool<TimerSlice> _timeSlicePool = new BasalPool<TimerSlice>();
        private int _allocateTimerId = 0;
        public override void Initialize() {
            StartCoroutine(TriggerTimer());
        }
        internal TimerSlice GetTimeSlice() {
            return _timeSlicePool.Allocate();
        }
        internal void RecycleTimeSlice(TimerSlice ts) {
            _timeSlicePool.Recycle(ts);
        }
        public int SetTimeout(int millisecond, Action<TimerSlice> callback) {
            _allocateTimerId++;
            TimerEntity tp = _timerEntityPool.Allocate();
            tp.SetTimer(millisecond, callback, _allocateTimerId);
            _timerEventDict[_allocateTimerId] = tp;
            return _allocateTimerId;
        }
        public int SetInterval(int millisecond, Action<TimerSlice> callback) {
            _allocateTimerId++;
            TimerEntity tp = _timerEntityPool.Allocate();
            tp.SetTimer(millisecond, callback, _allocateTimerId, true);
            _timerEventDict[_allocateTimerId] = tp;
            return _allocateTimerId;
        }
        public void ClearTimer(int timerId) {
            if (_timerEventDict.ContainsKey(timerId)) {
                // timerEventDict[timerId].OnRecycled();
                _timerEntityPool.Recycle(_timerEventDict[timerId]);
            } else {
                LogManager.Log(logTag, "Timer " + timerId + " is not exist !");
            }
        }
        IEnumerator TriggerTimer() {
            int i, count;
            while (true) {
                foreach (KeyValuePair<int, TimerEntity> tp in _timerEventDict) {
                    if (!tp.Value.DoUpdate()) {
                        _removeStack.Push(tp.Key);
                    }
                }
                if (_removeStack.Count > 0) {
                    count = _removeStack.Count;
                    for (i = 0; i < count; i++) {
                        int id = _removeStack.Pop();
                        if (_timerEventDict.ContainsKey(id)) {
                            // timerEventDict[id].OnRecycled();
                            // timerEventDict.Remove(id);
                            _timerEntityPool.Recycle(_timerEventDict[id]);
                        }
                    }
                    if (_timerEventDict.Count <= 0) {
                        StopCoroutine(TriggerTimer());
                    }
                }
                yield return null;
            }
        }
    }
}