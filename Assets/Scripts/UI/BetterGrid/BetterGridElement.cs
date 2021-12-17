using System;
using UnityEngine;
using UnityEngine.UI;

public class BetterGridElement : MonoBehaviour
{
    public Button Button;
    public Text Text;
    public Image Icon;
    public Image FavoriteIcon;
    public CanvasGroup CanvasGroup;

    public Sprite FavoriteIconAssetUnlit;
    public Sprite FavoriteIconAssetLit;
    
    public virtual void SetData(params object[] args)
    {

    }

}