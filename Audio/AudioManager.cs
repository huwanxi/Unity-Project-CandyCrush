using UnityEngine;
using System.Collections.Generic;

public class AudioManager
{
    public static AudioManager Instance { get; private set; }

    private GameObject audioContainer;
    private AudioController bgmController;
    private List<AudioController> sfxControllers;
    
    private float globalVolume = 1f;

    /// <summary>
    /// 初始化 AudioManager
    /// </summary>
    public void Init()
    {
        if (Instance != null) return;
        Instance = this;

        // 创建全局音频容器
        audioContainer = new GameObject("AudioManager_Container");
        Object.DontDestroyOnLoad(audioContainer);

        // 初始化 BGM Controller
        bgmController = new AudioController(audioContainer, "BGM_Source");

        // 初始化 SFX Controller 列表
        sfxControllers = new List<AudioController>();
    }

    /// <summary>
    /// 控制全局音量
    /// </summary>
    public void SetGlobalVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);
        
        if (bgmController != null) 
            bgmController.SetVolume(globalVolume);
            
        if (sfxControllers != null)
        {
            foreach (var sfx in sfxControllers)
            {
                if (sfx != null) sfx.SetVolume(globalVolume);
            }
        }
    }

    /// <summary>
    /// 播放音频 API
    /// </summary>
    /// <param name="key">JSON配置中的资源Key</param>
    /// <param name="volume">音量大小 (会受全局音量影响)</param>
    /// <param name="loop">是否循环</param>
    /// <param name="overrideSame">播放相同音效时，是否覆盖重新播放 (false 则不作用忽略本次请求)</param>
    /// <param name="isSfx">是否为音效 (false 表示为全局背景音乐)</param>
    public void PlayAudio(string key, float volume, bool loop, bool overrideSame, bool isSfx)
    {
        if (audioContainer == null) return;

        // 1. 获取资源路径，并指定在 Json/AudioJson 下寻找配置
        string path = ResourceConfigManager.GetPath(key, "Json/AudioJson");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"AudioManager: 未找到音频资源路径 key: {key}");
            return;
        }

        // 2. 加载资源
        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debug.LogWarning($"AudioManager: 无法加载音频资源 path: {path}");
            return;
        }

        if (!isSfx)
        {
            // 播放音乐 (不同音乐不能叠加，同一时间只有一个 BGM)
            if (bgmController.Clip == clip && bgmController.IsPlaying)
            {
                if (!overrideSame) return; // 如果相同音乐已经在播且不覆盖，则忽略
            }
            
            bgmController.Play(clip, volume * globalVolume, loop);
        }
        else
        {
            // 播放音效 (音乐可以跟音效叠加，但相同音效不能叠加)
            AudioController targetController = null;
            
            // 清理已销毁的controller并查找是否有相同音效正在播放
            for (int i = sfxControllers.Count - 1; i >= 0; i--)
            {
                var sfx = sfxControllers[i];
                if (sfx == null)
                {
                    sfxControllers.RemoveAt(i);
                    continue;
                }

                if (sfx.IsPlaying && sfx.Clip == clip)
                {
                    if (overrideSame)
                    {
                        targetController = sfx;
                    }
                    else
                    {
                        return; // 相同音效不叠加，直接返回
                    }
                }
            }

            // 如果没有被覆盖的同名音效，找一个空闲的 Controller
            if (targetController == null)
            {
                foreach (var sfx in sfxControllers)
                {
                    if (!sfx.IsPlaying)
                    {
                        targetController = sfx;
                        break;
                    }
                }
            }

            // 如果没有空闲的，新建一个 Controller
            if (targetController == null)
            {
                targetController = new AudioController(audioContainer, $"SFX_Source_{sfxControllers.Count}");
                sfxControllers.Add(targetController);
            }

            targetController.Play(clip, volume * globalVolume, loop);
        }
    }

    /// <summary>
    /// 结束与清理
    /// </summary>
    public void Dispose()
    {
        if (bgmController != null)
        {
            bgmController.Dispose();
            bgmController = null;
        }

        if (sfxControllers != null)
        {
            foreach (var sfx in sfxControllers)
            {
                sfx?.Dispose();
            }
            sfxControllers.Clear();
            sfxControllers = null;
        }

        if (audioContainer != null)
        {
            Object.Destroy(audioContainer);
            audioContainer = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
