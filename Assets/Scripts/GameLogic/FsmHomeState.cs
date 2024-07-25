using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;

public class FsmHomeState : IStateNode
{
    public void OnCreate(StateMachine machine)
    {
        
    }

    public void OnEnter()
    {
        //来到主界面 切换背景
        //UIManager.Instance.SetBackground(0,)
        UIManager.Instance.OpenUI(UIViewID.UIHomePage, null);
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
