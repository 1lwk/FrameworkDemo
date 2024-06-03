public interface ILateUpdateSystem
{
    // ���ʵ���Ƿ��ɴ�ϵͳ�۲�
    bool ObservingEntity(ECSEntity entity);

    // ����ʵ��
    void LateUpdate(ECSEntity entity);
}

// ECSSystem ���ԣ����ڱ��ϵͳ
[ECSSystem]
public abstract class LateUpdateSystem<C1> : ILateUpdateSystem where C1 : ECSComponent
{
    // ���ʵ���Ƿ������� C1
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        return true;
    }

    // ���󷽷�����Ҫ����ʵ�֣����ڸ���ʵ��
    public abstract void LateUpdate(ECSEntity entity);
}

// ECSSystem ���ԣ����ڱ��ϵͳ
[ECSSystem]
public abstract class LateUpdateSystem<C1, C2> : ILateUpdateSystem where C1 : ECSComponent where C2 : ECSComponent
{
    // ���ʵ���Ƿ������� C1 �� C2
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        if (!entity.HasComponent<C2>())
            return false;

        return true;
    }

    // ���󷽷�����Ҫ����ʵ�֣����ڸ���ʵ��
    public abstract void LateUpdate(ECSEntity entity);
}

// ECSSystem ���ԣ����ڱ��ϵͳ
[ECSSystem]
public abstract class LateUpdateSystem<C1, C2, C3> : ILateUpdateSystem where C1 : ECSComponent where C2 : ECSComponent where C3 : ECSComponent
{
    // ���ʵ���Ƿ������� C1��C2 �� C3
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        if (!entity.HasComponent<C2>())
            return false;

        if (!entity.HasComponent<C3>())
            return false;

        return true;
    }

    // ���󷽷�����Ҫ����ʵ�֣����ڸ���ʵ��
    public abstract void LateUpdate(ECSEntity entity);
}
