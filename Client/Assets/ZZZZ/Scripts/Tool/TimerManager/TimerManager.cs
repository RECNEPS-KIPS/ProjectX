using System;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

public class ZZZZTimerManager : Singleton<ZZZZTimerManager>
{
    [SerializeField, Header("��ʱ��������")] private int timerCount;
    private Queue<GameTimer> notWorkTimers = new Queue<GameTimer>();
    private List<GameTimer> isWorkingTimers = new List<GameTimer>();

    protected void Start()
    {
        for (int i = 0; i < timerCount; i++) 
        {
            CreateTimer();
        }
    }
    private void Update()
    {
        UpdateTime();
        
    }
    private void CreateTimer()
    { 
        var timer = new GameTimer();
        notWorkTimers.Enqueue(timer);
    }
    /// <summary>
    /// ���ܵ�ScaleTimeӰ��ļ�ʱ��
    /// </summary>
    /// <param name="timer">��ʱ��ʱ��</param>
    /// <param name="action">��ʱ��ɺ���õ�ί��</param>
    public void GetOneTimer(float timer, Action action)
    {
        if (notWorkTimers.Count == 0) { CreateTimer(); }
        GameTimer gameTimer = null;
        gameTimer= notWorkTimers.Dequeue();
        gameTimer.StartTimer(false,timer, action);  
        isWorkingTimers.Add(gameTimer);
    }
   /// <summary>
   /// �����ܵ�ScaleTimeӰ��ļ�ʱ��
   /// </summary>
   /// <param name="time"></param>
   /// <param name="action"></param>
   /// <returns></returns>
    public GameTimer GetRealTimer(float time, Action action)
    {
        if (notWorkTimers.Count == 0) { CreateTimer(); }
        GameTimer gameTimer = new GameTimer();
        gameTimer = notWorkTimers.Dequeue();
        gameTimer.StartTimer(true,time, action);
        isWorkingTimers.Add(gameTimer);
        return gameTimer;

    }
    public GameTimer GetTimer(float time, Action action)
    {
        if (notWorkTimers.Count == 0) { CreateTimer(); }
        GameTimer gameTimer = new GameTimer();
        gameTimer = notWorkTimers.Dequeue();
        gameTimer.StartTimer(false, time, action);
        isWorkingTimers.Add(gameTimer);
        return gameTimer;

    }
    /// <summary>
    /// ���ⲿ�ṩһ�����ټ�ʱ���ķ���
    /// </summary>
    /// <param name="gameTimer"></param>
    public void UnregisterTimer(GameTimer gameTimer)
    {
        if (gameTimer == null) { return; }
        //�ǹ�����ʱ�����ܱ����٣���Ϊ���ܻ�ע�������¼�
        if (gameTimer.TimerStation != TimerStation.DoWorking) { return; }
        gameTimer.InitTimer();
        isWorkingTimers.Remove(gameTimer);
        notWorkTimers.Enqueue(gameTimer);
    }
    /// <summary>
    /// �ƶ�WorkingTimer�ļ�ʱ�������Լ�������ɹ�����Timer
    /// </summary>
    private void UpdateTime()
    {
        if (isWorkingTimers.Count == 0) { return; }
        for (int i = 0;i < isWorkingTimers.Count; i++) 
        {
            if (isWorkingTimers[i].TimerStation == TimerStation.DoWorking)
            {
                if (!isWorkingTimers[i].IsRealTime)
                {
                    isWorkingTimers[i].UpdateTimer();
                }
                else
                {
                    isWorkingTimers[i].UpdateRealTimer();
                }
            }       
            else if (isWorkingTimers[i].TimerStation==TimerStation.DoneWorked)
            {
                isWorkingTimers[i].InitTimer();
                notWorkTimers.Enqueue(isWorkingTimers[i]);
                isWorkingTimers.Remove(isWorkingTimers[i]);
            }
        }
    }
}
