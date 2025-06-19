using System;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

public class ZZZZTimerManager : Singleton<ZZZZTimerManager>
{
    [SerializeField, Header("timerCount")]
    private int timerCount;

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
    
    public void GetOneTimer(float timer, Action action)
    {
        if (notWorkTimers.Count == 0)
        {
            CreateTimer();
        }

        GameTimer gameTimer = null;
        gameTimer = notWorkTimers.Dequeue();
        gameTimer.StartTimer(false, timer, action);
        isWorkingTimers.Add(gameTimer);
    }

    public GameTimer GetRealTimer(float time, Action action)
    {
        if (notWorkTimers.Count == 0)
        {
            CreateTimer();
        }

        GameTimer gameTimer = new GameTimer();
        gameTimer = notWorkTimers.Dequeue();
        gameTimer.StartTimer(true, time, action);
        isWorkingTimers.Add(gameTimer);
        return gameTimer;
    }

    public GameTimer GetTimer(float time, Action action)
    {
        if (notWorkTimers.Count == 0)
        {
            CreateTimer();
        }

        GameTimer gameTimer = new GameTimer();
        gameTimer = notWorkTimers.Dequeue();
        gameTimer.StartTimer(false, time, action);
        isWorkingTimers.Add(gameTimer);
        return gameTimer;
    }
    
    public void UnregisterTimer(GameTimer gameTimer)
    {
        if (gameTimer == null)
        {
            return;
        }
        
        if (gameTimer.TimerStation != TimerStation.DoWorking)
        {
            return;
        }

        gameTimer.InitTimer();
        isWorkingTimers.Remove(gameTimer);
        notWorkTimers.Enqueue(gameTimer);
    }
    
    private void UpdateTime()
    {
        if (isWorkingTimers.Count == 0)
        {
            return;
        }

        for (int i = 0; i < isWorkingTimers.Count; i++)
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
            else if (isWorkingTimers[i].TimerStation == TimerStation.DoneWorked)
            {
                isWorkingTimers[i].InitTimer();
                notWorkTimers.Enqueue(isWorkingTimers[i]);
                isWorkingTimers.Remove(isWorkingTimers[i]);
            }
        }
    }
}