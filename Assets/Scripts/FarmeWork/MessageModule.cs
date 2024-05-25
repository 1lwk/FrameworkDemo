using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// ��Ϣģ�飬���ڹ���ʹ���ȫ�ֺ;ֲ���Ϣ�������
/// </summary>
public class MessageModule : BaseGameModule
{
    /// <summary>
    /// ��Ϣ�����¼�ί��
    /// </summary>
    /// <typeparam name="T">��Ϣ��������</typeparam>
    /// <param name="arg">��Ϣ����</param>
    public delegate Task MessageHandlerEventArgs<T>(T arg);

    private Dictionary<Type, List<object>> globalMessageHandlers; // ȫ����Ϣ��������ֵ�
    private Dictionary<Type, List<object>> localMessageHandlers; // �ֲ���Ϣ��������ֵ�

    /// <summary>
    /// �����������ڼ�����Ϣ
    /// </summary>
    public Monitor Monitor { get; private set; }

    /// <summary>
    /// ģ���ʼ��ʱ����
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers = new Dictionary<Type, List<object>>();//��ʼ���ֲ��������ֵ�
        Monitor = new Monitor();
        LoadAllMessageHandlers();
    }

    /// <summary>
    /// ģ��ֹͣʱ����
    /// </summary>
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }

    /// <summary>
    /// ����������Ϣ�������
    /// </summary>
    private void LoadAllMessageHandlers()
    {
        // ��ʼ��һ���ֵ䣬��Ϊ���ͣ�ֵΪ�����б�
        globalMessageHandlers = new Dictionary<Type, List<object>>();

        //�������ø÷����ĳ����е���������
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            //��������ǳ����࣬������
            if (type.IsAbstract)
                continue;

            //��ȡ�����ϱ�ǵ� MessageHandlerAttribute ����
            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);

            //����������� MessageHandlerAttribute ����
            if (messageHandlerAttribute != null)
            {
                //���������͵�ʵ������ǿ��ת��Ϊ IMessageHandler �ӿ�
                IMessageHandler messageHandler = Activator.CreateInstance(type) as IMessageHandler;

                //��� globalMessageHandlers �ֵ���û�и����͵ļ��������һ���µļ�ֵ��
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }

                //��ʵ����ӵ���Ӧ���ͼ����б���
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);
            }
        }
    }

    /// <summary>
    /// ������Ϣ�������
    /// </summary>
    /// <typeparam name="T">��Ϣ��������</typeparam>
    /// <param name="handler">��Ϣ�������</param>
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
    /// ȡ��������Ϣ�������
    /// </summary>
    /// <typeparam name="T">��Ϣ��������</typeparam>
    /// <param name="handler">��Ϣ�������</param>
    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))
            return;

        handlerList.Remove(handler);
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <typeparam name="T">��Ϣ��������</typeparam>
    /// <param name="arg">��Ϣ����</param>
    /// <returns>�첽����</returns>
    public async Task Post<T>(T arg) where T : struct
    {
        //����ȫ����Ϣ�������
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))
        {
            foreach (var handler in globalHandlerList)
            {
                if (!(handler is MessageHandler<T> messageHandler))
                    continue;

                await messageHandler.HandlerMessage(arg);
            }
        }

        // ����ֲ���Ϣ�������
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
