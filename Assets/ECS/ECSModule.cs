using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class ECSModule : BaseGameModule
{
    // ����ʵ��
    public ECSWorld World { get; private set; }

    // ����ϵͳ���ֵ�ӳ��
    private Dictionary<Type, IAwakeSystem> awakeSystemMap;//awakeϵͳ���ֵ�
    private Dictionary<Type, IDestroySystem> destroySystemMap;//Destoryϵͳ���ֵ�

    private Dictionary<Type, IUpdateSystem> updateSystemMap;//updateϵͳ���ֵ�
    private Dictionary<IUpdateSystem, List<ECSEntity>> updateSystemRelatedEntityMap;//updateϵͳ������ʵ������

    private Dictionary<Type, ILateUpdateSystem> lateUpdateSystemMap;//lateupdateϵͳ���ֵ�
    private Dictionary<ILateUpdateSystem, List<ECSEntity>> lateUpdateSystemRelatedEntityMap;//lateupdateϵͳ������ʵ������

    private Dictionary<Type, IFixedUpdateSystem> fixedUpdateSystemMap;//fixedupdateϵͳ���ֵ�
    private Dictionary<IFixedUpdateSystem, List<ECSEntity>> fixedUpdateSystemRelatedEntityMap;//fixedϵͳ������ʵ������

    // ʵ����ֵ�ӳ��
    private Dictionary<long, ECSEntity> entities = new Dictionary<long, ECSEntity>();
    // ʵ����Ϣ��������RPC���������ֵ�ӳ��
    private Dictionary<Type, List<IEntityMessageHandler>> entityMessageHandlerMap;
    private Dictionary<Type, IEntityRpcHandler> entityRpcHandlerMap;

    // ģ���ʼ��ʱ����
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        // ��������ϵͳ
        LoadAllSystems();
        // ��ʼ������
        World = new ECSWorld();
    }

    // ģ��ÿ֡����ʱ����
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        // ��������ϵͳ
        DriveUpdateSystem();
    }

    // ģ��ÿ֡�����ʱ����
    protected internal override void OnModuleLateUpdate(float deltaTime)
    {
        base.OnModuleLateUpdate(deltaTime);
        // ���������ϵͳ
        DriveLateUpdateSystem();
    }

    // ģ��ÿ֡�̶�����ʱ����
    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
        // �����̶�����ϵͳ
        DriveFixedUpdateSystem();
    }

    /// <summary>
    /// ��������ϵͳ
    /// </summary>
    public void LoadAllSystems()
    {
        // ��ʼ������ϵͳ���ֵ�ӳ��
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

        // ������ǰ�����е���������
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            // ��������ǳ���ģ�����
            if (type.IsAbstract)
                continue;

            // ���������ECSSystemAttribute����
            if (type.GetCustomAttribute<ECSSystemAttribute>(true) != null)
            {
                // AwakeSystem ��ȡIAwakeSystem�ӿڵ�Type����
                Type awakeSystemType = typeof(IAwakeSystem);
                //�ж�awakeSystem��type�Ƿ�Ϊ���ࡢ�ӿڻ�ͬһ������
                if (awakeSystemType.IsAssignableFrom(type))
                {
                    //�ж�awake�ֵ����Ƿ���������͵�key
                    if (awakeSystemMap.ContainsKey(type))
                    {
                        UnityEngine.Debug.Log($"Duplicated Awake System:{type.FullName}");
                        continue;
                    }

                    // ʵ����AwakeSystem����ӵ��ֵ�
                    IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem;
                    awakeSystemMap.Add(type, awakeSystem);
                }

                // DestroySystem ��ȡIDestroySystem�ӿڵ�Type����
                Type destroySystemType = typeof(IDestroySystem);
                //�ж�destroySystemType��type�Ƿ��ǻ��ࡢ�ӿڻ�ͬһ������
                if (destroySystemType.IsAssignableFrom(type))
                {
                    if (destroySystemMap.ContainsKey(type))
                    {
                        UnityEngine.Debug.Log($"Duplicated Destroy System:{type.FullName}");
                        continue;
                    }

                    // ʵ����DestroySystem����ӵ��ֵ�
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

                    // ʵ����UpdateSystem����ӵ��ֵ�
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

                    // ʵ����LateUpdateSystem����ӵ��ֵ�
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

                    // ʵ����FixedUpdateSystem����ӵ��ֵ�
                    IFixedUpdateSystem fixedUpdateSystem = Activator.CreateInstance(type) as IFixedUpdateSystem;
                    fixedUpdateSystemMap.Add(type, fixedUpdateSystem);

                    fixedUpdateSystemRelatedEntityMap.Add(fixedUpdateSystem, new List<ECSEntity>());
                }
            }

            // ���������EntityMessageHandlerAttribute���� ����true��ʾҲҪ�ڼ̳в�νṹ������
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

            // ���������EntityRpcHandlerAttribute���� ����true��ʾҲҪ�ڼ̳в�νṹ������
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

    // ��������ϵͳ
    private void DriveUpdateSystem()
    {
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // ���ֵ��л�ȡ�뵱ǰ updateSystem ��ص�ʵ���б�
            List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];

            // ���ʵ���б�Ϊ�գ���������ǰϵͳ
            if (updateSystemRelatedEntities.Count == 0)
                continue;

            // �Ӷ���ػ�ȡһ���µ�ʵ���б�
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();

            // �����ʵ���б��е�����ʵ����ӵ��»�ȡ��ʵ���б��У�����������ڴ�
            entityList.AddRangeNonAlloc(updateSystemRelatedEntities);

            // ����ʵ���б��е�ÿ��ʵ��
            foreach (var entity in entityList)
            {
                // �����ǰϵͳû�й۲��ʵ�壬������
                if (!updateSystem.ObservingEntity(entity))
                    continue;

                // ����ϵͳ�� Update ��������ʵ����и��´���
                updateSystem.Update(entity);
            }

            // �ͷ�ʵ���б�ض���أ��Ա�����
            ListPool<ECSEntity>.Release(entityList);
        }
    }


    // ���������ϵͳ
    private void DriveLateUpdateSystem()
    {
        // ���� lateUpdateSystemMap �ֵ��е����������ϵͳʵ��
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            // ���ֵ��л�ȡ�뵱ǰ lateUpdateSystem ��ص�ʵ���б� 
            List<ECSEntity> lateUpdateSystemRelatedEntities = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];

            // ���ʵ���б�Ϊ�գ���������ǰϵͳ
            if (lateUpdateSystemRelatedEntities.Count == 0)
                continue;

            // �Ӷ���ػ�ȡһ���µ�ʵ���б�
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();

            // �����ʵ���б��е�����ʵ����ӵ��»�ȡ��ʵ���б��У�����������ڴ�
            entityList.AddRangeNonAlloc(lateUpdateSystemRelatedEntities);

            // ����ʵ���б��е�ÿ��ʵ��
            foreach (var entity in entityList)
            {
                // �����ǰϵͳû�й۲쵽��ʵ�壬������
                if (!lateUpdateSystem.ObservingEntity(entity))
                    continue;

                // ����ϵͳ�� LateUpdate ��������ʵ����и��´���
                lateUpdateSystem.LateUpdate(entity);
            }

            // ��ʵ���б��ͷŻض���أ��Ա�����
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    /// <summary>
    /// �̶�֡����
    /// </summary>
    private void DriveFixedUpdateSystem()
    {
        // �������еĹ̶�����ϵͳ
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            // ��ȡ��ǰ�̶�����ϵͳ��ص�ʵ���б�
            List<ECSEntity> fixedUpdateSystemRelatedEntities = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
            if (fixedUpdateSystemRelatedEntities.Count == 0)
                continue; // ���û�����ʵ�壬������

            // �ӳ��л�ȡһ���б������ʵ�帴�Ƶ�����
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(fixedUpdateSystemRelatedEntities);

            // �������Ƶ�ʵ���б�
            foreach (var entity in entityList)
            {
                if (!fixedUpdateSystem.ObservingEntity(entity))
                    continue; // ���ϵͳδ�۲��ʵ�壬������

                fixedUpdateSystem.FixedUpdate(entity); // ����ʵ���FixedUpdate����
            }

            // ���б��ͷŻس���
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    /// <summary>
    /// ��ȡawakeϵͳ ��ӵ�������
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <param name="list"></param>
    private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
    {
        // �������е�awakeϵͳ������ƥ��������͵�ϵͳ��ӵ��б���
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
        // ���¸��������ʵ���б�
        UpdateSystemEntityList(component.Entity);

        // �ӳ��л�ȡһ���б���ȡ��ص�Awakeϵͳ
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        GetAwakeSystems<C>(list);

        bool found = false;
        // ����Awakeϵͳ�����������Awake����
        foreach (var item in list)
        {
            AwakeSystem<C> awakeSystem = item as AwakeSystem<C>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component);
            found = true;
        }

        // ���б��ͷŻس��У����δ�ҵ�ϵͳ���¼��־
        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityEngine.Debug.Log($"δ�ҵ�Awakeϵͳ:<{typeof(C).Name}>");
        }
    }

    public void AwakeComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // ���¸��������ʵ���б�
        UpdateSystemEntityList(component.Entity);

        // �ӳ��л�ȡһ���б���ȡ��صĻ���ϵͳ
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        // ��������ϵͳ�����������Awake���������в���
        foreach (var item in list)
        {
            AwakeSystem<C, P1> awakeSystem = item as AwakeSystem<C, P1>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component, p1);
            found = true;
        }

        // ���б��ͷŻس��У����δ�ҵ�ϵͳ���¼��־
        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityEngine.Debug.Log($"δ�ҵ�����ϵͳ:<{typeof(C).Name}, {typeof(P1).Name}>");
        }
    }

    public void AwakeComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        // ���¸��������ʵ���б�
        UpdateSystemEntityList(component.Entity);

        // �ӳ��л�ȡһ���б���ȡ��صĻ���ϵͳ
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        // ��������ϵͳ�����������Awake������������������
        foreach (var item in list)
        {
            AwakeSystem<C, P1, P2> awakeSystem = item as AwakeSystem<C, P1, P2>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component, p1, p2);
            found = true;
        }

        // ���б��ͷŻس��У����δ�ҵ�ϵͳ���¼��־
        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityEngine.Debug.Log($"δ�ҵ�����ϵͳ:<{typeof(C).Name}, {typeof(P1).Name}, {typeof(P2).Name}>");
        }
    }

    private void GetDestroySystems<C>(List<IDestroySystem> list) where C : ECSComponent
    {
        // �������е�����ϵͳ������ƥ��������͵�ϵͳ��ӵ��б���
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
        // �������е�����ϵͳ������ƥ��������͵�ϵͳ��ӵ��б���
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
        // ���¸��������ʵ���б�
        UpdateSystemEntityList(component.Entity);

        // �ӳ��л�ȡһ���б���ȡ��ص�����ϵͳ
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);

        // ��������ϵͳ�����������Destroy����
        foreach (var item in list)
        {
            DestroySystem<C> destroySystem = item as DestroySystem<C>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component);
            component.Disposed = true;
        }

        // ���б��ͷŻس���
        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent(ECSComponent component)
    {
        // ���¸��������ʵ���б�
        UpdateSystemEntityList(component.Entity);

        // �ӳ��л�ȡһ���б���ȡ��ص�����ϵͳ
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems(component.GetType(), list);

        // ��������ϵͳ�����������Destroy����
        foreach (var item in list)
        {
            item.Destroy(component);
            component.Disposed = true;
        }

        // ���б��ͷŻس���
        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // ���¸��������ʵ���б�
        UpdateSystemEntityList(component.Entity);

        // �ӳ��л�ȡһ���б���ȡ��ص�����ϵͳ
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);

        // ��������ϵͳ�����������Destroy���������в���
        foreach (var item in list)
        {
            DestroySystem<C, P1> destroySystem = item as DestroySystem<C, P1>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1);
            component.Disposed = true;
        }

        // ���б��ͷŻس���
        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        // ���¸��������ʵ���б�
        UpdateSystemEntityList(component.Entity);

        // �ӳ��л�ȡһ���б���ȡ��ص�����ϵͳ
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);

        // ��������ϵͳ�����������Destroy������������������
        foreach (var item in list)
        {
            DestroySystem<C, P1, P2> destroySystem = item as DestroySystem<C, P1, P2>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1, p2);
            component.Disposed = true;
        }

        // ���б��ͷŻس���
        ListPool<IDestroySystem>.Release(list);
    }

    /// <summary>
    /// ���¸���ϵͳ��ʵ���б�
    /// </summary>
    /// <param name="entity"></param>
    private void UpdateSystemEntityList(ECSEntity entity)
    {
        // ���� updateSystemMap �ֵ��е����и���ϵͳʵ��
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // ���ֵ��л�ȡ��ǰ����ϵͳ��ص�ʵ���б�
            List<ECSEntity> entityList = updateSystemRelatedEntityMap[updateSystem];

            // ���ʵ���б����Ƿ񲻰���ָ��ʵ��
            if (!entityList.Contains(entity))
            {
                // �����ǰ����ϵͳ�۲쵽��ʵ�壬��ʵ����ӵ�ʵ���б���
                if (updateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                // ���ʵ���б����Ѱ�����ʵ�壬����ǰ����ϵͳ���ٹ۲��ʵ�壬��ʵ���ʵ���б����Ƴ�
                if (!updateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }


        // �������ڸ���ϵͳ��ʵ���б�
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            // �����������ϵͳ��ص�ʵ���б�
            List<ECSEntity> entityList = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];

            // ���ʵ���Ƿ����б���
            if (!entityList.Contains(entity))
            {
                // ��������ϵͳ���ڹ۲��ʵ�壬������ӵ��б���
                if (lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                // ��������ϵͳ���ٹ۲��ʵ�壬������б����Ƴ�
                if (!lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }

        // ����ÿ���̶�����ϵͳ���������ǵ�ʵ���б�
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            // ��ȡ�뵱ǰ�̶�����ϵͳ��ص�ʵ���б�
            List<ECSEntity> entityList = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];

            // ���ʵ���Ƿ����б���
            if (!entityList.Contains(entity))
            {
                // ����̶�����ϵͳ���ڹ۲��ʵ�壬������ӵ��б���
                if (fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                // ����̶�����ϵͳ���ٹ۲��ʵ�壬������б����Ƴ�
                if (!fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }
    }
    // �����ʵ�嵽ʵ���ֵ���
    public void AddEntity(ECSEntity entity)
    {
        entities.Add(entity.InstanceID, entity);
    }

    // ��ʵ���ֵ�����ĳ������Ƴ�ʵ��
    public void RemoveEntity(ECSEntity entity)
    {
        if (entity == null)
            return;

        entities.Remove(entity.InstanceID);
        ECSScene scene = entity.Scene;
        scene?.RemoveEntity(entity.InstanceID);
    }

    // ͨ��ID����ʵ��
    public ECSEntity FindEntity(long id)
    {
        return FindEntity<ECSEntity>(id);
    }

    // ͨ�÷�����ͨ��ID����ʵ�岢����ת��Ϊ�ض�����
    public T FindEntity<T>(long id) where T : ECSEntity
    {
        entities.TryGetValue(id, out ECSEntity entity);
        return entity as T;
    }

    // ͨ��ʵ��ID�����ض����
    public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
    {
        return FindEntity(entityID)?.GetComponent<T>();
    }

    // �첽������Ϣ���ض�ʵ��
    public async Task SendMessageToEntity<M>(long id, M m)
    {
        // ���IDΪ0����ֱ�ӷ���
        if (id == 0)
            return;

        // ����ID���Ҷ�Ӧ��ʵ��
        ECSEntity entity = FindEntity(id);

        // ���δ�ҵ���Ӧ��ʵ�壬��ֱ�ӷ���
        if (entity == null)
            return;

        // ��ȡ��Ϣ������
        Type messageType = m.GetType();

        // ��entityMessageHandlerMap�в����Ƿ�������Ϣ���Ͷ�Ӧ�Ĵ�������б�
        if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
            return;

        // �Ӷ�����л�ȡһ���µĴ�������б�
        List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain();

        // ���ҵ��Ĵ�������б���ӵ��µĴ�������б���
        entityMessageHandlers.AddRangeNonAlloc(list);

        // �����µĴ�������б�����ÿ����������Post����������ʵ�����Ϣ
        foreach (IEntityMessageHandler<M> handler in entityMessageHandlers)
        {
            await handler.Post(entity, m);
        }

        // ����������б��ͷŻض������
        ListPool<IEntityMessageHandler>.Release(entityMessageHandlers);
    }

    // �첽����Զ�̹��̵��ã�RPC�����ض�ʵ�岢��ȡ��Ӧ
    public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request) where Response : IEntityRpcResponse, new()
    {
        // ���ʵ��ID�Ƿ�Ϊ0��������򷵻ش�����Ӧ
        if (entityID == 0)
            return new Response() { Error = true };

        // ����ʵ��ID���Ҷ�Ӧ��ʵ��
        ECSEntity entity = FindEntity(entityID);

        // ���ʵ��Ϊ�գ���ʾδ�ҵ���Ӧ��ʵ�壬���ش�����Ӧ
        if (entity == null)
            return new Response() { Error = true };

        // ��ȡ���������
        Type messageType = request.GetType();

        // ��entityRpcHandlerMap�в����Ƿ������������Ͷ�Ӧ�Ĵ������
        if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
            return new Response() { Error = true };

        // ���Խ��ҵ��Ĵ������ת��Ϊ����Ĵ����������
        IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;

        // ���ת��ʧ�ܣ���ʾ���Ͳ�ƥ�䣬���ش�����Ӧ
        if (handler == null)
            return new Response() { Error = true };

        // ���ô�������Post����������ʵ������󣬲��ȴ���ִ����ɣ����ش�����
        return await handler.Post(entity, request);

    }
}

