using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// luanch���̽ű��̳й��̻���
/// </summary>
public class LaunchProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        Debug.Log("�л�Ϊlaunch��ģʽ");
        await Task.Yield();
    }
}

