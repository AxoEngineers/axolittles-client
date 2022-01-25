using UnityEngine;
using UnityEngine.UI;

public class StatusTicker : Mingleton<StatusTicker>
{
    public bool active;
    public Text statusText;
    
    private string[] _statusMessages;
    
    private float _lastRefresh;
    private float _interval = 2.5f;

    private const string FileName = "statusmessages";
    private const string JsonRoot = "Items";
    private const string ErrorMessage = "Status messages not loaded.";

    private new void Awake()
    {
        base.Awake();
        
        TextAsset messages = Resources.Load<TextAsset>(FileName);
        string encapsulation = $"{{\"{JsonRoot}\":{messages.text}}}";
        _statusMessages = JsonHelper.FromJson<string>(encapsulation);
    }

    private void Update()
    {
        if (!active) return;
        
        if (Time.time > _lastRefresh + _interval)
        {
            Randomize();
        }
    }
    
    public void Randomize()
    {
        _lastRefresh = Time.time;
        statusText.text = GetRandomStatus();
    }
    
    private string GetRandomStatus()
    {
        if (_statusMessages.Length == 0) Debug.LogError(ErrorMessage);
        return _statusMessages[Random.Range(0, _statusMessages.Length - 1)];
    }
}
