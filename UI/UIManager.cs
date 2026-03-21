using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System;
using UnityEngine.UI;

public class UIUnit
{
    public int uiId {private set;get;}

    public GameObject uiGameObject {private set;get;}

    public UIController uiController {private set;get;}

    public UIUnit(int uiId,GameObject uiGameObject,UIController uiController)
    {
        this.uiId = uiId;
        this.uiGameObject = uiGameObject;
        this.uiController = uiController;
    }
}

public class UIManager
{
    public static UIManager Instance {get;private set;}
    
    private string uiConfigJson = "Json/UIJson";
    private string uisConfig_Key = "UIs_Config";
    private string uiConfig_Key = "UI_Config";
    private string uisConfig_Path = "";
    private string uiConfig_Path = "";

    private UIsConfig uisConfig;
    private UIConfig uiConfig;
    //ui全集
    public UIUnit[] uiUnits{private set;get;}

    private List<GameObject> uiParents = new List<GameObject>();

    public UIManager()
    {
        if(Instance == null)
        {
            Debug.Log("开始初始化UIManager");
            Instance = this;
        }
        else
        {
            Debug.LogError("UIManager已存在");
        }
        
    }

    public void Initialize()
    {
        if(StaticProperties.Instance == null)
        {
            Debug.LogError("全局静态未初始化");
            return;
        }
        Debug.Log("全局静态已初始化");
        ResourceInitialize();

    }

    private class UIInitData
    {
        public int id;
        public GameObject uiGameObject;
        public UIConfig uiConfig;
        public UIController uiController;
    }

    private void ResourceInitialize()
    {
        
        Debug.Log("开始初始化UI资源");
        uisConfig_Path=ResourceConfigManager.GetPath(uisConfig_Key,uiConfigJson);
        if(string.IsNullOrEmpty(uisConfig_Path))
            Debug.LogError("UIsConfig_key未配置或配置错误");
        uiConfig_Path=ResourceConfigManager.GetPath(uiConfig_Key,uiConfigJson);
        if(string.IsNullOrEmpty(uiConfig_Path))
            Debug.LogError("UIConfig_key未配置或配置错误");
        
        uisConfig = Resources.Load<UIsConfig>(uisConfig_Path);
        if(uisConfig == null)
        {
            Debug.LogError($"UIManager: 无法加载 UIsConfig，路径: {uisConfig_Path}");
            return;
        }

        int count = uisConfig.uiGameObjectCount;
        uiUnits = new UIUnit[count];

        List<UIInitData> uiInitDataList = new List<UIInitData>();

        for(int i = 1; i <= count; i++)
        {
            string prefabPath = $"Prefabs/UI/UIGameObject_{i}";
            string configPath =  $"SO/UIConfig_{i}" ;

            GameObject uiGameObject = UnityEngine.Object.Instantiate( Resources.Load<GameObject>(prefabPath));
            Resources.UnloadUnusedAssets();
            if(uiGameObject == null)
            {
                Debug.LogError($"UIManager: 无法加载 UI GameObject，路径: {prefabPath}");
                continue;
            }

            UIConfig uiConfig = Resources.Load<UIConfig>(configPath);
            if(uiConfig == null)
            {
                Debug.LogError($"UIManager: 无法加载 UI Config, 路径: {configPath}");
                return;
            }

            UIController uiController = GetUIController(uiConfig, uiGameObject);
            
            if(!uiConfig.isActive)
            {
                uiGameObject.SetActive(false);
            }
            
            uiInitDataList.Add(new UIInitData()
            {
                id = i,
                uiGameObject = uiGameObject,
                uiConfig = uiConfig,
                uiController = uiController
            });
        }

        // 根据 parentPriority 进行分组
        var parentDict = new Dictionary<int, GameObject>();
        var parentPriorityList = new List<int>();

        foreach (var data in uiInitDataList)
        {
            if (!parentPriorityList.Contains(data.uiConfig.parentPriority))
            {
                parentPriorityList.Add(data.uiConfig.parentPriority);
            }
        }

        // 按 parentPriority 排序 (越小越靠前)
        parentPriorityList.Sort();

        foreach (var pp in parentPriorityList)
        {
            // 实例化不同的 uiparent
            GameObject parentObj = UnityEngine.Object.Instantiate(uisConfig.uiParent);
            parentObj.name = $"UIParent_Priority_{pp}";
            
            // 更改它的 sortingOrder (假设是 Canvas 组件)
            Canvas canvas = parentObj.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = pp;
            }

            parentDict[pp] = parentObj;
            uiParents.Add(parentObj);
        }

