using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Manager
{
    public class UIEditorManager : SingletonBaseMono<UIEditorManager>
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

        public void OneMoreTimeCallback(bool nextLevel)
        {
            GameManager.Instance.ReStart(nextLevel);
        }
    }
}
