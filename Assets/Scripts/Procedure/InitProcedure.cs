using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// 初始化过程脚本继承过程基类
/// </summary>
public class InitProcedure : BaseProcedure
{
    /// <summary>
    /// 重写进入流程时的异步方法
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override async Task OnEnterProcedure(object value)
    {
        UnityEngine.Debug.Log("切换为初试化的模式");
        await Task.Yield();
    }
}
