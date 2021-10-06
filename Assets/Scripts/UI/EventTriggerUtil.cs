using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class EventTriggerUtil
{
    public static void AddEvent(this EventTrigger e, EventTriggerType trigger, UnityAction<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry();
        entry.eventID = trigger;
        entry.callback.AddListener(callback);
        e.triggers.Add(entry);
    }
}