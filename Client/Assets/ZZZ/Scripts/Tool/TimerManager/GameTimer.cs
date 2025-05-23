using System;
using UnityEngine;
public enum TimerStation
{ 
    NotWorked,
    DoWorking,
    DoneWorked
}
public class GameTimer
{
    private float _startTime;
    private Action _task;
    private TimerStation _timerStation;
    private bool _isStopTime;
    private bool _isRealTime;

    public TimerStation TimerStation => _timerStation;
    public bool IsRealTime => _isRealTime;
    public GameTimer()//new初始化
    {
        InitTimer();
    }
    public void StartTimer(bool isRealTime,float startTime,Action task)
    {
        _isRealTime = isRealTime;
        _startTime =startTime;
        _task = task;
        _isStopTime = false;
        _timerStation = TimerStation.DoWorking;
    }

    public void UpdateTimer()
    {
        if (_isRealTime) { return; }
        if (_isStopTime == true) { return; }

        _startTime-=Time.deltaTime;
        if (_startTime <= 0)
        { 
           _task?.Invoke();
           _isStopTime = true;
            _timerStation = TimerStation.DoneWorked;
        }
    }
    /// <summary>
    /// 不会受到ScaleTime影响
    /// </summary>
    public void UpdateRealTimer()
    {
        if (!_isRealTime) { return; }
        if (_isStopTime == true) { return; }

        _startTime -= Time.unscaledDeltaTime;
        if (_startTime <= 0)
        {
            _task?.Invoke();
            _isStopTime = true;
            _timerStation = TimerStation.DoneWorked;
        }
    }
    public void InitTimer() 
    {
        _startTime = 0;
        _task = null;
        _isStopTime = true;
        _timerStation= TimerStation.NotWorked;
        _isRealTime = false;

    }
 

}
