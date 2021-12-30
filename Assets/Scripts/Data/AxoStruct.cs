using System;

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
}

[Serializable]
public class AxoArray
{
    public AxoStruct[] data;
}