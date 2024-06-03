/// <summary>
/// 表示实体-组件-系统（ECS）框架中的所有组件的基类。
/// </summary>
public abstract class ECSComponent
{
    /// <summary>
    /// 获取此组件的唯一标识符
    /// </summary>
    public long ID { get; private set; }

    /// <summary>
    /// 获取或设置此组件关联的实体的标识符
    /// </summary>
    public long EntityID { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示此组件是否已被释放
    /// </summary>
    public bool Disposed { get; set; }

    /// <summary>
    /// 获取此组件关联的实体
    /// 如果 EntityID 为 0，则返回默认值
    /// </summary>
    public ECSEntity Entity
    {
        get
        {
            // 如果 EntityID 为 0，则返回默认值
            if (EntityID == 0)
                return default;

            // 通过 EntityID 从 ECS 模块中查找并返回对应的实体
            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(EntityID);
        }
    }

    /// <summary>
    /// 初始化 ECSComponent 类的新实例，并生成唯一的 ID
    /// </summary>
    public ECSComponent()
    {
        // 生成一个新的唯一 ID 并赋值给 ID 属性
        ID = IDGenerator.NewInstanceID();
        // 初始化 Disposed 属性为 false
        Disposed = false;
    }
}
