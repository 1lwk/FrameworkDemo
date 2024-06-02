using System.Collections.Generic;
using System.Diagnostics;

public class ECSScene : ECSEntity
{
    // ����ʵ����ֵ䣬����ʵ���ID��ֵ��ʵ��
    private Dictionary<long, ECSEntity> entities;

    // ���캯������ʼ��ʵ���ֵ�
    public ECSScene()
    {
        entities = new Dictionary<long, ECSEntity>();
    }

    // ��дDispose����������������Դ
    public override void Dispose()
    {
        // ����Ѿ����ͷţ�ֱ�ӷ���
        if (Disposed)
            return;

        // ��ȡһ���������б��
        List<long> entityIDList = ListPool<long>.Obtain();
        // ������ʵ���ID��ӵ��б���
        foreach (var entityID in entities.Keys)
        {
            entityIDList.Add(entityID);
        }
        // ����ʵ��ID�б��ͷ�ÿ��ʵ��
        foreach (var entityID in entityIDList)
        {
            ECSEntity entity = entities[entityID];
            entity.Dispose();
        }
        // �ͷ��б��
        ListPool<long>.Release(entityIDList);

        // ���û����Dispose����
        base.Dispose();
    }

    // ���ʵ��ķ���
    public void AddEntity(ECSEntity entity)
    {
        // ���ʵ��Ϊ�գ�ֱ�ӷ���
        if (entity == null)
            return;

        // ��ȡʵ��ľɳ���
        ECSScene oldScene = entity.Scene;
        // ����ɳ�����Ϊ�գ��Ӿɳ������Ƴ�ʵ��
        if (oldScene != null)
        {
            oldScene.RemoveEntity(entity.InstanceID);
        }

        // ��ʵ����ӵ���ǰ������ʵ���ֵ���
        entities.Add(entity.InstanceID, entity);
        // ����ʵ��ĳ���IDΪ��ǰ������InstanceID
        entity.SceneID = InstanceID;
        // ��¼��־����ʾ��ǰ�����е�ʵ������
        UnityEngine.Debug.Log($"Scene Add Entity, Current Count:{entities.Count}");
    }

    // �Ƴ�ʵ��ķ���
    public void RemoveEntity(long entityID)
    {
        // ���Դ�ʵ���ֵ����Ƴ�ʵ�壬����ɹ�����¼��־
        if (entities.Remove(entityID))
        {
            UnityEngine.Debug.Log($"Scene Remove Entity, Current Count:{entities.Count}");
        }
    }

    // �����ض�����ʵ��ķ���
    public void FindEntities<T>(List<long> list) where T : ECSEntity
    {
        // ����ʵ���ֵ䣬��������ΪT��ʵ�壬������ID��ӵ��б���
        foreach (var item in entities)
        {
            if (item.Value is T)
            {
                list.Add(item.Key);
            }
        }
    }

    // ���Ҿ����ض������ʵ��ķ���
    public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
    {
        // ����ʵ���ֵ䣬���Ҿ�������ΪT�����ʵ�壬������ID��ӵ��б���
        foreach (var item in entities)
        {
            if (item.Value.HasComponent<T>())
            {
                list.Add(item.Key);
            }
        }
    }

    // ��ȡ����ʵ��ID�ķ���
    public void GetAllEntities(List<long> list)
    {
        // ����ʵ���ֵ䣬������ʵ���ID��ӵ��б���
        foreach (var item in entities)
        {
            list.Add(item.Key);
        }
    }
}
