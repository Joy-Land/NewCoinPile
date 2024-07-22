using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Manager
{
    public class UIManager : SingletonBaseMono<UIManager>
    {
        [SerializeField] private GameObject successCanvas;
        [SerializeField] private GameObject failCanvas;

        public void ShowSuccess()
        {
            successCanvas.SetActive(true);
        }
        
        public void ShowFail()
        {
            failCanvas.SetActive(true);
        }
        
        public void HideSuccess()
        {
            successCanvas.SetActive(false);
        }
        
        public void HideFail()
        {
            failCanvas.SetActive(false);
        }

        public void OneMoreTimeCallback(Boolean nextLevel)
        {
            GameManager.Instance.ReStart(nextLevel);
        }
    }
}
