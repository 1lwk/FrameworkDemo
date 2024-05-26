using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// luanch过程脚本继承过程基类
/// </summary>
public class LaunchProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        Debug.Log("切换为launch的模式");
        await Task.Yield();
    }
}

