using UnityEngine;
using UnityEngine.UI;

public class UIBase
{
    protected GameObject gameObject;
    protected Image image; 
    protected Button openBtn;
    protected Button closeBtn;
    
    public UIBase(GameObject _gameObject)
    {
        gameObject = _gameObject;

        // 尝试查找 Open 和 Close 按钮
        Transform open = gameObject.transform.Find("Open");
        if (open != null) openBtn = open.GetComponent<Button>();
        
        Transform close = gameObject.transform.Find("Close");
        if (close != null) closeBtn = close.GetComponent<Button>();
    }

    public virtual void Enter()
    {
        gameObject.SetActive(true);
    }
    
    public virtual void Pause()
    {
        // 默认暂停逻辑，禁用交互
        if (openBtn != null) openBtn.interactable = false;
        if (closeBtn != null) closeBtn.interactable = false;
    }

    public virtual void Resume()
    {
        // 默认恢复逻辑，重新激活交互
        gameObject.SetActive(true);
        if (openBtn != null) openBtn.interactable = true;
        if (closeBtn != null) closeBtn.interactable = true;
    }

    public virtual void Exit()
    {
        gameObject.SetActive(false);
    }

    public virtual void Destory()
    {
        Object.Destroy(gameObject);
    }
}
