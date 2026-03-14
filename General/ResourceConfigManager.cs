using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class ResourceConfigManager
{
    // JSON配置文件的目录相对路径 (Resources下的相对路径)
    private const string RELATIVE_DIR = "Json/LevelJson";

    [System.Serializable]
    public class ResourceItem
    {
        public string key;  // 资源名称/键
        public string path; // Resources下的相对路径
    }

    [System.Serializable]
    public class ResourceConfig
    {
        public string description;
        public string rootPath;
        public List<ResourceItem> resources;
    }

    public static ResourceConfig config;
    private static Dictionary<string, string> pathDict;

    /// <summary>
    /// 加载并解析 Resources/Json 目录下的第一个JSON配置
    /// </summary>
    public static void LoadConfig(string resourcePath)
    {
        // 加载 Resources/Json 目录下的所有 TextAsset
        // 注意：Json 文件在 Resources 中被识别为 TextAsset
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(resourcePath);
        
        if (textAssets != null && textAssets.Length > 0)
        {
            // 取第一个
            TextAsset configAsset = textAssets[0];
            try 
            {
                Debug.Log($"ResourceConfigManager: 正在加载配置文件: {configAsset.name}");
                config = JsonUtility.FromJson<ResourceConfig>(configAsset.text);
                
                InitializeDictionary();
                Debug.Log($"ResourceConfigManager: 成功加载配置，共 {pathDict.Count} 个路径。");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ResourceConfigManager: 解析配置失败 - {e.Message}");
            }
        }
        else
        {
             Debug.LogError($"ResourceConfigManager: 在 Resources/Json 下未找到任何配置文件。");
        }
    }

    /// <summary>
    /// 将List转换为Dictionary以提高查询效率
    /// </summary>
    private static void InitializeDictionary()
    {
        pathDict = new Dictionary<string, string>();
        if (config != null && config.resources != null)
        {
            foreach (var item in config.resources)
            {
                if (!string.IsNullOrEmpty(item.key) && !pathDict.ContainsKey(item.key))
                {
                    pathDict.Add(item.key, item.path);
                }
            }
        }
    }

    /// <summary>
    /// 获取资源路径
    /// </summary>
    public static string GetPath(string key,string resourcePath = RELATIVE_DIR)
    {
        if (pathDict != null && pathDict.ContainsKey(key))
        {
            return pathDict[key];
        }
        else
        {
            LoadConfig(resourcePath);
        }
        if(pathDict.ContainsKey(key))
            return pathDict[key];

        Debug.LogWarning($"ResourceConfigManager: 未找到Key为 '{key}' 的资源路径");
        return null;
    }
}