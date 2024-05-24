using UnityEngine;

/// <summary>
/// ������Ϸģ��
/// </summary>
public class BaseGameModule : MonoBehaviour
{
    //��װ�������������ڷ���
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    private void OnDestroy() { }

    //�������鷽�� ʵ����ģ����Ի� ģ�鿪ʼ����ģ����ͣ 
    protected internal virtual void OnModuleInit() { }
    protected internal virtual void OnModuleStart() { }
    protected internal virtual void OnModuleStop() { }
    // ʵ����ÿ֡�ĸ��� �Լ����º�͹̶�֡����
    protected internal virtual void OnModuleUpdate(float delaTime) { }
    protected internal virtual void OnModuleLateUpdate(float deltaTime) { }
    protected internal virtual void OnModuleFixedUpdate(float delaTime) { }
}
