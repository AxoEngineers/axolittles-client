using System;

[Serializable]
public class AxoStruct
{
    public string id;
    public string background;
    public string top;
    public string face;
    public string outfit;
    public string type;
}

[Serializable]
public class AxoArray
{
    public AxoStruct[] data;
}