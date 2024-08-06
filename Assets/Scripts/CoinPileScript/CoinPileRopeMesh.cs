using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SOData;
using Item;

namespace CoinPileScript
{
    public class CoinPileRopeMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinPileRope;

        private readonly float ropeLengthRate = 2f;
        
        private Dictionary<GameObject, GameObject> coinPileRopeMap;
        public void Init(List<GameObject> coinGameObjectList, List<CoinPileRopeItem> coinPileRopeList)
        {
            // 销毁之前保存的 GameObject
            if (coinPileRopeMap != null && coinPileRopeMap.Count > 0)
            {
                foreach (GameObject coinPileRopeMesh in coinPileRopeMap.Values)
                {
                    Destroy(coinPileRopeMesh);
                }
            }
            
            // 初始化 map
            coinPileRopeMap = new Dictionary<GameObject, GameObject>(coinPileRopeList.Count * 2);
            
            // 生成 Rope
            foreach (var coinPileRopeItem in coinPileRopeList)
            {
                var srcCoinPile = coinGameObjectList[coinPileRopeItem.srcCoinPileIndex];
                var srcCoinPileComponent = srcCoinPile.GetComponent<CoinPile>();
                Transform srcPosition = null;
                if (srcCoinPileComponent != null)
                {
                    srcPosition = srcCoinPileComponent.GetCoinPileElementRopePosition(coinPileRopeItem.srcCoinElementIndex);
                    srcCoinPileComponent.SetCoinPileElementRopePinStatus(coinPileRopeItem.srcCoinElementIndex, true);
                }
                
                var destCoinPile = coinGameObjectList[coinPileRopeItem.destCoinPileIndex];
                var destCoinPileComponent = destCoinPile.GetComponent<CoinPile>();
                Transform destPosition = null;
                if (destCoinPileComponent != null)
                {
                    destPosition = destCoinPileComponent.GetCoinPileElementRopePosition(coinPileRopeItem.destCoinElementIndex);
                    destCoinPileComponent.SetCoinPileElementRopePinStatus(coinPileRopeItem.destCoinElementIndex, true);
                }
                
                // 生成绳子
                var coinPileRopeGameObject = Instantiate(coinPileRope, transform);
                
                // 设置 coinPileRopeGameObject 的起点和终点，以及绳子的长度
                var coinRopeComponent = coinPileRopeGameObject.GetComponent<CoinRope>();
                if (coinRopeComponent != null && srcPosition != null && destPosition != null)
                {
                    coinRopeComponent.SetRopePosition(srcPosition, destPosition);
                    var distance = Vector3.Distance(srcPosition.position, destPosition.position);
                    coinRopeComponent.SetRopeLength(distance * ropeLengthRate);
                }
                
                // 添加 coinPileRopeMap 和 coinPileRopeReverseMap 记录
                coinPileRopeMap.Add(srcCoinPile, coinPileRopeGameObject);
                coinPileRopeMap.Add(destCoinPile, coinPileRopeGameObject);
            }
        }

        public Boolean GetRopeMesh(GameObject srcCoinPile, GameObject destCoinPile, out GameObject ropeMesh)
        {
            if (coinPileRopeMap.TryGetValue(srcCoinPile, out var coinPileRopeGameObject) && coinPileRopeMap.TryGetValue(destCoinPile, out var anotherCoinPileRopeGameObject))
            {
                if (coinPileRopeGameObject == anotherCoinPileRopeGameObject)
                {
                    ropeMesh = coinPileRopeGameObject;
                    return true;
                }
            }

            ropeMesh = null;
            return false;
        }
        
        public void DestroyRopeMesh(GameObject srcCoinPile, GameObject destCoinPile)
        {
            if (coinPileRopeMap.Remove(srcCoinPile, out var coinPileRopeGameObject))
            {
                Destroy(coinPileRopeGameObject);
            }
            
            if (coinPileRopeMap.Remove(destCoinPile, out var anotherCoinPileRopeGameObject))
            {
                Destroy(anotherCoinPileRopeGameObject);
            }
        }
    }
}
