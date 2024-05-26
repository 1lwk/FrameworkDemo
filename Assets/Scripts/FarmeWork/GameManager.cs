using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 游戏入口
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 流程组件
    /// </summary>
    [Module(2)]
    public static ProcedureModule Procedure { get => TGameFramework.Instance.GetModule<ProcedureModule>(); }
    /// <summary>
    /// 消息模块，使用Module特性标记，优先级为6
    /// </summary>
    [Module(6)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }

    private bool activing;

    /// <summary>
    /// 在游戏对象创建时调用
    /// </summary>
    private void Awake()
    {
        // 如果TGameFramework实例已经存在，销毁当前对象
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        activing = true;
        DontDestroyOnLoad(gameObject); //跳转不销毁

        TGameFramework.Initialize(); // 初始化游戏框架
        StartupModules(); // 启动模块
        TGameFramework.Instance.InitModules(); // 初始化所有模块
    }

    /// <summary>
    /// 在游戏对象启动时调用
    /// </summary>
    private void Start()
    {
        TGameFramework.Instance.StartModules(); // 启动所有模块
    }

    /// <summary>
    /// 每帧更新时调用
    /// </summary>
    private void Update()
    {
        TGameFramework.Instance.Update(); // 更新所有模块
    }

    /// <summary>
    /// 每帧在所有Update方法调用后调用
    /// </summary>
    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate(); // 后更新所有模块
    }

    /// <summary>
    /// 固定时间间隔更新时调用
    /// </summary>
    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate(); // 固定帧更新所有模块
    }

    /// <summary>
    /// 在游戏对象销毁时调用
    /// </summary>
    private void OnDestroy()
    {
        // 如果当前对象仍在激活状态，销毁TGameFramework实例
        if (activing)
        {
            TGameFramework.Instance.Destroy();
        }
    }

    /// <summary>
    /// 启动所有模块
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        // 获取当前类的所有属性信息，包括公有、私有、静态属性
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Type baseCompType = typeof(BaseGameModule);

        // 遍历所有属性
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            // 检查属性类型是否继承自BaseGameModule
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
               continue;

            // 获取属性上的ModuleAttribute特性
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            if (attrs.Length == 0)
                continue;

            // 查找子对象中的对应组件
            Component comp = GetComponentInChildren(property.PropertyType);
            if (comp == null)
            {
                Debug.LogError($"无法找到模块: {property.PropertyType}");
                continue;
            }

            // 添加模块特性到列表中
            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;
            moduleAttrs.Add(moduleAttr);
        }

        // 按优先级排序模块特性
        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;
        });

        // 添加所有模块到TGameFramework实例
        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
    }


    /// <summary>
    /// 自定义属性将属性标记为模块特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }

        /// <summary>
        /// 添加该特性才会被当作模块
        /// </summary>
        /// <param name="priority">控制器优先级,数值越小越先执行</param>
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
