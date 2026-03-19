using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class EvaluationManager
{
    //特殊，instance在全局配置脚本中
    private EvaluationData evaluationData;
    private float currentTimer;
    private int currentMoveCount;
    private int currentScore;
    
    // 事件
    public event Action<int> OnScoreUpdated;
    public event Action<float> OnTimeUpdated;
    public event Action<int> OnMoveUpdated;
    public event Action OnGameWin;
    public event Action OnGameLose;

    public int CurrentScore => currentScore;
    public float CurrentTimer => currentTimer;
    public int CurrentMoveCount => currentMoveCount;

    private System.Threading.CancellationTokenSource cts;
    private bool isGameRunning = false;
    public bool IsGameRunning => isGameRunning;
    private bool isPaused = false;

    public void Init(EvaluationData data)
    {
        evaluationData = data;
        ResetGame();
        Debug.Log($"EvaluationManager initialized. Mode: {evaluationData.gameMode}");
    }

    public void ResetGame()
    {
        StopGame();
        currentScore = 0;
        isPaused = false;
        
        if (evaluationData.gameMode == GameMode.TimeLimit)
        {
            currentTimer = evaluationData.timeLimit;
        }
        else
        {
            currentMoveCount = evaluationData.moveLimit;
        }
        
        OnScoreUpdated?.Invoke(currentScore);
        if (evaluationData.gameMode == GameMode.TimeLimit) OnTimeUpdated?.Invoke(currentTimer);
        else OnMoveUpdated?.Invoke(currentMoveCount);
    }

    public void StartGame()
    {
        if (isGameRunning) return;
        isGameRunning = true;
        isPaused = false;

        // 绑定消除事件
        if (CubeManager.Instance != null)
        {
            CubeManager.Instance.OnMatchesProcessed -= HandleMatchesProcessed; // 防止重复绑定
            CubeManager.Instance.OnMatchesProcessed += HandleMatchesProcessed;
            
            CubeManager.Instance.OnMoveUsed -= HandleMoveUsed;
            CubeManager.Instance.OnMoveUsed += HandleMoveUsed;
        }
        
        if (evaluationData.gameMode == GameMode.TimeLimit)
        {
            cts = new System.Threading.CancellationTokenSource();
            StartTimeCounter(cts.Token).Forget();
        }
    }
    
    public void PauseGame(bool pause)
    {
        isPaused = pause;
    }

    public void StopGame()
    {
        isGameRunning = false;
        isPaused = false;
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
        
        if (CubeManager.Instance != null)
        {
            CubeManager.Instance.OnMatchesProcessed -= HandleMatchesProcessed;
            CubeManager.Instance.OnMoveUsed -= HandleMoveUsed;
        }
    }

    private async UniTaskVoid StartTimeCounter(System.Threading.CancellationToken token)
    {
        while (currentTimer > 0 && isGameRunning && !token.IsCancellationRequested)
        {
            await UniTask.Delay(1000, cancellationToken: token);
            if (token.IsCancellationRequested || !isGameRunning) break;
            
            if (isPaused) continue;

            currentTimer -= 1f;
            OnTimeUpdated?.Invoke(currentTimer);
            
            if (currentTimer <= 0)
            {
                currentTimer = 0;
                CheckGameStatus();
            }
        }
    }

    private void HandleMatchesProcessed(int matchCount, int bonusScore)
    {
        // 计算得分
        int score = matchCount * evaluationData.baseMatchScore + bonusScore;
        AddScore(score);
    }

    private void HandleMoveUsed()
    {
        // 如果是步数模式，减少步数
        if (evaluationData.gameMode == GameMode.MoveLimit)
        {
            currentMoveCount--;
            OnMoveUpdated?.Invoke(currentMoveCount);
            CheckGameStatus();
        }
    }

    private void AddScore(int amount)
    {
        currentScore += amount;
        OnScoreUpdated?.Invoke(currentScore);
        Debug.Log($"Score Added: {amount}, Total: {currentScore}");
        
        //CheckGameStatus();不再只符合胜利条件触发结算
    }

    private void CheckGameStatus()
    {
        if (CheckWinCondition())
        {
            Debug.Log("Game Win!");
            OnGameWin?.Invoke();
        }
        else if (CheckLoseCondition())
        {
            Debug.Log("Game Lose!");
            OnGameLose?.Invoke();
        }
    }

    private bool CheckWinCondition()
    {
        // 简单判定：达到一星分数即视为胜利 (或者根据具体需求)
        return currentScore >= evaluationData.oneStarScore;
    }

    private bool CheckLoseCondition()
    {
        if (evaluationData.gameMode == GameMode.TimeLimit)
        {
            return currentTimer <= 0 && currentScore < evaluationData.oneStarScore;
        }
        else
        {
            return currentMoveCount <= 0 && currentScore < evaluationData.oneStarScore;
        }
    }

    // 清理事件绑定
    public void Dispose()
    {
        if (CubeManager.Instance != null)
        {
            CubeManager.Instance.OnMatchesProcessed -= HandleMatchesProcessed;
            CubeManager.Instance.OnMoveUsed -= HandleMoveUsed;
        }
    }
}
