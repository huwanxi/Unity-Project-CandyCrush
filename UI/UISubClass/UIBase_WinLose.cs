using UnityEngine;
using UnityEngine.UI;

public class UIBase_WinLose : UIBase
{
    private WinLoseConfig config;
    private Text titleText;
    private Text descriptionText;
    private Text scoreText;
    private Button continueButton;
    private bool isWin;

    public UIBase_WinLose(GameObject _gameObject) : base(_gameObject)
    {
        // 查找 UI 组件
        Transform bg = gameObject.transform.Find("Background");
        if (bg != null)
        {
            Transform title = bg.Find("TitleText");
            if (title != null) titleText = title.GetComponent<Text>();

            Transform desc = bg.Find("DescriptionText");
            if (desc != null) descriptionText = desc.GetComponent<Text>();
            
            Transform score = bg.Find("ScoreText");
            if (score != null) scoreText = score.GetComponent<Text>();

            Transform btn = bg.Find("Continue");
            if (btn != null) continueButton = btn.GetComponent<Button>();
        }
        else
        {
            // 尝试直接查找
            Transform title = gameObject.transform.Find("TitleText");
            if (title != null) titleText = title.GetComponent<Text>();

            Transform desc = gameObject.transform.Find("DescriptionText");
            if (desc != null) descriptionText = desc.GetComponent<Text>();
            
            Transform score = gameObject.transform.Find("ScoreText");
            if (score != null) scoreText = score.GetComponent<Text>();

            Transform btn = gameObject.transform.Find("Continue");
            if (btn != null) continueButton = btn.GetComponent<Button>();
        }

        // 绑定按钮事件
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClick);
        }
        
        LoadConfig();
    }

    private void LoadConfig()
    {
        // 加载配置
        string path = ResourceConfigManager.GetPath("WinLose_Config", "Json/UIJson");
        config = Resources.Load<WinLoseConfig>(path);
        if (config == null)
        {
            Debug.LogError($"UIBase_WinLose: 无法加载 WinLoseConfig，路径: {path}");
        }
    }

    public void Setup(bool win, int score)
    {
        isWin = win;
        
        if (config != null)
        {
            if (titleText != null) titleText.text = isWin ? config.winTitle : config.loseTitle;
            if (descriptionText != null) descriptionText.text = isWin ? config.winDescription : config.loseDescription;
            if (scoreText != null && config.showScore) scoreText.text = $"Score: {score}";
            else if (scoreText != null) scoreText.gameObject.SetActive(false);
        }
    }

    public override void Enter()
    {
        base.Enter();
        // 获取当前游戏状态
        bool win = false; // 默认为输，具体状态应该由外部传递或从 EvaluationManager 获取
        // 由于 EvaluationManager 事件触发时 UI 可能还未打开，或者打开时并未传递参数
        // 这里我们可以假设 Setup 会在 Enter 之前或之后被调用。
        // 或者我们可以在 Enter 中去查询 EvaluationManager 的状态 (但这可能不准确，因为状态可能已经重置)
        // 最好的方式是：LevelInitialization 打开 UI 后，获取 Controller 并调用 Setup。
        
        // 简单起见，我们也可以在这里再次检查 EvaluationManager (如果它保留了最后的状态)
        // 但 EvaluationManager.StopGame() 被调用后状态可能丢失。
        // 所以我们依赖 Setup 方法。
    }

    private void OnContinueClick()
    {
        Debug.Log("UIBase_WinLose: Continue 点击，返回主菜单...");
        
        // 关闭当前 UI
        Exit();

        // 调用 Initialization 的全局方法返回主菜单
        if (Initialization.Instance != null)
        {
            Initialization.Instance.ReturnToMainMenu();
        }
    }
}
