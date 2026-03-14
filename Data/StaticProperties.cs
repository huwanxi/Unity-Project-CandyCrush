using UnityEngine;

public class StaticProperties : MonoBehaviour
{
    //全局静态资源
    public static StaticProperties Instance;
    public CubeData cubeData;
    public LevelData levelData;
    public EvaluationData evaluationData;
    public EvaluationManager evaluationManager;
    public CubeObjectPool cubeObjectPool;
    public void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            Debug.LogWarning("CubeStaticProperties warning");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (evaluationManager != null)
        {
            evaluationManager.Dispose();
            evaluationManager = null;
        }

        if (cubeObjectPool != null)
        {
            cubeObjectPool.Dispose();
            cubeObjectPool = null;
        }
        
        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
}
