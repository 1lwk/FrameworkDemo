using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;

/// <summary>
/// 监视器类，用于管理等待对象和设置结果
/// </summary>
public class Monitor
{
    // 字典，用于存储等待对象，键为类型，值为对象
    private readonly Dictionary<Type, object> waitObjects = new Dictionary<Type, object>();

    /// <summary>
    /// 创建一个新的等待对象并添加到字典中
    /// </summary>
    /// <typeparam name="T">等待对象的类型，必须是结构体</typeparam>
    /// <returns>新的等待对象</returns>
    public WaitObject<T> Wait<T>() where T : struct
    {
        WaitObject<T> o = new WaitObject<T>();
        waitObjects.Add(typeof(T), o);
        return o;
    }

    /// <summary>
    /// 设置等待对象的结果并从字典中移除
    /// </summary>
    /// <typeparam name="T">结果的类型，必须是结构体</typeparam>
    /// <param name="result">结果值</param>
    public void SetResult<T>(T result) where T : struct
    {
        Type type = typeof(T);
        if (!waitObjects.TryGetValue(type, out object o))
            return;

        waitObjects.Remove(type);
        ((WaitObject<T>)o).SetResult(result);
    }

    /// <summary>
    /// 等待对象类，用于表示异步操作的结果
    /// </summary>
    /// <typeparam name="T">结果的类型，必须是结构体</typeparam>
    public class WaitObject<T> : INotifyCompletion where T : struct
    {
        /// <summary>
        /// 获取或设置是否已完成
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// 获取结果值
        /// </summary>
        public T Result { get; private set; }

        private Action callback;

        /// <summary>
        /// 设置结果并标记为已完成，调用回调
        /// </summary>
        /// <param name="result">结果值</param>
        public void SetResult(T result)
        {
            Result = result;
            IsCompleted = true;

            Action c = callback;
            callback = null;
            c?.Invoke();
        }

        /// <summary>
        /// 获取当前的等待对象（即自身）
        /// </summary>
        /// <returns>当前的等待对象</returns>
        public WaitObject<T> GetAwaiter()
        {
            return this;
        }

        /// <summary>
        /// 注册回调方法，在操作完成时调用
        /// </summary>
        /// <param name="callback">回调方法</param>
        public void OnCompleted(Action callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// 获取操作结果
        /// </summary>
        /// <returns>操作结果值</returns>
        public T GetResult()
        {
            return Result;
        }
    }
}