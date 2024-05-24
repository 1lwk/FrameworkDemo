using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class MessageModule : BaseGameModule
{
    public delegate Task MessageHandlerEventArgs<T>(T arg);

    private Dictionary<Type, List<object>> globalMessageHandlers;
    private Dictionary<Type, List<object>> localMessageHandlers;

    public Monitor Monitor { get; private set; }

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers = new Dictionary<Type, List<object>>();
        Monitor = new Monitor();
        LoadAllMessageHandlers();
    }

    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }

    private void LoadAllMessageHandlers()
    {
        // ��ʼ��һ���ֵ䣬��Ϊ���ͣ�ֵΪ�����б�
        globalMessageHandlers = new Dictionary<Type, List<object>>();

        // �������ø÷����ĳ����е���������
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            // ��������ǳ����࣬������
            if (type.IsAbstract)
                continue;

            // ��ȡ�����ϱ�ǵ� MessageHandlerAttribute ����
            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);

            // ����������� MessageHandlerAttribute ����
            if (messageHandlerAttribute != null)
            {
                // ���������͵�ʵ������ǿ��ת��Ϊ IMessageHander �ӿ�
                IMessageHandler messageHandler = Activator.CreateInstance(type) as IMessageHandler;

                // ��� globalMessageHandlers �ֵ���û�и����͵ļ���������һ���µļ�ֵ�� �������ǵ�¼�����MessageHandler
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }

                // ��ʵ�����ӵ���Ӧ���ͼ����б���
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);
            }
        }
    }

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

    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))
            return;

        handlerList.Remove(handler);
    }

    public async Task Post<T>(T arg) where T : struct
    {
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))
        {
            foreach (var handler in globalHandlerList)
            {
                if (!(handler is MessageHandler<T> messageHandler))
                    continue;

                await messageHandler.HandlerMessage(arg);
            }
        }

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
