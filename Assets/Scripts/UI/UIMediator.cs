using Config;
using Nirvana;
using System.Xml;
using UnityEngine;

/// <summary>
/// 抽象类 UI的Meditor基类，提供UI界面的管理
/// </summary>
/// <typeparam name="T">泛型 可以是各种UI界面 要求必须是UIView的子类</typeparam>
public abstract class UIMediator<T> : UIMediator where T : UIView
{
    protected T view;// 这个是UIview的对象

    /// <summary>
    ///重写基类的展示方法 显示UI界面时候调用
    /// </summary>
    /// <param name="arg">显示ui时传入的参数</param>
    protected override void OnShow(object arg)
    {
        base.OnShow(arg);
        view = ViewObject.GetComponent<T>();

    }

    protected override void OnHide()
    {
        view = default;
        base.OnHide();
    }

    protected void Close()
    {
        TGameFramework.Instance.GetModule<UIModule>().CloseUI(this);
    }

    public override void InitMediator(UIView view)
    {
        base.InitMediator(view);

        OnInit(view as T);
    }

    protected virtual void OnInit(T view) { }
}

/// <summary>
/// UI的Mediator基类，定义了基本的UI管理方法
/// </summary>
public abstract class UIMediator
{
    // 事件，在隐藏UI时触发
    public event System.Action OnMediatorHide;

    // UI视图对象
    public GameObject ViewObject { get; set; }
    // 事件表，用于管理UI事件
    public UIEventTable eventTable { get; set; }
    // 名称表，用于管理UI元素名称
    public UINameTable nameTable { get; set; }
    // 排序顺序，用于控制UI的显示层级
    public int SortingOrder { get; set; }
    // UI模式，用于定义UI的显示模式
    public UIMode UIMode { get; set; }

    /// <summary>
    /// 初始化Mediator
    /// </summary>
    /// <param name="view">UIView对象</param>
    public virtual void InitMediator(UIView view) { }

    /// <summary>
    /// 显示UI方法
    /// </summary>
    /// <param name="viewObject"></param>
    /// <param name="arg"></param>
    public void Show(GameObject viewObject, object arg)
    {
        ViewObject = viewObject;
        // 获取事件表组件
        eventTable = ViewObject.GetComponent<UIEventTable>();
        // 获取名称表组件
        nameTable = viewObject.GetComponent<UINameTable>();
        // 调用OnShow方法
        OnShow(arg);
    }

    /// <summary>
    /// 当显示UI时调用此方法
    /// </summary>
    /// <param name="arg"></param>
    protected virtual void OnShow(object arg) { }

    /// <summary>
    /// 隐藏UI
    /// </summary>
    public void Hide()
    {
        // 调用OnHide方法
        OnHide();
        // 触发隐藏事件
        OnMediatorHide?.Invoke();
        OnMediatorHide = null;
        // 清空视图对象
        ViewObject = default;
    }

    /// <summary>
    /// 当隐藏UI时调用此方法
    /// </summary>
    protected virtual void OnHide() { }

    /// <summary>
    /// 更新UI
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        OnUpdate(deltaTime);
    }

    /// <summary>
    /// 当更新UI时调用此方法
    /// </summary>
    /// <param name="deltaTime"></param>
    protected virtual void OnUpdate(float deltaTime) { }
}

