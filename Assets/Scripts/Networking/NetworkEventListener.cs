using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LwNetworking
{
    /// <summary>
    /// Useful for hooking events where 
    /// </summary>
    public class NetworkEventListener
    {
        public struct QueuedEvent
        {
            public NetworkEventEnum eventType;
            public object eventData;
        }
        
        // INVOKE QUEUE VARS
        public List<NetworkEventEnum> SupportedQueuedEvents = new List<NetworkEventEnum>();
        public List<QueuedEvent> QueuedEvents = new List<QueuedEvent>();
        public bool QueueInvoke { get; set; } = true; // good to use this if components havent loaded but are delayed
        
        
        Dictionary<NetworkEventEnum, UnityEvent<object>> Events = new Dictionary<NetworkEventEnum, UnityEvent<object>>();

        public NetworkEventListener()
        {
            foreach (NetworkEventEnum e in Enum.GetValues(typeof(NetworkEventEnum)))
            {
                Events.Add(e, new UnityEvent<object>());
            }
            
            Events[NetworkEventEnum.Disconnect].AddListener(obj =>
            {
                QueueInvoke = true;
            });
            
            SupportedQueuedEvents.Add(NetworkEventEnum.PlayerChat);
            SupportedQueuedEvents.Add(NetworkEventEnum.PlayerPermissions);
            SupportedQueuedEvents.Add(NetworkEventEnum.PlayerInfoRequest);
        }
        
        public void AddListener(NetworkEventEnum eventEnum, UnityAction<object> action)
        {
            if (!Events.ContainsKey(eventEnum))
            {
                Debug.LogError($"This event ${eventEnum} is currently not supported.");
                return;
            }
            
            Events[eventEnum].AddListener(action);
        }

        public void Invoke(NetworkEventEnum eventEnum, object value)
        {
            if (SupportedQueuedEvents.Contains(eventEnum))
            {
                if (QueueInvoke)
                {
                    QueuedEvents.Add(new QueuedEvent() { eventData = value, eventType = eventEnum });
                    return; // delay invocation
                }    
            }
            
            Events[eventEnum].Invoke(value);
        }

        public void FinalizeInvokeQueue()
        {
            if (QueueInvoke)
            {
                QueueInvoke = false;
                foreach (var e in QueuedEvents)
                {
                    Invoke(e.eventType, e.eventData);
                }
                QueuedEvents.Clear();
            }
        }
        
        public void RemoveListener(NetworkEventEnum eventEnum, UnityAction<object> action)
        {
            Events[eventEnum].RemoveListener(action);
        }

        public static NetworkEventEnum ToEnum(string eventName)
        {
            return (NetworkEventEnum)Enum.Parse(typeof(NetworkEventEnum), eventName);
        }
        
    }
}