using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxoPreview : Mingleton<AxoPreview>
{
    private GameObject previewObject;

    public void SetPreview(AxoInfo axo)
    {
        if (previewObject)
        {
            previewObject.SetActive(false);
        }

        previewObject = axo.gameObject;
        previewObject.SetActive(true);
    }
}
