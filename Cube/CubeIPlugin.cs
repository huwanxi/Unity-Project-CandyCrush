using UnityEngine;

public enum BonusType
{
    None,
    Horizontal, // 4-match horizontal
    Vertical,   // 4-match vertical
    Colorful    // 5-match
}

public struct cubeProperty
{
    //属性
    public int type;
    public BonusType bonusType;
    //位置
    public int x; public int y;
}

public enum Direction
{
     up,
     down,
     left,
     right
}
public interface ICube
{
    public void Start();
    public void Destory();
    
}
