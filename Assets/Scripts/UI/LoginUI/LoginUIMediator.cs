using Koakuma.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 登录界面的编辑脚本
/// </summary>
public class LoginUIMediator : UIMediator<LoginUIView>
{
    public override void InitMediator(UIView view)
    {
        base.InitMediator(view);
    }

    /// <summary>
    /// 初始化阶段
    /// </summary>
    /// <param name="view"></param>
    protected override void OnInit(LoginUIView view)
    {
        base.OnInit(view);
        view.btn_login.onClick.AddListener(OnClickLoginBtnCall);
        view.btn_shut.onClick.AddListener(OnClickShutLoginUiView);
    }
    
    /// <summary>
    /// 关闭登录界面
    /// </summary>
    private void OnClickShutLoginUiView()
    {
        GameManager.UI.CloseUI(UIViewID.LoginUI);
    }

    /// <summary>
    /// 登陆按钮点击回调
    /// </summary>
    private void OnClickLoginBtnCall()
    {
        Debug.Log("点击了登录按钮");
        view.text_showloginstate.text = "登录中";
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
