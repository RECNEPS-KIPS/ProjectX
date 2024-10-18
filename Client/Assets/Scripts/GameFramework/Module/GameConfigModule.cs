using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class GameConfigModule : GameFrameworkModule
    {
        public void InitCustomConfig<TKey,TValue>(ref CustomDictConfig<TKey,TValue> config,List<TValue> data,Func<TValue,TKey> getKeyFunc)
        {
            config = new CustomDictConfig<TKey, TValue>(getKeyFunc, data);
        }
    }
    
    public class CustomDictConfig<TKey,TValue>
    {
        private readonly List<TValue> m_data;
        private readonly Dictionary<TKey, TValue> m_dataDict;
        private readonly Dictionary<TKey, List<TValue>> m_dataListDict;
        
        public Dictionary<TKey, TValue> FirstDataDict => m_dataDict;
        public Dictionary<TKey, List<TValue>> AllDataDict => m_dataListDict;
        public List<TValue> List => m_data;
                
        public CustomDictConfig(Func<TValue,TKey> getKeyFunc,List<TValue> data)
        {
            m_data = data;
            m_dataDict = new Dictionary<TKey, TValue>();
            m_dataListDict = new Dictionary<TKey, List<TValue>>();
            for (int i = 0, length = data.Count; i < length; i++)
            {
                var item = m_data[i];
                var key = getKeyFunc(item);
                if (!m_dataDict.ContainsKey(key))
                {
                    m_dataDict[key] = item;
                    m_dataListDict[key] = new List<TValue>();
                }
                m_dataListDict[key].Add(item);
            }
        }
    }

}
