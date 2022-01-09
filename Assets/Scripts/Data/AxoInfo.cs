using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxoInfo : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    
    public string name;
    public int id;
    public Sprite sprite;
}