        // 重新对每个 parentPriority 内的 uiGameObject 进行 priority 排序并设置父物体
        foreach (var pp in parentPriorityList)
        {
            var group = uiInitDataList.FindAll(x => x.uiConfig.parentPriority == pp);
            group.Sort((a, b) => a.uiConfig.priority.CompareTo(b.uiConfig.priority));

            foreach (var data in group)
            {
                data.uiGameObject.transform.SetParent(parentDict[pp].transform, false);
                data.uiGameObject.transform.SetAsLastSibling();
            }
        }

        // 赋值到 uiUnits 数组并绑定按钮
        foreach (var data in uiInitDataList)
        {
            uiUnits[data.id - 1] = new UIUnit(data.id, data.uiGameObject, data.uiController);
            ButtonBind(data.uiConfig, data.id, data.uiGameObject);
        }
        
        Debug.Log("UIManager初始化完成");

    }

    public void PauseAll()
    {
        if (uiUnits == null) return;
        foreach (var unit in uiUnits)
        {
            if (unit != null && unit.uiController != null)
            {
                unit.uiController.Pause();
            }
        }
    }

    public void ResumeAll()
    {
        if (uiUnits == null) return;
        foreach (var unit in uiUnits)
        {
            if (unit != null && unit.uiController != null)
            {
                unit.uiController.Resume();
            }
        }
    }

    public void Destroy()
    {
        if(uiParents != null)
        {
            foreach(var p in uiParents)
            {
                if(p != null) UnityEngine.Object.Destroy(p);
            }
            uiParents.Clear();
        }
        for(int i=0;i<uiUnits.Length;i++)
        {
            if(uiUnits[i] != null)
            {
                uiUnits[i].uiController.Destory();
                uiUnits[i] = null;
            }
        }
        uiEventsList.Clear();
    }
    //反射
    private UIController GetUIController(UIConfig uiConfig, GameObject uiGameObject)
    {
        if(!uiConfig.isCustomUI)
        {
            return new UIController(uiGameObject);
        }
        else
        {
           
            Type uiType = Type.GetType(uiConfig.customUIClass);
            if (uiType == null)
            {
                uiType = Assembly.GetExecutingAssembly().GetType(uiConfig.customUIClass);
            }

            if(uiType == null)
            {
                Debug.LogError($"UIManager: 无法加载自定义UI Controller，类名: {uiConfig.customUIClass}");
                return new UIController(uiGameObject);
            }
            
            ConstructorInfo ctor = uiType.GetConstructor(new Type[] { typeof(GameObject) });
            if (ctor != null)
            {
                return (UIController)ctor.Invoke(new object[] { uiGameObject });
            }
            else
            {
                try
                {
                    return (UIController)Activator.CreateInstance(uiType);
                }
                catch (Exception e)
                {
                    Debug.LogError($"UIManager: 无法实例化 {uiType.Name}。既没有找到带 GameObject 参数的构造函数，默认构造函数也失败: {e.Message}");
                    return new UIController(uiGameObject);
                }
            }
        }
    }

    private void ButtonBind(UIConfig uiConfig,int id,GameObject gameObject)
    {
        Button open =gameObject.transform.Find("Open")?.GetComponent<Button>();
        Button close =gameObject.transform.Find("Close")?.GetComponent<Button>();
        if(open != null)
        {
            open.onClick.AddListener(()=>
            {
                OpenUI(uiConfig.openUIID);
            });
        }
        if(close != null)
        {
            close.onClick.AddListener(()=>
            {
                CloseUI(uiConfig.closeUIID);
            });
        }

        if(uiConfig.openUIID != 0&&uiConfig.openUIID == id)
        {
            Debug.Log($"UI_{id}没有绑定open");
            return;
        }
        if(uiConfig.closeUIID != 0&&uiConfig.closeUIID == id)
        {
            Debug.Log($"UI_{id}没有绑定close");
            return;
        }
    }
    public void OpenMainMenuUI()
    {
        // 关闭其他 UI
        CloseUI(2); // 游戏界面      
        CloseUI(4); // 结算界面
        CloseUI(6); // 游戏评估界面
        
        // 打开主菜单
        OpenUI(1);
        OpenUI(3); // 关卡选择
    }

    public void OpenGameUI()
    {
        // 关闭其他 UI
        CloseUI(1); // 主菜单
        CloseUI(3); // 关卡选择
        CloseUI(4); // 结算界面
        
        // 打开游戏界面
        OpenUI(2); // 游戏主界面
        OpenUI(6); // 游戏评估界面 (分数、星级等)
    }

    public async void OpenUI(int id)
    {
        if(id <= 0 || id > uisConfig.uiGameObjectCount)
        {
            Debug.LogError($"UIManager: 无法打开 UI，ID: {id}");
            return;
        }
        
        // 延迟一小段时间，确保按钮动画（如果有）能够执行完毕
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.15f));

        if (uiUnits[id-1] != null && uiUnits[id-1].uiController != null)
        {
             uiUnits[id-1].uiController.Enter();
        }
        else if (uiUnits[id-1] != null && uiUnits[id-1].uiGameObject != null)
        {
             uiUnits[id-1].uiGameObject.SetActive(true);
        }
    }

    public async void CloseUI(int id)
    {
        if(id <= 0 || id > uisConfig.uiGameObjectCount)
        {
            Debug.LogError($"UIManager: 无法关闭 UI，ID: {id}");
            return;
        }
        
        // 延迟一小段时间，确保按钮动画（如果有）能够执行完毕
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.15f));

        if (uiUnits[id-1] != null && uiUnits[id-1].uiController != null)
        {
             uiUnits[id-1].uiController.Exit();
        }
        else if (uiUnits[id-1] != null && uiUnits[id-1].uiGameObject != null)
        {
             uiUnits[id-1].uiGameObject.SetActive(false);
        }
    }
    public Dictionary<int, Delegate> uiEventsList = new Dictionary<int, Delegate>();

    public void EventBind<T>(int id, Action<T> action) where T : UIEventParameter
    {
        try
        {
            if (uiEventsList.ContainsKey(id - 1))
            {
                // 如果已经有绑定的事件，可以叠加（使用 Delegate.Combine）
                uiEventsList[id - 1] = Delegate.Combine(uiEventsList[id - 1], action);
            }
            else
            {
                uiEventsList.Add(id - 1, action);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UIManager: 无法绑定事件，ID: {id}，错误: {e.Message}");
        }
    }

    public void EventTrigger<T>(int id, T uiEventParameter) where T : UIEventParameter
    {
        try
        {
            if (uiEventsList.ContainsKey(id - 1))
            {
                if (uiEventsList[id - 1] is Action<T> action)
                {
                    action.Invoke(uiEventParameter);
                }
                else
                {
                    Debug.LogWarning($"UIManager: 事件类型不匹配，ID: {id}，期望: {typeof(Action<T>)}，实际: {uiEventsList[id - 1].GetType()}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UIManager: 触发事件失败，ID: {id}，错误: {e.Message}");
        }
    }
}
public class UIEventParameter
{
    public int intParam;
    public bool boolParam;
    public float floatParam;
    public string strParam;
    public object objParam;
    
}

public class ScoreUpdateEventParam : UIEventParameter
{
    public int currentScore;
    public int maxScore; // 三星的分数，用于进度条
    public int[] starScores; // 三个星级的分数要求
}

public class BoardUpdateEventParam : UIEventParameter
{
    public int levelId;
}