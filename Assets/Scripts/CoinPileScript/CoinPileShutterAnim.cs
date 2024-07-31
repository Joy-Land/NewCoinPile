using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using DG.Tweening;
using Manager;

namespace CoinPileScript
{
    public class CoinPileShutterAnim : MonoBehaviour
    {
        private readonly float flashDest = 0.2f;
        private readonly float flashDuration = 0.6f;
        private readonly int flashNum = 10;
        private readonly float flashPeriod = 1.0f;
        
        public void OpenShutter(List<GameObject> coinShutterGameObjectList, List<Boolean> coinShutterStatusList, List<Action> onCompleteList, Action onAllComplete)
        {
            for (var i = 0; i < coinShutterGameObjectList.Count; i++)
            {
                OpenShutterAnim(coinShutterGameObjectList[i], coinShutterStatusList[i], onCompleteList[i], i == coinShutterGameObjectList.Count - 1 ? onAllComplete : null);
            }
        }
        
        private void OpenShutterAnim(GameObject coinShutterGameObject, Boolean isOpen, Action onComplete, Action onAllComplete)
        {
            var coinShutterComponent = coinShutterGameObject.GetComponent<CoinShutter>();
            if (coinShutterComponent != null)
            {
                coinShutterComponent.ChangeShutterStatus(isOpen, false, onComplete, onAllComplete);
            }
        }
        
        public void ShakeShutter(GameObject coinShutterGameObject, GameObject topCoinGroupGameObject, Action onComplete)
        {
            var sequence = DOTween.Sequence();

            sequence.AppendCallback(() =>
                {
                    ShakeManager.Instance.Vibrate();
                })
                .Join(topCoinGroupGameObject.transform.DOLocalMoveY(flashDest, flashDuration).SetEase(Ease.Flash, flashNum * 2, flashPeriod))
                .Join(coinShutterGameObject.transform.DOLocalMoveY(flashDest, flashDuration).SetEase(Ease.Flash, flashNum * 2, flashPeriod))
                .AppendCallback(() =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                });
        }
    }
}
