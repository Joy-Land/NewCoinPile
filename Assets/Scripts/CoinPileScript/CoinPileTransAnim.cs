using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Manager;
using Placeholder;

namespace CoinPileScript
{
    public class CoinPileTransAnim : MonoBehaviour
    {
        [SerializeField] private AudioClip coinFlyClip;
        [SerializeField] private float coinFlyClipVolume;
        [SerializeField] private AnimationCurve zCurve;
        
        private readonly float flyDuration = 0.5f;
        private readonly float intervalPercent = 0.2f;
        
        public void TransferCoins(List<int> coinTransNumberList, List<Stack<GameObject>> coinGoList, List<GameObject> destGoList, List<int> destStartIndexList, List<Action> onCompleteList, Action onAllComplete, Action onAnimStart, Action onAnimEnd)
        {
            StartCoroutine(TransferCoinsAnimList(coinTransNumberList, coinGoList, destGoList, destStartIndexList, onCompleteList, onAllComplete, onAnimStart, onAnimEnd));
        }
        
        private IEnumerator TransferCoinsAnimList(List<int> coinTransNumberList, List<Stack<GameObject>> coinGoList, List<GameObject> destGoList, List<int> destStartIndexList, List<Action> onCompleteList, Action onAllComplete, Action onAnimStart, Action onAnimEnd)
        {
            if (coinTransNumberList.Count != coinGoList.Count && coinGoList.Count != destGoList.Count && destGoList.Count != destStartIndexList.Count && destStartIndexList.Count != onCompleteList.Count)
            {
                throw new Exception("参数数量不匹配");
            }

            if (onAnimStart != null)
            {
                onAnimStart();
            }
            
            for (var i = 0; i < coinTransNumberList.Count; i++)
            {
                TransferCoinsAnim(coinTransNumberList[i], coinGoList[i], destGoList[i], destStartIndexList[i], onCompleteList[i], i == coinTransNumberList.Count - 1 ? onAllComplete : null, i == coinTransNumberList.Count - 1 ? onAnimEnd : null);
                if (i < coinTransNumberList.Count - 1)
                {
                    yield return new WaitForSeconds(flyDuration);
                }
            }
        }
        
        private void TransferCoinsAnim(int number, Stack<GameObject> coins, GameObject dest, int destStartIndex, Action onComplete, Action onAllComplete, Action onAnimEnd)
        {
            Sequence sequence = DOTween.Sequence();
            for (var i = 0; i < number; i++)
            {
                if (dest.GetComponent<CoinStackPlaceholder>().GetIndexedPlaceholder(destStartIndex, out GameObject destPlaceholder))
                {
                    var coin = coins.Pop();
                    var position = destPlaceholder.transform.position;
                    sequence.InsertCallback(i*flyDuration * intervalPercent, () =>
                            {
                                SoundFXManager.Instance.PlaySoundFXClip(coinFlyClip, coin.transform.position, coinFlyClipVolume);
                                ShakeManager.Instance.Vibrate();
                            })
                            .Join(coin.transform.DOMoveX(position.x, flyDuration))
                            .Join(coin.transform.DOMoveY(position.y, flyDuration))
                            .Join(coin.transform.DOMoveZ(position.z, flyDuration).SetEase(zCurve))
                            .Join(coin.transform.DORotateQuaternion(destPlaceholder.transform.rotation, flyDuration))
                            .InsertCallback(i*flyDuration * intervalPercent,() =>
                            {
                                // Debug.Log("Move Complete");
                                coin.transform.parent = destPlaceholder.transform;
                                destPlaceholder.GetComponent<CoinPlaceholder>().coin = coin;
                            });
                    
                    // 插入 onAnimEnd
                    if (onAnimEnd != null && i == number - 1)
                    {
                        sequence.InsertCallback(i * flyDuration * intervalPercent, () =>
                        {
                            onAnimEnd();
                        });
                    }
                }
                destStartIndex++;
            }

            // sequence.AppendCallback(() =>
            // {
            //     if (onComplete != null)
            //     {
            //         onComplete();
            //     }
            //
            //     if (onAllComplete != null)
            //     {
            //         onAllComplete();
            //     }
            // });
            
            sequence.onComplete += () =>
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
