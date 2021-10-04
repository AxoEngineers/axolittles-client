using UnityEngine;
using System.Collections;

public class Timestamp
{
    private float time = 0f;
    private float defaultDelay = 1f;
        
    public float Duration { get; private set; }
    public float ExpirationDuration { get { var t = time - Time.time;  return t > 0 ? t : 0; } }
    public float Progress { get { return 1.0f - (ExpirationDuration / Duration); } }

    public float StartTime => time - Duration;
    public bool Expired {  get { return ExpirationDuration <= 0; } }
        
    public Timestamp(float delay)
    {
        defaultDelay = delay;
        Duration = delay;
    }

    public void Reset(float delay)
    {
        time = Time.time + delay;
        Duration = delay;
    }

    public void Reset()
    {
        Reset(defaultDelay);
    }

    public void Expire()
    {
        time = 0;
    }
}