using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIBase_Game : UIBase
{
    private Text scoreText;
    private Text timeText;
    private Text moveText;

    // 评估系统 UI 组件
    private Slider scoreSlider;
    private Transform[] stars = new Transform[3];
    private bool[] starReached = new bool[3];
    private bool isStarsPositioned = false;

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

        // 绑定 UIManager 的事件接口
        // 当前 UI 的 ID 是 6
        UIManager.Instance.EventBind<ScoreUpdateEventParam>(6, OnScoreEventTriggered);
    }

    private void FindComponents(Transform root)
    {
        Transform score = root.Find("ScoreText");
        if (score != null) scoreText = score.GetComponent<Text>();

        Transform time = root.Find("TimeText");
        if (time != null) timeText = time.GetComponent<Text>();

        Transform move = root.Find("MoveText");
        if (move != null) moveText = move.GetComponent<Text>();

        // 查找进度条和星级
        Transform sliderTrans = root.Find("ScoreSlider");
        if (sliderTrans != null)
        {
            scoreSlider = sliderTrans.GetComponent<Slider>();
            
            // 查找 Slider 下的 3 颗星
            for (int i = 0; i < 3; i++)
            {
                Transform star = sliderTrans.Find($"Star_{i + 1}");
                if (star != null)
                {
                    stars[i] = star;
                    // 保存初始状态或设置初始大小
                    stars[i].localScale = Vector3.one;
                }
                else
                {
                    Debug.LogWarning($"UIBase_Game: 未在 ScoreSlider 下找到 Star_{i + 1}");
                }
            }
        }
        else
        {
            Debug.LogWarning("UIBase_Game: 未找到 ScoreSlider 组件");
        }
    }

    public override void Enter()
    {
        base.Enter();
        BindEvents();
        
        // 初始化显示
        isStarsPositioned = false;
        for (int i = 0; i < 3; i++) starReached[i] = false;
        if (stars != null)
        {
            for (int i = 0; i < 3; i++)
            {
                if (stars[i] != null) stars[i].localScale = Vector3.one;
            }
        }
        
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

    // 处理 UIManager 触发的分数更新事件 (包含进度条和星级逻辑)
    private void OnScoreEventTriggered(ScoreUpdateEventParam param)
    {
        if (scoreSlider != null)
        {
            scoreSlider.maxValue = param.maxScore;
            scoreSlider.value = param.currentScore;
        }

        // 检查星级并动态设置星星位置
        if (param.starScores != null && param.starScores.Length >= 3 && param.maxScore > 0)
        {
            if (!isStarsPositioned)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (stars[i] != null)
                    {
                        RectTransform starRect = stars[i].GetComponent<RectTransform>();
                        if (starRect != null)
                        {
                            // 计算当前星级分数占总分的比例
                            float ratio = (float)param.starScores[i] / param.maxScore;
                            
                            // 动态设置星星的锚点位置，让其与进度条完美匹配
                            starRect.anchorMin = new Vector2(ratio, starRect.anchorMin.y);
                            starRect.anchorMax = new Vector2(ratio, starRect.anchorMax.y);
                            starRect.anchoredPosition = new Vector2(0, starRect.anchoredPosition.y);
                        }
                    }
                }
                isStarsPositioned = true;
            }

            for (int i = 0; i < 3; i++)
            {
                if (param.currentScore >= param.starScores[i] && !starReached[i])
                {
                    starReached[i] = true;
                    if (stars[i] != null)
                    {
                        // 播放变大动画
                        stars[i].DOScale(1.5f, 0.5f).SetEase(Ease.OutBack);
                    }
                }
            }
        }
    }
}
