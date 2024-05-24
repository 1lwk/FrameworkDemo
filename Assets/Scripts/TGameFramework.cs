using System.Collections.Generic;
using System;
using System.Diagnostics;

/// <summary>
/// 核心管理器
/// </summary>
public sealed class TGameFramework
{
    public static TGameFramework Instance { get;private set; }//本类的一个实例
    public static bool Initialized {  get; private set; }//布尔值标记是否被初始化
    public Dictionary<Type, BaseGameModule> m_allmodules = new Dictionary<Type, BaseGameModule>();//存放所有模块的实例

    /// <summary>
    /// 初始化方法 实例出一个TGameFramework对象
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
    /// 获取指定类型的模块
    /// </summary>
    /// <typeparam name="T">需要获取某个模块的类型 例如 UIModule</typeparam>
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
    /// 往字典中添加模块
    /// </summary>
    /// <param name="module"></param>
    public void AddModule(BaseGameModule module)
    {
        Type moduleType=module.GetType();
        //判断是否已经在字典中存在 如果存在直接打印提示信息返回 否则添加
        if(m_allmodules.ContainsKey(moduleType))
        {
            UnityEngine.Debug.LogError($"Module添加失败，重复:{moduleType.Name}");
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
