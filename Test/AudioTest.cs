//using UnityEngine;
//using System.Collections.Generic;

//public class AudioTest : MonoBehaviour
//{
//    public GameObject player;
//    private RuntimeAnimatorController animatorController;
    
//    void Start()
//    {
//        animatorController = player.GetComponent<Animator>().runtimeAnimatorController;
        
//        // 为指定动画剪辑添加事件
//        AddEventToAnimation("Idel", 0.3f, "OnIdel");
//        AddEventToAnimation("Jump", 0.1f, "OnJump");
//        AddEventToAnimation("Move", 0.5f, "OnMove");
//    }
    
//    void AddEventToAnimation(string clipName, float eventTime, string functionName)
//    {
//        AnimationClip clip = GetAnimationClip(clipName);
//        if (clip != null)
//        {
//            // 创建动画事件
//            AnimationEvent animationEvent = new AnimationEvent();
//            animationEvent.time = eventTime;        // 触发时间（秒）
//            animationEvent.functionName = functionName;  // 回调函数名
//            animationEvent.stringParameter = clipName;    // 可传递字符串参数
//            animationEvent.intParameter = 0;               // 可传递整数参数
//            animationEvent.floatParameter = 1.0f;          // 可传递浮点数参数
            
//            // 添加事件到剪辑
//            clip.AddEvent(animationEvent);
//        }
//    }
//    private void GetAnimationClip(string clipName)
//    {
//        foreach (AnimationClip clip in animatorController.animationClips)
//        {
//            if (clip.name == clipName)
//            {
//                return clip;
//            }
//        }
//        return null;
//    }
//    // 事件回调方法
//    public void OnAttackHit(string clipName)
//    {
//        Debug.Log($"攻击命中！来自动画：{clipName}");
//        // 在这里添加伤害判定逻辑
//    }
    
//    public void OnJumpStart()
//    {
//        Debug.Log("开始跳跃");
//        GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
//    }
    
//    public void OnJumpPeak()
//    {
//        Debug.Log("到达跳跃最高点");
//    }
//}