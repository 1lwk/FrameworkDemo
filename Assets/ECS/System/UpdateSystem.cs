// 定义一个更新系统的接口，包含观察实体和更新实体的方法
public interface IUpdateSystem
{
    // 检查实体是否由此系统观察
    bool ObservingEntity(ECSEntity entity);

    // 更新实体
    void Update(ECSEntity entity);
}

// 使用自定义特性标记此类为 ECS 系统
[ECSSystem]
public abstract class UpdateSystem<C1> : IUpdateSystem where C1 : ECSComponent
{
    // 检查实体是否具有组件 C1
    public bool ObservingEntity(ECSEntity entity)
    {
        // 如果实体不具有组件 C1，则返回 false
        if (!entity.HasComponent<C1>())
            return false;

        // 如果实体具有组件 C1，则返回 true
        return true;
    }

    // 抽象方法，需要子类实现，用于更新实体
    public abstract void Update(ECSEntity entity);
}

// 使用自定义特性标记此类为 ECS 系统
[ECSSystem]
public abstract class UpdateSystem<C1, C2> : IUpdateSystem where C1 : ECSComponent where C2 : ECSComponent
{
    // 检查实体是否具有组件 C1 和 C2
    public bool ObservingEntity(ECSEntity entity)
    {
        // 如果实体不具有组件 C1，则返回 false
        if (!entity.HasComponent<C1>())
            return false;

        // 如果实体不具有组件 C2，则返回 false
        if (!entity.HasComponent<C2>())
            return false;

        // 如果实体具有组件 C1 和 C2，则返回 true
        return true;
    }

    // 抽象方法，需要子类实现，用于更新实体
    public abstract void Update(ECSEntity entity);
}

// 使用自定义特性标记此类为 ECS 系统
[ECSSystem]
public abstract class UpdateSystem<C1, C2, C3> : IUpdateSystem where C1 : ECSComponent where C2 : ECSComponent where C3 : ECSComponent
{
    // 检查实体是否具有组件 C1、C2 和 C3
    public bool ObservingEntity(ECSEntity entity)
    {
        // 如果实体不具有组件 C1，则返回 false
        if (!entity.HasComponent<C1>())
            return false;

        // 如果实体不具有组件 C2，则返回 false
        if (!entity.HasComponent<C2>())
            return false;

        // 如果实体不具有组件 C3，则返回 false
        if (!entity.HasComponent<C3>())
            return false;

        // 如果实体具有组件 C1、C2 和 C3，则返回 true
        return true;
    }

    // 抽象方法，需要子类实现，用于更新实体
    public abstract void Update(ECSEntity entity);
}
