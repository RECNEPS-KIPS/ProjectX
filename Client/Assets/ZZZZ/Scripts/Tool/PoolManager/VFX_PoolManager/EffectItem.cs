
using UnityEngine;

public class EffectItem : PoolItemBase
{
    //���߱��ο��ԣ���Ϊ���ܵ�������Ӱ�죬����loopȡ�������
    [SerializeField,Header("��Ч����ʱ��")] private float playTime;

    [SerializeField, Header("��Ч���ŵ��ٶ�")] private float playSpeed;

    private ParticleSystem[] ParticleSystem;

    private void Awake()
    {
        ParticleSystem =GetComponentsInChildren<ParticleSystem>();
       
        for (int i = 0; i < ParticleSystem.Length; i++) 
        {
            VFXManager.MainInstance.AddVFX(ParticleSystem[i], playSpeed);
        }
       
    }
    protected override void Spawn()
    {
        StartPlay();
    }
    private void StartPlay()
    {
        for (int i = 0;i < ParticleSystem.Length;i++) 
        {
            ParticleSystem[i].Play();
        }
      
        ZZZZTimerManager.MainInstance.GetOneTimer(playTime, StartReCycle);
    }
    private void StartReCycle()
    { 
       this.gameObject.SetActive(false);  
    }
    protected override void ReSycle()
    {

        for (int i = 0; i < ParticleSystem.Length; i++)
        {
            ParticleSystem[i].Stop();
        }
    }
}
