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

    private GameObject uiParent;

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
        uiParent = UnityEngine.Object.Instantiate(uisConfig.uiParent);

        int count = uisConfig.uiGameObjectCount;
        uiUnits = new UIUnit[count];

        for(int i = 1; i <= count; i++)
        {
            GameObject uiGameObject = null;
            UIConfig uiConfig = null;

            string prefabPath = $"Prefabs/UI/UIGameObject_{i}";
;
                
            string configPath =  $"SO/UIConfig_{i}" ;

            uiGameObject = UnityEngine.Object.Instantiate( Resources.Load<GameObject>(prefabPath));
            Resources.UnloadUnusedAssets();
            if(uiGameObject == null)
            {
                Debug.LogError($"UIManager: 无法加载 UI GameObject，路径: {prefabPath}");
                continue;
            }

            uiGameObject.transform.SetParent(uiParent.transform);
            uiConfig = Resources.Load<UIConfig>(configPath);
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
            
            uiUnits[i-1] = new UIUnit(i,uiGameObject,uiController);
        }
        
        // 绑定按钮
        for(int i = 0; i < count; i++)
        {
            if (uiUnits[i] != null)
            {
                string configPath =$"Json/UIJson/UIConfig_{i}" ;
                
                UIConfig currentConfig = Resources.Load<UIConfig>(configPath);
                
                if (currentConfig != null)
                {
                    ButtonBind(currentConfig, i+1, uiUnits[i].uiGameObject);
                }
            }
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
        if(uiParent != null)
        {
            UnityEngine.Object.Destroy(uiParent);
            uiParent = null;
        }
        for(int i=0;i<uiUnits.Length;i++)
        {
            if(uiUnits[i] != null)
            {
                uiUnits[i].uiController.Destory();
                uiUnits[i] = null;
            }
        }
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
                // Fallback: Try finding type by name in current assembly if namespace is not provided
                uiType = Assembly.GetExecutingAssembly().GetType(uiConfig.customUIClass);
            }

            if(uiType == null)
            {
                Debug.LogError($"UIManager: 无法加载自定义UI Controller，类名: {uiConfig.customUIClass}");
                return new UIController(uiGameObject); // Return base controller instead of null to prevent crash
            }
            
            // Try to find a constructor that takes GameObject
            ConstructorInfo ctor = uiType.GetConstructor(new Type[] { typeof(GameObject) });
            if (ctor != null)
            {
                return (UIController)ctor.Invoke(new object[] { uiGameObject });
            }
            else
            {
                // If no matching constructor found, try default constructor
                try
                {
                    return (UIController)Activator.CreateInstance(uiType);
                }
                catch (Exception e)
                {
                    Debug.LogError($"UIManager: 无法实例化 {uiType.Name}。既没有找到带 GameObject 参数的构造函数，默认构造函数也失败: {e.Message}");
                    // Fallback to base controller
                    return new UIController(uiGameObject);
                }
            }
        }
    }

    private void ButtonBind(UIConfig uiConfig,int id,GameObject gameObject)
    {
        Button open =gameObject.transform.Find("Open").GetComponent<Button>();
        Button close =gameObject.transform.Find("Close").GetComponent<Button>();
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
    public void OpenUI(int id)
    {
        if(id <= 0 || id > uisConfig.uiGameObjectCount)
        {
            Debug.LogError($"UIManager: 无法打开 UI，ID: {id}");
            return;
        }
        
        if (uiUnits[id-1] != null && uiUnits[id-1].uiController != null)
        {
             uiUnits[id-1].uiController.Enter();
        }
        else if (uiUnits[id-1] != null && uiUnits[id-1].uiGameObject != null)
        {
             uiUnits[id-1].uiGameObject.SetActive(true);
        }
    }

    public void CloseUI(int id)
    {
        if(id <= 0 || id > uisConfig.uiGameObjectCount)
        {
            Debug.LogError($"UIManager: 无法关闭 UI，ID: {id}");
            return;
        }
        
        if (uiUnits[id-1] != null && uiUnits[id-1].uiController != null)
        {
             uiUnits[id-1].uiController.Exit();
        }
        else if (uiUnits[id-1] != null && uiUnits[id-1].uiGameObject != null)
        {
             uiUnits[id-1].uiGameObject.SetActive(false);
        }
    }
}