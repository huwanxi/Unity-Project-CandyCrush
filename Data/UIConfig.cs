using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig",menuName="SO/UIConfig",order=3)]
public class UIConfig : ScriptableObject
{
    [Tooltip("Enter点击后需要触发的打开的界面,0为不绑定")]
    [Min(0)]
    public int openUIID = 0;
    [Tooltip("Exit点击后需要触发的关闭的界面，0为不绑定")]
    [Min(0)]
    public int closeUIID = 0;

    [Tooltip("初始激活状态")]
    public bool isActive = false;

    [Tooltip("是否是自定义，需要继承UIBase的脚本")]//采用反射
    public bool isCustomUI = false;

    [Tooltip("自定义UI的脚本类")]
    public string customUIClass = "";

    [Tooltip("UI层级排序，越小越靠前")]
    public int priority = 0;

    [Tooltip("父级优先级，相同优先级的UI会在同一个父物体下")]
    public int parentPriority = 0;
}
