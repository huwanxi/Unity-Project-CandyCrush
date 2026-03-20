using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

public class Cube:ICube
{
    //组件
    private RectTransform trans;
    private Image image;
    private Animator animator;

    //属性
    public cubeProperty property = new cubeProperty();
    private Vector2[,] posMap;
    private Material defaultMaterial;
    private Material glowMaterial;
    private Vector3 originalScale; // 用于记录方块初始化的真实大小

    public Cube(RectTransform _trans, Image _image, Vector2[,] _posMap,int _type)
    {
        
        trans = _trans;
        image = _image;
        posMap = _posMap;
        property.type = _type;
        animator = trans.GetComponent<Animator>();
        glowMaterial = trans.GetComponent<Material>();

        originalScale = trans.localScale; // 记录真实大小
        if (originalScale == Vector3.zero) originalScale = Vector3.one;

        if (animator != null)
            animator.Play("Start");
        if (image != null)
        {
            defaultMaterial = image.material;
        }
    }
    

    //生命周期
    public void Start()
    {

        
    }

    public void SetBonus(BonusType bonusType)
    {
        property.bonusType = bonusType;
        if (image == null) return;

        if (bonusType == BonusType.None)
        {
            image.material = defaultMaterial;
        }
        else
        {
            if (glowMaterial == null)
            {
                Shader shader = Shader.Find("UI/EdgeGlow");
                if (shader != null)
                {
                    glowMaterial = new Material(shader);
                }
            }

            if (glowMaterial != null)
            {
                image.material = glowMaterial;
                Color glowColor = Color.white;
                switch (bonusType)
                {
                    case BonusType.Horizontal:
                        glowColor = Color.blue;
                        break;
                    case BonusType.Vertical:
                        glowColor = Color.green;
                        
                        break;
                    case BonusType.Colorful:
                        glowColor = Color.yellow;
                        break;
                }
                glowMaterial.SetColor("_GlowColor", glowColor);
                glowMaterial.SetFloat("_GlowThickness", 0.4f);
                glowMaterial.SetFloat("_GlowIntensity", 8.0f);
            }
        }
    }
    public Tween Move(int x, int y, float time = 1)
    {
        property.x = x;
        property.y = y;
        if (trans == null) return null;
        return trans.DOMove(posMap[x,y], time);
    }
    public Tween Gradient(bool appear = true, float time = 1)
    {
        if (image == null) return null;
        
        if (!appear)
            return image.DOColor(Color.clear, time);
        else
            return image.DOColor(Color.white, time);
    }
    public Tween DropWithGravity(int x,int y,float time=0.8f)
    {
        
        float endY = posMap[x,y].y;
        float startY = trans.position.y;
        float midY = Mathf.Max(startY, endY) + Mathf.Abs(startY - endY) * 0.3f;

        if (animator != null) ;//animator.Play("Drop");下落没有动画

        return DOVirtual.Float(0, 1, time, t =>
        {
            float u = 1 - t;
            float y = (u * u) * startY +
                     (2 * u * t) * midY +
            (t * t) * endY;

            Vector3 pos = trans.position;
            pos.y = y;
            trans.position = pos;
        });
    }


    public async UniTask EliminateAnim(float time = 0.35f)
    {
        if(animator != null) animator.Play("Eliminate");
        //TimeSpan.FromSeconds 5秒时间变换
        await UniTask.Delay(TimeSpan.FromSeconds(time));
    }

    public Tween ScaleAnim(bool isScaleUp, float time = 0.2f)
    {
        // 基于 originalScale 进行比例缩放，比如放大是原来的 1.1 倍，恢复是 1 倍
        Vector3 targetScale = isScaleUp ? originalScale * 1.2f : originalScale;
        return trans.DOScale(targetScale, time);
    }

    public void PlaySound(string key, float volume = 1f, bool loop = false, bool overrideSame = true)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(key, volume, loop, overrideSame, true);
        }
    }

    public void Destory()
    {
        
        if (trans != null) trans.DOKill();
        if (image != null) image.DOKill();
        // 重置状态以便对象池复用
        if (trans != null) trans.localScale = originalScale;
        if (image != null) 
        {
            image.color = Color.white;
            image.material = defaultMaterial;
        }
        if(animator != null) animator.Rebind(); // 重置动画状态
        property.bonusType = BonusType.None;
    }
}
