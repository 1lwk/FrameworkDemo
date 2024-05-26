using System.Threading.Tasks;

/// <summary>
/// 游戏流程控制脚本基类
/// </summary>
public abstract class BaseProcedure
{
    /// <summary>
    /// 改变流程的异步方法
    /// </summary>
    /// <typeparam name="T">目标流程类型</typeparam>
    /// <param name="value">传递给目标流程的可选参数</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task ChangeProcedure<T>(object value = null) where T : BaseProcedure
    {
        await GameManager.Procedure.ChangeProcedure<T>(value); // 调用 GameManager 中的流程管理器改变流程
    }

    /// <summary>
    /// 进入流程时的异步方法
    /// </summary>
    /// <param name="value">进入流程时传递的参数</param>
    /// <returns>表示异步操作的任务</returns>
    public virtual async Task OnEnterProcedure(object value)
    {
        await Task.Yield(); // 异步让出当前线程的执行权
    }

    /// <summary>
    /// 离开流程时的异步方法
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    public virtual async Task OnLeaveProcedure()
    {
        await Task.Yield(); // 异步让出当前线程的执行权
    }
}
