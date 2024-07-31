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
        
        // 状态
        private List<GameObject> coinGameObjectList = new List<GameObject>();
        private Dictionary<GameObject, CoinPileRopeItem> coinPileRopeMap;
        
        // 组件
        private CoinPileRopeMesh coinPileRopeMesh;
        private CoinPileRopeAnim coinPileRopeAnim;

        public void Init(List<CoinList> coinLists, CoinPileRopeList coinPileRopeList)
        {
            // 初始化钱堆
            if (coinGameObjectList.Count > 0)
            {
                foreach (var coinGo in coinGameObjectList)
                {
                    var coinPile = coinGo.GetComponent<CoinPile>();
                    if (coinPile != null)
                    {
                        coinPile.Destroy();
                    }
                    
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
                        coinPile.Init(coinLists[i].data, coinListSetting.hasTunnel, coinListSetting.tunnelStartIndex);
                    }
                    else
                    {
                        throw new Exception("第" + i + "个 coinPile 缺乏配置文件");
                    }
                }
            }
            
            // 初始化钱币连线状态
            coinPileRopeMap = new Dictionary<GameObject, CoinPileRopeItem>(coinPileRopeList.data.Count * 2);
            foreach (CoinPileRopeItem lineSetting in coinPileRopeList.data)
            {
                // 添加 SRC
                coinPileRopeMap.Add(coinGameObjectList[lineSetting.srcCoinPileIndex], new CoinPileRopeItem()
                {
                    srcCoinPileIndex = lineSetting.srcCoinPileIndex,
                    srcCoinElementIndex = lineSetting.srcCoinElementIndex,
                    destCoinPileIndex = lineSetting.destCoinPileIndex,
                    destCoinElementIndex = lineSetting.destCoinElementIndex
                });
                
                // 添加 DEST
                coinPileRopeMap.Add(coinGameObjectList[lineSetting.destCoinPileIndex], new CoinPileRopeItem()
                {
                    srcCoinPileIndex = lineSetting.destCoinPileIndex,
                    srcCoinElementIndex = lineSetting.destCoinElementIndex,
                    destCoinPileIndex = lineSetting.srcCoinPileIndex,
                    destCoinElementIndex = lineSetting.srcCoinElementIndex
                });
            }
            
            // 初始化钱币连线 Mesh 和 Anim 组件
            coinPileRopeMesh = GetComponent<CoinPileRopeMesh>();
            if (coinPileRopeMesh == null)
            {
                throw new Exception("coinPileRopeMesh component missing");
            }
            coinPileRopeMesh.Init(coinGameObjectList, coinPileRopeList.data);

            coinPileRopeAnim = GetComponent<CoinPileRopeAnim>();
            if (coinPileRopeAnim == null)
            {
                throw new Exception("coinPileRopeAnim component missing");
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
            foreach (var coinGameObject in coinGameObjectList)
            {
                var coinPileComponent = coinGameObject.GetComponent<CoinPile>();
                if (coinPileComponent != null)
                {
                    coinPileComponent.TryMeltIce();
                }
            }
        }
        
        public void CheckCoinPileCanOpenShutter(GameObject excludeGameObject, int excludeCoinId)
        {
            foreach (var coinGameObject in coinGameObjectList)
            {
                var coinPileComponent = coinGameObject.GetComponent<CoinPile>();
                if (coinPileComponent != null)
                {
                    if (coinGameObject == excludeGameObject)
                    {
                        coinPileComponent.TryOpenShutter(excludeCoinId);
                    }
                    else
                    {
                        coinPileComponent.TryOpenShutter(-1);
                    }
                }
            }
        }

        public Boolean CheckAllCoinPilesIsFrozenOrShuttered()
        {
            foreach (var coinGameObject in coinGameObjectList)
            {
                var coinPileComponent = coinGameObject.GetComponent<CoinPile>();
                if (coinPileComponent != null)
                {
                    if (!coinPileComponent.CheckTopCoinIsFrozen() && !coinPileComponent.CheckTopCoinIsShuttered())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Boolean CheckCoinPileIsBlockByRope(GameObject coinGameObject, int coinGroupId, Action onBlock)
        {
            // 根据 coinGameObject 找到记录的绳子的状态
            if (coinPileRopeMap.TryGetValue(coinGameObject, out var coinPileRopeItem))
            {
                // 检测栈顶是不是连接着绳子
                if (coinGroupId == coinPileRopeItem.srcCoinElementIndex)
                {
                    // 再检测绳子的另一端是不是处于栈顶
                    var srcCoinGameObject = coinGameObject;
                    var srcCoinPileComponent = srcCoinGameObject.GetComponent<CoinPile>();
                    var destCoinGameObject = coinGameObjectList[coinPileRopeItem.destCoinPileIndex];
                    var destCoinPileComponent = destCoinGameObject.GetComponent<CoinPile>();
                    if (srcCoinPileComponent != null && destCoinPileComponent != null)
                    {
                        if (destCoinPileComponent.CheckIsBelowTopCoin(coinPileRopeItem.destCoinElementIndex))
                        {
                            // 如果在栈顶下面，说明被挡住了，就不销毁绳子，然后把当前绳子两端的钱堆都加上震动动画
                            var coinGroupGameObjectList = new List<GameObject>();
                            if(srcCoinPileComponent.GetCoinGroupsAboveIndex(coinPileRopeItem.srcCoinElementIndex, out var srcCoinGroups) && destCoinPileComponent.GetCoinGroupsAboveIndex(coinPileRopeItem.destCoinElementIndex, out var destCoinGroups))
                            {
                                coinGroupGameObjectList.AddRange(srcCoinGroups);
                                coinGroupGameObjectList.AddRange(destCoinGroups);
                                
                                coinPileRopeAnim.ShakeBlockedCoins(coinGroupGameObjectList, () =>
                                {
                                    if (onBlock != null)
                                    {
                                        onBlock();
                                    }
                                });
                            }
                            return true;
                        }
                    
                        // 如果处于栈顶，就销毁绳子，修改记录的绳子的状态，然后触发绳子另一端钱堆的点击
                        coinPileRopeMap.Remove(srcCoinGameObject);
                        coinPileRopeMap.Remove(destCoinGameObject);

                        if (coinPileRopeMesh.GetRopeMesh(srcCoinGameObject, destCoinGameObject, out var ropeMesh))
                        {
                            if (srcCoinPileComponent != null && destCoinPileComponent != null)
                            {
                                // 隐藏 RopePin
                                srcCoinPileComponent.SetCoinPileElementRopePinStatus(
                                    coinPileRopeItem.srcCoinElementIndex, false);
                                destCoinPileComponent.SetCoinPileElementRopePinStatus(
                                    coinPileRopeItem.srcCoinElementIndex, false);
                            }
                            
                            coinPileRopeAnim.TearOffRope(ropeMesh, () =>
                            {
                                coinPileRopeMesh.DestroyRopeMesh(srcCoinGameObject, destCoinGameObject);
                                if (destCoinPileComponent != null)
                                {
                                    destCoinPileComponent.OnPointerClick(null);
                                }
                            });
                        }
                    }
                }
            }

            return false;
        }
    }
}
