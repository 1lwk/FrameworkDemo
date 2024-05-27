using UnityEditor;

public class BaseInspector : UnityEditor.Editor
{
    // 虚拟属性，默认返回 true，子类可以重写以控制是否绘制基础 GUI
    protected virtual bool DrawBaseGUI { get { return true; } }

    // 标志是否正在编译
    private bool isCompiling = false;

    // 虚方法，子类可以重写在编辑器更新时执行的操作
    protected virtual void OnInspectorUpdateInEditor() { }

    // 在启用检查器时调用
    private void OnEnable()
    {
        UnityEngine.Debug.Log("sss");
        OnInspectorEnable(); // 调用虚方法，供子类重写
        EditorApplication.update += UpdateEditor; // 订阅编辑器更新事件
    }

    // 虚方法，子类可以重写以在启用检查器时执行操作
    protected virtual void OnInspectorEnable() { }

    // 在禁用检查器时调用
    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor; // 取消订阅编辑器更新事件
        OnInspectorDisable(); // 调用虚方法，供子类重写
    }

    // 虚方法，子类可以重写以在禁用检查器时执行操作
    protected virtual void OnInspectorDisable() { }

    // 更新编辑器，在每帧调用
    private void UpdateEditor()
    {
        // 检查编译状态变化
        if (!isCompiling && EditorApplication.isCompiling)
        {
            isCompiling = true;
            OnCompileStart(); // 调用编译开始时的虚方法，供子类重写
        }
        else if (isCompiling && !EditorApplication.isCompiling)
        {
            isCompiling = false;
            OnCompileComplete(); // 调用编译完成时的虚方法，供子类重写
        }

        // 调用虚方法，供子类重写在编辑器更新时执行的操作
        OnInspectorUpdateInEditor();
    }

    // 绘制检查器 GUI
    public override void OnInspectorGUI()
    {
        if (DrawBaseGUI)
        {
            base.OnInspectorGUI(); // 调用基类的 OnInspectorGUI 以绘制默认 GUI
        }
    }

    // 虚方法，子类可以重写以在编译开始时执行操作
    protected virtual void OnCompileStart() { }

    // 虚方法，子类可以重写以在编译完成时执行操作
    protected virtual void OnCompileComplete() { }
}
