using UnityEngine;

[CreateAssetMenu(fileName = "WinLoseConfig", menuName = "SO/WinLoseConfig", order = 4)]
public class WinLoseConfig : ScriptableObject
{
    [Tooltip("胜利时的标题文本")]
    public string winTitle = "You Win!";
    
    [Tooltip("失败时的标题文本")]
    public string loseTitle = "Game Over";

    [Tooltip("胜利时的描述文本")]
    public string winDescription = "Congratulations!";

    [Tooltip("失败时的描述文本")]
    public string loseDescription = "Try Again!";
    
    [Tooltip("是否显示分数")]
    public bool showScore = true;
}
