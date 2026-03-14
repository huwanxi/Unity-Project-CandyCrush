using UnityEngine;

public class CubeStaticProperties : MonoBehaviour
{
    public static CubeStaticProperties Instance;
    public CubeData cubeData;
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
}
