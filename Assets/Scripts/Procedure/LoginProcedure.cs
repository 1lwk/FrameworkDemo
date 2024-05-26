using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 登录流程脚本继承流程基类脚本
/// </summary>
public class LoginProcedure : BaseProcedure
{
    public override Task OnEnterProcedure(object value)
    {
        Debug.Log("进入登录模式");
        return base.OnEnterProcedure(value);
    }

    public override Task OnLeaveProcedure()
    {
        Debug.Log("离开登录模式");
        return base.OnLeaveProcedure();
    }
}
