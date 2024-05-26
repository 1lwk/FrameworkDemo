using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 处理游戏总流程,控制游戏过程
/// </summary>
public partial class ProcedureModule : BaseGameModule
{
    /// <summary>
    /// SerializeField特性是指将私有的字段也显示在unity检视界面
    /// </summary>
    [SerializeField]
    private string[] proceduresNames = null; // 程序流程类名字符串数组

    [SerializeField]
    private string defaultProcedureName = null; //默认程序流程名

    public BaseProcedure CurrentProcedure { get; private set; } //当前的程序流程
    public bool IsRunning { get; private set; } //标识模块是否正在运行
    public bool IsChangingProcedure { get; private set; } //标识是否正在更改程序流程

    private Dictionary<Type, BaseProcedure> procedures; //存储流程类型和实例的字典
    private BaseProcedure defaultProcedure; //默认的程序流程实例
    private ObjectPool<ChangeProcedureRequest> changeProcedureRequestPool = new ObjectPool<ChangeProcedureRequest>(null); // 变更流程请求的对象池
    private Queue<ChangeProcedureRequest> changeProcedureQ = new Queue<ChangeProcedureRequest>(); //管理流程变更请求的队列

    /// <summary>
    /// 初始化模块，基于提供的类名创建程序流程实例
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit(); // 调用基类的初始化方法
        procedures = new Dictionary<Type, BaseProcedure>(); //初始化流程字典
        bool findDefaultState = false; //标识是否找到默认流程

        for (int i = 0; i < proceduresNames.Length; i++) //遍历流程类名数组
        {
            string procedureTypeName = proceduresNames[i]; //获取当前流程类名
            if (string.IsNullOrEmpty(procedureTypeName)) //如果类名为空，跳过本次循环（IsNullOrEmpty检查字符串是否为null或者""）
                continue;
            Type procedureType = Type.GetType(procedureTypeName, true); // 使用反射方法GetType获取流程类型 procedureTypeName类名 布尔值表示
            if (procedureType == null) // 如果类型为空，记录错误信息并跳过
            {
                Debug.LogError($"无法找到流程:`{procedureTypeName}`");
                continue;
            }

            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure; // 通过反射实例一个流程子对象或者父对象
            bool isDefaultState = procedureTypeName == defaultProcedureName; // 检查是否为默认流程
            procedures.Add(procedureType, procedure); // 将流程添加到字典中

            if (isDefaultState) // 如果是默认流程，记录并设置标识
            {
                defaultProcedure = procedure;
                findDefaultState = true;
            }
        }

        if (!findDefaultState) // 如果未找到默认流程，记录错误信息
        {
            Debug.LogError($"必须设置正确的默认流程以启动游戏");
        }
    }

    /// <summary>
    /// 模块启动时调用
    /// </summary>
    protected internal override void OnModuleStart()
    {
        base.OnModuleStart(); // 调用基类的启动方法
    }

    /// <summary>
    /// 模块停止时调用
    /// </summary>
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop(); // 调用基类的停止方法
        changeProcedureRequestPool.Clear(); // 清空流程请求池
        changeProcedureQ.Clear(); // 清空流程请求队列
        IsRunning = false; // 设置模块为未运行状态
    }

    /// <summary>
    /// 模块更新时调用
    /// </summary>
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime); // 调用基类的更新方法
    }

    /// <summary>
    /// 启动流程模块，设置为默认流程
    /// </summary>
    public async Task StartProcedure()
    {
        if (IsRunning) // 如果模块正在运行，则返回
            return;

        IsRunning = true; // 设置模块为运行状态
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain(); // 从对象池中获取一个流程请求对象
        changeProcedureRequest.TargetProcedure = defaultProcedure; // 设置目标流程为默认流程
        changeProcedureQ.Enqueue(changeProcedureRequest); // 将流程请求加入队列
        await ChangeProcedureInternal(); // 调用内部方法处理流程变更
    }

    /// <summary>
    /// 更改当前流程为指定的流程类型
    /// </summary>
    /// <typeparam name="T">要更改到的流程类型</typeparam>
    public async Task ChangeProcedure<T>() where T : BaseProcedure
    {
        await ChangeProcedure<T>(null); // 调用重载方法，传入null作为参数
    }

    /// <summary>
    /// 更改当前流程为指定的流程类型并带有可选参数
    /// </summary>
    /// <typeparam name="T">要更改到的流程类型</typeparam>
    /// <param name="value">新流程的可选参数</param>
    public async Task ChangeProcedure<T>(object value) where T : BaseProcedure
    {
        if (!IsRunning) // 如果模块未运行，则返回
            return;

        if (!procedures.TryGetValue(typeof(T), out BaseProcedure procedure)) // 尝试获取目标流程类型的实例
        {
            Debug.LogError($"更改流程失败，无法找到流程: ${typeof(T).FullName}"); // 记录错误信息
            return;
        }

        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain(); // 从对象池中获取一个流程请求对象
        changeProcedureRequest.TargetProcedure = procedure; // 设置目标流程
        changeProcedureRequest.Value = value; // 设置流程参数
        changeProcedureQ.Enqueue(changeProcedureRequest); // 将流程请求加入队列

        if (!IsChangingProcedure) // 如果未在进行流程变更
        {
            await ChangeProcedureInternal(); // 调用内部方法处理流程变更
        }
    }

    /// <summary>
    /// 内部处理流程变更过程
    /// </summary>
    private async Task ChangeProcedureInternal()
    {
        if (IsChangingProcedure) // 如果正在进行流程变更，则返回
            return;

        IsChangingProcedure = true; // 设置标识为正在进行流程变更
        while (changeProcedureQ.Count > 0) // 当队列中有流程变更请求时
        {
            ChangeProcedureRequest request = changeProcedureQ.Dequeue(); // 取出一个流程请求
            if (request == null || request.TargetProcedure == null) // 如果请求或目标流程为空，跳过
                continue;

            if (CurrentProcedure != null) // 如果当前有流程
            {
                await CurrentProcedure.OnLeaveProcedure(); // 调用当前流程的离开方法
            }
            CurrentProcedure = request.TargetProcedure; // 设置当前流程为目标流程
            await CurrentProcedure.OnEnterProcedure(request.Value); // 调用新流程的进入方法
        }
        IsChangingProcedure = false; // 结束流程变更，设置标识为未在进行流程变更
    }
}

/// <summary>
/// 表示一个更改当前流程的请求
/// </summary>
public class ChangeProcedureRequest
{
    public BaseProcedure TargetProcedure { get; set; } // 要更改到的流程
    public object Value { get; set; } // 新流程的可选参数
}
