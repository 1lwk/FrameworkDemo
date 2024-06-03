// ����һ������ϵͳ�Ľӿڣ������۲�ʵ��͸���ʵ��ķ���
public interface IUpdateSystem
{
    // ���ʵ���Ƿ��ɴ�ϵͳ�۲�
    bool ObservingEntity(ECSEntity entity);

    // ����ʵ��
    void Update(ECSEntity entity);
}

// ʹ���Զ������Ա�Ǵ���Ϊ ECS ϵͳ
[ECSSystem]
public abstract class UpdateSystem<C1> : IUpdateSystem where C1 : ECSComponent
{
    // ���ʵ���Ƿ������� C1
    public bool ObservingEntity(ECSEntity entity)
    {
        // ���ʵ�岻������� C1���򷵻� false
        if (!entity.HasComponent<C1>())
            return false;

        // ���ʵ�������� C1���򷵻� true
        return true;
    }

    // ���󷽷�����Ҫ����ʵ�֣����ڸ���ʵ��
    public abstract void Update(ECSEntity entity);
}

// ʹ���Զ������Ա�Ǵ���Ϊ ECS ϵͳ
[ECSSystem]
public abstract class UpdateSystem<C1, C2> : IUpdateSystem where C1 : ECSComponent where C2 : ECSComponent
{
    // ���ʵ���Ƿ������� C1 �� C2
    public bool ObservingEntity(ECSEntity entity)
    {
        // ���ʵ�岻������� C1���򷵻� false
        if (!entity.HasComponent<C1>())
            return false;

        // ���ʵ�岻������� C2���򷵻� false
        if (!entity.HasComponent<C2>())
            return false;

        // ���ʵ�������� C1 �� C2���򷵻� true
        return true;
    }

    // ���󷽷�����Ҫ����ʵ�֣����ڸ���ʵ��
    public abstract void Update(ECSEntity entity);
}

// ʹ���Զ������Ա�Ǵ���Ϊ ECS ϵͳ
[ECSSystem]
public abstract class UpdateSystem<C1, C2, C3> : IUpdateSystem where C1 : ECSComponent where C2 : ECSComponent where C3 : ECSComponent
{
    // ���ʵ���Ƿ������� C1��C2 �� C3
    public bool ObservingEntity(ECSEntity entity)
    {
        // ���ʵ�岻������� C1���򷵻� false
        if (!entity.HasComponent<C1>())
            return false;

        // ���ʵ�岻������� C2���򷵻� false
        if (!entity.HasComponent<C2>())
            return false;

        // ���ʵ�岻������� C3���򷵻� false
        if (!entity.HasComponent<C3>())
            return false;

        // ���ʵ�������� C1��C2 �� C3���򷵻� true
        return true;
    }

    // ���󷽷�����Ҫ����ʵ�֣����ڸ���ʵ��
    public abstract void Update(ECSEntity entity);
}
