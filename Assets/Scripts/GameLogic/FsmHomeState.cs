using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

public class FsmHomeState : IStateNode
{
    private StateMachine m_Machine;
    public void OnCreate(StateMachine machine)
    {
        m_Machine = machine;
    }

    public void OnEnter()
    {
        EventManager.Instance.AddEvent(GameEventGlobalDefine.EnterGamePage, OnEnterGamePageEvent);
        //来到主界面 切换背景
        //UIManager.Instance.SetBackground(0,)
        var assetHandle = YooAssets.LoadAssetAsync<Sprite>("home_bg");
        assetHandle.Completed += (handle) =>
        {
            var sp = handle.AssetObject as Sprite;
            UIManager.Instance.SetBackground(0, sp);
        };
        UIManager.Instance.OpenUI(UIViewID.UIHomePage, null);
    }

    public void OnEnterGamePageEvent(object sender, EventArgsPack e)
    {
        m_Machine.ChangeState<FsmGameState>();
        
    }

    public void OnExit()
    {
        EventManager.Instance.RemoveEvent(GameEventGlobalDefine.ExitGamePage, OnEnterGamePageEvent);
        UIManager.Instance.CloseUI(UIViewID.UIHomePage);
    }

    public void OnUpdate()
    {
        
    }
}
