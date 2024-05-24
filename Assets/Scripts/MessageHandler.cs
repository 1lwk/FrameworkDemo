using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ����һ���ӿ�
/// </summary>
public interface IMessageHandler
{
    Type GetHandlerType();//������һ������ֵΪType�ķ���
}

/// <summary>
/// ʹ���Զ������� MessageHandler���б��
/// </summary>
/// <typeparam name="T"></typeparam>
[MessageHandler]
public abstract class MessageHandler<T> : IMessageHandler where T : struct//�̳�IMessageHandler�ӿ� �Է���T����Լ��
{
    /// <summary>
    /// ��д�ӿڵķ���
    /// </summary>
    /// <returns></returns>
    public Type GetHandlerType()
    {
        return typeof(T);
    }

    /// <summary>
    /// ������һ������ķ��� ��������������ʵ��
    /// </summary>
    /// <param name="arg">������һ������T</param>
    /// <returns></returns>
    public abstract Task HandlerMessage(T arg);
}

/// <summary>
/// ʵ���Զ�������MessageHandlerAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Class,Inherited =true,AllowMultiple =false)]//AttributeTargets.Class ��ָֻ���������� Inherited �Ƿ���Ա��̳� AllowMultiple ÿ�����Ƿ���Զ��Ӧ��
sealed class MessageHandlerAttribute: Attribute { } //�ܷ��಻�ܱ�����