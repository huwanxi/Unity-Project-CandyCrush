using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIBase_EnterLevel : UIBase
{
    private LevelButtonConfig levelConfig;
    private int levelIndex;

    public UIBase_EnterLevel(GameObject _gameObject) : base(_gameObject)
    {
        // 这里的 _gameObject 是单个关卡按钮
    }

    public void Binding(int index, LevelButtonConfig config)
    {
        levelIndex = index;
        levelConfig = config;

        // 设置显示的关卡数字
        var text = gameObject.GetComponentInChildren<Text>();
        if (text != null) text.text = levelIndex.ToString();

        // 绑定点击事件
        var button = gameObject.GetComponent<Button>();
        if (button != null)
        {
            // 移除旧的监听器 (防止重复绑定，如果对象池复用)
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnLevelButtonClick);
        }
    }

    public override void Pause()
    {
        base.Pause();
        var button = gameObject.GetComponent<Button>();
        if (button != null) button.interactable = false;
    }

    public override void Resume()
    {
        base.Resume();
        var button = gameObject.GetComponent<Button>();
        if (button != null) button.interactable = true;
    }

    private void OnLevelButtonClick()
    {
        if (levelConfig == null)
        {
             Debug.LogError("UIBase_EnterLevel: LevelConfig 未设置！");
             return;
        }

        string levelPath = $"{levelConfig.levelDataPathPrefix}{levelIndex}"; // e.g., SO/LevelData_1
        string evalPath = $"{levelConfig.evaluationDataPathPrefix}{levelIndex}"; // e.g., SO/EvaluationData_1
        
        Debug.Log($"UIBase_EnterLevel: 请求进入关卡 {levelIndex}");

        // 调用 CubeManager 的 Start API (如果实例存在)
        if (CubeManager.Instance != null)
        {
            CubeManager.Instance.StartGame(levelPath, evalPath).Forget();
        }
        else if (LevelInitialization.Instance != null)
        {
            // 如果 CubeManager 未初始化，直接调用 LevelInitialization
            LevelInitialization.Instance.StartGame(levelPath, evalPath).Forget();
        }
        else
        {
            Debug.LogError("UIBase_EnterLevel: 无法开始游戏，Initialization 模块未就绪。");
        }
    }
}
