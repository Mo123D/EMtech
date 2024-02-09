using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class TaskData
{
    public string Description;
    public bool IsRepetitive;
    public bool IsComplete;
    public List<DayOfWeek> ScheduledDays;
    public TimeSpan ScheduledTime;
    public DateTime ScheduledDate;
    public bool HasBeenShownToday;
    public bool IsTimedTask;
    public float TimerDuration; 
    public Coroutine timerCoroutine;

    public TaskData(string description, bool isRepetitive, List<DayOfWeek> scheduledDays,
                       TimeSpan scheduledTime, DateTime scheduledDate,
                       bool isTimedTask = false, float timerDuration = 0)
    {
        Description = description;
        IsRepetitive = isRepetitive;
        IsComplete = false;
        ScheduledDays = scheduledDays ?? new List<DayOfWeek>();
        ScheduledTime = scheduledTime;
        ScheduledDate = scheduledDate;
        HasBeenShownToday = false;
        IsTimedTask = isTimedTask;
        TimerDuration = timerDuration;
    }
}