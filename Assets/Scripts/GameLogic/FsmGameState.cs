using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UniFramework.Machine;
using UnityEngine;

public class FsmGameState : IStateNode
{
    private StateMachine m_Machine;
    public void OnCreate(StateMachine machine)
    {
        m_Machine = machine;

    }

    public void OnEnter()
    {
        EventManager.Instance.AddEvent(GameEventGlobalDefine.ExitGamePage, OnExitGameEvent);
        UIManager.Instance.OpenUI(UIViewID.UIGamePage);
    }

    public void OnExit()
    {
        EventManager.Instance.AddEvent(GameEventGlobalDefine.ExitGamePage, OnExitGameEvent);
        UIManager.Instance.CloseUI(UIViewID.UIGamePage);
    }

    public void OnExitGameEvent(object sender, EventArgsPack e)
    {
        m_Machine.ChangeState<FsmHomeState>();
    }
    public void OnUpdate()
    {
        
    }
}
