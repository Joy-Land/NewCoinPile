using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SOData;

namespace CoinPileScript
{
    public class CoinPilePanelMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinPanel;
        
        private Dictionary<GameObject, Dictionary<int, GameObject>> coinPilePanelMap;

        public void Init(List<GameObject> coinGameObjectList, List<CoinPilePanelData> coinPilePanelItemList)
        {
            foreach (var coinPilePanelItem in coinPilePanelItemList)
            {
                // 销毁上一次的状态
                if (coinPilePanelMap != null)
                {
                    foreach (var coinPilePanelItemMap in coinPilePanelMap.Values)
                    {
                        foreach (var coinPanelItem in coinPilePanelItemMap.Values)
                        {
                            Destroy(coinPanelItem);
                        }
                    }
                }
                
                // 生成 coinPanel 
                var coinPanelGameObject = Instantiate(coinPanel, this.transform);
                coinPanelGameObject.transform.localPosition = coinPilePanelItem.coinPanelMesh.position;
                var coinPanelComponent = coinPanelGameObject.GetComponent<CoinPanel>();
                if (coinPanelComponent != null)
                {
                    coinPanelComponent.SetUpColor(coinPilePanelItem.coinPanelMesh.color);
                    coinPanelComponent.SetUpBlendShape(coinPilePanelItem.coinPanelBlendShape);
                }
                
                // 初始化 coinPilePanelMap 状态
                coinPilePanelMap = new Dictionary<GameObject, Dictionary<int, GameObject>>();
                foreach (var coinPanelItem in coinPilePanelItem.coinPanelItemList.data)
                {
                    if (coinPanelItem.coinPileIndex < coinGameObjectList.Count)
                    {
                        var coinPileGameObject = coinGameObjectList[coinPanelItem.coinPileIndex];
                        if (coinPileGameObject != null)
                        {
                            if (!coinPilePanelMap.ContainsKey(coinPileGameObject))
                            {
                                coinPilePanelMap.Add(coinPileGameObject, new Dictionary<int, GameObject>());
                            }

                            if (coinPilePanelMap.TryGetValue(coinPileGameObject, out var coinPilePanelItemMap))
                            {
                                coinPilePanelItemMap.Add(coinPanelItem.coinElementIndex, coinPanelGameObject);
                            }
                        }
                    }
                }
            }
        }

        public void DestroyPanel(List<GameObject> coinGameObjectList, List<CoinPanelItem> coinPanelItemList)
        {
            foreach (var coinPanelItem in coinPanelItemList)
            {
                var coinPileGameObject = coinGameObjectList[coinPanelItem.coinPileIndex];
                if (coinPilePanelMap.TryGetValue(coinPileGameObject,
                        out var coinPilePanelItemMap))
                {
                    if (coinPilePanelItemMap.Remove(coinPanelItem.coinElementIndex, out var coinPanelGameObject))
                    {
                        Destroy(coinPanelGameObject);
                    }

                    if (coinPilePanelItemMap.Count == 0)
                    {
                        coinPilePanelMap.Remove(coinPileGameObject);
                    }
                }
            }
        }
    }
}
