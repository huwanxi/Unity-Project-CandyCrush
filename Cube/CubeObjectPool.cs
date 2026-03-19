using System.Collections.Generic;
using UnityEngine;

public class CubeObjectPool
{
    public static CubeObjectPool Instance;
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();
    private Transform poolRoot;
    private GameObject rootObj;

    public CubeObjectPool(Transform parent)
    {
        if(Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("CubeObjectPool已存在");
            return;
        }

        
    }

    /// <summary>
    /// 获取方块
    /// </summary>
    /// <param name="type">方块类型索引</param>
    /// <returns></returns>
    public GameObject GetCube(int type)
    {
        if (rootObj == null)
            rootObj = Object.Instantiate(StaticProperties.Instance.cubeData.cubeParent);
        else
            Debug.Log("rootObj!=null");

        if (poolDictionary.ContainsKey(type) && poolDictionary[type].Count > 0)
        {
            GameObject cube = poolDictionary[type].Dequeue();
            cube.SetActive(true);
            cube.transform.SetParent(rootObj.transform,false);
            return cube;
        }
        else
        {
            // 检查索引是否有效
            if (type < 0)
            {
                Debug.LogError($"Cube type {type} is invalid.");
                return null;
            }

            // 如果 type 超出了 cube 数组范围，尝试使用取模或其他方式回退，或者报错
            // 这里假设 type 应该在 0 到 cube.Length-1 之间
            // 但如果 CubeManager 请求了更大的 type (例如特殊方块)，我们需要处理
            
            GameObject prefab = null;
            if (type < StaticProperties.Instance.cubeData.cube.Length)
            {
                prefab = StaticProperties.Instance.cubeData.cube[type];
            }
            
            // 如果找不到对应 prefab，尝试使用第一个作为 fallback (防止空指针)
            if (prefab == null && StaticProperties.Instance.cubeData.cube.Length > 0)
            {
                 Debug.LogWarning($"Prefab for cube type {type} is null or out of range. Using fallback.");
                 prefab = StaticProperties.Instance.cubeData.cube[0];
            }

            if (prefab == null)
            {
                Debug.LogError($"Prefab for cube type {type} is null and no fallback available.");
                return null;
            }

            GameObject newCube = Object.Instantiate(prefab);
            // 确保实例化后的对象名字不带 (Clone)，或者带上也没关系
            // newCube.name = prefab.name; 
            newCube.transform.SetParent(rootObj.transform);
            return newCube;
        }
    }

    /// <summary>
    /// 回收方块
    /// </summary>
    /// <param name="cube">方块对象</param>
    /// <param name="type">方块类型</param>
    public void ReturnCube(GameObject cube, int type)
    {
        if (cube == null) return;

        cube.SetActive(false);
        cube.transform.SetParent(poolRoot);
        // 重置位置等属性，防止下次取出时有残留状态
        cube.transform.localPosition = Vector3.zero;

        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary[type] = new Queue<GameObject>();
        }
        poolDictionary[type].Enqueue(cube);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 销毁池中所有对象
        foreach(var queue in poolDictionary.Values)
        {
            while(queue.Count > 0)
            {
                var obj = queue.Dequeue();
                if(obj != null) Object.Destroy(obj);
            }
        }
        poolDictionary.Clear();
        
        // 销毁自身
        if (rootObj != null)
        {
            Object.Destroy(rootObj);
            rootObj = null;
        }
        poolRoot = null;
    }
}
