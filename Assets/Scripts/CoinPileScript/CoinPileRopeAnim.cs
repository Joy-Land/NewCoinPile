using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Manager;

namespace CoinPileScript
{
    public class CoinPileRopeAnim : MonoBehaviour
    {
        [SerializeField] private GameObject vfxTearRope;
        private readonly float vfxDuration = 1.3f;
        
        private readonly float flashDest = 0.2f;
        private readonly float flashDuration = 0.6f;
        private readonly int flashNum = 10;
        private readonly float flashPeriod = 1.0f;
        
        public void ShakeBlockedCoins(List<GameObject> coinGroupGameObjectList, Action onComplete)
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

        public void TearOffRope(GameObject srcCoinGameObject, GameObject destCoinGameObject, Action onComplete)
        {
            if (vfxTearRope != null)
            {
                var postion = srcCoinGameObject.transform.position + destCoinGameObject.transform.position;
                postion /= 2.0f;
                
                var vfxGameObject = Instantiate(vfxTearRope);
                vfxGameObject.transform.position = postion;
                    
                Destroy(vfxGameObject, vfxDuration);
            }
            
            var sequence = DOTween.Sequence();
            sequence.InsertCallback(0.2f, () =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            });
        }
    }
}
