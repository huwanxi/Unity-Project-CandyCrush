using UnityEngine;

public class UIController_Game : UIController
{
    private UIBase_Game uiBaseGame;

    public UIController_Game(GameObject _gameObject) : base(_gameObject)
    {
        uiBaseGame = new UIBase_Game(gameObject);
        uiBase = uiBaseGame;
    }
}
