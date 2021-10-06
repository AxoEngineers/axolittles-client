using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarPanel : MonoBehaviour
{
    public Image hoverImg;
    
    private EventTrigger trigger;

    private float hoverStartAlpha;
    
    // Start is called before the first frame update
    void Start()
    {
        trigger = GetComponent<EventTrigger>();
        trigger.AddEvent(EventTriggerType.PointerEnter, data =>
        {
            SetOutline(true);
        });
        trigger.AddEvent(EventTriggerType.PointerExit, data =>
        {
            SetOutline(false);
        });
        
        hoverStartAlpha = hoverImg.color.a;
        SetOutline(false);
    }

    public void SetOutline(bool state)
    {
        var color = hoverImg.color;
        color.a = state ? hoverStartAlpha : 0.0f;
        hoverImg.color = color;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
