
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class CubeManager
{
    public static CubeManager Instance { get; private set; }   

    public System.Action<int, int> OnMatchesProcessed; //评估绑定事件
    public System.Action OnMoveUsed; // 步数消耗事件

    private CubeController[,] grid;
    private int width;
    private int height;
    private Vector2[,] posMap;
    private bool[,] activeGrid;
    
    private Vector2Int lastSwapPos1 = new Vector2Int(-1, -1);
    private Vector2Int lastSwapPos2 = new Vector2Int(-1, -1);

    private CubeController selectedCube;
    public class MatchGroup
    {
        public List<CubeController> cubes = new List<CubeController>();
        public BonusType bonusType = BonusType.None;
        public CubeController mergeTarget = null;
        
    }

    public CubeManager(int width, int height, Vector2[,] posMap, bool[,] activeGrid = null)
    {
        Instance = this;
        this.width = width;
        this.height = height;
        this.posMap = posMap;
        this.activeGrid = activeGrid;
        this.grid = new CubeController[width, height];
    }

    /// <summary>
    /// 开始指定关卡 (API供外部调用)
    /// </summary>
    /// <param name="levelPath">关卡数据路径</param>
    /// <param name="evaluationPath">评估数据路径</param>
    public async UniTask StartGame(string levelPath, string evaluationPath)
    {
        if (LevelInitialization.Instance != null)
        {
            await LevelInitialization.Instance.StartGame(levelPath, evaluationPath);
        }
        else
        {
            Debug.LogError("CubeManager: LevelInitialization 实例未找到，无法开始游戏。");
        }
    }

    /// <summary>
    /// 生成初始棋盘（支持重新开始时的对象复用）
    /// </summary>
    public async UniTask InitBoardAsync()
    {
        // 重置选中状态
        if (selectedCube != null)
        {
            selectedCube.SetFocus(false);
            selectedCube = null;
        }

        var levelData = StaticProperties.Instance.levelData;
        float dropHeight = levelData.dropHeight;
        float rowInterval = levelData.dropRowInterval;
        Vector2 startPos = levelData.startPosition;
        float spawnY = startPos.y + dropHeight;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (!IsValid(col, row)) continue;

                // 确定新的随机类型 (避免初始消除)
                int maxType = levelData.cubeTypesCount;
                if (maxType > StaticProperties.Instance.cubeData.cube.Length)
                    maxType = StaticProperties.Instance.cubeData.cube.Length;

                int type = Random.Range(0, maxType);
                while (IsMatchAt(col, row, type))
                {
                    type = Random.Range(0, maxType);
                }

                // 计算掉落起始位置
                Vector2 initPos = new Vector2(posMap[col, row].x, spawnY);

                // 检查当前方块是否可以复用
                CubeController current = grid[col, row];
                if (current != null && !current.IsDestroyed && current.Type == type)
                {
                    // 类型相同，复用！
                    current.Reset(initPos, col, row);
                }
                else
                {
                    // 类型不同，销毁旧的（如果有），创建新的
                    if (current != null)
                    {
                        current.Destory();
                        grid[col, row] = null;
                    }
                    
                    CubeController controller = CreateCube(posMap, type, initPos, col, row);
                    if (controller != null)
                    {
                        grid[col, row] = controller;
                    }
                }

                // 触发掉落动画
                if (grid[col, row] != null)
                {
                    grid[col, row].DropWithGravityAsync(col, row, -1).Forget();
                }
            }
            
            // 逐行掉落间隔
            await UniTask.Delay((int)(rowInterval * 1000));
        }
        
        // 等待所有动画大概结束
        await UniTask.Delay(500);

        // 启动输入
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.StartGame();
        }

        // 启动评估系统
        if (StaticProperties.Instance.evaluationManager != null)
        {
            StaticProperties.Instance.evaluationManager.StartGame();
        }
    }



    /// <summary>
    /// 清空棋盘上的所有方块
    /// </summary>
    public void ClearBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].Destory(); 
                    grid[x, y] = null;
                }
            }
        }
    }

    /// <summary>
    /// 检查某位置是否会构成消除（用于初始化生成）
    /// </summary>
    private bool IsMatchAt(int x, int y, int type)
    {
        if (x >= 2)
        {
            if (grid[x - 1, y] != null && grid[x - 1, y].Type == type &&
                grid[x - 2, y] != null && grid[x - 2, y].Type == type)
                return true;
        }
        if (y >= 2)
        {
            if (grid[x, y - 1] != null && grid[x, y - 1].Type == type &&
                grid[x, y - 2] != null && grid[x, y - 2].Type == type)
                return true;
        }
        return false;
    }

    public CubeController CreateCube(Vector2[,] posMap, int type, Vector2 initPos, int x, int y)
    {
        // 从对象池获取 GameObject
        GameObject cubeObj = null;
        if (StaticProperties.Instance != null && StaticProperties.Instance.cubeObjectPool != null)
        {
            cubeObj = StaticProperties.Instance.cubeObjectPool.GetCube(type);
        }
        
        if (cubeObj == null) return null;

        // 创建控制器
        CubeController controller = new CubeController(cubeObj, posMap, type);
        
        // 初始化控制器
        controller.Start(initPos, x, y);

        // 存入网格
        if (IsValid(x, y))
        {
            grid[x, y] = controller;
        }

        return controller;
    }

    public void DestroyCube(CubeController controller)
    {
        if (controller == null) return;
        controller.Destory();
    }

    private bool IsValid(int x, int y)
    {
        bool inBounds = x >= 0 && x < width && y >= 0 && y < height;
        if (!inBounds) return false;
        
        // 优先使用 LevelData 的实时配置，避免使用缓存的 activeGrid (可能已过时)
        if (StaticProperties.Instance != null && StaticProperties.Instance.levelData != null)
        {
            return StaticProperties.Instance.levelData.IsActive(x, y);
        }
        
        if (activeGrid != null)
        {
            return activeGrid[x, y];
        }
        return true;
    }

    /// <summary>
    /// 尝试交换两个方块
    /// </summary>
    public async UniTask TrySwapAsync(int x1, int y1, int x2, int y2)
    {
        if (!IsValid(x1, y1) || !IsValid(x2, y2)) return;

        // 注意：输入控制现在由 GameInputBinder 在调用此方法时负责处理
        // 这样可以保持 CubeManager 专注于逻辑，不直接控制输入系统状态

        try
        {
            lastSwapPos1 = new Vector2Int(x1, y1);
            lastSwapPos2 = new Vector2Int(x2, y2);

            // 1. 执行交换
            await SwapAsync(x1, y1, x2, y2);

            // 1.5 检查特殊交换 (Colorful 消除逻辑)
            bool isSpecialSwap = await ProcessSpecialSwapAsync(x1, y1, x2, y2);

            if (isSpecialSwap)
            {
                 // 步数消耗
                 OnMoveUsed?.Invoke();

                 // 特殊交换触发成功，直接进入后续填充流程
                 // 不需要回退
                 await ApplyGravityAsync();
                 await RefillAsync();
                 // 检查后续连锁
                 bool hasMatch = await ProcessMatchesAsync();
                 int safetyCounter = 0;
                 while (hasMatch && safetyCounter < 20)
                 {
                     await UniTask.Delay(100); 
                     hasMatch = await ProcessMatchesAsync();
                     safetyCounter++;
                 }
            }
            else
            {
                // 2. 检查是否有消除 (常规消除)
                bool hasMatch = await ProcessMatchesAsync();
                
                if (hasMatch)
                {
                    // 步数消耗
                    OnMoveUsed?.Invoke();

                    // 3. 有消除，进入自动消除循环
                    int safetyCounter = 0;
                    while (hasMatch && safetyCounter < 20)
                    {
                        await UniTask.Delay(100); // 稍微停顿
                        hasMatch = await ProcessMatchesAsync();
                        safetyCounter++;
                    }
                }
                else
                {
                    // 4. 没有消除，交换回来 (非法移动)
                    await SwapAsync(x1, y1, x2, y2);
                }
            }
        }
        finally
        {
             // 逻辑完成，返回控制权
             // 输入恢复由 GameInputBinder 负责
        }
    }

    /// <summary>
    /// 处理特殊交换（如 Colorful 与其他方块交换）
    /// 返回 true 表示发生了特殊消除
    /// </summary>
    private async UniTask<bool> ProcessSpecialSwapAsync(int x1, int y1, int x2, int y2)
    {
        CubeController c1 = grid[x1, y1];
        CubeController c2 = grid[x2, y2];

        if (c1 == null || c2 == null) return false;

        bool c1IsColorful = c1.BonusType == BonusType.Colorful;
        bool c2IsColorful = c2.BonusType == BonusType.Colorful;

        if (!c1IsColorful && !c2IsColorful) return false;

        // 情况1: Colorful + Colorful = 全屏消除
        if (c1IsColorful && c2IsColorful)
        {
            Debug.Log("Special Swap: Colorful + Colorful");
            HashSet<CubeController> allCubes = new HashSet<CubeController>();
            for(int x=0; x<width; x++)
            {
                for(int y=0; y<height; y++)
                {
                    if (grid[x, y] != null) allCubes.Add(grid[x, y]);
                }
            }
            
            // 执行全屏消除
            List<UniTask> animTasks = new List<UniTask>();
            foreach(var c in allCubes)
            {
                 if (grid[c.X, c.Y] == c) grid[c.X, c.Y] = null;
                 animTasks.Add(c.EliminateAnimAsync());
            }
            await UniTask.WhenAll(animTasks);
            
            foreach(var c in allCubes) DestroyCube(c);
            
            return true;
        }

        // 情况2: Colorful + Normal/Special = 消除同色
        // 如果是 Colorful + Special，通常会把同色方块都变成那个 Special，这里先简化为消除同色
        CubeController colorfulCube = c1IsColorful ? c1 : c2;
        CubeController targetCube = c1IsColorful ? c2 : c1;
        
        // 目标颜色
        int targetType = targetCube.Type;
        
        Debug.Log($"Special Swap: Colorful + Type {targetType}");
        
        HashSet<CubeController> cubesToDestroy = new HashSet<CubeController>();
        cubesToDestroy.Add(colorfulCube); // 销毁 Colorful 自己
        
        // 查找所有同色方块
        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                if (grid[x, y] != null && grid[x, y].Type == targetType)
                {
                    cubesToDestroy.Add(grid[x, y]);
                }
            }
        }

        // 执行消除
        List<UniTask> tasks = new List<UniTask>();
        foreach(var c in cubesToDestroy)
        {
             if (grid[c.X, c.Y] == c) grid[c.X, c.Y] = null;
             tasks.Add(c.EliminateAnimAsync());
        }
        await UniTask.WhenAll(tasks);
        
        foreach(var c in cubesToDestroy) DestroyCube(c);

        return true;
    }

    public async UniTask HandleCubeClick(int x, int y)
    {
        Debug.Log($"CubeManager: Click at ({x}, {y})");
        if (!IsValid(x, y)) 
        {
            Debug.Log($"CubeManager: Invalid position ({x}, {y})");
            return;
        }
        CubeController clicked = grid[x, y];
        if (clicked == null) 
        {
            Debug.Log($"CubeManager: Clicked null cube at ({x}, {y})");
            return;
        }

        if (selectedCube == null)
        {
            // 选中第一个
            selectedCube = clicked;
            selectedCube.SetFocus(true);
            Debug.Log($"CubeManager: Selected ({x}, {y})");
        }
        else
        {
            // 再次点击同一个，取消选中
            if (selectedCube == clicked)
            {
                selectedCube.SetFocus(false);
                selectedCube = null;
                Debug.Log($"CubeManager: Deselected ({x}, {y})");
            }
            else
            {
                // 点击另一个
                // 检查是否相邻
                if (Mathf.Abs(selectedCube.X - x) + Mathf.Abs(selectedCube.Y - y) == 1)
                {
                    // 相邻，尝试交换
                    Debug.Log($"CubeManager: Swapping ({selectedCube.X}, {selectedCube.Y}) with ({x}, {y})");
                    selectedCube.SetFocus(false);
                    var prev = selectedCube;
                    selectedCube = null;
                    await TrySwapAsync(prev.X, prev.Y, x, y);
                }
                else
                {
                    // 不相邻，更新选中
                    Debug.Log($"CubeManager: Changed selection to ({x}, {y}) - Not adjacent");
                    selectedCube.SetFocus(false);
                    selectedCube = clicked;
                    selectedCube.SetFocus(true);
                }
            }
        }
    }

    public async UniTask HandleCubeSwipe(int x, int y, Vector2 direction)
    {
        Debug.Log($"CubeManager: Swipe at ({x}, {y}) dir {direction}");
        if (!IsValid(x, y)) return;
        
        // 确定主要方向
        int dx = 0;
        int dy = 0;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            dx = direction.x > 0 ? 1 : -1;
        else
            dy = direction.y > 0 ? 1 : -1;

        int tx = x + dx;
        int ty = y + dy;
        
        Debug.Log($"CubeManager: Swipe Target calc: ({x},{y}) + ({dx},{dy}) = ({tx},{ty})");

        if (IsValid(tx, ty))
        {
            // 如果有选中的，先取消选中
            if (selectedCube != null)
            {
                selectedCube.SetFocus(false);
                selectedCube = null;
            }
            Debug.Log($"CubeManager: Swipe Swap ({x}, {y}) with ({tx}, {ty})");
            await TrySwapAsync(x, y, tx, ty);
        }
        else
        {
             Debug.Log($"CubeManager: Swipe target invalid ({tx}, {ty})");
        }
    }

    /// <summary>
    /// 交换逻辑
    /// </summary>
    private async UniTask SwapAsync(int x1, int y1, int x2, int y2)
    {
        CubeController c1 = grid[x1, y1];
        CubeController c2 = grid[x2, y2];

        // 更新网格数据
        grid[x1, y1] = c2;
        grid[x2, y2] = c1;

        List<UniTask> tasks = new List<UniTask>();
        if (c1 != null) tasks.Add(c1.MoveAsync(x2, y2));
        if (c2 != null) tasks.Add(c2.MoveAsync(x1, y1));

        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// 查找所有匹配组（支持特殊方块检测）
    /// </summary>
    public List<MatchGroup> FindMatches()
    {
        List<List<CubeController>> rawMatches = new List<List<CubeController>>();
        
        // Horizontal
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; )
            {
                int matchLen = 1;
                while (x + matchLen < width && grid[x, y] != null && grid[x + matchLen, y] != null && grid[x, y].Type == grid[x + matchLen, y].Type)
                {
                    matchLen++;
                }
                if (matchLen >= 3)
                {
                    List<CubeController> match = new List<CubeController>();
                    for (int k = 0; k < matchLen; k++) match.Add(grid[x + k, y]);
                    rawMatches.Add(match);
                    x += matchLen;
                }
                else x++;
            }
        }
        
        // Vertical
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; )
            {
                int matchLen = 1;
                while (y + matchLen < height && grid[x, y] != null && grid[x, y + matchLen] != null && grid[x, y].Type == grid[x, y + matchLen].Type)
                {
                    matchLen++;
                }
                if (matchLen >= 3)
                {
                    List<CubeController> match = new List<CubeController>();
                    for (int k = 0; k < matchLen; k++) match.Add(grid[x, y + k]);
                    rawMatches.Add(match);
                    y += matchLen;
                }
                else y++;
            }
        }

        // Merge
        List<MatchGroup> groups = new List<MatchGroup>();
        Dictionary<CubeController, MatchGroup> cubeToGroup = new Dictionary<CubeController, MatchGroup>();

        foreach (var match in rawMatches)
        {
            MatchGroup currentGroup = null;
            
            foreach (var cube in match)
            {
                if (cubeToGroup.ContainsKey(cube))
                {
                    if (currentGroup == null)
                    {
                        currentGroup = cubeToGroup[cube];
                    }
                    else if (currentGroup != cubeToGroup[cube])
                    {
                        MatchGroup otherGroup = cubeToGroup[cube];
                        foreach (var c in otherGroup.cubes)
                        {
                            if (!currentGroup.cubes.Contains(c)) currentGroup.cubes.Add(c);
                            cubeToGroup[c] = currentGroup;
                        }
                        groups.Remove(otherGroup);
                    }
                }
            }
            
            if (currentGroup == null)
            {
                currentGroup = new MatchGroup();
                groups.Add(currentGroup);
            }
            
            foreach (var cube in match)
            {
                if (!currentGroup.cubes.Contains(cube))
                {
                    currentGroup.cubes.Add(cube);
                    cubeToGroup[cube] = currentGroup;
                }
            }
        }
        
        // Determine Type
        bool hasSpecial = false;
        List<BonusType> allowedTypes = null;
        if (StaticProperties.Instance != null && StaticProperties.Instance.levelData != null)
        {
            hasSpecial = StaticProperties.Instance.levelData.hasSpecialBlocks;
            allowedTypes = StaticProperties.Instance.levelData.availableSpecialBlocks;
        }

        foreach(var group in groups)
        {
            BonusType potentialType = BonusType.None;

            if (group.cubes.Count >= 5) 
            {
                // 5消及以上统一生成 Colorful (全屏消除/同色消除)
                potentialType = BonusType.Colorful; 
            }
            else if (group.cubes.Count == 4) 
            {
                // 检测方向
                bool isHorizontal = true;
                bool isVertical = true;
                if (group.cubes.Count > 0)
                {
                    int y0 = group.cubes[0].Y;
                    int x0 = group.cubes[0].X;
                    foreach(var c in group.cubes)
                    {
                        if (c.Y != y0) isHorizontal = false;
                        if (c.X != x0) isVertical = false;
                    }
                }

                if (isHorizontal) potentialType = BonusType.Horizontal; 
                else if (isVertical) potentialType = BonusType.Vertical; 
            }

            // 应用 LevelData 配置检查与降级策略
            if (hasSpecial && allowedTypes != null)
            {
                if (allowedTypes.Contains(potentialType))
                {
                    // 允许生成
                    group.bonusType = potentialType;
                }
                else if (potentialType == BonusType.Colorful)
                {
                    // 5消但不允许生成 Colorful，尝试降级为 4消效果
                    bool isHorizontal = true;
                    bool isVertical = true;
                    if (group.cubes.Count > 0)
                    {
                        int y0 = group.cubes[0].Y;
                        int x0 = group.cubes[0].X;
                        foreach (var c in group.cubes)
                        {
                            if (c.Y != y0) isHorizontal = false;
                            if (c.X != x0) isVertical = false;
                        }
                    }

                    if (isHorizontal && allowedTypes.Contains(BonusType.Horizontal))
                    {
                        group.bonusType = BonusType.Horizontal;
                    }
                    else if (isVertical && allowedTypes.Contains(BonusType.Vertical))
                    {
                        group.bonusType = BonusType.Vertical;
                    }
                    else
                    {
                        // 都不允许，无特殊效果
                        group.bonusType = BonusType.None;
                    }
                }
                else
                {
                    group.bonusType = BonusType.None;
                }
            }
            else
            {
                group.bonusType = BonusType.None;
            }
        }

        
        return groups;
    }

    /// <summary>
    /// 处理消除、下落、填充流程
    /// </summary>
    public async UniTask<bool> ProcessMatchesAsync()
    {
        List<MatchGroup> groups = FindMatches();
        if (groups.Count == 0) return false;

        // 1. 确定合成目标（Merge Target）
        foreach(var group in groups)
        {
            if (group.bonusType != BonusType.None)
            {
                foreach(var c in group.cubes)
                {
                    if ((c.X == lastSwapPos1.x && c.Y == lastSwapPos1.y) || 
                        (c.X == lastSwapPos2.x && c.Y == lastSwapPos2.y))
                    {
                        group.mergeTarget = c;
                        break;
                    }
                }
                if (group.mergeTarget == null && group.cubes.Count > 0) 
                    group.mergeTarget = group.cubes[Random.Range(0, group.cubes.Count)];
            }
        }

        // 2. 收集所有需要消除的方块
        HashSet<CubeController> cubesToDestroy = new HashSet<CubeController>();
        HashSet<CubeController> cubesToPromote = new HashSet<CubeController>();

        foreach(var group in groups)
        {
            foreach (var c in group.cubes)
            {
                if (group.mergeTarget == c)
                {
                    cubesToPromote.Add(c);
                }
                else
                {
                    cubesToDestroy.Add(c);
                }
            }
        }

        // 3. 处理特殊方块触发的连锁消除
        // 使用队列进行广度优先搜索
        Queue<CubeController> processingQueue = new Queue<CubeController>(cubesToDestroy);
        
        // 为了防止无限循环，我们需要记录已处理过的触发源
        HashSet<CubeController> processedTriggers = new HashSet<CubeController>();

        while(processingQueue.Count > 0)
        {
            CubeController c = processingQueue.Dequeue();
            
            // 如果这个方块已经被销毁或者正在变身，我们检查它的 BonusType 是否能触发更多消除
            // 注意：只有被消除的特殊方块才会触发效果。正在变身的方块（Promote）不会触发。
            
            if (processedTriggers.Contains(c)) continue;
            processedTriggers.Add(c);

            BonusType bType = c.BonusType;
            
            if (bType == BonusType.Horizontal)
            {
                for(int x = 0; x < width; x++)
                {
                    if (grid[x, c.Y] != null)
                    {
                        CubeController target = grid[x, c.Y];
                        if (!cubesToDestroy.Contains(target) && !cubesToPromote.Contains(target))
                        {
                            cubesToDestroy.Add(target);
                            processingQueue.Enqueue(target);
                        }
                    }
                }
            }
            else if (bType == BonusType.Vertical)
            {
                for(int y = 0; y < height; y++)
                {
                    if (grid[c.X, y] != null)
                    {
                        CubeController target = grid[c.X, y];
                        if (!cubesToDestroy.Contains(target) && !cubesToPromote.Contains(target))
                        {
                            cubesToDestroy.Add(target);
                            processingQueue.Enqueue(target);
                        }
                    }
                }
            }
            // Bomb 逻辑暂时不启用，5消及以上统一为 Colorful
            if (bType == BonusType.Colorful)
            {
                // 如果 Colorful 方块被被动消除（例如被 Horizontal/Vertical 击中）
                // 消除该 Colorful 方块自身颜色的所有方块
                int targetType = c.Type;

                Debug.Log($"Passive Colorful Trigger: Destroying all type {targetType}");

                for(int x = 0; x < width; x++)
                {
                    for(int y = 0; y < height; y++)
                    {
                        if (grid[x, y] != null && grid[x, y].Type == targetType)
                        {
                             CubeController target = grid[x, y];
                             if (!cubesToDestroy.Contains(target) && !cubesToPromote.Contains(target))
                             {
                                 cubesToDestroy.Add(target);
                                 processingQueue.Enqueue(target);
                             }
                        }
                    }
                }
            }
        }

        // 4. 执行消除动画
        List<UniTask> animTasks = new List<UniTask>();
        foreach(var c in cubesToDestroy)
        {
            if (grid[c.X, c.Y] == c) grid[c.X, c.Y] = null;
            animTasks.Add(c.EliminateAnimAsync());
        }
        
        // 执行变身逻辑（无动画，直接设置状态，或者可以加个简单动画）
        foreach(var group in groups)
        {
            if (group.mergeTarget != null && group.bonusType != BonusType.None)
            {
                Debug.Log($"Promoting cube at ({group.mergeTarget.X}, {group.mergeTarget.Y}) to {group.bonusType}");
                group.mergeTarget.SetBonus(group.bonusType);
                // 可以在这里播放一个升级音效或特效
            }
        }
        
        await UniTask.WhenAll(animTasks);

        // 5. 触发事件 (计分)
        foreach(var group in groups)
        {
            int matchScore = 0;
            if (group.cubes.Count >= 5) matchScore = 100; 
            else if (group.cubes.Count == 4) matchScore = 50;

            OnMatchesProcessed?.Invoke(group.cubes.Count, matchScore);
        }

        // 6. 真正销毁对象
        foreach(var c in cubesToDestroy)
        {
             DestroyCube(c);
        }
        
        // 7. 下落
        await ApplyGravityAsync();
        
        // 8. 填充新方块
        await RefillAsync();

        // 再次检查是否有新的匹配（这在外部循环处理，这里返回true表示发生过消除）
        return true;
    }

    private async UniTask ApplyGravityAsync()
    {
         List<UniTask> tasks = new List<UniTask>();
         for (int x = 0; x < width; x++)
         {
             for (int y = 0; y < height; y++)
             {
                 if (!IsValid(x, y)) continue; // 跳过无效格子

                 if (grid[x, y] == null)
                 {
                     // 向上寻找最近的一个方块
                     for (int k = y + 1; k < height; k++)
                     {
                         if (grid[x, k] != null)
                         {
                             CubeController c = grid[x, k];
                             grid[x, k] = null;
                             grid[x, y] = c;
                             // 使用 DropWithGravityAsync 掉落到新位置
                             tasks.Add(c.DropWithGravityAsync(x, y));
                             break;
                         }
                     }
                 }
             }
         }
         await UniTask.WhenAll(tasks);
    }

    private async UniTask RefillAsync()
    {
        var levelData = StaticProperties.Instance.levelData;
        float dropHeight = levelData.dropHeight;
        Vector2 startPos = levelData.startPosition;
        float spawnY = startPos.y + dropHeight;

         List<UniTask> tasks = new List<UniTask>();
         for (int x = 0; x < width; x++)
         {
             for (int y = 0; y < height; y++)
             {
                 if (!IsValid(x, y)) continue; // 跳过无效格子

                 if (grid[x, y] == null)
                 {
                     // 获取最大类型数（受限于关卡配置和已加载资源）
                     int maxType = StaticProperties.Instance.levelData.cubeTypesCount;
                     if (maxType > StaticProperties.Instance.cubeData.cube.Length)
                        maxType = StaticProperties.Instance.cubeData.cube.Length;

                     // 随机生成 (0 到 maxType-1)
                     int type = Random.Range(0, maxType);
                     
                     // 从上方生成
                     Vector2 initPos = new Vector2(posMap[x,y].x, spawnY); 
                     CubeController controller = CreateCube(posMap, type, initPos, x, y);
                     if (controller != null)
                     {
                        // 使用 DropWithGravityAsync 掉落
                        // 传入固定时间或基于高度的时间，避免新生成的方块掉落过慢
                        float time = StaticProperties.Instance.cubeData.dropUnitTime * 5; 
                        tasks.Add(controller.DropWithGravityAsync(x, y, time));
                     }
                 }
             }
         }
         // 等待新生成的方块掉落
         await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 清理事件监听
        OnMatchesProcessed = null;

        // 清理棋盘数据
        ClearBoard();
        grid = null;
        posMap = null;
        activeGrid = null;

        // 清空单例引用
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
