using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UIsConfig",menuName="SO/UIsConfig",order=2)]
public class UIsConfig : ScriptableObject
{
    [Tooltip("需要配置的UI GameObject数量")]
    [Min(0)]
    public int uiGameObjectCount = 0;

    [Tooltip("ui父级")]
    public GameObject uiParent;
}
