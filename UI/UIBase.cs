using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIBase
{
    protected GameObject gameObject;
    protected Image image; 
    protected Button openBtn;
    protected Button closeBtn;
    
    // 动画配置
    protected float clickScale = 0.9f;
    protected float clickDuration = 0.1f;
    
    public UIBase(GameObject _gameObject)
    {
        gameObject = _gameObject;

        // 尝试查找 Open 和 Close 按钮
        Transform open = gameObject.transform.Find("Open");
        if (open != null) 
        {
            openBtn = open.GetComponent<Button>();
            //BindButtonAnimation(openBtn.gameObject);
        }
        
        Transform close = gameObject.transform.Find("Close");
        if (close != null) 
        {
            closeBtn = close.GetComponent<Button>();
            //BindButtonAnimation(closeBtn.gameObject);
        }
    }

    /// <summary>
    /// 为按钮绑定点击缩放动画
    /// </summary>
    /// <param name="btnObj">按钮的游戏物体</param>
    protected void BindButtonAnimation(GameObject btnObj)
    {
        if (btnObj == null) return;

        // 确保物体上有 EventTrigger 组件
        EventTrigger trigger = btnObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = btnObj.AddComponent<EventTrigger>();
        }

        // 记录原始缩放值
        Vector3 originalScale = btnObj.transform.localScale;

        // 绑定按下事件 (PointerDown)
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => {
            btnObj.transform.DOScale(originalScale * clickScale, clickDuration).SetEase(Ease.OutQuad);
        });
        trigger.triggers.Add(pointerDownEntry);

        // 绑定抬起事件 (PointerUp)
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => {
            btnObj.transform.DOScale(originalScale, clickDuration).SetEase(Ease.OutBack);
        });
        trigger.triggers.Add(pointerUpEntry);
    }

    public virtual void Enter()
    {
        gameObject.SetActive(true);

        // 获取或添加 CanvasGroup 用于控制透明度
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 获取物体原本的 localScale
        // 因为 Instantiate 后 localScale 已经被设为了预制体的真实大小
        Vector3 targetScale = gameObject.transform.localScale;

        // 如果 targetScale 是 0，则给个默认值防止动画失效
        if (targetScale == Vector3.zero) 
        {
             targetScale = Vector3.one;
        }

        // 初始状态：缩小到目标大小的 0.5 倍且透明
        gameObject.transform.localScale = targetScale * 0.5f;
        canvasGroup.alpha = 0f;

        // 执行出现动画：放大到目标大小，透明度渐变到 1
        gameObject.transform.DOScale(targetScale, 1f).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, 1f).SetEase(Ease.OutQuad);
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
        // 移除所有事件
        UIManager.Instance.EventBind<UIEventParameter>(gameObject.GetInstanceID(),null);
    }
}
