using System;
using UnityEngine;

public class Mingleton<T> : MonoBehaviour where T : Mingleton<T>
{
    protected static T _Instance;
        
    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = (T)FindObjectOfType(typeof(T));
            }
            return _Instance;
        }
    }

    protected void Awake()
    {
        _Instance = (T)this;
    }
}