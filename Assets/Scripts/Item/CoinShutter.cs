using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public class CoinShutter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        private event Action OnShutterComplete;

        public void TriggerShutterComplete()
        {
            if (OnShutterComplete != null)
            {
                OnShutterComplete.Invoke();
            }
        }
        
        public void ChangeShutterStatus(Boolean isOpen, Boolean isInitial, Action onComplete, Action onAllComplete)
        {
            animator.SetBool("isInitial", isInitial);
            animator.SetBool("isOpen", isOpen);
            OnShutterComplete = () =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                if (onAllComplete != null)
                {
                    onAllComplete();
                }
            };
        }
    }
}
