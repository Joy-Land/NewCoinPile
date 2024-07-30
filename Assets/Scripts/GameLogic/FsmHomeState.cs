using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

public class FsmHomeState : IStateNode
{
    public void OnCreate(StateMachine machine)
    {
        
    }

    public void OnEnter()
    {
        //来到主界面 切换背景
        //UIManager.Instance.SetBackground(0,)
        //var assetHandle = YooAssets.LoadAssetAsync<Sprite>("home_bg");
        //assetHandle.Completed += (handle) =>
        //{
        //    var sp = handle.AssetObject as Sprite;
        //    UIManager.Instance.SetBackground(0, sp);
        //};
        UIManager.Instance.OpenUI(UIViewID.UIHomePage, null);
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
