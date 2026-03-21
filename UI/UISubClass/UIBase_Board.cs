using UnityEngine;
using UnityEngine.UI;

public class UIBase_Board : UIBase
{
    private Image boardImage;

    public UIBase_Board(GameObject _gameObject) : base(_gameObject)
    {
        Transform boardTrans = gameObject.transform.Find("Board");
        if (boardTrans != null)
        {
            boardImage = boardTrans.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("UIBase_Board: 未在 UI_2 下找到 Board 子物体！");
        }

        // 绑定 UIManager 事件接口，当前 UI_2 的 ID 为 2
        UIManager.Instance.EventBind<BoardUpdateEventParam>(2, OnBoardUpdateEventTriggered);
    }

    private void OnBoardUpdateEventTriggered(BoardUpdateEventParam param)
    {
        UpdateBoardImage(param.levelId);
    }

    private void UpdateBoardImage(int levelId)
    {
        if (boardImage == null) return;

        // 根据关卡 ID 构建配置中的 key
        string boardKey = $"Board_{levelId}";
        
        // 从 ResourceConfigManager 中获取路径
        string path = ResourceConfigManager.GetPath(boardKey, "Json/UIJson");
        if (!string.IsNullOrEmpty(path))
        {
            // 加载精灵图
            Sprite boardSprite = Resources.Load<Sprite>(path);
            if (boardSprite != null)
            {
                boardImage.sprite = boardSprite;
                //boardImage.SetNativeSize();  可选：设置图片为原始大小
            }
            else
            {
                Debug.LogWarning($"UIBase_Board: 无法加载棋盘图片资源，路径: {path}");
            }
        }
        else
        {
            Debug.LogWarning($"UIBase_Board: 未在 JSON 配置中找到 {boardKey} 的路径");
        }
    }
}
