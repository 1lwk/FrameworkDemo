using System;
using System.Collections.Generic;

/// <summary>
/// 表示实体-组件-系统（ECS）框架中的实体类，实现了 IDisposable 接口
/// </summary>
public class ECSEntity : IDisposable
{
    /// <summary>
    /// 获取实体的唯一标识符
    /// </summary>
    public long InstanceID { get; private set; }

    /// <summary>
    /// 获取父实体的标识符
    /// </summary>
    public long ParentID { get; private set; }

    /// <summary>
    /// 获取一个值，指示此实体是否已被释放
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// 获取父实体
    /// 如果 ParentID 为 0，则返回默认值
    /// </summary>
    public ECSEntity Parent
    {
        get
        {
            if (ParentID == 0)
                return default;

            // 从 ECS 模块中查找并返回父实体
            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(ParentID);
        }
    }

    /// <summary>
    /// 获取或设置场景标识符
    /// </summary>
    public long SceneID { get; set; }

    /// <summary>
    /// 获取关联的场景
    /// 如果 SceneID 为 0，则返回默认值
    /// </summary>
    public ECSScene Scene
    {
        get
        {
            if (SceneID == 0)
                return default;

            // 从 ECS 模块中查找并返回关联的场景
            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(SceneID) as ECSScene;
        }
    }

    // 存储子实体的列表
    private List<ECSEntity> children = new List<ECSEntity>();

    // 存储组件的字典，键为组件类型，值为组件实例
    private Dictionary<Type, ECSComponent> componentMap = new Dictionary<Type, ECSComponent>();

    /// <summary>
    /// 初始化 ECSEntity 类的新实例，并生成唯一的 ID
    /// </summary>
    public ECSEntity()
    {
        // 生成一个新的唯一 ID 并赋值给 InstanceID 属性
        InstanceID = IDGenerator.NewInstanceID();
        // 将实体添加到 ECS 模块中
        TGameFramework.Instance.GetModule<ECSModule>().AddEntity(this);
    }

    /// <summary>
    /// 释放实体及其子实体和组件
    /// </summary>
    public virtual void Dispose()
    {
        if (Disposed)
            return;

        Disposed = true;

        // 释放子实体
        for (int i = children.Count - 1; i >= 0; i--)
        {
            ECSEntity child = children[i];
            children.RemoveAt(i);
            child?.Dispose();
        }

        // 释放组件
        List<ECSComponent> componentList = ListPool<ECSComponent>.Obtain();
        foreach (var component in componentMap.Values)
        {
            componentList.Add(component);
        }

        foreach (var component in componentList)
        {
            componentMap.Remove(component.GetType());
            TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent(component);
        }
        ListPool<ECSComponent>.Release(componentList);

        // 从父节点移除
        Parent?.RemoveChild(this);
        // 从 ECS 模块中移除实体
        TGameFramework.Instance.GetModule<ECSModule>().RemoveEntity(this);
    }

    /// <summary>
    /// 检查实体是否具有指定类型的组件。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <returns>如果具有该类型的组件，则返回 true；否则返回 false。</returns>
    public bool HasComponent<C>() where C : ECSComponent
    {
        return componentMap.ContainsKey(typeof(C));
    }

    /// <summary>
    /// 获取指定类型的组件。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <returns>返回指定类型的组件。</returns>
    public C GetComponent<C>() where C : ECSComponent
    {
        componentMap.TryGetValue(typeof(C), out var component);
        return component as C;
    }

