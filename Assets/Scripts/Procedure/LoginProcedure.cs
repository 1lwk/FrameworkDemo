using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ��¼���̽ű��̳����̻���ű�
/// </summary>
public class LoginProcedure : BaseProcedure
{
    public override Task OnEnterProcedure(object value)
    {
        Debug.Log("�����¼ģʽ");
        return base.OnEnterProcedure(value);
    }

    public override Task OnLeaveProcedure()
    {
        Debug.Log("�뿪��¼ģʽ");
        return base.OnLeaveProcedure();
    }
}
