using System;
using System.Collections.Generic;
using UnityEngine;
using SOData;
using Manager;

namespace CoinPileScript
{
    public class CoinPileCollection : MonoBehaviour
    {
        [SerializeField] private GameObject coinPilePrefab;

        private List<GameObject> coinGameObjectList = new List<GameObject>();

        public void Init(List<CoinList> coinLists)
        {
            if (coinGameObjectList.Count > 0)
            {
                foreach (var coinGo in coinGameObjectList)
                {
                    Destroy(coinGo);
                }
            }
            
            coinGameObjectList = new List<GameObject>();
            
            for (var i= 0; i < coinLists.Count; i++)
            {
                var coinListSetting = coinLists[i];
                var coinPileGo = Instantiate(coinPilePrefab, transform);
                coinGameObjectList.Add(coinPileGo);
                coinPileGo.transform.localPosition =
                    new Vector3(coinListSetting.position.x, coinListSetting.position.y, 0);
                var coinPile = coinPileGo.GetComponent<CoinPile>();
                if (coinPile != null)
                {
                    coinPile.SetCoinPileCollection(this);
                    if (i < coinLists.Count)
                    {
                        coinPile.Init(coinLists[i].data);
                    }
                    else
                    {
                        throw new Exception("第" + i + "个 coinPile 缺乏配置文件");
                    }
                }
            }
        }

        public Boolean CheckCoinPilesHasColor(CoinColor color, out List<int> coinIndexList)
        {
            coinIndexList = new List<int>();
            Boolean checkFlag = false;
            for (var i = 0; i < coinGameObjectList.Count; i++)
            {
                var coinGameObject = coinGameObjectList[i];
                var coinPileComponent = coinGameObject.GetComponent<CoinPile>();
                if (coinPileComponent != null)
                {
                    if (coinPileComponent.CheckCoinTopHasSameColor(color))
                    {
                        checkFlag = true;
                        coinIndexList.Add(i);
                    }
                }
            }
            return checkFlag;
        }

        public void CheckCoinPileCanMeltIce()
        {
            for (var i = 0; i < coinGameObjectList.Count; i++)
            {
                var coinGameObject = coinGameObjectList[i];
                var coinPileComponent = coinGameObject.GetComponent<CoinPile>();
                if (coinPileComponent != null)
                {
                    coinPileComponent.TryMeltIce();
                }
            }
        }
    }
}
