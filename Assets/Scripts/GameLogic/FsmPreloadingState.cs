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

    private Sprite m_Bg;
    public void OnCreate(StateMachine machine)
    {
        m_Machine = machine;
    }

    public void OnEnter()
    {
        m_UIStartGameNode = m_Machine.GetBlackboardValue("_UIStartGameNode") as GameObject;

        NetworkManager.Instance.Initialize("https://xuyuanchi-grayscale.joylandstudios.com/");

        J.Minigame.Initialize(false, PlayerPrefsManager.GetUserBool(GamePlayerPrefsKey.EnableVibrate, true));

        UIManager.Instance.Init();

        m_Bg = Resources.Load<Sprite>("FirstAssets/bg2");
        UIManager.Instance.SetBackground(0, m_Bg);
        UIManager.Instance.GetAndOpenUIViewOnNode<UIStartGame>(m_UIStartGameNode, UIViewID.UIStartGame, 
            new UIViewConfig() { bundleName = "", layer = UIViewLayerEnum.Lowest, packageName = "" }, new EventArgsPack((int)LoadingStageEventCode.Finish));

        AudioManager.Instance.LoadAudioConfig();

        m_Machine.ChangeState<FsmStartGameState>();
    }

    public void OnExit()
    {
        Resources.UnloadAsset(m_Bg);
    }

    public void OnUpdate()
    {

    }

}
