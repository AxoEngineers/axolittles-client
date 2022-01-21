using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource;
    public Image toggleImage;
    public Sprite playingSprite;
    public Sprite mutedSprite;
    
    public bool muted;
    public float transitionSpeed = 0.1f;

    private float _defaultVolume;
    private float _targetVolume;

    private void Awake()
    {
        _defaultVolume = audioSource.volume;
        _targetVolume = _defaultVolume;
    }

    private void Update()
    {
        if (audioSource.volume != _targetVolume)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, _targetVolume, transitionSpeed);
        }
    }
    
    public void Toggle()
    {
        muted = !muted;
        
        _targetVolume = muted ? 0 : _defaultVolume;
        toggleImage.sprite = muted ? mutedSprite : playingSprite;
    }
}
