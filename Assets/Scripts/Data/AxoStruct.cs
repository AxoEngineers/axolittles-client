using System;
using UnityEngine;

[Serializable]
public class AxoStruct
{
    // FROM WEB3
    public string id;
    public string background;
    public string top;
    public string face;
    public string outfit;
    public string type;
    
    // FROM DEVS
    public int rhue;
    public string rtop;
    public string rface;
    public string routfit;
    public string rbodytype;

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

[Serializable]
public class AxoArray
{
    public AxoStruct[] data;
}