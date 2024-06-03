using System;
using System.Threading.Tasks;

// ����һ���ӿڣ����ڴ���ʵ����Ϣ
public interface IEntityMessageHandler
{
    // ������Ϣ���͵� Type ����
    Type MessageType();
}

// ���ͽӿڣ��̳��� IEntityMessageHandler�����ڴ����ض����͵���Ϣ
public interface IEntityMessageHandler<M> : IEntityMessageHandler
{
    // �첽���������ڷ�����Ϣ��ָ��ʵ��
    Task Post(ECSEntity entity, M m);
}

// �����࣬ʹ�� EntityMessageHandler ���Ա�ǣ��ṩ��Ϣ����Ļ���ʵ��
[EntityMessageHandler]
public abstract class EntityMessageHandler<M> : IEntityMessageHandler<M>
{
    // ���󷽷�����������������ʵ�֣����ڴ�����Ϣ
    public abstract Task HandleMessage(ECSEntity entity, M message);

    // ʵ�ֽӿڷ�����������Ϣ������
    public Type MessageType()
    {
        return typeof(M);
    }

    // ʵ�ֽӿڷ������첽������Ϣ������
    public async Task Post(ECSEntity entity, M m)
    {
        await HandleMessage(entity, m);
    }
}

// �Զ������ԣ����ڱ������ʵ����Ϣ������
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class EntityMessageHandlerAttribute : Attribute
{
}

// ����һ���ӿڣ����ڴ���ʵ���Զ�̹��̵��ã�RPC��
public interface IEntityRpcHandler
{
    // ���� RPC �������͵� Type ����
    Type RpcType();
}

// ����һ���ӿڣ���ʾ RPC ��Ӧ
public interface IEntityRpcResponse
{
    // �Ƿ�������
    bool Error { get; set; }
    // ������Ϣ
    string ErrorMessage { get; set; }
}

// ���ͽӿڣ��̳��� IEntityRpcHandler�����ڴ����ض����͵� RPC �������Ӧ
public interface IEntityRpcHandler<Request, Response> : IEntityRpcHandler where Response : IEntityRpcResponse
{
    // �첽���������� RPC ���󲢷�����Ӧ
    Task<Response> Post(ECSEntity entity, Request request);
}

// �����࣬ʹ�� EntityRpcHandler ���Ա�ǣ��ṩ RPC ����Ļ���ʵ��
[EntityRpcHandler]
public abstract class EntityRpcHandler<Request, Response> : IEntityRpcHandler<Request, Response> where Response : IEntityRpcResponse
{
    // ���󷽷�����������������ʵ�֣����ڴ��� RPC ����
    public abstract Task<Response> HandleRpc(ECSEntity entity, Request request);

    // ʵ�ֽӿڷ������첽���� RPC ���󲢴���
    public async Task<Response> Post(ECSEntity entity, Request request)
    {
        return await HandleRpc(entity, request);
    }

    // ʵ�ֽӿڷ��������� RPC ���������
    public Type RpcType()
    {
        return typeof(Request);
    }
}

// �Զ������ԣ����ڱ������ʵ�� RPC ������
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class EntityRpcHandlerAttribute : Attribute
{
}
