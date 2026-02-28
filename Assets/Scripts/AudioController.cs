using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    static AudioController instance;

    public static AudioController GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<AudioController>();
        }
        return instance;
    }

    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioMixerGroup masterGroup;
    [SerializeField] AudioMixerGroup musicGroup;
    [SerializeField] AudioMixerGroup soundsGroup;

    float masterVolumeMultiplier = 1f;

    public AudioMixer GetMixer() => mixer;
    public AudioMixerGroup MasterGroup => masterGroup;
    public AudioMixerGroup MusicGroup => musicGroup;
    public AudioMixerGroup SoundsGroup => soundsGroup;

    public void SetMasterVolumeMultiplier(float multiplier)
    {
        masterVolumeMultiplier = multiplier;
    }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        SetMixerVolume("MasterVolume", Save.Instance.masterVolume * masterVolumeMultiplier);
        SetMixerVolume("MusicVolume", Save.Instance.musicVolume);
        SetMixerVolume("SoundsVolume", Save.Instance.sfxVolume);
    }

    void SetMixerVolume(string parameter, float rawVolume)
    {
        var dbVolume = Mathf.Log10(rawVolume) * 20f;
        mixer.SetFloat(parameter, dbVolume);
    }
}
