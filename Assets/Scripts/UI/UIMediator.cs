using Config;
using Nirvana;
using System.Xml;
using UnityEngine;

/// <summary>
/// ������ UI��Meditor���࣬�ṩUI����Ĺ���
/// </summary>
/// <typeparam name="T">���� �����Ǹ���UI���� Ҫ�������UIView������</typeparam>
public abstract class UIMediator<T> : UIMediator where T : UIView
{
    protected T view;// �����UIview�Ķ���

    /// <summary>
    ///��д�����չʾ���� ��ʾUI����ʱ�����
    /// </summary>
    /// <param name="arg">��ʾuiʱ����Ĳ���</param>
    protected override void OnShow(object arg)
    {
        base.OnShow(arg);
        view = ViewObject.GetComponent<T>();

    }

    protected override void OnHide()
    {
        view = default;
        base.OnHide();
    }

    protected void Close()
    {
        TGameFramework.Instance.GetModule<UIModule>().CloseUI(this);
    }

    public override void InitMediator(UIView view)
    {
        base.InitMediator(view);

        OnInit(view as T);
    }

    protected virtual void OnInit(T view) { }
}

/// <summary>
/// UI��Mediator���࣬�����˻�����UI������
/// </summary>
public abstract class UIMediator
{
    // �¼���������UIʱ����
    public event System.Action OnMediatorHide;

    // UI��ͼ����
    public GameObject ViewObject { get; set; }
    // �¼������ڹ���UI�¼�
    public UIEventTable eventTable { get; set; }
    // ���Ʊ����ڹ���UIԪ������
    public UINameTable nameTable { get; set; }
    // ����˳�����ڿ���UI����ʾ�㼶
    public int SortingOrder { get; set; }
    // UIģʽ�����ڶ���UI����ʾģʽ
    public UIMode UIMode { get; set; }

    /// <summary>
    /// ��ʼ��Mediator
    /// </summary>
    /// <param name="view">UIView����</param>
    public virtual void InitMediator(UIView view) { }

    /// <summary>
    /// ��ʾUI����
    /// </summary>
    /// <param name="viewObject"></param>
    /// <param name="arg"></param>
    public void Show(GameObject viewObject, object arg)
    {
        ViewObject = viewObject;
        // ��ȡ�¼������
        eventTable = ViewObject.GetComponent<UIEventTable>();
        // ��ȡ���Ʊ����
        nameTable = viewObject.GetComponent<UINameTable>();
        // ����OnShow����
        OnShow(arg);
    }

    /// <summary>
    /// ����ʾUIʱ���ô˷���
    /// </summary>
    /// <param name="arg"></param>
    protected virtual void OnShow(object arg) { }

    /// <summary>
    /// ����UI
    /// </summary>
    public void Hide()
    {
        // ����OnHide����
        OnHide();
        // ���������¼�
        OnMediatorHide?.Invoke();
        OnMediatorHide = null;
        // �����ͼ����
        ViewObject = default;
    }

    /// <summary>
    /// ������UIʱ���ô˷���
    /// </summary>
    protected virtual void OnHide() { }

    /// <summary>
    /// ����UI
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        OnUpdate(deltaTime);
    }

    /// <summary>
    /// ������UIʱ���ô˷���
    /// </summary>
    /// <param name="deltaTime"></param>
    protected virtual void OnUpdate(float deltaTime) { }
}

