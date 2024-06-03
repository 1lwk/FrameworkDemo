using System;
using System.Threading.Tasks;

// 定义一个接口，用于处理实体消息
public interface IEntityMessageHandler
{
    // 返回消息类型的 Type 对象
    Type MessageType();
}

// 泛型接口，继承自 IEntityMessageHandler，用于处理特定类型的消息
public interface IEntityMessageHandler<M> : IEntityMessageHandler
{
    // 异步方法，用于发送消息给指定实体
    Task Post(ECSEntity entity, M m);
}

// 抽象类，使用 EntityMessageHandler 属性标记，提供消息处理的基本实现
[EntityMessageHandler]
public abstract class EntityMessageHandler<M> : IEntityMessageHandler<M>
{
    // 抽象方法，必须在派生类中实现，用于处理消息
    public abstract Task HandleMessage(ECSEntity entity, M message);

    // 实现接口方法，返回消息的类型
    public Type MessageType()
    {
        return typeof(M);
    }

    // 实现接口方法，异步发送消息并处理
    public async Task Post(ECSEntity entity, M m)
    {
        await HandleMessage(entity, m);
    }
}

// 自定义属性，用于标记类是实体消息处理器
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class EntityMessageHandlerAttribute : Attribute
{
}

// 定义一个接口，用于处理实体的远程过程调用（RPC）
public interface IEntityRpcHandler
{
    // 返回 RPC 请求类型的 Type 对象
    Type RpcType();
}

// 定义一个接口，表示 RPC 响应
public interface IEntityRpcResponse
{
    // 是否发生错误
    bool Error { get; set; }
    // 错误信息
    string ErrorMessage { get; set; }
}

// 泛型接口，继承自 IEntityRpcHandler，用于处理特定类型的 RPC 请求和响应
public interface IEntityRpcHandler<Request, Response> : IEntityRpcHandler where Response : IEntityRpcResponse
{
    // 异步方法，发送 RPC 请求并返回响应
    Task<Response> Post(ECSEntity entity, Request request);
}

// 抽象类，使用 EntityRpcHandler 属性标记，提供 RPC 处理的基本实现
[EntityRpcHandler]
public abstract class EntityRpcHandler<Request, Response> : IEntityRpcHandler<Request, Response> where Response : IEntityRpcResponse
{
    // 抽象方法，必须在派生类中实现，用于处理 RPC 请求
    public abstract Task<Response> HandleRpc(ECSEntity entity, Request request);

    // 实现接口方法，异步发送 RPC 请求并处理
    public async Task<Response> Post(ECSEntity entity, Request request)
    {
        return await HandleRpc(entity, request);
    }

    // 实现接口方法，返回 RPC 请求的类型
    public Type RpcType()
    {
        return typeof(Request);
    }
}

// 自定义属性，用于标记类是实体 RPC 处理器
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class EntityRpcHandlerAttribute : Attribute
{
}
