using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class LevelInitialization
{
    public static LevelInitialization Instance { get; private set; }

    // TODO: 请根据实际配置修改 Win/Lose UI 的 ID
    private const int WIN_UI_ID = 4; 
    private const int LOSE_UI_ID = 4;

    public LevelInitialization()
    {
        if(Instance!=null)
        {
            Debug.LogError("LevelInitialization: 实例已存在！");
            return;
        }
        Instance = this;
    }

    // 1. 退出当前关卡并清理资源（不处理 UI，UI 由 Initialization 控制）
    public void ExitCurrentLevel()
    {
        Debug.Log("LevelInitialization: 退出当前关卡，清理资源...");
        
        // 清理可能存在的游戏状态
        if (CubeManager.Instance != null)
        {
            CubeManager.Instance.ClearBoard();
        }
        
        
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            // 解绑事件
            StaticProperties.Instance.evaluationManager.OnGameWin -= OnGameWin;
            StaticProperties.Instance.evaluationManager.OnGameLose -= OnGameLose;
            StaticProperties.Instance.evaluationManager.StopGame();
        }

        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.StopGame();
        }
    }

    // 2. 点击游戏关卡按钮需要执行的游戏资源初始化和ui资源初始化函数
    // 检测有哪些资源欠缺需要初始化
    // levelPath: 关卡数据路径 ("SO/Level_One")
    // evaluationPath: 评估数据路径 ("SO/Evaluation_One")
    public async UniTask LoadLevel(string levelPath, string evaluationPath)
    {
        Debug.Log($"LevelInitialization: 加载关卡资源 Level:{levelPath}, Evaluation:{evaluationPath}...");

        if (StaticProperties.Instance == null)
        {
            Debug.LogError("LevelInitialization: StaticProperties 未初始化！");
            return;
        }

        // --- 1. 加载 LevelData ---
        LevelData levelData = Resources.Load<LevelData>(levelPath);
        if (levelData == null)
        {
            Debug.LogError($"LevelInitialization: 无法加载 LevelData，路径: {levelPath}");
            return;
        }

        // 释放旧的 LevelData 资源
        if (StaticProperties.Instance.levelData != null && StaticProperties.Instance.levelData != levelData)
        {
            Resources.UnloadAsset(StaticProperties.Instance.levelData);
            Debug.Log("LevelInitialization: 已卸载旧的 LevelData 资源。");
        }
        StaticProperties.Instance.levelData = levelData;

        // --- 1.5 准备方块资源 (基于新的 LevelData) ---
        await new ResourceInitialization().PrepareCubeAssets(levelData);

        // --- 2. 加载 EvaluationData ---
        EvaluationData evalData = Resources.Load<EvaluationData>(evaluationPath);
        if (evalData == null)
        {
             Debug.LogError($"LevelInitialization: 无法加载 EvaluationData，路径: {evaluationPath}");
        }

        // 释放旧的 EvaluationData 资源
        if (StaticProperties.Instance.evaluationData != null && StaticProperties.Instance.evaluationData != evalData)
        {
            Resources.UnloadAsset(StaticProperties.Instance.evaluationData);
            Debug.Log("LevelInitialization: 已卸载旧的 EvaluationData 资源。");
        }
        StaticProperties.Instance.evaluationData = evalData;

        // --- 3. 初始化 EvaluationManager ---
        if (StaticProperties.Instance.evaluationManager == null)
        {
            var evalManager = new EvaluationManager();
            evalManager.Init(evalData);
            StaticProperties.Instance.evaluationManager = evalManager;
        }
        else
        {
             // 重新初始化数据
             StaticProperties.Instance.evaluationManager.Init(evalData);
        }

        // 绑定胜负事件
        StaticProperties.Instance.evaluationManager.OnGameWin -= OnGameWin;
        StaticProperties.Instance.evaluationManager.OnGameWin += OnGameWin;
        StaticProperties.Instance.evaluationManager.OnGameLose -= OnGameLose;
        StaticProperties.Instance.evaluationManager.OnGameLose += OnGameLose;

        // --- 4. 初始化 CubeManager ---
        var (posMap, activeGrid) = CalculateGridPositions(levelData);
        new CubeManager(levelData.width, levelData.height, posMap, activeGrid);//构造函数instance引用避免gc
        
        Debug.Log("LevelInitialization: 关卡资源加载完成。");
    }

    private void OnGameWin()
    {
        Debug.Log("LevelInitialization: 游戏胜利！");

        // 1. 禁止输入 (InputManager)
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.StopGame();
        }
        
        // 获取分数
        int score = 0;
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            score = StaticProperties.Instance.evaluationManager.CurrentScore;
            StaticProperties.Instance.evaluationManager.StopGame();
        }

        // 2. 显示胜利 UI (UIManager)
        if (UIManager.Instance != null)
        {
            // 尝试传递数据给 WinLose Controller
            if (UIManager.Instance.uiUnits != null && UIManager.Instance.uiUnits.Length >= WIN_UI_ID)
            {
                var unit = UIManager.Instance.uiUnits[WIN_UI_ID - 1];
                if (unit != null && unit.uiController is UIController_WinLose winLoseCtrl)
                {
                    winLoseCtrl.ShowResult(true, score);
                }
            }
            UIManager.Instance.OpenUI(WIN_UI_ID);
        }
    }

    private void OnGameLose()
    {
        Debug.Log("LevelInitialization: 游戏失败！");

        // 1. 禁止输入 (InputManager)
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.StopGame();
        }
        
        // 获取分数
        int score = 0;
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            score = StaticProperties.Instance.evaluationManager.CurrentScore;
            StaticProperties.Instance.evaluationManager.StopGame();
        }

        // 2. 显示失败 UI (UIManager)
        if (UIManager.Instance != null) 
        {
             // 尝试传递数据给 WinLose Controller
            if (UIManager.Instance.uiUnits != null && UIManager.Instance.uiUnits.Length >= LOSE_UI_ID)
            {
                var unit = UIManager.Instance.uiUnits[LOSE_UI_ID - 1];
                if (unit != null && unit.uiController is UIController_WinLose winLoseCtrl)
                {
                    winLoseCtrl.ShowResult(false, score);
                }
            }
            UIManager.Instance.OpenUI(LOSE_UI_ID);
        }
    }

    // 3. 开始游戏函数，包含方块下落和ui显性这些功能
    public async UniTask StartGame(string levelPath, string evaluationPath)
    {
        Debug.Log("LevelInitialization: 开始游戏...");
        await LoadLevel(levelPath, evaluationPath);

        // UI 处理：先打开游戏界面(2)，关闭其他，确保玩家能看到背景
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseUI(1); // 主菜单
            UIManager.Instance.CloseUI(3); // 关卡选择
            UIManager.Instance.CloseUI(WIN_UI_ID); // 确保结算关闭
            UIManager.Instance.OpenUI(2);  // 游戏界面
        }

        // 播放游戏音乐
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio("Game", 1f, true, false, false);
        }

        //生成棋盘 (包含方块下落动画)
        if (CubeManager.Instance != null)
        {
            await CubeManager.Instance.InitBoardAsync();
        }
        
        // 启动评估系统
        if (StaticProperties.Instance.evaluationManager != null)
        {
            StaticProperties.Instance.evaluationManager.StartGame();
        }

        // 启动输入系统
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.StartGame();
            Debug.Log("LevelInitialization: 输入系统已启动。");
        }
        else
        {
            Debug.LogError("LevelInitialization: GameInputManager.Instance 为空，无法启动输入系统！");
        }
    }

    // 4. 重新开始，调用ui对应模块和cube对应模块的重新开始函数
    public async UniTask RestartGame()
    {
        Debug.Log("LevelInitialization: 重新开始游戏...");

        // 重置评估系统
        if (StaticProperties.Instance.evaluationManager != null)
        {
            StaticProperties.Instance.evaluationManager.ResetGame();
            StaticProperties.Instance.evaluationManager.StartGame(); // 确保它是运行状态
        }

        // 重置棋盘
        if (CubeManager.Instance != null)
        {
            // 使用 InitBoardAsync (内部已优化复用逻辑)
            await CubeManager.Instance.InitBoardAsync();
        }
        
        // TODO: 通知 UI 重置
    }

    // 5. 暂停游戏
    public void PauseGame(bool pause)
    {
        Debug.Log($"LevelInitialization: 暂停游戏 {pause}");
        
        // 1. 处理评估系统 (暂停倒计时等)
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            StaticProperties.Instance.evaluationManager.PauseGame(pause);
        }

        if (pause)
        {
            Time.timeScale = 0;
            if (GameInputManager.Instance != null) GameInputManager.Instance.StopGame();
            // TODO: UI 显示暂停菜单
        }
        else
        {
            Time.timeScale = 1;
            if (GameInputManager.Instance != null) GameInputManager.Instance.StartGame();
            // TODO: UI 隐藏暂停菜单
        }
    }

    // 6. 退出游戏，清理所有资源并关闭程序
    public void ExitGame()
    {
        Debug.Log("LevelInitialization: 退出游戏并清理资源...");
        
        // 解绑事件
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            StaticProperties.Instance.evaluationManager.OnGameWin -= OnGameWin;
            StaticProperties.Instance.evaluationManager.OnGameLose -= OnGameLose;
        }

        // 1. 停止并清理输入管理器
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.Dispose();
        }

        // 2. 清理方块管理器 (逻辑)
        if (CubeManager.Instance != null)
        {
            CubeManager.Instance.Dispose();
        }
        
        // 3. 退出应用 (交由 Initialization 处理)
        if (Initialization.Instance != null)
        {
            Initialization.Instance.QuitGame();
        }
    }
    
    // 辅助方法：计算网格位置
    private (Vector2[,], bool[,]) CalculateGridPositions(LevelData levelData)
    {
        int width = levelData.width;
        int height = levelData.height;
        Vector2[,] posMap = new Vector2[width, height];
        bool[,] activeGrid = new bool[width, height];

        // 使用 LevelData 中的配置计算位置
        // 注意：根据 LevelData 的 Tooltip，startPosition 被定义为“左上角”
        // 而逻辑坐标 y=0 是底部（重力向下），y=height-1 是顶部
        // 因此需要将 visual position 从 startPosition 向下延伸
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // X轴：从左向右
                float posX = levelData.startPosition.x + x * levelData.gridSpacing.x;
                
                // Y轴：从 startPosition (Top) 向下计算，对应的逻辑 y 是从 Bottom (0) 到 Top (height-1)
                // 当 y = height - 1 (Top) 时，应该是 startPosition.y
                // 当 y = 0 (Bottom) 时，应该是 startPosition.y - (height - 1) * spacing.y
                float posY = levelData.startPosition.y - (height - 1 - y) * levelData.gridSpacing.y;

                posMap[x, y] = new Vector2(posX, posY);
                
                // 使用 LevelData 中封装的 IsActive 方法获取格子激活状态
                activeGrid[x, y] = levelData.IsActive(x, y);
            }
        }

        return (posMap, activeGrid);
    }
    
    public void Shutdown()
    {
         // 额外的清理逻辑
         Instance = null;
    }
}
