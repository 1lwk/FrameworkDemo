using Config;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI模块类，管理所有的UI操作和逻辑
/// </summary>
public partial class UIModule : BaseGameModule // 继承自BaseGameModule的UI模块类
{
    // 各种UI的根节点
    public Transform normalUIRoot; // 正常UI的根节点
    public Transform modalUIRoot; // 模态UI的根节点
    public Transform closeUIRoot; // 关闭UI的根节点

    public Image imgMask; // UI遮罩图片

    // Quantum Console的预制件 用于调试和控制台操作
    public QuantumConsole prefabQuantumConsole; 

    // 用于存储UIViewID与Mediator实例
    private static Dictionary<UIViewID, Type> MEDIATOR_MAPPING;

    // 用于存储UIViewID与Asset的实例
    private static Dictionary<UIViewID, Type> ASSET_MAPPING;

    // 当前正在使用的UIMediator列表
    private readonly List<UIMediator> usingMediators = new List<UIMediator>();

    // 存储空闲的UIMediator实例，按类型分类
    private readonly Dictionary<Type, Queue<UIMediator>> freeMediators = new Dictionary<Type, Queue<UIMediator>>();

    // UI对象池，用于管理UI对象的加载和回收
    private readonly GameObjectPool<GameObjectAsset> uiObjectPool = new GameObjectPool<GameObjectAsset>();

    // Quantum Console实例
    private QuantumConsole quantumConsole;

