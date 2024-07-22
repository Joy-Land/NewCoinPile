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
        private List<GameObject> coinGroupGameObjectList;
        private GameObject transCoinGroupGameObject; // 在 GetTopCoins 中获取的 coin 对象，会从 coinGameObjectList 中删除，暂时会挂载在该对象下面作为子对象
        
        public void Init(in List<CoinPileItem> coinsSettings)
        {
            var coinsSum = 0;
            foreach (var setting in coinsSettings)
            {
                coinsSum += setting.number;
            }
            
            coinGameObjectList = new List<GameObject>(coinsSum);
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
                    
                    // 创建 coin 对象
                    for(var j = 0; j < coin.number; j++)
                    {
                        var mesh = Instantiate(coinPrefab, this.transform);
                        mesh.transform.localPosition = new Vector3(0, 0, coinOffsetZ * n);
                        // 转移 coin 的 parent
                        mesh.transform.parent = group.transform;
                        mesh.GetComponent<CoinPrefab>().ChangeColor(coinMaterial);
                        coinGameObjectList.Add(mesh);
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
                coinGameObjectList.RemoveAt(number - 1 - i);
            }
            
            // 检查此时处于栈顶的 coinGroup 是否有子对象，如果没有，需要进行移除，并销毁
            var coinGroup = coinGroupGameObjectList[0];
            if (coinGroup.transform.childCount <= 0)
            {
                coinGroupGameObjectList.RemoveAt(0);
                Destroy(coinGroup);
            }
    
            return result;
        }

        public Boolean GetTopCoinGroup(out GameObject topCoinGroup)
        {
            if (coinGroupGameObjectList.Count > 0)
            {
                topCoinGroup = coinGroupGameObjectList[0];
                return true;
            }

            topCoinGroup = null;
            return false;
        }
    }
}
