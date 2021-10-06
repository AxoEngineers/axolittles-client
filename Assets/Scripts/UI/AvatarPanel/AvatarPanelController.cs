using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarPanelController : MonoBehaviour
{
    private ScrollRect scrollRect;

    public Text pageNumberText;
    public Button panelUpBtn;
    public Button panelDownBtn;
    
    // Start is called before the first frame update
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        MovePageVertical(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateAvatar(int index)
    {
        
    }

    public void MovePageVertical(int direction)
    {
        var maxPages = Mathf.CeilToInt(scrollRect.content.childCount / 5) + 1;
        var pages = 2;
        var interval = (1.0f / (pages - 1));
        var currScrollValue = scrollRect.verticalScrollbar.value;
        
        scrollRect.verticalScrollbar.value = Mathf.Clamp((float)Math.Round((decimal)(currScrollValue + (interval * direction)), 1), 0, 1);
        panelUpBtn.interactable = (float)Math.Round((decimal)scrollRect.verticalScrollbar.value, 1) < 1.0f;
        panelDownBtn.interactable = (float)Math.Round((decimal)scrollRect.verticalScrollbar.value, 1) > 0.0f;

        pageNumberText.text = $"{ (float)Math.Round((decimal)(maxPages - scrollRect.verticalScrollbar.value)) } / { maxPages }";
    }
    
}
