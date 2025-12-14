using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _audioSourceLoop;
    // singleton
    private static AudioManager _audioManager;
    public static AudioManager instance => _audioManager;
    public Button audioButton;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _audioManager = this;
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
}
