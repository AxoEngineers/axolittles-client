using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ActiveAvatarGridElement : BetterGridElement
{
    private EventTrigger trigger;
    private NftAddress nftAddress;

    public void Awake()
    {
        trigger = GetComponent<EventTrigger>();
        trigger.triggers.Clear();
        trigger.AddEvent(EventTriggerType.PointerClick,
            data =>
            {
                ActiveAxoManager.Instance.Set(int.Parse(Text.text));
            });
    }
}