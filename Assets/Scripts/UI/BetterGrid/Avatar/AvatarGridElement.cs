using System;
using System.Collections;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AvatarGridElement : BetterGridElement
{
    private EventTrigger trigger;
    private NftAddress nftAddress;
    
    private Coroutine loadSprite;

    public override void SetData(params object[] args)
    {
        if (args[0] is NftAddress)
        {
            if (loadSprite != null) StopCoroutine(loadSprite);

            nftAddress = (NftAddress) args[0];

            if (nftAddress.Equals(NftAddress.Null))
            {
                gameObject.SetActive(false);
                return;
            }

            Text.text = $"{nftAddress.id}";
            Icon.color = Color.clear;
            
            if (AvatarGrid.Instance.spriteCache.ContainsKey(nftAddress.id))
            {
                Icon.sprite = AvatarGrid.Instance.spriteCache[nftAddress.id];
                Icon.color = Color.white;
            }
            else
            {
                try
                {
                    loadSprite = AxoModelGenerator.Instance.GetImage(nftAddress, image =>
                    {
                        Icon.sprite = image;
                        Icon.color = Color.white;
                        if (!AvatarGrid.Instance.spriteCache.ContainsKey(nftAddress.id))
                        {
                            AvatarGrid.Instance.spriteCache.Add(nftAddress.id, image);
                        }

                        LoadingIndicator.Instance.spriteRoutine = null;
                    });
                    LoadingIndicator.Instance.spriteRoutine = loadSprite;
                }
                catch (Exception)
                {
                    Debug.LogError($"Failed to retrieve sprite for NFT #{nftAddress.id}");
                }
            }

            trigger = GetComponent<EventTrigger>();
            trigger.triggers.Clear();
            trigger.AddEvent(EventTriggerType.PointerClick,
                data =>
                {
                    ActiveAxoManager.Instance.Set(nftAddress.id);
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