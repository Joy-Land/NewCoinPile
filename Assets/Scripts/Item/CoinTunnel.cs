using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Item                                                                       
{
    public class CoinTunnel : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private Animator animator;

        public void ShowNumber(int number)
        {
            textMesh.text = number.ToString();
        }

        private event Action OnDisappearComplete;

        public void TriggerDisappearComplete()
        {
            if (OnDisappearComplete != null)
            {
                OnDisappearComplete.Invoke();
            }
        }

        public void Disappear(Action onComplete)
        {
            if (animator != null)
            {
                animator.SetTrigger("disappear");
                if (onComplete != null)
                {
                    OnDisappearComplete = onComplete;
                }
            }
        }
    }
}
