using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// ��ʼ�����̽ű��̳й��̻���
/// </summary>
public class InitProcedure : BaseProcedure
{
    /// <summary>
    /// ��д��������ʱ���첽����
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override async Task OnEnterProcedure(object value)
    {
        UnityEngine.Debug.Log("�л�Ϊ���Ի���ģʽ");
        await Task.Yield();
    }
}
