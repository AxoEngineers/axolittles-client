using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingIndicator : Mingleton<LoadingIndicator>
{
    public Text loadingText;

    public Coroutine spriteRoutine;
    public Coroutine modelRoutine;

    void Update()
    {
        loadingText.enabled = spriteRoutine != null || modelRoutine != null;
    }
}
