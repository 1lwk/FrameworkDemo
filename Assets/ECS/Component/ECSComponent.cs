/// <summary>
/// ��ʾʵ��-���-ϵͳ��ECS������е���������Ļ��ࡣ
/// </summary>
public abstract class ECSComponent
{
    /// <summary>
    /// ��ȡ�������Ψһ��ʶ��
    /// </summary>
    public long ID { get; private set; }

    /// <summary>
    /// ��ȡ�����ô����������ʵ��ı�ʶ��
    /// </summary>
    public long EntityID { get; set; }

    /// <summary>
    /// ��ȡ������һ��ֵ����ֵָʾ������Ƿ��ѱ��ͷ�
    /// </summary>
    public bool Disposed { get; set; }

    /// <summary>
    /// ��ȡ�����������ʵ��
    /// ��� EntityID Ϊ 0���򷵻�Ĭ��ֵ
    /// </summary>
    public ECSEntity Entity
    {
        get
        {
            // ��� EntityID Ϊ 0���򷵻�Ĭ��ֵ
            if (EntityID == 0)
                return default;

            // ͨ�� EntityID �� ECS ģ���в��Ҳ����ض�Ӧ��ʵ��
            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(EntityID);
        }
    }

    /// <summary>
    /// ��ʼ�� ECSComponent �����ʵ����������Ψһ�� ID
    /// </summary>
    public ECSComponent()
    {
        // ����һ���µ�Ψһ ID ����ֵ�� ID ����
        ID = IDGenerator.NewInstanceID();
        // ��ʼ�� Disposed ����Ϊ false
        Disposed = false;
    }
}
