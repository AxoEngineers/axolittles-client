using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarGridElement : BetterGridElement
{
    private EventTrigger trigger;
    
    public override void SetData(params object[] args)
    {
        if (args[0] is AxoInfo)
        {
            AxoInfo avatar = (AxoInfo) args[0];
            trigger = GetComponent<EventTrigger>();

            Text.text = avatar.name;
            Icon.sprite = avatar.sprite;
            
            trigger.AddEvent(EventTriggerType.PointerClick, data =>
            {
                AxoPreview.Instance.SetPreview(avatar);
            });

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}