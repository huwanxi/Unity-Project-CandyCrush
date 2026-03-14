using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class CubeController
{
    private Cube cube;
    private GameObject cubeObject;
    private float gradientTime;
    private float fadeInTime;
    private float dropUnitTime;
    private float moveUnitTime;

    public int X => cube.property.x;
    public int Y => cube.property.y;
    public int Type => cube.property.type;
    public BonusType BonusType => cube.property.bonusType;

    public CubeController(GameObject _cubeObject, Vector2[,] _posMap,int _type)
    {
        cubeObject = _cubeObject;
        Image image = _cubeObject.GetComponentInChildren<Image>();
        RectTransform rectTransform = _cubeObject.GetComponentInChildren<RectTransform>();
        
        if(image == null)
            Debug.LogWarning($"CubeController: 缺少 Image 组件 (in {_cubeObject.name})");
        if (rectTransform == null)
            Debug.LogWarning($"CubeController: 缺少 RectTransform 组件 (in {_cubeObject.name})");
            
        cube = new Cube(rectTransform, image, _posMap,_type);
        gradientTime = StaticProperties.Instance.cubeData.gradientTime;
        fadeInTime = StaticProperties.Instance.cubeData.fadeInTime;
        dropUnitTime = StaticProperties.Instance.cubeData.dropUnitTime;
        moveUnitTime = StaticProperties.Instance.cubeData.moveUnitTime;
        _cubeObject.SetActive(false);
        
        // 附加输入系统视图
        var view = _cubeObject.GetComponent<CubeView>();
        if (view == null) view = _cubeObject.AddComponent<CubeView>();
        view.Init(this);

        // 确保有碰撞体以支持 Physics2D 射线检测
        var collider = _cubeObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = _cubeObject.AddComponent<BoxCollider2D>();
            // 根据 RectTransform 设置碰撞体大小
            if (rectTransform != null)
            {
                collider.size = rectTransform.rect.size;
                collider.offset = rectTransform.rect.center;
            }
        }
    }
     
    public void Start(Vector2 initpos,int x,int y)
    {
        cube.Start();
        cubeObject.SetActive(true); // 确保激活
        cubeObject.transform.position = initpos;
        cube.Gradient(false, 0);
        cube.Gradient(true, fadeInTime);
        
        
        // 更新视图坐标
        var view = cubeObject.GetComponent<CubeView>();
        if(view != null) view.UpdateCoordinates();
    }

    public async UniTask MoveAsync(int x, int y)
    {
        float time = moveUnitTime * Mathf.Max(Mathf.Abs(cube.property.x - x), Mathf.Abs(cube.property.y - y));
        await cube.Move(x, y, time).AsyncWaitForCompletion();
        // 移动后更新视图坐标
        var view = cubeObject.GetComponent<CubeView>();
        if(view != null) view.UpdateCoordinates();
    }

    public async UniTask DropWithGravityAsync(int x, int y, float time = -1)
    {
        if (time < 0) time = dropUnitTime * (Mathf.Abs(cube.property.y - y) + 1); // 简单的基于距离的时间估算
        // 更新坐标
        cube.property.x = x;
        cube.property.y = y;
        await cube.DropWithGravity(x, y, time).AsyncWaitForCompletion();
        // 掉落后更新视图坐标
        var view = cubeObject.GetComponent<CubeView>();
        if(view != null) view.UpdateCoordinates();
    }

    public async UniTask EliminateAnimAsync(float time = 0.3f)
    {
        await cube.EliminateAnim(time);
    }

    /// <summary>
    /// 重置方块状态（用于重新开始游戏时的复用）
    /// </summary>
    public void Reset(Vector2 startPos, int x, int y)
    {
        cubeObject.SetActive(true);
        cubeObject.transform.position = startPos;
        cube.property.x = x;
        cube.property.y = y;
        
        // 重置视觉状态
        cubeObject.transform.localScale = Vector3.one;
        var img = cubeObject.GetComponent<Image>();
        if (img != null) img.color = Color.white;
        var anim = cubeObject.GetComponent<Animator>();
        if (anim != null) anim.Rebind();
        cube.SetBonus(BonusType.None);

        // 更新视图坐标
        var view = cubeObject.GetComponent<CubeView>();
        if(view != null) view.UpdateCoordinates();
        
        // 播放入场/掉落动画
        cube.Gradient(false, 0);
        cube.Gradient(true, fadeInTime);
    }

    public void SetFocus(bool isFocus)
    {
        cube.ScaleAnim(isFocus);
    }

    public void SetBonus(BonusType bonusType)
    {
        cube.SetBonus(bonusType);
    }

    public void Move(int x,int y)
    {
        cube.Move(x,y, moveUnitTime*Mathf.Max(Mathf.Abs( cube.property.x-x), Mathf.Abs(cube.property.y - y)));
    }
    
    public bool IsDestroyed => cubeObject == null;

    public void Destory()
    {
        cube.Destory();
        if (StaticProperties.Instance != null && StaticProperties.Instance.cubeObjectPool != null && cubeObject != null)
        {
            StaticProperties.Instance.cubeObjectPool.ReturnCube(cubeObject, cube.property.type);
        }
        else if (cubeObject != null)
        {
            Object.Destroy(cubeObject);
        }
    }
}
