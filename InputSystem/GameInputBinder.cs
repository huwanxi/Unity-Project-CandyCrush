using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameInputBinder
{
    public void Bind()
    {
        if (GameInputManager.Instance != null && CubeManager.Instance != null)
        {
            GameInputManager.Instance.OnInputClick += HandleClick;
            GameInputManager.Instance.OnInputSwipe += HandleSwipe;
            Debug.Log("GameInputBinder: 输入事件已绑定到 CubeManager。");
        }
        else
        {
            Debug.LogWarning("GameInputBinder: 绑定失败或部分实例缺失 (可能尚未初始化)。");
        }
    }

    public void Unbind()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnInputClick -= HandleClick;
            GameInputManager.Instance.OnInputSwipe -= HandleSwipe;
        }
    }

    private async UniTask HandleClick(Vector2Int pos)
    {
        if (CubeManager.Instance != null)
        {
             await CubeManager.Instance.HandleCubeClick(pos.x, pos.y);
        }
    }

    private async UniTask HandleSwipe(Vector2Int pos, Vector2 dir)
    {
        if (CubeManager.Instance != null)
        {
             await CubeManager.Instance.HandleCubeSwipe(pos.x, pos.y, dir);
        }
    }
}