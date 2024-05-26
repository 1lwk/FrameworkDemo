using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// ��Ϸ���
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// �������
    /// </summary>
    [Module(2)]
    public static ProcedureModule Procedure { get => TGameFramework.Instance.GetModule<ProcedureModule>(); }
    /// <summary>
    /// ��Ϣģ�飬ʹ��Module���Ա�ǣ����ȼ�Ϊ6
    /// </summary>
    [Module(6)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }

    private bool activing;

    /// <summary>
    /// ����Ϸ���󴴽�ʱ����
    /// </summary>
    private void Awake()
    {
        // ���TGameFrameworkʵ���Ѿ����ڣ����ٵ�ǰ����
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        activing = true;
        DontDestroyOnLoad(gameObject); //��ת������

        TGameFramework.Initialize(); // ��ʼ����Ϸ���
        StartupModules(); // ����ģ��
        TGameFramework.Instance.InitModules(); // ��ʼ������ģ��
    }

    /// <summary>
    /// ����Ϸ��������ʱ����
    /// </summary>
    private void Start()
    {
        TGameFramework.Instance.StartModules(); // ��������ģ��
    }

    /// <summary>
    /// ÿ֡����ʱ����
    /// </summary>
    private void Update()
    {
        TGameFramework.Instance.Update(); // ��������ģ��
    }

    /// <summary>
    /// ÿ֡������Update�������ú����
    /// </summary>
    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate(); // ���������ģ��
    }

    /// <summary>
    /// �̶�ʱ��������ʱ����
    /// </summary>
    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate(); // �̶�֡��������ģ��
    }

    /// <summary>
    /// ����Ϸ��������ʱ����
    /// </summary>
    private void OnDestroy()
    {
        // �����ǰ�������ڼ���״̬������TGameFrameworkʵ��
        if (activing)
        {
            TGameFramework.Instance.Destroy();
        }
    }

    /// <summary>
    /// ��������ģ��
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        // ��ȡ��ǰ�������������Ϣ���������С�˽�С���̬����
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Type baseCompType = typeof(BaseGameModule);

        // ������������
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            // ������������Ƿ�̳���BaseGameModule
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
               continue;

            // ��ȡ�����ϵ�ModuleAttribute����
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            if (attrs.Length == 0)
                continue;

            // �����Ӷ����еĶ�Ӧ���
            Component comp = GetComponentInChildren(property.PropertyType);
            if (comp == null)
            {
                Debug.LogError($"�޷��ҵ�ģ��: {property.PropertyType}");
                continue;
            }

            // ���ģ�����Ե��б���
            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;
            moduleAttrs.Add(moduleAttr);
        }

        // �����ȼ�����ģ������
        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;
        });

        // �������ģ�鵽TGameFrameworkʵ��
        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
    }


    /// <summary>
    /// �Զ������Խ����Ա��Ϊģ������
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// ģ��
        /// </summary>
        public BaseGameModule Module { get; set; }

        /// <summary>
        /// ��Ӹ����ԲŻᱻ����ģ��
        /// </summary>
        /// <param name="priority">���������ȼ�,��ֵԽСԽ��ִ��</param>
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }

        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}
