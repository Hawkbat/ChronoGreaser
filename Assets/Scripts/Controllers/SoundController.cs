using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundController : MonoBehaviour
{
    [SerializeField] bool isMusic = false;
    [SerializeField] bool muteOnRewind = false;
    [SerializeField] bool muteOnFastForward = false;

    AudioSource audioSource;
    Stack<SoundTrigger> triggers = new();
    float baseVolume = 1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = isMusic ? AudioController.Instance.MusicGroup : AudioController.Instance.SoundsGroup;
        baseVolume = audioSource.volume;
        triggers = new();
    }

    void Update()
    {
        audioSource.mute = (muteOnRewind && TimeLoop.IsRewinding) || (muteOnFastForward && TimeLoop.IsFastForwarding) || (isMusic && Save.Instance.musicVolume <= 0f) || (!isMusic && Save.Instance.sfxVolume <= 0f);
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
                }
            }
        }
    }

    public bool IsPlaying() => audioSource.isPlaying;

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
