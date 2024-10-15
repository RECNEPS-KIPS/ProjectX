// author:KIPKIPS
// describe:消息管理器
using Framework.Core.Singleton;
using System.Collections.Generic;
using System;
using Framework.Core.Pool;

namespace Framework.Core.Manager.Event {
    [MonoSingletonPath("[Manager]/EventManager")]
    public class EventManager : MonoSingleton<EventManager> {
        private string logTag = "EventManager";
        private BasalPool<EventEntity> _eventEntityPool = new BasalPool<EventEntity>();
        private Dictionary<EventType, EventEntity> _eventDict = new Dictionary<EventType, EventEntity>();
        public void Register(EventType type, Action<dynamic> callback) {
            if (!_eventDict.ContainsKey(type)) {
                EventEntity e = _eventEntityPool.Allocate();
                _eventDict[type] = e;
                e.AddCallback(callback);
            } else {
                _eventDict[type].AddCallback(callback);
            }
        }
        public void Register(EventType type, Action callback) {
            if (!_eventDict.ContainsKey(type)) {
                EventEntity e = _eventEntityPool.Allocate();
                _eventDict[type] = e;
                e.AddCallback(callback);
            } else {
                _eventDict[type].AddCallback(callback);
            }
        }
        public void Remove(EventType type, Action<dynamic> callback) {
            if (_eventDict.ContainsKey(type)) {
                _eventDict[type].RemoveCallback(callback);
                if (_eventDict[type].CanRemove) {
                    _eventEntityPool.Recycle(_eventDict[type]);
                    // EventQueue.Remove(type);
                }
            }
        }
        public void Remove(EventType type, Action callback) {
            if (_eventDict.ContainsKey(type)) {
                _eventDict[type].RemoveCallback(callback);
                if (_eventDict[type].CanRemove) {
                    // EventQueue.Remove(type);
                    _eventEntityPool.Recycle(_eventDict[type]);
                }
            }
        }
        public void Dispatch(EventType type, dynamic data = null) {
            if (_eventDict != null && _eventDict.ContainsKey(type)) {
                _eventDict[type].Execute(data);
            }
        }
    }
}