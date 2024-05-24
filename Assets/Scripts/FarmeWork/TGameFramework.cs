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
        Instance = new TGameFramework();
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

    /// <summary>
    /// 
    /// </summary>
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

    public void LateUpdate()
    {
        if (!Initialized)
            return;
        if (m_allmodules == null)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var item in m_allmodules.Values)
        {
            item.OnModuleLateUpdate(deltaTime);
        }
    }

    public void FixedUpdate()
    {
        if (!Initialized)
            return;
        if (m_allmodules == null)
            return;

        float deltaTime= UnityEngine.Time.deltaTime;
        foreach (var item in m_allmodules.Values)
        {
            item.OnModuleFixedUpdate(deltaTime);
        }
    }

    public void InitModules()
    {
        if (Initialized)
            return;
        Initialized = true;

        foreach (var item in m_allmodules.Values)
        {
            item.OnModuleInit();
        }
    }

    public void StartModules()
    {
        if(m_allmodules==null)
            return;
        if (!Initialized)
            return;

        foreach (var item in m_allmodules.Values)
        {
            item.OnModuleStart();
        }
    }

    public void Destroy()
    {
        if (!Initialized)
            return;
        if (Instance != this)
            return;
        if (m_allmodules == null)
            return;

        foreach (var item in Instance.m_allmodules.Values)
        {
            item.OnModuleStop();
        }

        Instance = null;
        Initialized = false;
    }
}
