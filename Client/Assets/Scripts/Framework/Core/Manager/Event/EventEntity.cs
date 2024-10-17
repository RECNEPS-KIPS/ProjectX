// author:KIPKIPS
// describe:消息实体类

using System.Collections.Generic;
using System;
using System.Linq;
using Framework.Core.Pool;

namespace Framework.Core.Manager.Event
{
    /// <summary>
    /// 事件实例
    /// </summary>
    public class EventEntity : IPoolAble
    {
        private List<Action<dynamic>> _dynamicCallbackList;
        private List<Action> _callbackList;
        private List<Action<dynamic>> DynamicCallbackList => _dynamicCallbackList ?? new List<Action<dynamic>>();
        private List<Action> CallbackList => _callbackList ?? new List<Action>();

        /// <summary>
        /// 
        /// </summary>
        public bool CanRemove => CallbackList.Count == 0 && DynamicCallbackList.Count == 0;

        private List<Action<dynamic>> _dynamicRemoveList;
        private List<Action> _removeList;
        private List<Action<dynamic>> DynamicRemoveList => _dynamicRemoveList ?? new List<Action<dynamic>>();
        private List<Action> RemoveList => _removeList ?? new List<Action>();

        /// <summary>
        /// 回收事件实例
        /// </summary>
        public void OnRecycled()
        {
            DynamicCallbackList.Clear();
            CallbackList.Clear();
            DynamicRemoveList.Clear();
            RemoveList.Clear();
        }

        /// <summary>
        /// 是否回收
        /// </summary>
        public bool IsRecycled { get; set; }

        private bool _lockRemove;

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="data"></param>
        public void Execute(dynamic data)
        {
            _lockRemove = true;
            foreach (var dcb in DynamicCallbackList.Where(dcb => !DynamicRemoveList.Contains(dcb)))
            {
                dcb?.Invoke(data);
            }

            foreach (var cb in CallbackList.Where(cb => !RemoveList.Contains(cb)))
            {
                cb?.Invoke();
            }

            //true remove
            foreach (var dcb in DynamicRemoveList)
            {
                DynamicCallbackList.Remove(dcb);
            }

            DynamicRemoveList.Clear();
            foreach (var cb in RemoveList)
            {
                CallbackList.Remove(cb);
            }

            RemoveList.Clear();
            _lockRemove = false;
        }

        /// <summary>
        /// 添加回调
        /// </summary>
        /// <param name="dynamicCallback"></param>
        public void AddCallback(Action<dynamic> dynamicCallback)
        {
            if (!DynamicCallbackList.Contains(dynamicCallback))
            {
                DynamicCallbackList.Add(dynamicCallback);
            }
        }

        /// <summary>
        /// 添加回调
        /// </summary>
        /// <param name="callback"></param>
        public void AddCallback(Action callback)
        {
            if (!CallbackList.Contains(callback))
            {
                CallbackList.Add(callback);
            }
        }

        /// <summary>
        /// 移除回调
        /// </summary>
        /// <param name="dynamicCallback"></param>
        public void RemoveCallback(Action<dynamic> dynamicCallback)
        {
            if (_lockRemove)
            {
                DynamicRemoveList.Add(dynamicCallback);
            }
            else
            {
                if (DynamicCallbackList.Contains(dynamicCallback))
                {
                    DynamicCallbackList.Remove(dynamicCallback);
                }
            }
        }

        /// <summary>
        /// 移除回调
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveCallback(Action callback)
        {
            if (_lockRemove)
            {
                RemoveList.Add(callback);
            }
            else
            {
                if (CallbackList.Contains(callback))
                {
                    CallbackList.Remove(callback);
                }
            }
        }
    }
}