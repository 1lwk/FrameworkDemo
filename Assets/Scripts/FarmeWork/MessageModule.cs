using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 消息模块，用于管理和处理全局和局部消息处理程序
/// </summary>
public class MessageModule : BaseGameModule
{
    /// <summary>
    /// 消息处理事件委托
    /// </summary>
    /// <typeparam name="T">消息参数类型</typeparam>
    /// <param name="arg">消息参数</param>
    public delegate Task MessageHandlerEventArgs<T>(T arg);

    private Dictionary<Type, List<object>> globalMessageHandlers; // 全局消息处理程序字典
    private Dictionary<Type, List<object>> localMessageHandlers; // 局部消息处理程序字典

    /// <summary>
    /// 监视器，用于监视消息
    /// </summary>
    public Monitor Monitor { get; private set; }

    /// <summary>
    /// 模块初始化时调用
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers = new Dictionary<Type, List<object>>();//初始化局部变量的字典
        Monitor = new Monitor();
        LoadAllMessageHandlers();
    }

    /// <summary>
    /// 模块停止时调用
    /// </summary>
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }

    /// <summary>
    /// 加载所有消息处理程序
    /// </summary>
    private void LoadAllMessageHandlers()
    {
        // 初始化一个字典，键为类型，值为对象列表
        globalMessageHandlers = new Dictionary<Type, List<object>>();

        //遍历调用该方法的程序集中的所有类型
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            //如果类型是抽象类，则跳过
            if (type.IsAbstract)
                continue;

            //获取类型上标记的 MessageHandlerAttribute 特性
            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);

            //如果类型上有 MessageHandlerAttribute 特性
            if (messageHandlerAttribute != null)
            {
                //创建该类型的实例，并强制转换为 IMessageHandler 接口
                IMessageHandler messageHandler = Activator.CreateInstance(type) as IMessageHandler;

                //如果 globalMessageHandlers 字典中没有该类型的键，则添加一个新的键值对
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }

                //将实例添加到对应类型键的列表中
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);
            }
        }
    }

    /// <summary>
    /// 订阅消息处理程序
    /// </summary>
    /// <typeparam name="T">消息参数类型</typeparam>
    /// <param name="handler">消息处理程序</param>
    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        Type argType = typeof(T);
        if (!localMessageHandlers.TryGetValue(argType, out var handlerList))
        {
            handlerList = new List<object>();
            localMessageHandlers.Add(argType, handlerList);
        }

        handlerList.Add(handler);
    }

    /// <summary>
    /// 取消订阅消息处理程序
    /// </summary>
    /// <typeparam name="T">消息参数类型</typeparam>
    /// <param name="handler">消息处理程序</param>
    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))
            return;

        handlerList.Remove(handler);
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <typeparam name="T">消息参数类型</typeparam>
    /// <param name="arg">消息参数</param>
    /// <returns>异步任务</returns>
    public async Task Post<T>(T arg) where T : struct
    {
        //处理全局消息处理程序
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))
        {
            foreach (var handler in globalHandlerList)
            {
                if (!(handler is MessageHandler<T> messageHandler))
                    continue;

                await messageHandler.HandlerMessage(arg);
            }
        }

        // 处理局部消息处理程序
        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))
        {
            List<object> list = ListPool<object>.Obtain();
            list.AddRangeNonAlloc(localHandlerList);
            foreach (var handler in list)
            {
                if (!(handler is MessageHandlerEventArgs<T> messageHandler))
                    continue;

                await messageHandler(arg);
            }
            ListPool<object>.Release(list);
        }
    }
}
