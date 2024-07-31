using Joyland.GamePlay;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    public GameObject uiStartGameNode;
    private StateMachine m_Machine;

    private void Awake()
    {
        //var data = new { };
        //var s = JsonConvert.SerializeObject(data);
        //console.error(s);

        m_Machine = new StateMachine(this);
        m_Machine.AddNode<FsmPreloadingState>();
        m_Machine.AddNode<FsmStartGameState>();
        m_Machine.AddNode<FsmHomeState>();
        m_Machine.AddNode<FsmGameState>();

        m_Machine.SetBlackboardValue("_UIStartGameNode", uiStartGameNode);
    }

    private void Start()
    {
        m_Machine.Run<FsmPreloadingState>();
    }

    private void Update()
    {
        m_Machine.Update();
    }
}
