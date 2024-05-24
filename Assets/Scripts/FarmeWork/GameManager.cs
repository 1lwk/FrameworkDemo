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
    [Module(6)]
    public static MessageModule Message { get=>TGameFramework.Instance.GetModule<MessageModule>(); }

    private bool activing;
    

    private void Awake()
    {
        if(TGameFramework.Instance!=null)
        {
            Destroy(gameObject);
            return;
        }

        activing= true;
        DontDestroyOnLoad(gameObject);

        TGameFramework.Initialize();
        StartupModules();
        TGameFramework.Instance.InitModules();
    }

    private void Start()
    {
        TGameFramework.Instance.StartModules();
    }

    private void Update()
    {
        TGameFramework.Instance.Update();
    }

    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate();
    }

    private void OnDestroy()
    {
        if(activing)
        {
            TGameFramework.Instance.Destroy();
        }
    }

    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        PropertyInfo[] propertyInfos=GetType().GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
        Type baseCompType = typeof(BaseGameModule);
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
                continue;

            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            if (attrs.Length == 0)
                continue;

            Component comp = GetComponentInChildren(property.PropertyType);
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }

            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;
            moduleAttrs.Add(moduleAttr);
        }

        moduleAttrs.Sort((a,b) =>
        {
            return a.Priority-b.Priority;
        });

        for(int i=0;i<moduleAttrs.Count;i++)
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
