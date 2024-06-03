using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class ECSModule : BaseGameModule
{
    // 世界实例
    public ECSWorld World { get; private set; }

    // 各种系统的字典映射
    private Dictionary<Type, IAwakeSystem> awakeSystemMap;//awake系统的字典
    private Dictionary<Type, IDestroySystem> destroySystemMap;//Destory系统的字典

    private Dictionary<Type, IUpdateSystem> updateSystemMap;//update系统的字典
    private Dictionary<IUpdateSystem, List<ECSEntity>> updateSystemRelatedEntityMap;//update系统关联的实体容器

    private Dictionary<Type, ILateUpdateSystem> lateUpdateSystemMap;//lateupdate系统的字典
    private Dictionary<ILateUpdateSystem, List<ECSEntity>> lateUpdateSystemRelatedEntityMap;//lateupdate系统关联的实体容器

    private Dictionary<Type, IFixedUpdateSystem> fixedUpdateSystemMap;//fixedupdate系统的字典
    private Dictionary<IFixedUpdateSystem, List<ECSEntity>> fixedUpdateSystemRelatedEntityMap;//fixed系统关联的实体容器

    // 实体的字典映射
    private Dictionary<long, ECSEntity> entities = new Dictionary<long, ECSEntity>();
    // 实体消息处理器和RPC处理器的字典映射
    private Dictionary<Type, List<IEntityMessageHandler>> entityMessageHandlerMap;
    private Dictionary<Type, IEntityRpcHandler> entityRpcHandlerMap;

    // 模块初始化时调用
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        // 加载所有系统
        LoadAllSystems();
        // 初始化世界
        World = new ECSWorld();
    }

    // 模块每帧更新时调用
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        // 驱动更新系统
        DriveUpdateSystem();
    }

    // 模块每帧后更新时调用
    protected internal override void OnModuleLateUpdate(float deltaTime)
    {
        base.OnModuleLateUpdate(deltaTime);
        // 驱动后更新系统
        DriveLateUpdateSystem();
    }

    // 模块每帧固定更新时调用
    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
        // 驱动固定更新系统
        DriveFixedUpdateSystem();
    }

    /// <summary>
    /// 加载所有系统
    /// </summary>
    public void LoadAllSystems()
    {
        // 初始化各种系统的字典映射
        awakeSystemMap = new Dictionary<Type, IAwakeSystem>();
        destroySystemMap = new Dictionary<Type, IDestroySystem>();

        updateSystemMap = new Dictionary<Type, IUpdateSystem>();
        updateSystemRelatedEntityMap = new Dictionary<IUpdateSystem, List<ECSEntity>>();

        lateUpdateSystemMap = new Dictionary<Type, ILateUpdateSystem>();
        lateUpdateSystemRelatedEntityMap = new Dictionary<ILateUpdateSystem, List<ECSEntity>>();

        fixedUpdateSystemMap = new Dictionary<Type, IFixedUpdateSystem>();
        fixedUpdateSystemRelatedEntityMap = new Dictionary<IFixedUpdateSystem, List<ECSEntity>>();

        entityMessageHandlerMap = new Dictionary<Type, List<IEntityMessageHandler>>();
        entityRpcHandlerMap = new Dictionary<Type, IEntityRpcHandler>();

        // 遍历当前程序集中的所有类型
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            // 如果类型是抽象的，跳过
            if (type.IsAbstract)
                continue;

            // 如果类型有ECSSystemAttribute特性
            if (type.GetCustomAttribute<ECSSystemAttribute>(true) != null)
            {
                // AwakeSystem 获取IAwakeSystem接口的Type对象
                Type awakeSystemType = typeof(IAwakeSystem);
                //判断awakeSystem与type是否为基类、接口或同一个类型
                if (awakeSystemType.IsAssignableFrom(type))
                {
                    //判断awake字典中是否有这个类型的key
                    if (awakeSystemMap.ContainsKey(type))
                    {
                        UnityEngine.Debug.Log($"Duplicated Awake System:{type.FullName}");
                        continue;
                    }

                    // 实例化AwakeSystem并添加到字典
                    IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem;
                    awakeSystemMap.Add(type, awakeSystem);
                }

                // DestroySystem 获取IDestroySystem接口的Type对象
                Type destroySystemType = typeof(IDestroySystem);
                //判断destroySystemType与type是否是基类、接口或同一个类型
                if (destroySystemType.IsAssignableFrom(type))
                {
                    if (destroySystemMap.ContainsKey(type))
                    {
                        UnityEngine.Debug.Log($"Duplicated Destroy System:{type.FullName}");
                        continue;
                    }

                    // 实例化DestroySystem并添加到字典
                    IDestroySystem destroySytem = Activator.CreateInstance(type) as IDestroySystem;
                    destroySystemMap.Add(type, destroySytem);
                }

                // UpdateSystem
                Type updateSystemType = typeof(IUpdateSystem);
                if (updateSystemType.IsAssignableFrom(type))
                {
                    if (updateSystemMap.ContainsKey(type))
                    {
                        UnityEngine.Debug.Log($"Duplicated Update System:{type.FullName}");
                        continue;
                    }

                    // 实例化UpdateSystem并添加到字典
                    IUpdateSystem updateSystem = Activator.CreateInstance(type) as IUpdateSystem;
                    updateSystemMap.Add(type, updateSystem);

                    updateSystemRelatedEntityMap.Add(updateSystem, new List<ECSEntity>());
                }

                // LateUpdateSystem
                Type lateUpdateSystemType = typeof(ILateUpdateSystem);
                if (lateUpdateSystemType.IsAssignableFrom(type))
                {
                    if (lateUpdateSystemMap.ContainsKey(type))
                    {
                        UnityEngine.Debug.Log($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    // 实例化LateUpdateSystem并添加到字典
                    ILateUpdateSystem lateUpdateSystem = Activator.CreateInstance(type) as ILateUpdateSystem;
                    lateUpdateSystemMap.Add(type, lateUpdateSystem);

                    lateUpdateSystemRelatedEntityMap.Add(lateUpdateSystem, new List<ECSEntity>());
                }

                // FixedUpdateSystem
                Type fixedUpdateSystemType = typeof(IFixedUpdateSystem);
                if (fixedUpdateSystemType.IsAssignableFrom(type))
                {
                    if (fixedUpdateSystemMap.ContainsKey(type))
                    {
                        UnityEngine.Debug.Log($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    // 实例化FixedUpdateSystem并添加到字典
                    IFixedUpdateSystem fixedUpdateSystem = Activator.CreateInstance(type) as IFixedUpdateSystem;
                    fixedUpdateSystemMap.Add(type, fixedUpdateSystem);

                    fixedUpdateSystemRelatedEntityMap.Add(fixedUpdateSystem, new List<ECSEntity>());
                }
            }

            // 如果类型有EntityMessageHandlerAttribute特性 参数true表示也要在继承层次结构中搜索
            if (type.GetCustomAttribute<EntityMessageHandlerAttribute>(true) != null)
            {
                // EntityMessage
                Type entityMessageType = typeof(IEntityMessageHandler);
                if (entityMessageType.IsAssignableFrom(type))
                {
                    IEntityMessageHandler entityMessageHandler = Activator.CreateInstance(type) as IEntityMessageHandler;

                    if (!entityMessageHandlerMap.TryGetValue(entityMessageHandler.MessageType(), out List<IEntityMessageHandler> list))
                    {
                        list = new List<IEntityMessageHandler>();
                        entityMessageHandlerMap.Add(entityMessageHandler.MessageType(), list);
                    }

                    list.Add(entityMessageHandler);
                }
            }

            // 如果类型有EntityRpcHandlerAttribute特性 参数true表示也要在继承层次结构中搜索
            if (type.GetCustomAttribute<EntityRpcHandlerAttribute>(true) != null)
            {
                // EntityRPC
                Type entityMessageType = typeof(IEntityRpcHandler);
                if (entityMessageType.IsAssignableFrom(type))
                {
                    IEntityRpcHandler entityRpcHandler = Activator.CreateInstance(type) as IEntityRpcHandler;

                    if (entityRpcHandlerMap.ContainsKey(entityRpcHandler.RpcType()))
                    {
                        UnityEngine.Debug.Log($"Duplicate Entity Rpc, type:{entityRpcHandler.RpcType().FullName}");
                        continue;
                    }

                    entityRpcHandlerMap.Add(entityRpcHandler.RpcType(), entityRpcHandler);
                }
            }
        }
    }

    // 驱动更新系统
    private void DriveUpdateSystem()
    {
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // 从字典中获取与当前 updateSystem 相关的实体列表
            List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];

            // 如果实体列表为空，则跳过当前系统
            if (updateSystemRelatedEntities.Count == 0)
                continue;

            // 从对象池获取一个新的实体列表
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();

            // 将相关实体列表中的所有实体添加到新获取的实体列表中，避免分配新内存
            entityList.AddRangeNonAlloc(updateSystemRelatedEntities);

            // 遍历实体列表中的每个实体
            foreach (var entity in entityList)
            {
                // 如果当前系统没有观察该实体，则跳过
                if (!updateSystem.ObservingEntity(entity))
                    continue;

                // 调用系统的 Update 方法，对实体进行更新处理
                updateSystem.Update(entity);
            }

            // 释放实体列表回对象池，以便重用
            ListPool<ECSEntity>.Release(entityList);
        }
    }


    // 驱动后更新系统
    private void DriveLateUpdateSystem()
    {
        // 遍历 lateUpdateSystemMap 字典中的所有晚更新系统实例
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            // 从字典中获取与当前 lateUpdateSystem 相关的实体列表 
            List<ECSEntity> lateUpdateSystemRelatedEntities = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];

            // 如果实体列表为空，则跳过当前系统
            if (lateUpdateSystemRelatedEntities.Count == 0)
                continue;

            // 从对象池获取一个新的实体列表
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();

            // 将相关实体列表中的所有实体添加到新获取的实体列表中，避免分配新内存
            entityList.AddRangeNonAlloc(lateUpdateSystemRelatedEntities);

            // 遍历实体列表中的每个实体
            foreach (var entity in entityList)
            {
                // 如果当前系统没有观察到该实体，则跳过
                if (!lateUpdateSystem.ObservingEntity(entity))
                    continue;

                // 调用系统的 LateUpdate 方法，对实体进行更新处理
                lateUpdateSystem.LateUpdate(entity);
            }

            // 将实体列表释放回对象池，以便重用
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    /// <summary>
    /// 固定帧更新
    /// </summary>
    private void DriveFixedUpdateSystem()
    {
        // 遍历所有的固定更新系统
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            // 获取当前固定更新系统相关的实体列表
            List<ECSEntity> fixedUpdateSystemRelatedEntities = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
            if (fixedUpdateSystemRelatedEntities.Count == 0)
                continue; // 如果没有相关实体，则跳过

            // 从池中获取一个列表并将相关实体复制到其中
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(fixedUpdateSystemRelatedEntities);

            // 遍历复制的实体列表
            foreach (var entity in entityList)
            {
                if (!fixedUpdateSystem.ObservingEntity(entity))
                    continue; // 如果系统未观察该实体，则跳过

                fixedUpdateSystem.FixedUpdate(entity); // 调用实体的FixedUpdate方法
            }

            // 将列表释放回池中
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    /// <summary>
    /// 获取awake系统 添加到集合中
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <param name="list"></param>
    private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
    {
        // 遍历所有的awake系统，并将匹配组件类型的系统添加到列表中
        foreach (var awakeSystem in awakeSystemMap.Values)
        {
            if (awakeSystem.ComponentType() == typeof(C))
            {
                list.Add(awakeSystem);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <param name="component"></param>
    public void AwakeComponent<C>(C component) where C : ECSComponent
    {
        // 更新给定组件的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从池中获取一个列表并获取相关的Awake系统
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        GetAwakeSystems<C>(list);

        bool found = false;
        // 遍历Awake系统并调用组件的Awake方法
        foreach (var item in list)
        {
            AwakeSystem<C> awakeSystem = item as AwakeSystem<C>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component);
            found = true;
        }

        // 将列表释放回池中，如果未找到系统则记录日志
        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityEngine.Debug.Log($"未找到Awake系统:<{typeof(C).Name}>");
        }
    }

    public void AwakeComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // 更新给定组件的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从池中获取一个列表并获取相关的唤醒系统
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        // 遍历唤醒系统并调用组件的Awake方法，带有参数
        foreach (var item in list)
        {
            AwakeSystem<C, P1> awakeSystem = item as AwakeSystem<C, P1>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component, p1);
            found = true;
        }

        // 将列表释放回池中，如果未找到系统则记录日志
        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityEngine.Debug.Log($"未找到唤醒系统:<{typeof(C).Name}, {typeof(P1).Name}>");
        }
    }

    public void AwakeComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        // 更新给定组件的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从池中获取一个列表并获取相关的唤醒系统
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        // 遍历唤醒系统并调用组件的Awake方法，带有两个参数
        foreach (var item in list)
        {
            AwakeSystem<C, P1, P2> awakeSystem = item as AwakeSystem<C, P1, P2>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component, p1, p2);
            found = true;
        }

        // 将列表释放回池中，如果未找到系统则记录日志
        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityEngine.Debug.Log($"未找到唤醒系统:<{typeof(C).Name}, {typeof(P1).Name}, {typeof(P2).Name}>");
        }
    }

    private void GetDestroySystems<C>(List<IDestroySystem> list) where C : ECSComponent
    {
        // 遍历所有的销毁系统，并将匹配组件类型的系统添加到列表中
        foreach (var destroySystem in destroySystemMap.Values)
        {
            if (destroySystem.ComponentType() == typeof(C))
            {
                list.Add(destroySystem);
            }
        }
    }

    private void GetDestroySystems(Type componentType, List<IDestroySystem> list)
    {
        // 遍历所有的销毁系统，并将匹配组件类型的系统添加到列表中
        foreach (var destroySystem in destroySystemMap.Values)
        {
            if (destroySystem.ComponentType() == componentType)
            {
                list.Add(destroySystem);
            }
        }
    }

    public void DestroyComponent<C>(C component) where C : ECSComponent
    {
        // 更新给定组件的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从池中获取一个列表并获取相关的销毁系统
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);

        // 遍历销毁系统并调用组件的Destroy方法
        foreach (var item in list)
        {
            DestroySystem<C> destroySystem = item as DestroySystem<C>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component);
            component.Disposed = true;
        }

        // 将列表释放回池中
        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent(ECSComponent component)
    {
        // 更新给定组件的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从池中获取一个列表并获取相关的销毁系统
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems(component.GetType(), list);

        // 遍历销毁系统并调用组件的Destroy方法
        foreach (var item in list)
        {
            item.Destroy(component);
            component.Disposed = true;
        }

        // 将列表释放回池中
        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // 更新给定组件的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从池中获取一个列表并获取相关的销毁系统
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);

        // 遍历销毁系统并调用组件的Destroy方法，带有参数
        foreach (var item in list)
        {
            DestroySystem<C, P1> destroySystem = item as DestroySystem<C, P1>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1);
            component.Disposed = true;
        }

        // 将列表释放回池中
        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        // 更新给定组件的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从池中获取一个列表并获取相关的销毁系统
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);

        // 遍历销毁系统并调用组件的Destroy方法，带有两个参数
        foreach (var item in list)
        {
            DestroySystem<C, P1, P2> destroySystem = item as DestroySystem<C, P1, P2>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1, p2);
            component.Disposed = true;
        }

        // 将列表释放回池中
        ListPool<IDestroySystem>.Release(list);
    }

    /// <summary>
    /// 更新更新系统的实体列表
    /// </summary>
    /// <param name="entity"></param>
    private void UpdateSystemEntityList(ECSEntity entity)
    {
        // 遍历 updateSystemMap 字典中的所有更新系统实例
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // 从字典中获取当前更新系统相关的实体列表
            List<ECSEntity> entityList = updateSystemRelatedEntityMap[updateSystem];

            // 检查实体列表中是否不包含指定实体
            if (!entityList.Contains(entity))
            {
                // 如果当前更新系统观察到该实体，则将实体添加到实体列表中
                if (updateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                // 如果实体列表中已包含该实体，但当前更新系统不再观察该实体，则将实体从实体列表中移除
                if (!updateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }


        // 更新晚期更新系统的实体列表
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            // 更新与晚更新系统相关的实体列表
            List<ECSEntity> entityList = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];

            // 检查实体是否不在列表中
            if (!entityList.Contains(entity))
            {
                // 如果晚更新系统正在观察该实体，则将其添加到列表中
                if (lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                // 如果晚更新系统不再观察该实体，则将其从列表中移除
                if (!lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }

        // 遍历每个固定更新系统，更新它们的实体列表
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            // 获取与当前固定更新系统相关的实体列表
            List<ECSEntity> entityList = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];

            // 检查实体是否不在列表中
            if (!entityList.Contains(entity))
            {
                // 如果固定更新系统正在观察该实体，则将其添加到列表中
                if (fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                // 如果固定更新系统不再观察该实体，则将其从列表中移除
                if (!fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }
    }
    // 添加新实体到实体字典中
    public void AddEntity(ECSEntity entity)
    {
        entities.Add(entity.InstanceID, entity);
    }

    // 从实体字典和它的场景中移除实体
    public void RemoveEntity(ECSEntity entity)
    {
        if (entity == null)
            return;

        entities.Remove(entity.InstanceID);
        ECSScene scene = entity.Scene;
        scene?.RemoveEntity(entity.InstanceID);
    }

    // 通过ID查找实体
    public ECSEntity FindEntity(long id)
    {
        return FindEntity<ECSEntity>(id);
    }

    // 通用方法，通过ID查找实体并将其转换为特定类型
    public T FindEntity<T>(long id) where T : ECSEntity
    {
        entities.TryGetValue(id, out ECSEntity entity);
        return entity as T;
    }

    // 通过实体ID查找特定组件
    public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
    {
        return FindEntity(entityID)?.GetComponent<T>();
    }

    // 异步发送消息到特定实体
    public async Task SendMessageToEntity<M>(long id, M m)
    {
        // 如果ID为0，则直接返回
        if (id == 0)
            return;

        // 根据ID查找对应的实体
        ECSEntity entity = FindEntity(id);

        // 如果未找到对应的实体，则直接返回
        if (entity == null)
            return;

        // 获取消息的类型
        Type messageType = m.GetType();

        // 在entityMessageHandlerMap中查找是否有与消息类型对应的处理程序列表
        if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
            return;

        // 从对象池中获取一个新的处理程序列表
        List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain();

        // 将找到的处理程序列表添加到新的处理程序列表中
        entityMessageHandlers.AddRangeNonAlloc(list);

        // 遍历新的处理程序列表，调用每个处理程序的Post方法，传递实体和消息
        foreach (IEntityMessageHandler<M> handler in entityMessageHandlers)
        {
            await handler.Post(entity, m);
        }

        // 将处理程序列表释放回对象池中
        ListPool<IEntityMessageHandler>.Release(entityMessageHandlers);
    }

    // 异步发送远程过程调用（RPC）到特定实体并获取响应
    public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request) where Response : IEntityRpcResponse, new()
    {
        // 检查实体ID是否为0，如果是则返回错误响应
        if (entityID == 0)
            return new Response() { Error = true };

        // 根据实体ID查找对应的实体
        ECSEntity entity = FindEntity(entityID);

        // 如果实体为空，表示未找到对应的实体，返回错误响应
        if (entity == null)
            return new Response() { Error = true };

        // 获取请求的类型
        Type messageType = request.GetType();

        // 在entityRpcHandlerMap中查找是否有与请求类型对应的处理程序
        if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
            return new Response() { Error = true };

        // 尝试将找到的处理程序转换为具体的处理程序类型
        IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;

        // 如果转换失败，表示类型不匹配，返回错误响应
        if (handler == null)
            return new Response() { Error = true };

        // 调用处理程序的Post方法，传递实体和请求，并等待其执行完成，返回处理结果
        return await handler.Post(entity, request);

    }
}