    /// <summary>
    /// 添加一个新的指定类型的组件，并唤醒组件。
    /// 如果已存在该类型的组件，则先移除它。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <returns>返回新添加的组件。</returns>
    public C AddNewComponent<C>() where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
        return component;
    }

    /// <summary>
    /// 添加一个新的指定类型的组件，并使用一个参数唤醒组件。
    /// 如果已存在该类型的组件，则先移除它。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <typeparam name="P1">唤醒组件的参数类型。</typeparam>
    /// <param name="p1">唤醒组件的参数。</param>
    /// <returns>返回新添加的组件。</returns>
    public C AddNewComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
        return component;
    }

    /// <summary>
    /// 添加一个新的指定类型的组件，并使用两个参数唤醒组件。
    /// 如果已存在该类型的组件，则先移除它。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <typeparam name="P1">唤醒组件的第一个参数类型。</typeparam>
    /// <typeparam name="P2">唤醒组件的第二个参数类型。</typeparam>
    /// <param name="p1">唤醒组件的第一个参数。</param>
    /// <param name="p2">唤醒组件的第二个参数。</param>
    /// <returns>返回新添加的组件。</returns>
    public C AddNewComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
        return component;
    }

    /// <summary>
    /// 添加一个新的指定类型的组件，并唤醒组件。
    /// 如果已存在该类型的组件，则返回默认值。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <returns>返回新添加的组件或默认值。</returns>
    public C AddComponent<C>() where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityEngine.Debug.Log($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
        return component;
    }

    /// <summary>
    /// 添加一个新的指定类型的组件，并使用一个参数唤醒组件。
    /// 如果已存在该类型的组件，则返回默认值。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <typeparam name="P1">唤醒组件的参数类型。</typeparam>
    /// <param name="p1">唤醒组件的参数。</param>
    /// <returns>返回新添加的组件或默认值。</returns>
    public C AddComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityEngine.Debug.Log($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
        return component;
    }

    /// <summary>
    /// 添加一个新的指定类型的组件，并使用两个参数唤醒组件。
    /// 如果已存在该类型的组件，则返回默认值。
    /// </summary>
    /// <typeparam name="C">组件类型。</typeparam>
    /// <typeparam name="P1">唤醒组件的第一个参数类型。</typeparam>
    /// <typeparam name="P2">唤醒组件的第二个参数类型。</typeparam>
    /// <param name="p1">唤醒组件的第一个参数。</param>
    /// <param name="p2">唤醒组件的第二
    public C AddComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        // 检查实体中是否已经存在类型为C的组件
        if (HasComponent<C>())
        {
            // 如果组件已存在，记录一条日志消息
            UnityEngine.Debug.Log($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        // 创建一个新的组件实例
        C component = new C();
        // 设置组件的EntityID为实体的InstanceID
        component.EntityID = InstanceID;
        // 将组件添加到组件映射表中
        componentMap.Add(typeof(C), component);
        // 使用ECSModule初始化该组件，并传入提供的参数
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
        // 返回新创建的组件
        return component;
    }

    public void RemoveComponent<C>() where C : ECSComponent, new()
    {
        // 获取要移除的组件的类型
        Type componentType = typeof(C);
        // 尝试从组件映射表中获取该组件
        if (!componentMap.TryGetValue(componentType, out var component))
            return; // 如果组件不存在，则返回

        // 从组件映射表中移除该组件
        componentMap.Remove(componentType);
        // 使用ECSModule销毁该组件
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component);
    }

    public void RemoveComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        // 获取要移除的组件的类型
        Type componentType = typeof(C);
        // 尝试从组件映射表中获取该组件
        if (!componentMap.TryGetValue(componentType, out var component))
            return; // 如果组件不存在，则返回

        // 从组件映射表中移除该组件
        componentMap.Remove(componentType);
        // 使用ECSModule销毁该组件，并传入一个额外的参数
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1);
    }

    public void RemoveComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        // 获取要移除的组件的类型
        Type componentType = typeof(C);
        // 尝试从组件映射表中获取该组件
        if (!componentMap.TryGetValue(componentType, out var component))
            return; // 如果组件不存在，则返回

        // 从组件映射表中移除该组件
        componentMap.Remove(componentType);
        // 使用ECSModule销毁该组件，并传入两个额外的参数
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1, p2);
    }

    public void AddChild(ECSEntity child)
    {
        // 如果子实体为空，则返回
        if (child == null)
            return;

        // 如果子实体已被销毁，则返回
        if (child.Disposed)
            return;

        // 获取子实体的旧父实体
        ECSEntity oldParent = child.Parent;
        // 如果旧父实体不为空，从旧父实体中移除子实体
        if (oldParent != null)
        {
            oldParent.RemoveChild(child);
        }

        // 将子实体添加到当前实体的子实体列表中
        children.Add(child);
        // 设置子实体的父ID为当前实体的InstanceID
        child.ParentID = InstanceID;
    }

    public void RemoveChild(ECSEntity child)
    {
        // 如果子实体为空，则返回
        if (child == null)
            return;

        // 从子实体列表中移除子实体
        children.Remove(child);
        // 将子实体的父ID重置为0
        child.ParentID = 0;
    }

    public T FindChild<T>(long id) where T : ECSEntity
    {
        // 遍历子实体列表，查找与给定ID匹配的子实体
        foreach (var child in children)
        {
            if (child.InstanceID == id)
                return child as T; // 返回找到的子实体，强制转换为类型T
        }

        // 如果未找到匹配的子实体，返回默认值
        return default;
    }

    public T FindChild<T>(Predicate<T> predicate) where T : ECSEntity
    {
        // 遍历子实体列表，查找符合条件的子实体
        foreach (var child in children)
        {
            T c = child as T;
            if (c == null)
                continue;

            if (predicate.Invoke(c))
            {
                return c; // 返回第一个符合条件的子实体
            }
        }

        // 如果未找到符合条件的子实体，返回默认值
        return default;
    }

    public void FindChildren<T>(List<T> list) where T : ECSEntity
    {
        // 遍历子实体列表，查找类型为T的所有子实体
        foreach (var child in children)
        {
            if (child is T)
            {
                // 将找到的子实体添加到提供的列表中
                list.Add(child as T);
            }
        }
    }
}