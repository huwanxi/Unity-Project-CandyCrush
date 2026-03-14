using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "CandyCrush/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("关卡配置")]
    [Tooltip("方块种类个数")]
    public int cubeTypesCount = 4;

    [Header("特殊方块")]
    [Tooltip("是否有特殊方块")]
    public bool hasSpecialBlocks = false;
    
    [Tooltip("启用的特殊方块类型")]
    public List<BonusType> availableSpecialBlocks;

    [Header("布局设置")]
    [Tooltip("棋盘宽度 (X轴个数)")]
    public int width = 9;
    
    [Tooltip("棋盘高度 (Y轴个数)")]
    public int height = 9;
    
    [Tooltip("方块间隔")]
    public Vector2 gridSpacing = new Vector2(100, 100);

    [Tooltip("起始位置 (左上角)")]
    public Vector2 startPosition = new Vector2(0, 0);

    [Header("掉落设置")]
    [Tooltip("初始掉落高度 (相对于第一排上方)")]
    public float dropHeight = 1000f;

    [Tooltip("每排掉落间隔时间 (秒)")]
    public float dropRowInterval = 0.1f;

    [System.Serializable]
    public class RowData
    {
        public List<bool> cells;
    }

    [Tooltip("自定义形状布局 (True=有效格子, False=空洞)")]
    public List<RowData> activeGrid;

    public LevelData()
    {
        activeGrid = new List<RowData>();
        for (int y = 0; y < height; y++)
        {
            RowData row = new RowData();
            row.cells = new List<bool>();
            for (int x = 0; x < width; x++)
            {
                row.cells.Add(true); // 默认为矩形填满
            }
            activeGrid.Add(row);
        }
    }

    /// <summary>
    /// 获取指定坐标是否为有效格子，矩阵坐标系 (row, col)
    /// </summary>
    public bool IsActive(int x, int y)
    {
        // 1. 如果 activeGrid 未配置 (null 或空)，默认全部有效
        if (activeGrid == null || activeGrid.Count == 0) return true;
        // 2. 基础边界检查
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        
        // 3. 如果行数不足，或者该行数据为空，默认该行有效 (根据用户需求：不填则全为true)
        if (y >= activeGrid.Count || activeGrid[y] == null) return true;

        // 4. 如果该行列数不足，默认该列有效
        if (activeGrid[y].cells == null || x >= activeGrid[y].cells.Count) return true;

        // 5. 返回配置的值
        return activeGrid[y].cells[x];
    }
}