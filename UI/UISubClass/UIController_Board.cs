using UnityEngine;

public class UIController_Board : UIController
{
    private UIBase_Board uiBaseBoard;

    public UIController_Board(GameObject _gameObject) : base(_gameObject)
    {
        uiBaseBoard = new UIBase_Board(gameObject);
        uiBase = uiBaseBoard;
    }
}
