using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using ThinRL.Core.Tools;
using UniFramework.Machine;
using UnityEngine;

public class FsmPreloadingState : IStateNode
{
    //private GameObject m_UIStartGameNode;
    private StateMachine m_Machine;


    public void OnCreate(StateMachine machine)
    {
        m_Machine = machine;
    }

    public void OnEnter()
    {


        NetworkManager.Instance.Initialize("https://xuyuanchi-grayscale.joylandstudios.com/");

        J.Minigame.Initialize(false, PlayerPrefsManager.GetUserBool(GamePlayerPrefsKey.EnableVibrate, true));

        UIManager.Instance.Init();

        AudioManager.Instance.LoadAudioConfig();

        m_Machine.ChangeState<FsmStartGameState>();
    }  

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }

}
