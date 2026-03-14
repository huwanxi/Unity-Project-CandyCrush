using UnityEngine;


public class UIController_WinLose : UIController
{
    private UIBase_WinLose uiBaseWinLose;

    public UIController_WinLose(GameObject _gameObject) : base(_gameObject)
    {
        gameObject = _gameObject;
        uiBaseWinLose = new UIBase_WinLose(gameObject);
        uiBase = uiBaseWinLose; // 将其赋值给基类的 uiBase 以便统一管理
    }
    
    public void ShowResult(bool isWin, int score)
    {
        uiBaseWinLose.Setup(isWin, score);
        Enter();
    }
}
