using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Manager;

namespace CoinPileScript
{
    public class CoinPileShakeAnim : MonoBehaviour
    {
        private readonly float flashDest = 0.2f;
        private readonly float flashDuration = 0.6f;
        private readonly int flashNum = 10;
        private readonly float flashPeriod = 1.0f;
        
        public void ShakeCoins(List<GameObject> coinGroupGameObjectList, Action onComplete)
        {
            var sequence = DOTween.Sequence();

            sequence.AppendCallback(() =>
            {
                ShakeManager.Instance.Vibrate();
            });

            foreach (var coinGroup in coinGroupGameObjectList)
            {
                sequence.Join(coinGroup.transform.DOLocalMoveY(flashDest, flashDuration)
                    .SetEase(Ease.Flash, flashNum * 2, flashPeriod));
            }
                
            sequence.AppendCallback(() =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            });
        }
    }    
}
