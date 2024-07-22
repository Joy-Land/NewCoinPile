using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Manager;
using Prefab;

namespace TransPileScript
{
    public class TransPileAnim : MonoBehaviour
    {
        [SerializeField] private AudioClip coverClip;
        [SerializeField] private float coverClipVolume;
        
        [SerializeField] private AudioClip coinHolderSwiperClip;
        [SerializeField] private float coinHolderSwiperClipVolume;
        
        private readonly float coinStockTopOffset = 6.0f;
        private readonly float coinStockTopMoveTime = 0.5f;
        private readonly float currentCoinStockMoveOffset = 15f;
        private readonly float coinStockMoveTime = 0.5f;
        
        [SerializeField] private GameObject coinStackTopPrefab;
        [SerializeField] private GameObject coinStockTopPlaceholder;
        private Transform coinStockTopPlaceholderTransform;

        public void Init()
        {
            coinStockTopPlaceholderTransform = coinStockTopPlaceholder.transform;
            coinStockTopPlaceholder.SetActive(false);
        }

        public void MoveTransferPile(CoinColor coinStockTopColor, GameObject currentTransGameObject, GameObject nextTransGameObject, GameObject lastTransGameObject, Action onComplete)
        {
            StartCoroutine(MoveTransferPileAnim(coinStockTopColor, currentTransGameObject, nextTransGameObject, lastTransGameObject, onComplete));
        }
        
        private IEnumerator MoveTransferPileAnim(CoinColor coinStockTopColor, GameObject currentTransGameObject, GameObject nextTransGameObject, GameObject lastTransGameObject, Action onComplete)
        {
            // 1. 生成一个盖子，并落下
            var destPos = coinStockTopPlaceholderTransform.position;
            var rotation = coinStockTopPlaceholderTransform.rotation;
            var startPos = new Vector3(destPos.x, destPos.y + coinStockTopOffset, destPos.z);
            var coinStackTop = Instantiate(coinStackTopPrefab,startPos, rotation);
            var colorMaterial = MaterialManager.Instance.GetMaterialByColor(coinStockTopColor);
            if (colorMaterial != null)
            {
                coinStackTop.GetComponent<CoinStackTopPrefab>().ChangeColor(colorMaterial);
            }
            yield return coinStackTop.transform.DOMove(destPos, coinStockTopMoveTime).WaitForCompletion();
            SoundFXManager.Instance.PlaySoundFXClip(coverClip, destPos, coverClipVolume);
            
            // 2. 将盒子盖和盒子合为一体
            if (currentTransGameObject != null)
            {
                coinStackTop.transform.parent = currentTransGameObject.transform;
            }
            
            // 3. 移动盒子列表
            Sequence sequence = DOTween.Sequence();
            Boolean isMoved = false;
            
            var currentPos = currentTransGameObject != null ? currentTransGameObject.transform.position : new Vector3();
            Vector3 nextPos = nextTransGameObject != null ? nextTransGameObject.transform.position : new Vector3();
            
            // 移动 current 运钱区
            if (currentTransGameObject != null)
            {
                sequence.Join(currentTransGameObject.transform.DOMove(new Vector3(currentPos.x + currentCoinStockMoveOffset, 0f, 0f), coinStockMoveTime));
                isMoved = true;
            }
            // 移动 next 运钱区
            if (nextTransGameObject != null)
            {
                sequence.Join(nextTransGameObject.transform.DOMove(currentPos, coinStockMoveTime));
                isMoved = true;
            }
            // 移动 last 运钱区
            if (lastTransGameObject != null)
            {
                sequence.Join(lastTransGameObject.transform.DOMove(nextPos, coinStockMoveTime));
                isMoved = true;
            }
            // 添加移动音效
            if (isMoved)
            {
                SoundFXManager.Instance.PlaySoundFXClip(coinHolderSwiperClip, destPos, coinHolderSwiperClipVolume);
            }

            sequence.onComplete += () =>
            {
                // 动画播放完成后，卸载 currentTransGameObject
                if (currentTransGameObject != null)
                {
                    Destroy(currentTransGameObject);
                }
            
                if (onComplete != null)
                {
                    onComplete();
                }
            };
        }
    }
}
