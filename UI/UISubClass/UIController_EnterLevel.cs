using UnityEngine;
using System.Collections.Generic;

public class UIController_EnterLevel : UIController
{
    private LevelButtonConfig levelButtonConfig;
    private List<UIBase_EnterLevel> levelButtons = new List<UIBase_EnterLevel>();

    // 注意：UIManager 现在会通过 Activator.CreateInstance(type, gameObject) 调用此构造函数
    public UIController_EnterLevel(GameObject _gameObject) : base(_gameObject)
    {
        LoadLevelConfig();
        GenerateLevelButtons();
    }

    private void LoadLevelConfig()
    {
        // 从资源配置中获取 LevelConfig 路径
        string path = ResourceConfigManager.GetPath("Levels_ButtonConfig", "Json/UIJson");
        levelButtonConfig = Resources.Load<LevelButtonConfig>(path);
        if (levelButtonConfig == null)
        {
            Debug.LogError($"UIController_EnterLevel: 无法加载 LevelConfig，路径: {path}");
        }
    }

    private void GenerateLevelButtons()
    {
        if (levelButtonConfig == null) return;

        // 清理旧按钮 (如果不是对象池管理)
        // 注意：如果是在 Editor 模式下多次运行，可能需要清理
        // 但通常 Instantiate 是运行时的，不会持久化
        
        for (int i = 0; i < levelButtonConfig.levelCount; i++)
        {
            if (levelButtonConfig.levelButtonPrefab == null)
            {
                Debug.LogError("UIController_EnterLevel: LevelButtonPrefab 未配置！");
                break;
            }

            GameObject btnObj = Object.Instantiate(levelButtonConfig.levelButtonPrefab, gameObject.transform);
            btnObj.name = $"LevelButton_{i+1}";
            // 确保激活
            btnObj.SetActive(true);

            // 为每个按钮创建 UIBase_EnterLevel 控制器实例
            UIBase_EnterLevel btnController = new UIBase_EnterLevel(btnObj);
            btnController.Binding(i + 1, levelButtonConfig);
            
            levelButtons.Add(btnController);
        }
    }

    public override void Pause()
    {
        base.Pause();
        for(int i =0; i < levelButtons.Count; i++)
        {
            levelButtons[i].Pause();
        }
    }

    public override void Resume()
    {
        base.Resume();
    }
}
