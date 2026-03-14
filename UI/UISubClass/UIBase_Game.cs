using UnityEngine;
using UnityEngine.UI;

public class UIBase_Game : UIBase
{
    private Text scoreText;
    private Text timeText;
    private Text moveText;

    public UIBase_Game(GameObject _gameObject) : base(_gameObject)
    {
        // 查找 UI 组件
        // 尝试在 Background 子节点查找 (常见结构)
        Transform bg = gameObject.transform.Find("Background");
        if (bg != null)
        {
            FindComponents(bg);
        }
        else
        {
            // 尝试直接查找
            FindComponents(gameObject.transform);
        }
    }

    private void FindComponents(Transform root)
    {
        Transform score = root.Find("ScoreText");
        if (score != null) scoreText = score.GetComponent<Text>();

        Transform time = root.Find("TimeText");
        if (time != null) timeText = time.GetComponent<Text>();

        Transform move = root.Find("MoveText");
        if (move != null) moveText = move.GetComponent<Text>();
    }

    public override void Enter()
    {
        base.Enter();
        BindEvents();
        // 初始化显示
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            UpdateScore(StaticProperties.Instance.evaluationManager.CurrentScore);
            UpdateMove(StaticProperties.Instance.evaluationManager.CurrentMoveCount);
            UpdateTime(StaticProperties.Instance.evaluationManager.CurrentTimer);
        }
    }

    public override void Exit()
    {
        base.Exit();
        UnbindEvents();
    }

    private void BindEvents()
    {
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            StaticProperties.Instance.evaluationManager.OnScoreUpdated += UpdateScore;
            StaticProperties.Instance.evaluationManager.OnTimeUpdated += UpdateTime;
            StaticProperties.Instance.evaluationManager.OnMoveUpdated += UpdateMove;
        }
    }

    private void UnbindEvents()
    {
        if (StaticProperties.Instance != null && StaticProperties.Instance.evaluationManager != null)
        {
            StaticProperties.Instance.evaluationManager.OnScoreUpdated -= UpdateScore;
            StaticProperties.Instance.evaluationManager.OnTimeUpdated -= UpdateTime;
            StaticProperties.Instance.evaluationManager.OnMoveUpdated -= UpdateMove;
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void UpdateTime(float time)
    {
        if (timeText != null) timeText.text = $"Time: {Mathf.CeilToInt(time)}";
    }

    private void UpdateMove(int move)
    {
        if (moveText != null) moveText.text = $"Moves: {move}";
    }
}
