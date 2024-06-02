using System.Collections.Generic;
using System.Diagnostics;

public class ECSScene : ECSEntity
{
    // 保存实体的字典，键是实体的ID，值是实体
    private Dictionary<long, ECSEntity> entities;

    // 构造函数，初始化实体字典
    public ECSScene()
    {
        entities = new Dictionary<long, ECSEntity>();
    }

    // 重写Dispose方法，用于清理资源
    public override void Dispose()
    {
        // 如果已经被释放，直接返回
        if (Disposed)
            return;

        // 获取一个长整型列表池
        List<long> entityIDList = ListPool<long>.Obtain();
        // 将所有实体的ID添加到列表中
        foreach (var entityID in entities.Keys)
        {
            entityIDList.Add(entityID);
        }
        // 遍历实体ID列表，释放每个实体
        foreach (var entityID in entityIDList)
        {
            ECSEntity entity = entities[entityID];
            entity.Dispose();
        }
        // 释放列表池
        ListPool<long>.Release(entityIDList);

        // 调用基类的Dispose方法
        base.Dispose();
    }

    // 添加实体的方法
    public void AddEntity(ECSEntity entity)
    {
        // 如果实体为空，直接返回
        if (entity == null)
            return;

        // 获取实体的旧场景
        ECSScene oldScene = entity.Scene;
        // 如果旧场景不为空，从旧场景中移除实体
        if (oldScene != null)
        {
            oldScene.RemoveEntity(entity.InstanceID);
        }

        // 将实体添加到当前场景的实体字典中
        entities.Add(entity.InstanceID, entity);
        // 设置实体的场景ID为当前场景的InstanceID
        entity.SceneID = InstanceID;
        // 记录日志，显示当前场景中的实体数量
        UnityEngine.Debug.Log($"Scene Add Entity, Current Count:{entities.Count}");
    }

    // 移除实体的方法
    public void RemoveEntity(long entityID)
    {
        // 尝试从实体字典中移除实体，如果成功，记录日志
        if (entities.Remove(entityID))
        {
            UnityEngine.Debug.Log($"Scene Remove Entity, Current Count:{entities.Count}");
        }
    }

    // 查找特定类型实体的方法
    public void FindEntities<T>(List<long> list) where T : ECSEntity
    {
        // 遍历实体字典，查找类型为T的实体，并将其ID添加到列表中
        foreach (var item in entities)
        {
            if (item.Value is T)
            {
                list.Add(item.Key);
            }
        }
    }

    // 查找具有特定组件的实体的方法
    public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
    {
        // 遍历实体字典，查找具有类型为T组件的实体，并将其ID添加到列表中
        foreach (var item in entities)
        {
            if (item.Value.HasComponent<T>())
            {
                list.Add(item.Key);
            }
        }
    }

    // 获取所有实体ID的方法
    public void GetAllEntities(List<long> list)
    {
        // 遍历实体字典，将所有实体的ID添加到列表中
        foreach (var item in entities)
        {
            list.Add(item.Key);
        }
    }
}
