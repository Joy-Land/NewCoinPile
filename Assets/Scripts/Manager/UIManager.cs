using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using Joyland.GamePlay;
using ThinRL.Core;
using UnityEngine;

namespace Manager
{
    public class UIManager : SingletonBaseMono<UIManager>
    {
        // [SerializeField] private GameObject successCanvas;
        // [SerializeField] private GameObject failCanvas;

        public void ShowSuccess()
        {
            Joyland.GamePlay.UIManager.Instance.OpenUI(UIViewID.UIGameOver, new EventArgsPack(true));
        }

        public void ShowFail()
        {
            //failCanvas.SetActive(true);
            //Joyland.GamePlay.UIManager.Instance.OpenUI(UIViewID.UIGameOver,new EventArgsPack(false));
            Joyland.GamePlay.UIManager.Instance.OpenUI(UIViewID.UIGameSettlement, new EventArgsPack(false));
        }
    }
}
