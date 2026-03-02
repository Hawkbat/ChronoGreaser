using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundController : MonoBehaviour
{
    [SerializeField] AudioChannel channel = AudioChannel.Sounds;
    [SerializeField] bool muteOnRewind = false;
    [SerializeField] bool muteOnFastForward = false;

    AudioSource audioSource;
    Stack<SoundTrigger> triggers = new();
    float baseVolume = 1f;

    public bool IsPlaying => audioSource.isPlaying;

    public float Duration => audioSource.clip != null ? audioSource.clip.length : 0f;

    AudioMixerGroup OutputGroup => channel switch
    {
        AudioChannel.Music => AudioController.Instance.MusicGroup,
        AudioChannel.Sounds => AudioController.Instance.SoundsGroup,
        AudioChannel.Voice => AudioController.Instance.VoiceGroup,
        _ => AudioController.Instance.MasterGroup,
    };

    bool ShouldMute => (muteOnRewind && TimeLoop.IsRewinding) || (muteOnFastForward && TimeLoop.IsFastForwarding) || Save.Instance.masterVolume <= 0f || channel switch
    {
        AudioChannel.Music => Save.Instance.musicVolume <= 0f,
        AudioChannel.Sounds => Save.Instance.sfxVolume <= 0f,
        AudioChannel.Voice => Save.Instance.voiceVolume <= 0f,
        _ => false,
    };

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = OutputGroup;
        baseVolume = audioSource.volume;
        triggers = new();
    }

    void Update()
    {
        audioSource.mute = ShouldMute;
        audioSource.pitch = TimeLoop.TimeScale;

        if (TimeLoop.IsRewinding)
        {
            while (triggers.Count > 0 && triggers.Peek().EndTime > TimeLoop.CurrentTime)
            {
                var trigger = triggers.Pop();
                if (TimeLoop.CurrentTime >= trigger.time && !muteOnRewind)
                {
                    audioSource.volume = baseVolume * trigger.volume;
                    audioSource.Play();
                    audioSource.time = TimeLoop.CurrentTime - trigger.time;
                }
            }
        }
    }

    public void Play(float volume = 1f)
    {
        audioSource.volume = volume * baseVolume;
        audioSource.Play();
        triggers.Push(new SoundTrigger
        {
            time = TimeLoop.CurrentTime,
            duration = audioSource.clip.length,
            volume = volume
        });
    }

    public void Stop()
    {
        audioSource.Stop();
        if (triggers.Count > 0)
        {
            var trigger = triggers.Pop();
            trigger.duration = TimeLoop.CurrentTime - trigger.time;
            triggers.Push(trigger);
        }
    }

    public void SetPlaying(bool playing, float volume = 1f)
    {
        if (playing)
        {
            if (!audioSource.isPlaying)
            {
                Play(volume);
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                Stop();
            }
        }
    }

    struct SoundTrigger
    {
        public float time;
        public float duration;
        public float volume;

        public readonly float EndTime => time + duration;
    }
}
