using HuHu;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ZZZZUIManager : Singleton<ZZZZUIManager>
{
    [SerializeField] public StateBarUI stateBarUI;
    [SerializeField] public SwitchTimeUI switchTimeUI;
     GameObject uIRoot;
    private Dictionary<Type, IUI> uiDictionary = new Dictionary<Type, IUI>();
    public void RegisterUI<T>(IUI uI) where T : IUI
    { 
        Type type = typeof(T);
        if (!uiDictionary.ContainsKey(type))
        {
            uiDictionary.Add(type, uI);
        }
        else
        {
            uiDictionary[type] = uI;
        }
    }
    public T Get<T>() where T : class,IUI
    { 
        Type t = typeof(T);
        if (uiDictionary.TryGetValue(t,out var uI))
        {
            return uI as T;
        }
        return default(T);
    }
    protected override void Awake()
    {
        base.Awake();
        stateBarUI =transform.Find("UIRoot/State Bar").GetComponent<StateBarUI>();
        switchTimeUI =transform.Find("UIRoot/Switch Time").GetComponent <SwitchTimeUI>();
    }
  


}
