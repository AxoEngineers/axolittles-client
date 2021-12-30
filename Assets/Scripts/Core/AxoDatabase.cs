using System;
using UnityEngine;

public class AxoDatabase : MonoBehaviour
{
    public AxoStruct[] db;
    
    private void Start()
    {
        var arr = JsonUtility.FromJson<AxoArray>("{\"data\":" + Resources.Load<TextAsset>("axotraitsdb").text + "}");
        db = arr.data;
    }
}