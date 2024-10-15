// author:KIPKIPS
// describe:消息实体类
using System.Collections.Generic;
using System;
using Framework.Core.Pool;

namespace Framework.Core.Manager.Event {
    public class EventEntity : IPoolAble {
        private List<Action<dynamic>> _dynamicCallbackList;
        private List<Action> _callbackList;
        private List<Action<dynamic>> DynamicCallbackList {
            get => _dynamicCallbackList = _dynamicCallbackList ?? new List<Action<dynamic>>();
        }
        private List<Action> CallbackList {
            get => _callbackList = _callbackList ?? new List<Action>();
        }
        public bool CanRemove {
            get => CallbackList.Count == 0 && DynamicCallbackList.Count == 0;
        }
        private List<Action<dynamic>> _dynamicRemoveList;
        private List<Action> _removeList;
        private List<Action<dynamic>> DynamicRemoveList {
            get => _dynamicRemoveList = _dynamicRemoveList ?? new List<Action<dynamic>>();
        }
        private List<Action> RemoveList {
            get => _removeList = _removeList ?? new List<Action>();
        }
        public void OnRecycled() {
            DynamicCallbackList.Clear();
            CallbackList.Clear();
            DynamicRemoveList.Clear();
            RemoveList.Clear();
        }
        public bool IsRecycled { get; set; }
        private bool lockRemove;
        public void Execute(dynamic data) {
            lockRemove = true;
            foreach (var dcb in DynamicCallbackList) {
                if (!DynamicRemoveList.Contains(dcb)) {
                    dcb?.Invoke(data);
                }
            }
            foreach (var cb in CallbackList) {
                if (!RemoveList.Contains(cb)) {
                    cb?.Invoke();
                }
            }
            //true remove
            foreach (var dcb in DynamicRemoveList) {
                DynamicCallbackList.Remove(dcb);
            }
            DynamicRemoveList.Clear();
            foreach (var cb in RemoveList) {
                CallbackList.Remove(cb);
            }
            RemoveList.Clear();
            lockRemove = false;
        }
        public void AddCallback(Action<dynamic> dynamicCallback) {
            if (!DynamicCallbackList.Contains(dynamicCallback)) {
                DynamicCallbackList.Add(dynamicCallback);
            }
        }
        public void AddCallback(Action callback) {
            if (!CallbackList.Contains(callback)) {
                CallbackList.Add(callback);
            }
        }
        public void RemoveCallback(Action<dynamic> dynamicCallback) {
            if (lockRemove) {
                DynamicRemoveList.Add(dynamicCallback);
            } else {
                if (DynamicCallbackList.Contains(dynamicCallback)) {
                    DynamicCallbackList.Remove(dynamicCallback);
                }
            }
        }
        public void RemoveCallback(Action callback) {
            if (lockRemove) {
                RemoveList.Add(callback);
            } else {
                if (CallbackList.Contains(callback)) {
                    CallbackList.Remove(callback);
                }
            }
        }
    }
}