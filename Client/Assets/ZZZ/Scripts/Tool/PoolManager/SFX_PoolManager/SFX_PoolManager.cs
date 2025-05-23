
using System.Collections.Generic;
using UnityEngine;
using HuHu;

public class SFX_PoolManager : Singleton<SFX_PoolManager>
{
    //有SoundName的代表分的更细的音频
    [System.Serializable]
    public class SoundItem
    {
        public SoundStyle soundStyle;
        public string soundName;   
        public GameObject soundPrefab;
        public int soundCount;
        public bool ApplyBigCenter;
    }
    
    [SerializeField] private List<SoundItem> soundPools = new List<SoundItem>();
    private Dictionary<SoundStyle, Queue<GameObject>> soundCenter = new Dictionary<SoundStyle, Queue<GameObject>>();
    private Dictionary<string, Dictionary<SoundStyle, Queue<GameObject>>> bigSoundCenter = new Dictionary<string, Dictionary<SoundStyle, Queue<GameObject>>>();
    protected override void Awake()
    {
        base.Awake();
        InitSoundPool();
    }
    private void InitSoundPool()
    {
        if (soundPools.Count == 0) { return; }
        for (int i = 0; i < soundPools.Count; i++)
        {
            if (soundPools[i].ApplyBigCenter)
            {
                for (int j = 0; j < soundPools[i].soundCount;j++)
                {
                    //实例化
                    var go = Instantiate(soundPools[i].soundPrefab);
                    //设置父级点
                    go.transform.parent = this.transform;
                    //掩藏
                    go.SetActive(false);
                    if (!bigSoundCenter.ContainsKey(soundPools[i].soundName))
                    {
                        Debug.Log(soundPools[i].soundName + "加入对象池");
                        bigSoundCenter.Add(soundPools[i].soundName, new Dictionary<SoundStyle, Queue<GameObject>>());
                    }
                    if (!bigSoundCenter[soundPools[i].soundName].ContainsKey(soundPools[i].soundStyle))
                    {
                        bigSoundCenter[soundPools[i].soundName].Add(soundPools[i].soundStyle, new Queue<GameObject>());
                    }
                    bigSoundCenter[soundPools[i].soundName][soundPools[i].soundStyle].Enqueue(go);
                }
            }
            else
            {
                for (int j = 0; j < soundPools[i].soundCount; j++)
                {
                    //实例化
                    var go = Instantiate(soundPools[i].soundPrefab);
                    //设置父级点
                    go.transform.parent = this.transform;
                    //掩藏
                    go.SetActive(false);
                    //放入字典
                    if (!soundCenter.ContainsKey(soundPools[i].soundStyle))
                    {
                        //加入Kay
                        soundCenter.Add(soundPools[i].soundStyle, new Queue<GameObject>());
                        //把新实例加入Value，而不是预制体
                        soundCenter[soundPools[i].soundStyle].Enqueue(go);
                    }
                    else
                    {
                        //只用加Value
                        soundCenter[soundPools[i].soundStyle].Enqueue(go);
                    }
                }
            }

        }

    }

    public void TryGetSoundPool(SoundStyle soundStyle, string soundName, Vector3 position)
    {
        if (bigSoundCenter.ContainsKey(soundName))
        {
            if (bigSoundCenter[soundName].TryGetValue(soundStyle, out var Q))
            {
                GameObject go = Q.Dequeue();
                go.transform.position = position;
                go.gameObject.SetActive(true);
                Q.Enqueue(go);
               // Debug.Log("播放音乐"+ soundName+"类型是"+soundStyle);
              
            }
            else
            {
               // Debug.LogWarning(soundStyle + "找不到");
            }
        }
        else
        {
           // Debug.LogWarning(soundName + "找不到");
        }

    }
    public void TryGetSoundPool( SoundStyle soundStye, Vector3 position, Quaternion quaternion)
    {
        if (soundCenter.TryGetValue(soundStye, out var sound))
        {
           // Debug.Log(soundStye + "播放");
            GameObject go = sound.Dequeue();
            go.transform.position = position;
            go.gameObject.SetActive(true);
            soundCenter[soundStye].Enqueue(go);
        }
        else
        {
           // Debug.Log(soundStye + "不存在");
        }
    }
   


}
