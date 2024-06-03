// ����һ���ӿڣ���ʾһ���̶�����ϵͳ
public interface IFixedUpdateSystem
{
    // ����ϵͳ�Ƿ�۲�ָ����ʵ��
    bool ObservingEntity(ECSEntity entity);
    // �̶����·�������ÿ֡�Ĺ̶�ʱ��������
    void FixedUpdate(ECSEntity entity);
}

// ʹ���Զ������Ա��ΪECSϵͳ
[ECSSystem]
// ����һ�������࣬��ʾֻ��Ҫһ������Ĺ̶�����ϵͳ
public abstract class FixedUpdateSystem<C1> : IFixedUpdateSystem where C1 : ECSComponent
{
    // ʵ�ֹ۲�ʵ��ķ��������ʵ���Ƿ�������C1
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        return true;
    }

    // ����Ĺ̶����·���������ʵ���������ṩ
    public abstract void FixedUpdate(ECSEntity entity);
}

// ʹ���Զ������Ա��ΪECSϵͳ
[ECSSystem]
// ����һ�������࣬��ʾ��Ҫ��������Ĺ̶�����ϵͳ
public abstract class FixedUpdateSystem<C1, C2> : IFixedUpdateSystem where C1 : ECSComponent where C2 : ECSComponent
{
    // ʵ�ֹ۲�ʵ��ķ��������ʵ���Ƿ�������C1��C2
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        if (!entity.HasComponent<C2>())
            return false;

        return true;
    }

    // ����Ĺ̶����·���������ʵ���������ṩ
    public abstract void FixedUpdate(ECSEntity entity);
}

// ʹ���Զ������Ա��ΪECSϵͳ
[ECSSystem]
// ����һ�������࣬��ʾ��Ҫ��������Ĺ̶�����ϵͳ
public abstract class FixedUpdateSystem<C1, C2, C3> : IFixedUpdateSystem where C1 : ECSComponent where C2 : ECSComponent where C3 : ECSComponent
{
    // ʵ�ֹ۲�ʵ��ķ��������ʵ���Ƿ�������C1��C2��C3
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

    // ����Ĺ̶����·���������ʵ���������ṩ
    public abstract void FixedUpdate(ECSEntity entity);
}
