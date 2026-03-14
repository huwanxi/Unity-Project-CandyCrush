using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class ResourceInitialization
{
    // Configuration Keys (Customizable)
    public string LevelDataKey = "Level_One_Data";
    public string EvaluationDataKey = "Evaluation_Data";
    
    public string CubeBasePrefabKey = "Cube_Base_Prefab";
    
    public string CubeSpriteKeyPrefix = "Cube_Type_";
    public string CubeSpriteKeySuffix = "_Sprite";
    
    public string CubeAnimatorKeyPrefix = "Cube_Type_";
    public string CubeAnimatorKeySuffix = "_Animator";

    public async UniTask Initialize()
    {
        Debug.Log("ResourceInitialization: 开始加载资源...");

        if (StaticProperties.Instance == null)
        {
            Debug.LogError("ResourceInitialization: StaticProperties 未初始化！");
            return;
        }

        // 1. 获取 LevelData
        string levelDataPath = ResourceConfigManager.GetPath(LevelDataKey);
        if (string.IsNullOrEmpty(levelDataPath)) 
        {
            levelDataPath = "SO/Level_One"; 
            Debug.LogWarning($"ResourceInitialization: 未在配置中找到路径，使用默认路径: {levelDataPath}");
        }
        
        LevelData levelData = Resources.Load<LevelData>(levelDataPath);
        if (levelData == null)
        {
            Debug.LogError($"ResourceInitialization: 无法加载 LevelData，路径: {levelDataPath}");
            return;
        }
        StaticProperties.Instance.levelData = levelData;

        // 2. 获取 EvaluationData
        string evalDataPath = ResourceConfigManager.GetPath(EvaluationDataKey);
        if (string.IsNullOrEmpty(evalDataPath)) evalDataPath = "SO/EvaluationData";
        EvaluationData evalData = Resources.Load<EvaluationData>(evalDataPath);
        
        if (evalData != null)
        {
             StaticProperties.Instance.evaluationData = evalData;
             // 创建管理器实例 (非 Mono)
             var evalManager = new EvaluationManager();
             evalManager.Init(evalData);
             StaticProperties.Instance.evaluationManager = evalManager;
             Debug.Log("ResourceInitialization: EvaluationManager 已配置。");
        }
        else
        {
             Debug.LogWarning($"ResourceInitialization: 无法加载 EvaluationData ({evalDataPath})");
        }

        // 3. 获取 CubeData (如果未在 Inspector 中赋值)
        if (StaticProperties.Instance.cubeData == null)
        {
             string cubeDataPath = ResourceConfigManager.GetPath("Cube_Data");
             if (string.IsNullOrEmpty(cubeDataPath)) cubeDataPath = "SO/CubeData";
             
             CubeData cubeData = Resources.Load<CubeData>(cubeDataPath);
             if (cubeData != null)
             {
                 StaticProperties.Instance.cubeData = cubeData;
                 Debug.Log("ResourceInitialization: CubeData 已加载。");
             }
             else
             {
                 Debug.LogError($"ResourceInitialization: 无法加载 CubeData ({cubeDataPath})");
                 // 严重错误，可能需要中止
             }
        }

        // 4. 准备方块资源
        await PrepareCubeAssets(levelData);

        Debug.Log("ResourceInitialization: 资源加载完成。");
    }

    public async UniTask PrepareCubeAssets(LevelData levelData)
    {
        // 检查 StaticProperties 是否已有缓存的 Cube 模板
        var cachedCubes = StaticProperties.Instance.cubeData.cube;
        List<GameObject> typePrefabs;

        // 如果已有缓存且数量足够，我们假设是复用 (这里假设同一类型 ID 在不同关卡表现一致)
        // 如果不同关卡同一 ID 表现不同，则需要强制刷新。这里暂定为复用策略。
        if (cachedCubes != null && cachedCubes.Length >= levelData.cubeTypesCount)
        {
            // 检查缓存的有效性 (防止已被销毁)
            bool allValid = true;
            foreach (var cube in cachedCubes)
            {
                if (cube == null)
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                Debug.Log($"ResourceInitialization: 复用已有的 {cachedCubes.Length} 个方块模板，无需重新生成。");
                // 如果需要裁剪多余的 (虽然保留也没事，只要生成的 Type 索引不超过范围)
                // 这里我们直接返回，使用现有的缓存
                return;
            }
        }

        Debug.Log("ResourceInitialization: 缓存不足或无效，准备生成/补充方块模板...");

        // 初始化列表：尝试复用现有的有效对象
        typePrefabs = new List<GameObject>();
        if (cachedCubes != null)
        {
            foreach (var cube in cachedCubes)
            {
                if (cube != null) typePrefabs.Add(cube);
            }
        }

        // 获取基础Prefab
        string prefabPath = ResourceConfigManager.GetPath(CubeBasePrefabKey);
        if (string.IsNullOrEmpty(prefabPath)) prefabPath = "Prefabs/Cube";
        
        GameObject basePrefab = Resources.Load<GameObject>(prefabPath);
        if (basePrefab == null)
        {
            Debug.LogError($"ResourceInitialization: 未找到基础Prefab，路径: {prefabPath}");
            return;
        }

        // 补齐所需的数量
        int currentCount = typePrefabs.Count;
        int targetCount = levelData.cubeTypesCount;

        for (int i = currentCount + 1; i <= targetCount; i++)
        {
            // 实例化一个作为模板 (在内存中)
            GameObject template = Object.Instantiate(basePrefab);
            template.name = $"Cube_Type_{i}";
            template.SetActive(false); // 保持隐藏，仅作为数据模板
            Object.DontDestroyOnLoad(template); // 防止场景切换销毁

            // 获取资源路径 (图片和动画)
            string spriteKey = $"{CubeSpriteKeyPrefix}{i}{CubeSpriteKeySuffix}";
            string animKey = $"{CubeAnimatorKeyPrefix}{i}{CubeAnimatorKeySuffix}";
            
            string spritePath = ResourceConfigManager.GetPath(spriteKey);
            if (string.IsNullOrEmpty(spritePath)) spritePath = $"Art/Gems/Type_{i}";

            string animPath = ResourceConfigManager.GetPath(animKey);
            if (string.IsNullOrEmpty(animPath)) animPath = $"Animations/Type_{i}";

            // 加载资源
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            RuntimeAnimatorController animator = Resources.Load<RuntimeAnimatorController>(animPath);

            // 配置组件
            // 1. SpriteRenderer (2D World)
            var sr = template.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                if (sprite != null) sr.sprite = sprite;
                else Debug.LogWarning($"ResourceInitialization: 未找到图片资源 Type_{i} ({spritePath})");
            }

            // 1.5 Image (UI) - 修复: 支持 UI Image 组件赋值
            var img = template.GetComponentInChildren<UnityEngine.UI.Image>();
            if (img != null)
            {
                if (sprite != null) img.sprite = sprite;
                else Debug.LogWarning($"ResourceInitialization: 未找到图片资源 Type_{i} ({spritePath})");
            }

            // 2. Animator
            var anim = template.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                if (animator != null) anim.runtimeAnimatorController = animator;
            }
            
            typePrefabs.Add(template);
        }

        // 注入到全局配置 StaticProperties 中
        if (StaticProperties.Instance.cubeData != null)
        {
            // 即使有更多缓存，也只将当前关卡需要的数量暴露给 cubeData.cube
            // 这样 Random.Range(0, cubeData.cube.Length) 就会是正确的范围
            // 不过为了保持缓存引用，我们最好还是保留所有缓存，但在外部使用 levelData.cubeTypesCount 来限制
            // 这里我们更新 cube 数组为所有可用缓存，以备后续关卡使用
            StaticProperties.Instance.cubeData.cube = typePrefabs.ToArray();
            Debug.Log($"ResourceInitialization: 已更新配置，当前共有 {typePrefabs.Count} 种方块类型 (目标需 {targetCount})。");
        }
        else
        {
            Debug.LogError("ResourceInitialization: CubeData 缺失，无法注入方块配置！");
        }
        
        await UniTask.Yield();
    }
}
