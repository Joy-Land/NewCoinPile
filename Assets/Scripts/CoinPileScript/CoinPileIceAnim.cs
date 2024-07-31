using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Item;
using Manager;

namespace CoinPileScript
{
    public class CoinPileIceAnim : MonoBehaviour
    {
        private float shakeDuration = 1.0f;
        private Vector3 shakeStrength = new Vector3(0.06f, 0.06f, 0.0f);
        private int shakeNumber = 10;
        private float shakeRandom = 45.0f;
        
        public void MeltIce(GameObject coinIceGameObject, int status, Action onComplete)
        {
            CoinIce coinIce = coinIceGameObject.GetComponent<CoinIce>();
            if (coinIce != null)
            {
                // TODO: 播放冰破碎的效果动画
                coinIce.SetActiveStatus(status);
            }

            if (onComplete != null)
            {
                onComplete();
            }
        }

        public void ShakeIce(GameObject coinIceGameObject, GameObject topCoinGroupGameObject, Action onComplete)
        {
            var sequence = DOTween.Sequence();

            sequence.AppendCallback(() =>
                {
                    ShakeManager.Instance.Vibrate();
                })
                .Join(topCoinGroupGameObject.transform.DOShakePosition(shakeDuration, shakeStrength, shakeNumber, shakeRandom, false,
                    true, ShakeRandomnessMode.Harmonic))
                .Join(coinIceGameObject.transform.DOShakePosition(shakeDuration, shakeStrength, shakeNumber, shakeRandom, false,
                    true, ShakeRandomnessMode.Harmonic))
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
