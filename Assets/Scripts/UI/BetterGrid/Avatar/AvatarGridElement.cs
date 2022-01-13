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
            Icon.color = Icon.sprite ? Color.white : new Color(0, 0, 0, 0);
            
            trigger.AddEvent(EventTriggerType.PointerClick, data =>
            {
                AxoModelGenerator.Instance.Generate(avatar, arg0 =>
                {
                    AxoPreview.Instance.SetPreview(avatar);
                });
                
            });

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}