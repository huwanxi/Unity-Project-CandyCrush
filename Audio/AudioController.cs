using UnityEngine;

/// <summary>
/// 控制 Audio 组合逻辑和生命周期，负责管理自身挂载的 GameObject（对应 CubeController）
/// </summary>
public class AudioController
{
    private Audio audio;
    private GameObject audioObject;

    public bool IsPlaying => audio != null && audio.IsPlaying;
    public AudioClip Clip => audio != null ? audio.Clip : null;

    /// <summary>
    /// 初始化，自动在指定的父节点下创建 GameObject 并挂载 AudioSource
    /// </summary>
    public AudioController(GameObject parent, string name)
    {
        audioObject = new GameObject(name);
        audioObject.transform.SetParent(parent.transform);
        
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        
        audio = new Audio(source);
    }

    /// <summary>
    /// 组合播放逻辑
    /// </summary>
    public void Play(AudioClip clip, float volume, bool loop)
    {
        if (audio == null) return;
        
        audio.SetClip(clip);
        audio.SetVolume(volume);
        audio.SetLoop(loop);
        audio.Play();
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    public void Stop()
    {
        if (audio != null) audio.Stop();
    }

    /// <summary>
    /// 单独设置音量（用于全局音量调节时同步）
    /// </summary>
    public void SetVolume(float volume)
    {
        if (audio != null) audio.SetVolume(volume);
    }

    /// <summary>
    /// 销毁自身及其关联的 GameObject，处理生命周期结束
    /// </summary>
    public void Dispose()
    {
        if (audioObject != null)
        {
            Object.Destroy(audioObject);
            audioObject = null;
        }
        audio = null;
    }
}