using UnityEngine;
using UnityEngine.UI;

public class UIController
{
    public UIBase uiBase;
    public GameObject gameObject;

    public UIController(GameObject _gameObject)
    {
        gameObject = _gameObject;
        uiBase = new UIBase(gameObject);
    }

    public void Enter()
    {
        uiBase.Enter();
    }
    public virtual void Pause()
    {
        uiBase.Pause();
    }
    public virtual void Resume()
    {
        uiBase.Resume();
    }
    public void Exit()
    {
        uiBase.Exit();
    }

    public void Destory()
    {
        uiBase.Destory();
    }


}