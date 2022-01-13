using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveAxoManager : Mingleton<ActiveAxoManager>
{
    private EventTrigger eventTrigger;

    private int maxActive = 5;
    private List<AxoInfo> active;
    private bool generating;
    
    void Awake()
    {
        active = new List<AxoInfo>();
    }
    
    public void Set(int id)
    {
        AxoInfo existing = active.Find(axo => axo.id == id);
        
        if (existing)
        {
            existing.gameObject.SetActive(false);
            active.Remove(existing);
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
                    axoObject.gameObject.SetActive(true);
                    generating = false;
                });
            }
        }
    }
}
