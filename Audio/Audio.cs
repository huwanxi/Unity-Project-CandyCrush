using UnityEngine;

/// <summary>
/// 控制具体的 AudioSource 属性与基础播放方法（对应 Cube）
/// </summary>
public class Audio
{
    private AudioSource source;

    public bool IsPlaying => source != null && source.isPlaying;
    public AudioClip Clip => source != null ? source.clip : null;
    public float Volume => source != null ? source.volume : 0f;

    public Audio(AudioSource source)
    {
        this.source = source;
    }

    public void SetClip(AudioClip clip)
    {
        if (source != null) source.clip = clip;
    }

    public void SetVolume(float volume)
    {
        if (source != null) source.volume = volume;
    }

    public void SetLoop(bool loop)
    {
        if (source != null) source.loop = loop;
    }

    public void Play()
    {
        if (source != null) source.Play();
    }

    public void Stop()
    {
        if (source != null) source.Stop();
    }
}