using System;
using System.Collections.Generic;
using UnityEngine;

public static class AxoDatabase
{
    public static AxoStruct Get(int id)
    {
        return Data[id];
    }
    
    static Dictionary<int, AxoStruct> Data = new Dictionary<int, AxoStruct>();
    
    static AxoDatabase()
    {
        var arr = JsonUtility.FromJson<AxoArray>("{\"data\":" + Resources.Load<TextAsset>("axotraitsdb").text + "}");
        foreach (AxoStruct axo in arr.data)
        {
            Data.Add(int.Parse(axo.id), axo);
        }
    }
}