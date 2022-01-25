using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveAxoManager : Mingleton<ActiveAxoManager>
{
    private EventTrigger eventTrigger;
    
    private List<AxoInfo> active;
    private bool generating;

    public AudioClip addAxo;
    public AudioClip removeAxo;
    public AudioSource audioSource;
    
    private int maxActive => gridItems.Length;

    public ActiveAvatarGridElement[] gridItems;

    new void Awake()
    {
        base.Awake();
        active = new List<AxoInfo>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Set(int id)
    {
        AxoInfo existing = active.Find(axo => axo.id == id);
        
        if (existing)
        {
            existing.gameObject.SetActive(false);
            active.Remove(existing);
            PlaySound(removeAxo);
            RefreshGrid();
            return;
        }
        
        if (active.Count < maxActive)
        {
            if (!generating)
            {
                generating = true;
                AxoModelGenerator.Instance.Generate(id, axoObject =>
                {
                    active.Add(axoObject);
                    PlaySound(addAxo);
                    RefreshGrid();
                    axoObject.gameObject.SetActive(true);
                    generating = false;
                });
            }
        }
    }

    private void RefreshGrid()
    {
        for (int i = 0; i < maxActive; i++)
        {
            if (i >= active.Count)
            {
                gridItems[i].gameObject.SetActive(false);
                continue;
            }

            gridItems[i].Text.text = $"{active[i].id}";
            gridItems[i].Icon.sprite = AvatarGrid.Instance.spriteCache[active[i].id];
            gridItems[i].gameObject.SetActive(true);
        }
    }
    
    private void PlaySound(AudioClip sound)
    {
        audioSource.Stop();
        audioSource.clip = sound;
        audioSource.Play();
    }
}
