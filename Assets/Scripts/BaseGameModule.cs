using UnityEngine;

/// <summary>
/// 基类游戏模块
/// </summary>
public class BaseGameModule : MonoBehaviour
{
    //封装了声明基本周期方法
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    private void OnDestroy() { }

    //定义了虚方法 实现了模块初试化 模块开始启动模块暂停 
    protected internal virtual void OnModuleInit() { }
    protected internal virtual void OnModuleStart() { }
    protected internal virtual void OnModuleStop() { }
    // 实现了每帧的更新 以及更新后和固定帧更新
    protected internal virtual void OnModuleUpdate(float delaTime) { }
    protected internal virtual void OnModuleLateUpdate(float deltaTime) { }
    protected internal virtual void OnModuleFixedUpdate(float delaTime) { }
}
