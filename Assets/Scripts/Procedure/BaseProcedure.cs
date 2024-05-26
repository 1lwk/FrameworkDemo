using System.Threading.Tasks;

/// <summary>
/// ��Ϸ���̿��ƽű�����
/// </summary>
public abstract class BaseProcedure
{
    /// <summary>
    /// �ı����̵��첽����
    /// </summary>
    /// <typeparam name="T">Ŀ����������</typeparam>
    /// <param name="value">���ݸ�Ŀ�����̵Ŀ�ѡ����</param>
    /// <returns>��ʾ�첽����������</returns>
    public async Task ChangeProcedure<T>(object value = null) where T : BaseProcedure
    {
        await GameManager.Procedure.ChangeProcedure<T>(value); // ���� GameManager �е����̹������ı�����
    }

    /// <summary>
    /// ��������ʱ���첽����
    /// </summary>
    /// <param name="value">��������ʱ���ݵĲ���</param>
    /// <returns>��ʾ�첽����������</returns>
    public virtual async Task OnEnterProcedure(object value)
    {
        await Task.Yield(); // �첽�ó���ǰ�̵߳�ִ��Ȩ
    }

    /// <summary>
    /// �뿪����ʱ���첽����
    /// </summary>
    /// <returns>��ʾ�첽����������</returns>
    public virtual async Task OnLeaveProcedure()
    {
        await Task.Yield(); // �첽�ó���ǰ�̵߳�ִ��Ȩ
    }
}
