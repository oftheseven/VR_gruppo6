using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    // singleton
    private static AudioManager _audioManager;
    public static AudioManager instance => _audioManager;

    [Header("Audio references")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _audioSourceLoop;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _audioManager = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void PlayAudio(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }

    public void PlayAudioLoop(AudioClip clip)
    {
        _audioSourceLoop.clip = clip;
        _audioSourceLoop.Play();
    }

    public void StopAudio()
    {
        _audioSource.Stop();
        _audioSourceLoop.Stop();
    }

    // public void ToggleAudio()
    // {
    //     if (_audioSource.mute)
    //     {
    //         // _audioSource.mute = false;
    //         // _audioSourceLoop.mute = false;
    //         Debug.LogWarning("Audio unmuted");
    //     }
    //     else
    //     {
    //         // _audioSource.mute = true;
    //         // _audioSourceLoop.mute = true;
    //         Debug.LogWarning("Audio muted");
    //     }
    // }
}
