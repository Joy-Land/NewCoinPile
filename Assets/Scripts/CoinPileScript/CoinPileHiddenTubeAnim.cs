using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CoinPileScript
{
    public class CoinPileHiddenTubeAnim : MonoBehaviour
    {
        private readonly float hiddenSpeed = 0.06f;
        
        public void HideCoinTube(GameObject mesh, int hiddenNumber, Action onComplete)
        {
            mesh.transform.DOScaleY(0.0f, hiddenSpeed * hiddenNumber).onComplete += () =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            };
        }
    }
}
