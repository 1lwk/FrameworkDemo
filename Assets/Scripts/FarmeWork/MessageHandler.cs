using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 定义一个接口
/// </summary>
public interface IMessageHandler
{
    Type GetHandlerType();//定义了一个返回值为Type的方法
}

/// <summary>
/// 使用自定义属性 MessageHandler进行标记
/// </summary>
/// <typeparam name="T"></typeparam>
[MessageHandler]
public abstract class MessageHandler<T> : IMessageHandler where T : struct//继承IMessageHandler接口 对泛型T进行约束
{
    /// <summary>
    /// 重写接口的方法
    /// </summary>
    /// <returns></returns>
    public Type GetHandlerType()
    {
        return typeof(T);
    }

    /// <summary>
    /// 定义了一个抽象的方法 可以在派生类中实现
    /// </summary>
    /// <param name="arg">传入了一个参数T</param>
    /// <returns></returns>
    public abstract Task HandlerMessage(T arg);
}

/// <summary>
/// 实现自定义属性MessageHandlerAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Class,Inherited =true,AllowMultiple =false)]//AttributeTargets.Class 是指只能作用于类 Inherited 是否可以被继承 AllowMultiple 每个类是否可以多次应用
sealed class MessageHandlerAttribute: Attribute { } //密封类不能被派生