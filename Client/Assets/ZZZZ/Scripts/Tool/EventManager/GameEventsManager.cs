
using System.Collections.Generic;
using GGG.Tool.Singleton;
using System;
using GGG.Tool;

public class GameEventsManager : SingletonNonMono<GameEventsManager>
{
    private interface IEventface
    {

    }

    private class EventHander : IEventface
    {
        //声明事件
        private event Action _action;
        //提供核心方法
        public EventHander(Action action)
        {
            _action = action;
        }

        public void AddCallBack(Action action)
        {
            _action += action;
        }

        public void RemoveCallBack(Action action)
        {
            _action -= action;
        }
        public void CallBack()
        {
            _action?.Invoke();
        }

    }
    //构造函数泛型字段初始化类必须要为泛型
    private class EventHander<T> : IEventface
    {
        //声明事件
        private event Action<T> _action;
        //提供核心方法
        public EventHander(Action<T> action)
        {
            _action = action;
        }

        public void AddCallBack(Action<T> action)
        {
            _action += action;
        }

        public void RemoveCallBack(Action<T> action)
        {
            _action -= action;
        }
        public void CallBack(T value)
        {
            _action?.Invoke(value);
        }

    }
    private class EventHander<T1,T2> : IEventface
    {
        //声明事件
        private event Action<T1, T2> _action;
        //提供核心方法
        public EventHander(Action<T1,T2> action)
        {
            _action = action;
        }

        public void AddCallBack(Action<T1, T2> action)
        {
            _action += action;
        }

        public void RemoveCallBack(Action<T1, T2> action)
        {
            _action -= action;
        }
        public void CallBack(T1 t1 ,T2 t2)
        {
            _action?.Invoke(t1,t2);
        }

    }
    private class EventHander<T1, T2, T3, T4, T5,T6> : IEventface
    {
        //声明事件
        private event Action<T1, T2, T3, T4, T5,T6> _action;
        //提供核心方法
        public EventHander(Action<T1, T2, T3, T4, T5, T6> action)
        {
            _action = action;
        }

        public void AddCallBack(Action<T1, T2, T3, T4, T5, T6> action)
        {
            _action += action;
        }

        public void RemoveCallBack(Action<T1, T2, T3, T4, T5, T6> action)
        {
            _action -= action;
        }
        public void CallBack(T1 t1, T2 t2, T3 t3,T4 t4, T5 t5,T6 t6)
        {
            _action?.Invoke(t1, t2, t3, t4, t5,t6);
        }

    }

    //写一个字典来储存各个定义的类及事件（多态）
    private Dictionary<string, IEventface> EventCenters = new Dictionary<string, IEventface>();

    //注册事件：EventHander(action)写入字典或者添加多个回调
    public void AddEventListening(string name, Action action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander)?.AddCallBack(action);
        }
        else
        {
            EventCenters.Add(name, new EventHander(action));
        }

    }
    public void AddEventListening<T>(string name, Action<T> action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T>)?.AddCallBack(action);
        }
        else
        {
            EventCenters.Add(name, new EventHander<T>(action));
        }

    }
    public void AddEventListening<T1, T2>(string name, Action<T1, T2> action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T1, T2>)?.AddCallBack(action);
        }
        else
        {
            EventCenters.Add(name, new EventHander<T1, T2>(action));
        }

    }
    public void AddEventListening<T1, T2, T3, T4, T5,T6>(string name, Action<T1, T2, T3, T4, T5, T6> action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T1, T2, T3, T4, T5, T6>)?.AddCallBack(action);
        }
        else
        {
            EventCenters.Add(name, new EventHander<T1, T2, T3, T4, T5, T6>(action));
        }

    }
    public void CallEvent(string name)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander)?.CallBack();
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }
    }

    public void CallEvent<T>(string name, T value)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T>)?.CallBack(value);
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }
    }
    public void CallEvent<T1,T2>(string name, T1 t1,T2 t2)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T1, T2>)?.CallBack(t1,t2);
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }
    }
    public void CallEvent<T1, T2, T3, T4, T5, T6>(string name, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5,T6 t6)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T1,T2,T3,T4,T5,T6>)?.CallBack(t1,t2,t3,t4,t5, t6);
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }
    }
    public void ReMoveEvent(string name,Action action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander)?.RemoveCallBack(action);
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }
        
    }
    public void ReMoveEvent<T1>(string name, Action<T1> action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T1>)?.RemoveCallBack(action);
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }

    }
    public void ReMoveEvent<T1,T2>(string name, Action<T1,T2> action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T1, T2>)?.RemoveCallBack(action);
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }

    }
    public void ReMoveEvent<T1, T2, T3, T4, T5, T6>(string name, Action<T1, T2, T3, T4, T5, T6> action)
    {
        if (EventCenters.TryGetValue(name, out var e))
        {
            (e as EventHander<T1, T2, T3, T4, T5, T6>)?.RemoveCallBack(action);
        }
        else
        {
            DevelopmentToos.WTF("当前事件名在字典内不存在");
        }

    }
}
