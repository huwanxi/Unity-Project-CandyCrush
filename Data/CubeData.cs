
using UnityEngine;
[CreateAssetMenu(fileName ="CubeData",menuName ="SO/CubeData",order =1)]
public class CubeData : ScriptableObject
{
    public float gradientTime = 0.3f;
    [Tooltip("初始生成的显型/淡入时间")]
    public float fadeInTime = 0.5f;
    public float dropUnitTime = 0.1f;
    public float moveUnitTime = 0.3f;
    [HideInInspector] public GameObject[] cube;
    public GameObject cubeParent;
}
