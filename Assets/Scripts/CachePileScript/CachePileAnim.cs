using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Manager;
using Placeholder;

namespace CachePileScript
{
    public class CachePileAnim : MonoBehaviour
    {
        [SerializeField] private AudioClip coinFlyClip;
        [SerializeField] private float coinFlyClipVolume;
        [SerializeField] private AnimationCurve zCurve;
         
        private float flyDuration = 0.5f;
        private float intervalPercent = 0.2f;
        
        public void TransferCoinsFromCache(int number, GameObject src, int srcStartIndex, GameObject dest, int destStartIndex, Action onComplete)
        {
            TransferCoinsFromCacheAnim(number, src, srcStartIndex, dest, destStartIndex, onComplete);
        }
        
        private void TransferCoinsFromCacheAnim(int number, GameObject src, int srcStartIndex, GameObject dest, int destStartIndex, Action onComplete)
        {
            Sequence sequence = DOTween.Sequence();
            for (var i = 0; i < number; i++)
            {
                if (src.GetComponent<CoinStackPlaceholder>().GetIndexedPlaceholder(srcStartIndex, out GameObject srcPlaceholder) && dest.GetComponent<CoinStackPlaceholder>().GetIndexedPlaceholder(destStartIndex, out GameObject destPlaceholder))
                {
                    var coin = srcPlaceholder.GetComponent<CoinPlaceholder>().coin;
                    var position = destPlaceholder.transform.position;
                    sequence.InsertCallback(i*flyDuration * intervalPercent, () =>
                            {
                                SoundFXManager.Instance.PlaySoundFXClip(coinFlyClip, coin.transform.position, coinFlyClipVolume);
                                ShakeManager.Instance.Vibrate();
                            })
                            .Insert(i*flyDuration * intervalPercent,coin.transform.DOMoveX(position.x, flyDuration))
                            .Insert(i*flyDuration * intervalPercent,coin.transform.DOMoveY(position.y, flyDuration))
                            .Insert(i*flyDuration * intervalPercent,coin.transform.DOMoveZ(position.z, flyDuration).SetEase(zCurve))
                            .Join(coin.transform.DORotateQuaternion(destPlaceholder.transform.rotation, flyDuration)).onComplete +=
                        () =>
                        {
                            // Debug.Log("Move Complete");
                            coin.transform.parent = destPlaceholder.transform;
                            destPlaceholder.GetComponent<CoinPlaceholder>().coin = coin;
                        };
                }
    
                srcStartIndex--;
                destStartIndex++;
            }

            sequence.onComplete += () =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            };
        }
    }
}
