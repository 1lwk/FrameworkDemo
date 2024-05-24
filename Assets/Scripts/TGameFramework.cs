using System.Collections.Generic;
using System;
using System.Diagnostics;

/// <summary>
/// ���Ĺ�����
/// </summary>
public sealed class TGameFramework
{
    public static TGameFramework Instance { get;private set; }//�����һ��ʵ��
    public static bool Initialized {  get; private set; }//����ֵ����Ƿ񱻳�ʼ��
    public Dictionary<Type, BaseGameModule> m_allmodules = new Dictionary<Type, BaseGameModule>();//�������ģ���ʵ��

    /// <summary>
    /// ��ʼ������ ʵ����һ��TGameFramework����
    /// </summary>
    public static void Initialize()
    {
        if(!Initialized)
        {
            Instance = new TGameFramework();
            Initialized = true;
        }
    }

    /// <summary>
    /// ��ȡָ�����͵�ģ��
    /// </summary>
    /// <typeparam name="T">��Ҫ��ȡĳ��ģ������� ���� UIModule</typeparam>
    /// <returns></returns>
    public T GetModule<T>() where T : BaseGameModule
    {
        if(m_allmodules.TryGetValue(typeof(T),out BaseGameModule module))
        {
            return module as T;
        }
        return default(T);
    }

    /// <summary>
    /// ���ֵ������ģ��
    /// </summary>
    /// <param name="module"></param>
    public void AddModule(BaseGameModule module)
    {
        Type moduleType=module.GetType();
        //�ж��Ƿ��Ѿ����ֵ��д��� �������ֱ�Ӵ�ӡ��ʾ��Ϣ���� �������
        if(m_allmodules.ContainsKey(moduleType))
        {
            UnityEngine.Debug.LogError($"Module���ʧ�ܣ��ظ�:{moduleType.Name}");
            return;
        }
        m_allmodules.Add(moduleType, module);
    }

    public void Update()
    {
        if (!Initialized)
            return;

        if (m_allmodules == null)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var item in m_allmodules.Values)
        {
            item.OnModuleUpdate(deltaTime);
        }
    }
}
