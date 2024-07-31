using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prefab;
using DG.Tweening;
using Item;

namespace CoinPileScript
{
    public class CoinPileTunnelAnim : MonoBehaviour
    {
        private readonly float marchStepOffset = -0.18f;
        private readonly float marchStepTime = 0.1f;
        
        public void EmergeFromTunnel(GameObject coinPile, CoinPrefab[] coinChildren, Action onComplete)
        {
            var sequence = DOTween.Sequence();
            foreach (var coinPrefab in coinChildren)
            {
                sequence.AppendCallback(() => { coinPrefab.Show(); })
                    .Append(coinPile.transform.DOBlendableMoveBy(new Vector3(0, 0, marchStepOffset), marchStepTime));
            }

            sequence.AppendCallback(() =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            });
        }

        public void TunnelDisappear(GameObject coinTunnelGameObject, Action onComplete)
        {
            var coinTunnel = coinTunnelGameObject.GetComponent<CoinTunnel>();
            if (coinTunnel != null)
            {
                coinTunnel.Disappear(onComplete);
            }
        }
    }
}
