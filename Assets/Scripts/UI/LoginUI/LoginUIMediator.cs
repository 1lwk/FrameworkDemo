using Koakuma.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��¼����ı༭�ű�
/// </summary>
public class LoginUIMediator : UIMediator<LoginUIView>
{
    public override void InitMediator(UIView view)
    {
        base.InitMediator(view);
    }

    /// <summary>
    /// ��ʼ���׶�
    /// </summary>
    /// <param name="view"></param>
    protected override void OnInit(LoginUIView view)
    {
        base.OnInit(view);
        view.btn_login.onClick.AddListener(OnClickLoginBtnCall);
        view.btn_shut.onClick.AddListener(OnClickShutLoginUiView);
    }
    
    /// <summary>
    /// �رյ�¼����
    /// </summary>
    private void OnClickShutLoginUiView()
    {
        GameManager.UI.CloseUI(UIViewID.LoginUI);
    }

    /// <summary>
    /// ��½��ť����ص�
    /// </summary>
    private void OnClickLoginBtnCall()
    {
        Debug.Log("����˵�¼��ť");
        view.text_showloginstate.text = "��¼��";
    }

    protected override void OnShow(object arg)
    {
        base.OnShow(arg);
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
    }

    protected override void OnHide()
    {
        base.OnHide();
        //view.btn_login.onClick.RemoveListener(OnClickLoginBtnCall);
        //view.btn_shut.onClick.RemoveListener(OnClickShutLoginUiView);
    }
}
