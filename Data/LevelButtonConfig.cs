using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

[CreateAssetMenu(fileName = "LevelButtonConfig", menuName = "CandyCrush/LevelButtonConfig")]
public class LevelButtonConfig : ScriptableObject
{
    [Tooltip("关卡数量")]
    public int levelCount;

    [Tooltip("关卡按钮预制体")]
    public GameObject levelButtonPrefab;

    [Tooltip("关卡数据路径前缀 (e.g., 'SO/Level_')")]
    [SerializeField] public string levelDataPathPrefix = "SO/LevelData_";
    
    [Tooltip("评估数据路径前缀 (e.g., 'SO/Evaluation_')")]
    [SerializeField] public string evaluationDataPathPrefix = "SO/EvaluationData_";
}
