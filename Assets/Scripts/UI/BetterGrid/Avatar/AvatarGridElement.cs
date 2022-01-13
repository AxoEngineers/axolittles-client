using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AvatarGridElement : BetterGridElement
{
    private EventTrigger trigger;
    
    public override void SetData(params object[] args)
    {
        if (args[0] is NftAddress)
        {
            NftAddress avatar = (NftAddress) args[0];

            if (avatar.Equals(NftAddress.Null))
            {
                gameObject.SetActive(false);
                return;
            }
            
            Text.text = $"{avatar.id}";
            Icon.color = Icon.sprite ? Color.white : Color.clear;
            
            AxoModelGenerator.Instance.GetImage(avatar, image =>
            {
                Icon.sprite = image;
                Icon.color = Color.white;
            });

            trigger = GetComponent<EventTrigger>();
            trigger.AddEvent(EventTriggerType.PointerClick, data =>
            {
                AxoModelGenerator.Instance.Generate(avatar, axo =>
                {
                    AxoPreview.Instance.SetPreview(axo);
                });
                
            });

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator GetImage(int id, UnityAction<Sprite> callback = null)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture($"{Configuration.GetWeb3URL()}avatar/{id}.png");
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D axoTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
            callback?.Invoke(Sprite.Create(axoTexture, Rect.MinMaxRect(0, 0, axoTexture.width, axoTexture.height),
                new Vector2(0.5f, 0.5f)));
        }
    }
    
}