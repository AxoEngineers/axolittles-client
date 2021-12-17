using UnityEngine;
using UnityEngine.UI;

public class AvatarGridElement : BetterGridElement
{
    public override void SetData(params object[] args)
    {
        if (args[0] is AvatarInfo)
        {
            AvatarInfo avatar = (AvatarInfo) args[0];

            this.Text.text = avatar.name;
            this.Icon.sprite = avatar.sprite;

            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}