    // 模块初始化方法
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit(); // 调用基类的初始化方法
        //quantumConsole = Instantiate(prefabQuantumConsole); // 实例化Quantum Console
        //quantumConsole.transform.SetParentAndResetAll(transform); // 设置Quantum Console的父对象并重置所有变换
        //quantumConsole.OnActivate += OnConsoleActive; // 订阅Quantum Console激活事件
        //quantumConsole.OnDeactivate += OnConsoleDeactive; // 订阅Quantum Console停用事件
    }

    // 模块停止方法
    protected internal override void OnModuleStop()
    {
        //base.OnModuleStop(); // 调用基类的停止方法
        //quantumConsole.OnActivate -= OnConsoleActive; // 取消订阅Quantum Console激活事件
        //quantumConsole.OnDeactivate -= OnConsoleDeactive; // 取消订阅Quantum Console停用事件
    }

    // 缓存和初始化 UIView的实例和UIMeditor实例
    private static void CacheUIMapping()
    {
        if (MEDIATOR_MAPPING != null) // 如果MEDIATOR_MAPPING已初始化，则返回
            return;

        MEDIATOR_MAPPING = new Dictionary<UIViewID, Type>(); // 初始化MEDIATOR_MAPPING字典
        ASSET_MAPPING = new Dictionary<UIViewID, Type>(); // 初始化ASSET_MAPPING字典

        Type baseViewType = typeof(UIView); // 获取UIView的类型
        foreach (var type in baseViewType.Assembly.GetTypes()) // 遍历所有类型
        {
            if (type.IsAbstract) // 如果类型是抽象的，则跳过
                continue;
            if (baseViewType.IsAssignableFrom(type)) // 如果类型是UIView的子类
            {
                //获取 type 类型上应用的所有 UIViewAttribute 特性 并将它们存储在 attrs 数组中 bool指的是是否去基类上查找
                object[] attrs = type.GetCustomAttributes(typeof(UIViewAttribute), false); 
                if (attrs.Length == 0) // 如果没有UIViewAttribute属性
                {
                    Debug.LogError($"{type.FullName} 没有绑定 Mediator，请使用UIMediatorAttribute绑定一个Mediator以正确使用"); // 输出错误日志
                    continue;
                }

                foreach (UIViewAttribute attr in attrs) // 遍历所有UIViewAttribute属性
                {
                    MEDIATOR_MAPPING.Add(attr.ID, attr.MediatorType); // 添加UIViewID和Mediator实例
                    ASSET_MAPPING.Add(attr.ID, type); // 添加UIViewID和Asset实例
                    break;
                }
            }
        }
    }

    // 模块更新方法
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime); // 调用基类的更新方法
        uiObjectPool.UpdateLoadRequests(); // 更新UI对象池的加载请求
        foreach (var mediator in usingMediators) // 遍历所有正在使用的UIMediator
        {
            mediator.Update(deltaTime); // 更新UIMediator
        }
        UpdateMask(deltaTime); // 更新UI遮罩
    }

    // Quantum Console激活事件处理方法
    private void OnConsoleActive()
    {
        //GameManager.Input.SetEnable(false); // 禁用游戏管理器的输入
    }

    // Quantum Console停用事件处理方法
    private void OnConsoleDeactive()
    {
        //GameManager.Input.SetEnable(true); // 启用游戏管理器的输入
    }

    // 获取指定模式下顶层Meditor的SortingOrder
    private int GetTopMediatorSortingOrder(UIMode mode)
    {
        int lastIndexMediatorOfMode = -1; // 初始化最后一个指定模式的UIMediator的索引
        for (int i = usingMediators.Count - 1; i >= 0; i--) // 逆序遍历usingMediators列表
        {
            UIMediator mediator = usingMediators[i]; // 获取当前UIMediator
            if (mediator.UIMode != mode) // 如果UIMediator的模式不是指定模式，则跳过
                continue;

            lastIndexMediatorOfMode = i; // 更新最后一个指定模式的UIMediator的索引
            break;
        }

        if (lastIndexMediatorOfMode == -1) // 如果没有找到指定模式的UIMediator
            return mode == UIMode.Normal ? 0 : 1000; // 返回默认排序顺序

        return usingMediators[lastIndexMediatorOfMode].SortingOrder; // 返回最后一个指定模式的UIMediator的排序顺序
    }

    // 获取指定UIViewID的UIMediator实例的方法
    private UIMediator GetMediator(UIViewID id)
    {
        CacheUIMapping(); // 缓存UI映射关系

        if (!MEDIATOR_MAPPING.TryGetValue(id, out Type mediatorType)) // 尝试获取UIViewID对应的Mediator类型
        {
            Debug.LogError($"找不到 {id} 对应的Mediator"); // 输出错误日志
            return null;
        }

        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ)) // 尝试获取对应类型的空闲UIMediator队列
        {
            mediatorQ = new Queue<UIMediator>(); // 初始化空闲UIMediator队列
            freeMediators.Add(mediatorType, mediatorQ); // 添加到freeMediators字典
        }

        UIMediator mediator;
        if (mediatorQ.Count == 0) // 如果没有空闲的UIMediator
        {
            mediator = Activator.CreateInstance(mediatorType) as UIMediator; // 创建新的UIMediator实例
        }
        else
        {
            mediator = mediatorQ.Dequeue(); // 从空闲队列中取出一个UIMediator实例
        }

        return mediator; // 返回UIMediator实例
    }

    // 回收UIMediator实例的方法
    private void RecycleMediator(UIMediator mediator)
    {
        if (mediator == null) // 如果mediator为空，则返回
            return;

        Type mediatorType = mediator.GetType(); // 获取mediator的类型
        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ)) // 尝试获取对应类型的空闲UIMediator队列
        {
            mediatorQ = new Queue<UIMediator>(); // 初始化空闲UIMediator队列
            freeMediators.Add(mediatorType, mediatorQ); // 添加到freeMediators字典
        }
        mediatorQ.Enqueue(mediator); // 将mediator加入空闲队列
    }

    // 获取指定UIViewID正在打开的UIMediator实例的方法
    public UIMediator GetOpeningUIMediator(UIViewID id)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id); // 获取UIViewID对应的UI配置
        if (uiConfig.IsNull) // 如果UI配置为空，则返回null
            return null;

        UIMediator mediator = GetMediator(id); // 获取UIMediator实例
        if (mediator == null) // 如果mediator为空，则返回null
            return null;

        Type requiredMediatorType = mediator.GetType(); // 获取mediator的类型
        foreach (var item in usingMediators) // 遍历所有正在使用的UIMediator
        {
            if (item.GetType() == requiredMediatorType) // 如果类型匹配，则返回该UIMediator
                return item;
        }
        return null; // 返回null
    }

    // 将指定UIViewID的UI提升到顶层的方法
    public void BringToTop(UIViewID id)
    {
        UIMediator mediator = GetOpeningUIMediator(id); // 获取正在打开的UIMediator
        if (mediator == null) // 如果mediator为空，则返回
            return;

        int topSortingOrder = GetTopMediatorSortingOrder(mediator.UIMode); // 获取顶层的排序顺序
        if (mediator.SortingOrder == topSortingOrder) // 如果mediator的排序顺序已经是顶层，则返回
            return;

        int sortingOrder = topSortingOrder + 10; // 计算新的排序顺序
        mediator.SortingOrder = sortingOrder; // 设置mediator的排序顺序

        usingMediators.Remove(mediator); // 从usingMediators列表中移除mediator
        usingMediators.Add(mediator); // 将mediator添加到usingMediators列表的末尾

        Canvas canvas = mediator.ViewObject.GetComponent<Canvas>(); // 获取mediator的Canvas组件
        if (canvas != null)
        {
            canvas.sortingOrder = sortingOrder; // 设置Canvas的排序顺序
        }
    }

    // 检查指定UIViewID的UI是否已打开的方法
    public bool IsUIOpened(UIViewID id)
    {
        return GetOpeningUIMediator(id) != null; // 如果获取到的UIMediator不为空，则返回true，否则返回false
    }

    // 打开单个UI的方法（如果已打开则返回当前实例）
    public UIMediator OpenUISingle(UIViewID id, object arg = null)
    {
        UIMediator mediator = GetOpeningUIMediator(id); // 获取正在打开的UIMediator
        if (mediator != null) // 如果mediator不为空，则返回mediator
            return mediator;

        return OpenUI(id, arg); // 否则，打开新的UI
    }

    // 打开UI的方法
    public UIMediator OpenUI(UIViewID id, object arg = null)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id); // 获取UIViewID对应的UI配置
        if (uiConfig.IsNull) // 如果UI配置为空，则返回null
            return null;

        UIMediator mediator = GetMediator(id); // 获取UIMediator实例
        if (mediator == null) // 如果mediator为空，则返回null
            return null;

        GameObject uiObject = (uiObjectPool.LoadGameObject(uiConfig.Asset, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>(); // 获取加载的UI对象的UIView组件
            mediator.InitMediator(newView); // 初始化UIMediator
        })).gameObject;
        return OnUIObjectLoaded(mediator, uiConfig, uiObject, arg); // 返回加载后的UIMediator实例
    }

    // 异步打开单个UI的方法（如果已打开则不再加载）
    public IEnumerator OpenUISingleAsync(UIViewID id, object arg = null)
    {
        if (!IsUIOpened(id)) // 如果UI未打开
        {
            yield return OpenUIAsync(id, arg); // 异步打开UI
        }
    }

    // 异步打开UI的方法
    public IEnumerator OpenUIAsync(UIViewID id, object arg = null)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id); // 获取UIViewID对应的UI配置
        if (uiConfig.IsNull) // 如果UI配置为空，则退出
            yield break;

        UIMediator mediator = GetMediator(id); // 获取UIMediator实例
        if (mediator == null) // 如果mediator为空，则退出
            yield break;

        bool loadFinish = false; // 初始化加载完成标志
        uiObjectPool.LoadGameObjectAsync(uiConfig.Asset, (asset) =>
        {
            GameObject uiObject = asset.gameObject; // 获取加载的UI对象
            OnUIObjectLoaded(mediator, uiConfig, uiObject, arg); // 处理加载完成的UI对象
            loadFinish = true; // 设置加载完成标志
        }, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>(); // 获取加载的UI对象的UIView组件
            mediator.InitMediator(newView); // 初始化UIMediator
        });
        while (!loadFinish) // 等待加载完成
        {
            yield return null;
        }
        yield return null; // 等待一帧
        yield return null; // 等待一帧
    }

    // 处理UI对象加载完成的方法
    private UIMediator OnUIObjectLoaded(UIMediator mediator, UIConfig uiConfig, GameObject uiObject, object obj)
    {
        if (uiObject == null) // 如果UI对象为空，则输出错误日志并回收mediator
        {
            Debug.LogError($"加载UI失败:{uiConfig.Asset}"); // 输出错误日志
            RecycleMediator(mediator); // 回收mediator
            return null;
        }

        UIView view = uiObject.GetComponent<UIView>(); // 获取UI对象的UIView组件
        if (view == null) // 如果UIView组件为空，则输出错误日志并回收mediator和UI对象
        {
            Debug.LogError($"UI Prefab不包含UIView脚本:{uiConfig.Asset}"); // 输出错误日志
            RecycleMediator(mediator); // 回收mediator
            uiObjectPool.UnloadGameObject(view.gameObject); // 卸载UI对象
            return null;
        }

        mediator.UIMode = uiConfig.Mode; // 设置mediator的UIMode
        int sortingOrder = GetTopMediatorSortingOrder(uiConfig.Mode) + 10; // 获取顶层Meditor的SortingOrder并加10

        usingMediators.Add(mediator); // 将mediator添加到usingMediators列表

        Canvas canvas = uiObject.GetComponent<Canvas>(); // 获取UI对象的Canvas组件
        canvas.renderMode = RenderMode.ScreenSpaceCamera; // 设置Canvas的渲染模式
        //canvas.worldCamera = GameManager.Camera.uiCamera; // 设置Canvas的相机
        if (uiConfig.Mode == UIMode.Normal)
        {
            uiObject.transform.SetParentAndResetAll(normalUIRoot); // 设置UI对象的父对象为normalUIRoot
            canvas.sortingLayerName = "NormalUI"; // 设置Canvas的排序层为NormalUI
        }
        else
        {
            uiObject.transform.SetParentAndResetAll(modalUIRoot); // 设置UI对象的父对象为modalUIRoot
            canvas.sortingLayerName = "ModalUI"; // 设置Canvas的排序层为ModalUI
        }

        mediator.SortingOrder = sortingOrder; // 设置mediator的SortingOrder
        canvas.sortingOrder = sortingOrder; // 设置Canvas的SortingOrder

        uiObject.SetActive(true); // 激活UI对象
        mediator.Show(uiObject, obj); // 显示UI
        return mediator; // 返回mediator
    }

    // 关闭UI的方法
    public void CloseUI(UIMediator mediator)
    {
        if (mediator != null) // 如果mediator不为空
        {
            // 回收View
            uiObjectPool.UnloadGameObject(mediator.ViewObject); // 卸载UI对象
            mediator.ViewObject.transform.SetParentAndResetAll(closeUIRoot); // 设置UI对象的父对象为closeUIRoot

            // 回收Mediator
            mediator.Hide(); // 隐藏mediator
            RecycleMediator(mediator); // 回收mediator

            usingMediators.Remove(mediator); // 从usingMediators列表中移除mediator
        }
    }

    // 关闭所有UI的方法
    public void CloseAllUI()
    {
        for (int i = usingMediators.Count - 1; i >= 0; i--) // 逆序遍历usingMediators列表
        {
            CloseUI(usingMediators[i]); // 关闭UI
        }
    }

    // 关闭指定UIViewID的UI的方法
    public void CloseUI(UIViewID id)
    {
        UIMediator mediator = GetOpeningUIMediator(id); // 获取正在打开的UIMediator
        if (mediator == null) // 如果mediator为空，则返回
            return;

        CloseUI(mediator); // 关闭UI
    }

    // 设置所有正常UI的可见性的方法
    public void SetAllNormalUIVisibility(bool visible)
    {
        normalUIRoot.gameObject.SetActive(visible); // 设置normalUIRoot的可见性
    }

    // 设置所有模态UI的可见性的方法
    public void SetAllModalUIVisibility(bool visible)
    {
        modalUIRoot.gameObject.SetActive(visible); // 设置modalUIRoot的可见性
    }

    // 显示遮罩的方法
    public void ShowMask(float duration = 0.5f)
    {
        destMaskAlpha = 1; // 设置目标遮罩透明度为1
        maskDuration = duration; // 设置遮罩动画持续时间
    }

    // 隐藏遮罩的方法
    public void HideMask(float? duration = null)
    {
        destMaskAlpha = 0; // 设置目标遮罩透明度为0
        if (duration.HasValue)
        {
            maskDuration = duration.Value; // 如果提供了持续时间，则更新遮罩动画持续时间
        }
    }

    private float destMaskAlpha = 0; // 目标遮罩透明度
    private float maskDuration = 0; // 遮罩动画持续时间
    private void UpdateMask(float deltaTime)
    {
        Color c = imgMask.color; // 获取当前遮罩颜色
        c.a = maskDuration > 0 ? Mathf.MoveTowards(c.a, destMaskAlpha, 1f / maskDuration * deltaTime) : destMaskAlpha; // 更新遮罩透明度
        c.a = Mathf.Clamp01(c.a); // 限制透明度在0到1之间
        imgMask.color = c; // 设置更新后的遮罩颜色
        imgMask.enabled = imgMask.color.a > 0; // 根据透明度设置遮罩的启用状态
    }

    // 显示Quantum Console的方法
    public void ShowConsole()
    {
        quantumConsole.Activate(); // 激活Quantum Console
    }
}

// 自定义属性，用于标记UIView类型和对应的Mediator类型
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class UIViewAttribute : Attribute
{
    public UIViewID ID { get; } // UIViewID
    public Type MediatorType { get; } // Mediator类型

    public UIViewAttribute(Type mediatorType, UIViewID id)
    {
        ID = id; // 设置UIViewID
        MediatorType = mediatorType; // 设置Mediator类型
    }
}
