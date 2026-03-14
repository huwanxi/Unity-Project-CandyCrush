using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cysharp.Threading.Tasks;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }

    public event Func<Vector2Int, UniTask> OnInputClick;
    public event Action<Vector2Int> OnInputCancel;
    public event Func<Vector2Int, Vector2, UniTask> OnInputSwipe;

    private GameInputController inputController;
    private GameInputBinder inputBinder;
    
    // Unity Input System 动作
    private InputAction pointAction;
    private InputAction clickAction;
    
    private bool isGameActive = false;
    public bool IsGameActive => isGameActive; // 公开状态供检查
    private bool isPaused = false; // 临时暂停标志

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化动作 (手动初始化以避免 .inputactions 依赖)
        // 注意: 在生产环境中，建议使用 .inputactions 资产。
        pointAction = new InputAction(type: InputActionType.Value, binding: "<Pointer>/position");
        clickAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");
        
        inputController = new GameInputController(this);
        // 初始化绑定器
        inputBinder = new GameInputBinder();
    }

    public void StartGame()
    {
        pointAction.Enable();
        clickAction.Enable();
        isGameActive = true;
        isPaused = false; // 确保暂停状态被重置
        if (inputController != null) inputController.ResetState(); // 重置输入状态

        // 绑定事件
        inputBinder.Bind();
        // Debug.Log("GameInputManager: 游戏输入已激活");
    }

    public void StopGame()
    {
        pointAction.Disable();
        clickAction.Disable();
        isGameActive = false;
        isPaused = false; // 重置暂停状态

        if (inputController != null) inputController.ResetState(); // 重置输入状态

        // 解绑事件
        inputBinder.Unbind();
        // Debug.Log("GameInputManager: 游戏输入已停止");
    }

    /// <summary>
    /// 临时暂停/恢复输入（用于逻辑处理期间阻塞输入）
    /// </summary>
    public void SetInputPaused(bool paused)
    {
        isPaused = paused;
        if (paused && inputController != null)
        {
            inputController.ResetState();
        }
        Debug.Log($"GameInputManager: Input Paused = {paused}");
    }

    private void Update()
    {
        if (isGameActive && !isPaused)
        {
            if (WasPointerPressedThisFrame())
            {
                Debug.Log($"GameInputManager: Pointer Pressed at {GetPointerPosition()}");
            }
            inputController.Update();
        }
    }

    public Vector2 GetPointerPosition()
    {
        // 优先使用 Legacy Input
        // 即使是 (0,0) 也有可能是合法的鼠标位置，所以只要 Input.mousePosition 有值就用
        // 但是 Input.mousePosition 始终返回 Vector3
        return Input.mousePosition; 
    }

    public bool IsPointerDown()
    {
        // Legacy Input
        if (Input.GetMouseButton(0)) return true;
        return false;
    }

    public bool WasPointerPressedThisFrame()
    {
        // Legacy Input
        if (Input.GetMouseButtonDown(0)) return true;
        return false;
    }
    
    public bool WasPointerReleasedThisFrame()
    {
        // Legacy Input
        if (Input.GetMouseButtonUp(0)) return true;
        return false;
    }

    // 事件触发器
    public async void TriggerClick(Vector2Int pos)
    {
        if (OnInputClick != null)
        {
            SetInputPaused(true);
            try
            {
                await OnInputClick.Invoke(pos);
            }
            finally
            {
                // 只有当游戏仍然激活时才恢复输入
                // 避免在 OnGameWin/StopGame 后意外恢复输入
                if (isGameActive)
                {
                    SetInputPaused(false);
                }
            }
        }
    }

    public async void TriggerSwipe(Vector2Int pos, Vector2 direction)
    {
        if (OnInputSwipe != null)
        {
            SetInputPaused(true);
            try
            {
                await OnInputSwipe.Invoke(pos, direction);
            }
            finally
            {
                if (isGameActive)
                {
                    SetInputPaused(false);
                }
            }
        }
    }

    public void TriggerCancel(Vector2Int pos)
    {
        OnInputCancel?.Invoke(pos);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        StopGame();
        
        // 清理事件
        OnInputClick = null;
        OnInputSwipe = null;
        
        if (pointAction != null) pointAction.Dispose();
        if (clickAction != null) clickAction.Dispose();

        // 销毁自身
        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
}
