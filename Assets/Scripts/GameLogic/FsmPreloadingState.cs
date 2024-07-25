using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using ThinRL.Core.Tools;
using UniFramework.Machine;
using UnityEngine;

public class FsmPreloadingState : IStateNode
{
    private GameObject m_UIStartGameNode;
    private StateMachine m_Machine;
    public void OnCreate(StateMachine machine)
    {
        m_Machine = machine;
    }

    public void OnEnter()
    {
        m_UIStartGameNode = m_Machine.GetBlackboardValue("_UIStartGameNode") as GameObject;

        NetworkManager.Instance.Initialize("https://xuyuanchi-grayscale.joylandstudios.com/");

        J.Minigame.Initialize(false, PlayerPrefsManager.GetUserBool(GamePlayerPrefsKey.EnableVibrate, true));

        //console.error(s.textureRect);
        //设置ui背景
        UIManager.Instance.SetBackground(1, Resources.Load<Sprite>("TempTest/img_pattern_loading"));
        UIManager.Instance.GetAndOpenUIViewOnNode<UIStartGame>(m_UIStartGameNode, UIViewID.UIStartGame, 
            new UIViewConfig() { bundleName = "", layer = UIViewLayerEnum.Lowest, packageName = "" }, new EventArgsPack((int)LoadingStageEventCode.Finish));

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
