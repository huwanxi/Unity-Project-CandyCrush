using UnityEngine;
using DG.Tweening;

using Cysharp.Threading.Tasks;

public class Initialization : MonoBehaviour
{
    public static Initialization Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        //DOTween.Init
        DOTween.Init(this);

        // 初始化对象池 (托管给 StaticProperties)
        if (StaticProperties.Instance != null && StaticProperties.Instance.cubeObjectPool == null)
        {

             // 使用 Initialization 的 transform 作为根节点的父级，或者不设父级
             StaticProperties.Instance.cubeObjectPool = new CubeObjectPool(this.transform);
        }

        // 启动关卡初始化
        InitializeGame().Forget();
    }

    private async UniTaskVoid InitializeGame()
    {
        // 1..全局资源初始化
        //StaticProperties.Instance = new StaticProperties();

        // 2. 初始化资源
        await new ResourceInitialization().Initialize();

        // 3.初始化UIManager (确保UI能最先被加载和控制)
        new UIManager().Initialize();

        // 4. 初始化 AudioManager (确保音频系统可用)
        new AudioManager().Init();

        // 5. 初始化关卡管理器
        new LevelInitialization(); // 实例化并设置 Instance

        // 6. 初始化 GameInputManager (Monobehaviour 单例)
        if (GameInputManager.Instance == null)
        {
            GameObject inputGo = new GameObject("GameInputManager");
            inputGo.AddComponent<GameInputManager>();
            DontDestroyOnLoad(inputGo);
        }

        // 7. 进入主菜单状态
        ReturnToMainMenu();
    }

    /// <summary>
    /// 返回主菜单：清理当前关卡资源并打开主菜单 UI
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Initialization: 返回主菜单...");

        // 1. 清理关卡资源
        if (LevelInitialization.Instance != null)
        {
            LevelInitialization.Instance.ExitCurrentLevel();
        }

        // 2. UI 切换：打开主菜单(1)，关闭其他
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenMainMenuUI();
        }

        // 3. 播放主界面音乐
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio("Start", 1f, true, false, false);
        }
    }

    /// <summary>
    /// 退出游戏并清理全局资源
    /// </summary>
    public void QuitGame()
    {
        // 1. 清理对象池 (视觉)
        if (StaticProperties.Instance != null && StaticProperties.Instance.cubeObjectPool != null)
        {
            StaticProperties.Instance.cubeObjectPool.Dispose();
        }

        // 2. 清理静态全局数据 (包含评估系统)
        if (StaticProperties.Instance != null)
        {
            StaticProperties.Instance.Dispose();
        }

        // 3. 销毁关卡管理器实例
        if (LevelInitialization.Instance != null)
        {
            LevelInitialization.Instance.Shutdown();
        }

        // 4. 退出应用程序
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void Update()
    {
        
    }
}
