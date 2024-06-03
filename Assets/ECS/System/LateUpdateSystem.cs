public interface ILateUpdateSystem
{
    // 检查实体是否由此系统观察
    bool ObservingEntity(ECSEntity entity);

    // 更新实体
    void LateUpdate(ECSEntity entity);
}

// ECSSystem 特性，用于标记系统
[ECSSystem]
public abstract class LateUpdateSystem<C1> : ILateUpdateSystem where C1 : ECSComponent
{
    // 检查实体是否具有组件 C1
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        return true;
    }

    // 抽象方法，需要子类实现，用于更新实体
    public abstract void LateUpdate(ECSEntity entity);
}

// ECSSystem 特性，用于标记系统
[ECSSystem]
public abstract class LateUpdateSystem<C1, C2> : ILateUpdateSystem where C1 : ECSComponent where C2 : ECSComponent
{
    // 检查实体是否具有组件 C1 和 C2
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        if (!entity.HasComponent<C2>())
            return false;

        return true;
    }

    // 抽象方法，需要子类实现，用于更新实体
    public abstract void LateUpdate(ECSEntity entity);
}

// ECSSystem 特性，用于标记系统
[ECSSystem]
public abstract class LateUpdateSystem<C1, C2, C3> : ILateUpdateSystem where C1 : ECSComponent where C2 : ECSComponent where C3 : ECSComponent
{
    // 检查实体是否具有组件 C1、C2 和 C3
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

    // 抽象方法，需要子类实现，用于更新实体
    public abstract void LateUpdate(ECSEntity entity);
}
