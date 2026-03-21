using UnityEngine;
using UnityEngine.UI;
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
        if (string.IsNullOrEmpty(path)) return;
        
        levelButtonConfig = Resources.Load<LevelButtonConfig>(path);
        if (levelButtonConfig == null)
        {
            Debug.LogError($"UIController_EnterLevel: 无法加载 LevelConfig，路径: {path}");
        }
    }

    private void GenerateLevelButtons()
    {
        if (levelButtonConfig == null) return;

        // 在当前 UI 预制体中查找 ScrollRect 的 Content 作为父节点
        ScrollRect scrollRect = gameObject.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect == null)
        {
            Debug.LogError("UIController_EnterLevel: 未在 UI 预制体中找到 ScrollRect 组件！");
            return;
        }

        Transform content = scrollRect.content;
        if (content == null)
        {
            Debug.LogError("UIController_EnterLevel: ScrollRect 的 Content 未绑定！");
            return;
        }
        
        for (int i = 0; i < levelButtonConfig.levelCount; i++)
        {
            if (levelButtonConfig.levelButtonPrefab == null)
            {
                Debug.LogError("UIController_EnterLevel: LevelButtonPrefab 未配置！");
                break;
            }

            // 动态生成按钮并放到 Content 下
            GameObject btnObj = Object.Instantiate(levelButtonConfig.levelButtonPrefab, content);
            btnObj.name = $"LevelButton_{i+1}";
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
        for(int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i].Pause();
        }
    }

    public override void Resume()
    {
        base.Resume();
        for(int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i].Resume();
        }
    }
}
