using UnityEngine;

public enum GameMode
{
    MoveLimit, // 步数限制模式
    TimeLimit  // 时间限制模式
}

[CreateAssetMenu(fileName = "EvaluationData", menuName = "CandyCrush/EvaluationData")]
public class EvaluationData : ScriptableObject
{
    [Header("游戏模式")]
    [Tooltip("当前关卡的限制模式")]
    public GameMode gameMode = GameMode.MoveLimit;

    [Header("限制条件")]
    [Tooltip("限制时间 (秒)，仅在时间模式下有效")]
    public float timeLimit = 60f;

    [Tooltip("限制步数，仅在步数模式下有效")]
    public int moveLimit = 20;

    [Header("星级分数阈值")]
    [Tooltip("1星分数阈值")]
    public int oneStarScore = 1000;

    [Tooltip("2星分数阈值")]
    public int twoStarScore = 3000;

    [Tooltip("3星分数阈值")]
    public int threeStarScore = 6000;

    [Header("得分规则")]
    [Tooltip("普通三消每个方块的基础得分")]
    public int baseMatchScore = 10;
    
    [Tooltip("四消额外奖励分数")]
    public int match4Bonus = 50;

    [Tooltip("五消额外奖励分数")]
    public int match5Bonus = 100;
}
