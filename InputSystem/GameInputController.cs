using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameInputController
{
    private GameInputManager manager;
    
    // 状态
    private CubeView startCube;
    private CubeView endCube;
    private Vector2 startPointerPos;
    private bool isDragging;
    
    // 配置
    private float dragThreshold = 50f; // 判定为滑动的像素阈值

    public GameInputController(GameInputManager _manager)
    {
        manager = _manager;
    }

    public void Update()
    {
        if (manager.WasPointerPressedThisFrame())
        {
            HandlePointerDown();
        }
        else if (manager.WasPointerReleasedThisFrame())
        {
            HandlePointerUp();
        }
        else if (manager.IsPointerDown())
        {
            HandleDrag();
        }
    }

    private void HandlePointerDown()
    {
        CubeView hit = RaycastCube();
        if (hit != null)
        {
            if(startCube == null)
             startCube = hit;
            startPointerPos = manager.GetPointerPosition();
            isDragging = true;
        }
    }

    private void HandleDrag()
    {
        if (!isDragging || startCube == null) return;
        
        // 可以在这里添加实时的视觉反馈
    }

    private void HandlePointerUp()
    {
        if (!isDragging || startCube == null)
        {
            ResetState();
            return;
        }

        Vector2 endPointerPos = manager.GetPointerPosition();
        float distance = Vector2.Distance(startPointerPos, endPointerPos);
        
        if (distance > dragThreshold)
        {
            // 滑动操作 (Swipe)
            Vector2 direction = endPointerPos - startPointerPos;
            HandleSwipe(direction);
        }
        else
        {
            // 点击操作 (Click)
            endCube = RaycastCube();
            if (endCube != null && endCube == startCube)
            {
                // 确认是点击了同一个方块
                manager.TriggerClick(new Vector2Int(startCube.X, startCube.Y));
                return;
            }
            else if(endCube!= null && endCube!= startCube)
            {
                // 确认是点击了相邻方块
                if(CheckAdjacent(new Vector2Int(endCube.X, endCube.Y)))
                {
                    HandleSwipe(new Vector2Int(endCube.X-startCube.X, endCube.Y-startCube.Y));
                }
                else
                {
                    manager.TriggerCancel(new Vector2Int(startCube.X, startCube.Y));
                    manager.TriggerClick(new Vector2Int(endCube.X, endCube.Y));
                    startCube = endCube;
                    endCube = null;
                    return; 
                }
            }
        }

        ResetState();
    }

    private void HandleSwipe(Vector2 direction)
    {
        if (startCube == null) return;

        // 确定主要方向
        Vector2 normalizedDir;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            normalizedDir = direction.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            normalizedDir = direction.y > 0 ? Vector2.up : Vector2.down;
        }

        manager.TriggerSwipe(new Vector2Int(startCube.X, startCube.Y), normalizedDir);
    }

    public void ResetState()
    {
        endCube = null;
        startCube = null;
        isDragging = false;
        // Debug.Log("GameInputController: State Reset");
    }

    private CubeView RaycastCube()
    {
        Vector2 pointerPos = manager.GetPointerPosition();
        
        // 1. UI 射线检测 (适用于 Canvas GraphicRaycaster)
        if (EventSystem.current != null)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = pointerPos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            Debug.Log($"Input Debug: UI 射线命中 {results.Count} 个物体");
            foreach (var result in results)
            {
                // 优先检查 CubeView
                CubeView view = result.gameObject.GetComponent<CubeView>();
                if (view != null) return view;
                
                // 递归查找父级 (有时 Image 是子物体)
                view = result.gameObject.GetComponentInParent<CubeView>();
                if (view != null) return view;
            }
        }
        else
        {
            Debug.LogWarning("Input Debug: EventSystem.current 为空！无法进行 UI 射线检测。");
        }
        
        // 2. 2D 物理射线检测 (适用于 Collider2D)
        if (Camera.main != null&&false)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(pointerPos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                Debug.Log($"Input Debug: 2D 物理命中: {hit.collider.gameObject.name}");
                CubeView view = hit.collider.GetComponent<CubeView>();
                if (view != null) return view;
            }
        }
        else
        {
            Debug.LogError("Input Debug: Camera.main 为空！无法进行物理射线检测。");
        }

        // 3. 3D 物理射线检测 (备用)
        if (Camera.main != null&&false)
        {
            Ray ray = Camera.main.ScreenPointToRay(pointerPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                 CubeView view = hit.collider.GetComponent<CubeView>();
                 if (view != null) return view;
            }
        }

        return null;
        
    }
    private bool CheckAdjacent(Vector2Int pos)
    {
        // 检查相邻方块
        if(Mathf.Abs(pos.x - startCube.X) + Mathf.Abs(pos.y - startCube.Y) == 1)
            return true;
        return false;
    }
}
