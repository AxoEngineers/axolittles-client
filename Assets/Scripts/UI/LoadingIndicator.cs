using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingIndicator : Mingleton<LoadingIndicator>
{
    public Text loadingText;
    public int running;

    void Update()
    {
        loadingText.enabled = running > 0;
    }
}
