using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Manager;
using Prefab;
using SOData;

namespace CoinPileScript
{
    public class CoinPileMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinPrefab;
        
        private readonly float coinOffsetZ = 0.18f;
        
        private List<GameObject> coinGameObjectList;
        private Dictionary<int, GameObject> coinGameObjectMap;
        private Dictionary<GameObject, int> coinGameObjectReverseMap;
        private List<GameObject> coinGroupGameObjectList;
        private GameObject transCoinGroupGameObject; // 在 GetTopCoins 中获取的 coin 对象，会从 coinGameObjectList 中删除，暂时会挂载在该对象下面作为子对象
        // private GameObject coinStackGameObject; // 为了和钱堆上的各个功能区分开，只包含钱堆的部分
        
        public void Init(in List<CoinPileItem> coinsSettings, Boolean hasTunnel, int tunnelStartIndex)
        {
            var coinsSum = 0;
            foreach (var setting in coinsSettings)
            {
                coinsSum += setting.number;
            }
            
            coinGameObjectList = new List<GameObject>(coinsSum);
            coinGameObjectMap = new Dictionary<int, GameObject>(coinsSum);
            coinGameObjectReverseMap = new Dictionary<GameObject, int>(coinsSum);
            coinGroupGameObjectList = new List<GameObject>(coinsSettings.Count);
            transCoinGroupGameObject = new GameObject("Trans Coin Group")
            {
                transform =
                {
                    parent = this.transform,
                    localPosition = new Vector3(0, 0, 0)
                }
            };
            
            var n = 0;
            for (var i = 0; i < coinsSettings.Count; i++)
            {
                var coin = coinsSettings[i];
                var coinMaterial = MaterialManager.Instance.GetMaterialByColor(coin.color);
                if (coinMaterial != null)
                {
                    // 创建 coinGroup 游戏对象，把相同颜色的 coin 都放到同一个 coinGroup 下;
                    var group = new GameObject("Coin Group" + i)
                    {
                        transform =
                        {
                            parent = this.transform,
                            localPosition = new Vector3(0, 0, 0)
                        }
                    };
                    coinGroupGameObjectList.Add(group);
                    
                    // 在创建 coin 对象时，需要判断目前的 coin pile 是否有 tunnel
                    // 如果有 tunnel，需要判定目前的序号是否大于等于 tunnelStartIndex
                    // 如果大于等于，则需要将后续的钱币隐藏
                    bool isHidden = hasTunnel && i >= tunnelStartIndex;
                    
                    // 创建 coin 对象
                    for(var j = 0; j < coin.number; j++)
                    {
                        var mesh = Instantiate(coinPrefab, this.transform);
                        mesh.transform.localPosition = new Vector3(0, 0, coinOffsetZ * n);
                        // 转移 coin 的 parent
                        mesh.transform.parent = group.transform;
                        CoinPrefab coinPrefabComponent = mesh.GetComponent<CoinPrefab>();
                        if (coinPrefabComponent != null)
                        {
                            coinPrefabComponent.ChangeColor(coinMaterial);
                            // 判断是否需要隐藏 mesh
                            if (isHidden)
                            {
                                coinPrefabComponent.Hide();
                            }
                        }
                        coinGameObjectList.Add(mesh);
                        coinGameObjectMap.Add(n, mesh);
                        coinGameObjectReverseMap.Add(mesh, n);
                        n++;
                    }
                }
            }
        }

        public Stack<GameObject> GetTopCoins(int number)
        {
            var result = new Stack<GameObject>(number);
            
            for (var i = 0; i < number; i++)
            {
                // 转移 coin 的 parent
                var coin = coinGameObjectList[number - 1 - i];
                coin.transform.parent = transCoinGroupGameObject.transform;
                
                // 移除记录列表
                result.Push(coin);
                coinGameObjectReverseMap.Remove(coin, out var index);
                coinGameObjectMap.Remove(index);
                coinGameObjectList.RemoveAt(number - 1 - i);
            }
    
            return result;
        }

        public Boolean GetTopCoinGroup(out GameObject topCoinGroup)
        {
            foreach (var coinGroup in coinGroupGameObjectList)
            {
                if(coinGroup.transform.childCount > 0)
                {
                    topCoinGroup = coinGroup;
                    return true;
                }
            }

            topCoinGroup = null;
            return false;
        }

        public Boolean GetCoinGameObject(int coinPileElementIndex, out GameObject coinGameObject)
        {
            if (coinGameObjectMap.TryGetValue(coinPileElementIndex, out var coin))
            {
                coinGameObject = coin;
                return true;
            }

            coinGameObject = null;
            return false;
        }

        public Boolean GetCoinGroupsAboveIndex(int coinGroupIndex, out List<GameObject> coinGroupList)
        {
            coinGroupList = new List<GameObject>();
            for (var i = 0; i < coinGroupGameObjectList.Count; i++)
            {
                if (i <= coinGroupIndex && coinGroupGameObjectList[i].transform.childCount > 0)
                {
                    coinGroupList.Add(coinGroupGameObjectList[i]);
                }
            }

            return coinGroupList.Count > 0;
        }
    }
}
