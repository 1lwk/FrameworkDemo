// 定义一个接口，表示一个固定更新系统
public interface IFixedUpdateSystem
{
    // 检查该系统是否观察指定的实体
    bool ObservingEntity(ECSEntity entity);
    // 固定更新方法，在每帧的固定时间间隔调用
    void FixedUpdate(ECSEntity entity);
}

// 使用自定义特性标记为ECS系统
[ECSSystem]
// 定义一个抽象类，表示只需要一个组件的固定更新系统
public abstract class FixedUpdateSystem<C1> : IFixedUpdateSystem where C1 : ECSComponent
{
    // 实现观察实体的方法，检查实体是否包含组件C1
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        return true;
    }

    // 抽象的固定更新方法，具体实现由子类提供
    public abstract void FixedUpdate(ECSEntity entity);
}

// 使用自定义特性标记为ECS系统
[ECSSystem]
// 定义一个抽象类，表示需要两个组件的固定更新系统
public abstract class FixedUpdateSystem<C1, C2> : IFixedUpdateSystem where C1 : ECSComponent where C2 : ECSComponent
{
    // 实现观察实体的方法，检查实体是否包含组件C1和C2
    public bool ObservingEntity(ECSEntity entity)
    {
        if (!entity.HasComponent<C1>())
            return false;

        if (!entity.HasComponent<C2>())
            return false;

        return true;
    }

    // 抽象的固定更新方法，具体实现由子类提供
    public abstract void FixedUpdate(ECSEntity entity);
}

// 使用自定义特性标记为ECS系统
[ECSSystem]
// 定义一个抽象类，表示需要三个组件的固定更新系统
public abstract class FixedUpdateSystem<C1, C2, C3> : IFixedUpdateSystem where C1 : ECSComponent where C2 : ECSComponent where C3 : ECSComponent
{
    // 实现观察实体的方法，检查实体是否包含组件C1、C2和C3
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

    // 抽象的固定更新方法，具体实现由子类提供
    public abstract void FixedUpdate(ECSEntity entity);
}
