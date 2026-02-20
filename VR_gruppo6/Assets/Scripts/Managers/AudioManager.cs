using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // singleton
    private static AudioManager _audioManager;
    public static AudioManager instance => _audioManager;

    [Header("Audio mixer")]
    [SerializeField] private AudioMixer audioMixer; // riferimento all'audio mixer per gestire i volumi globali

    [Header("Audio sources")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioSource ambientAudioSource;

    [Header("UI library")]
    [SerializeField] private AudioClip uiClickSound;
    [SerializeField] private AudioClip uiOpenPanelSound;
    
    [Header("Ambient library")]
    [SerializeField] private AudioClip ambientStudioSound; // rumore del set

    [Header("SFX library")]
    [SerializeField] private AudioClip interactionSFX;
    [SerializeField] private AudioClip lensSFX;
    [SerializeField] private AudioClip itemGrabSFX;
    [SerializeField] private AudioClip itemDropSFX;

    private const string MASTER_VOLUME_PARAM = "MasterVolume";
    private const string UI_VOLUME_PARAM = "UIVolume";
    private const string AMBIENT_VOLUME_PARAM = "AmbientVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    public AudioMixer AudioMixer => audioMixer;

    // getter dei volumi attuali
    public float GetMasterVolume()
    {
        if (audioMixer != null)
        {
            float dB;
            audioMixer.GetFloat(MASTER_VOLUME_PARAM, out dB);
            return DecibelToLinear(dB);
        }
        return 1f; // default
    }
    public float GetUIVolume()
    {
        if (audioMixer != null)
        {
            float dB;
            audioMixer.GetFloat(UI_VOLUME_PARAM, out dB);
            return DecibelToLinear(dB);
        }
        return 1f; // default
    }
    public float GetAmbientVolume()
    {
        if (audioMixer != null)
        {
            float dB;
            audioMixer.GetFloat(AMBIENT_VOLUME_PARAM, out dB);
            return DecibelToLinear(dB);
        }
        return 1f; // default
    }
    public float GetSFXVolume()
    {
        if (audioMixer != null)
        {
            float dB;
            audioMixer.GetFloat(SFX_VOLUME_PARAM, out dB);
            return DecibelToLinear(dB);
        }
        return 1f; // default
    }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _audioManager = this;
        DontDestroyOnLoad(this.gameObject);
        
        SetupAudioSources();
    }

    void Start()
    {
        if (ambientStudioSound != null)
        {
            PlayAmbient(ambientStudioSound);
        }
    }

    private void SetupAudioSources()
    {
        if (uiAudioSource == null)
        {
            GameObject uiObj = new GameObject("UI_AudioSource");
            uiObj.transform.SetParent(transform);
            uiAudioSource = uiObj.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.spatialBlend = 0f;
        }

        if (audioMixer != null)
        {
            uiAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("UI")[0];
        }
        
        if (ambientAudioSource == null)
        {
            GameObject ambientObj = new GameObject("Ambient_AudioSource");
            ambientObj.transform.SetParent(transform);
            ambientAudioSource = ambientObj.AddComponent<AudioSource>();
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.loop = true;
            ambientAudioSource.spatialBlend = 0f;
        }

        if (audioMixer != null)
        {
            ambientAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Ambient")[0];
        }
    }
    
    public void PlayUIClick()
    {
        if (uiClickSound != null)
        {
            uiAudioSource.PlayOneShot(uiClickSound);
        }
    }
    
    public void PlayUIOpenPanel()
    {
        if (uiOpenPanelSound != null)
        {
            uiAudioSource.PlayOneShot(uiOpenPanelSound);
        }
    }
    
    public void PlayUISound(AudioClip clip)
    {
        if (clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }
    
    public void PlayAmbient(AudioClip ambientClip)
    {
        if (ambientClip != null && ambientAudioSource != null)
        {
            ambientAudioSource.clip = ambientClip;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }
    }

    // interaction sfx (suonato ogni volta che si interagisce con un oggetto)
    public void PlayInteractionSFX()
    {
        if (interactionSFX != null)
        {
            uiAudioSource.PlayOneShot(interactionSFX, GetUIVolume());
        }
    }

    // camera lens change sfx
    public void PlayCameraLensChange()
    {
        if (lensSFX != null)
        {
            uiAudioSource.PlayOneShot(lensSFX, GetUIVolume());
        }
    }

    // item grab/drop sfx
    public void PlayItemGrab()
    {
        if (itemGrabSFX != null)
        {
            uiAudioSource.PlayOneShot(itemGrabSFX, GetUIVolume());
        }
    }
    public void PlayItemDrop()
    {
        if (itemDropSFX != null)
        {
            uiAudioSource.PlayOneShot(itemDropSFX, GetUIVolume());
        }
    }
    
    public void StopAmbient()
    {
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }
    }

    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = LinearToDecibel(volume);
            audioMixer.SetFloat(MASTER_VOLUME_PARAM, dB);
        }
    }
    
    public void SetUIVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = LinearToDecibel(volume);
            audioMixer.SetFloat(UI_VOLUME_PARAM, dB);
        }
    }
    
    public void SetAmbientVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = LinearToDecibel(volume);
            audioMixer.SetFloat(AMBIENT_VOLUME_PARAM, dB);
        }
    }

    private float LinearToDecibel(float linear)
    {
        if (linear <= 0f)
            return -80f;
        
        return Mathf.Clamp(Mathf.Log10(linear) * 20f, -80f, 0f);
    }

    private float DecibelToLinear(float dB)
    {
        return Mathf.Pow(10f, dB / 20f);
    }

    public void MuteAll(bool mute)
    {
        if (audioMixer != null)
        {
            float volume = mute ? -80f : 0f;
            audioMixer.SetFloat(MASTER_VOLUME_PARAM, volume);
        }
    }
}