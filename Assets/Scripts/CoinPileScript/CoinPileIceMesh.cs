using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoinPileScript
{
    public class CoinPileIceMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinIce;
        
        private readonly float startZ = -0.09f;
        private readonly float coinOffsetZ = 0.18f;
        private readonly float coinScaleZ = 0.18f;

        private Dictionary<int, GameObject> coinIceMap;
        
        public void Init(in List<CoinPileItem> coinsSettings)
        {
            coinIceMap = new Dictionary<int, GameObject>();

            var sum = 0;
            foreach (var setting in coinsSettings)
            {
                sum += setting.number;
                
                if (setting.frozenNumber > 0)
                {
                    // 生成 CoinHiddenTube 的 Mesh
                    var mesh = Instantiate(coinIce, this.transform);
                    mesh.transform.localPosition = new Vector3(0, 0, startZ + coinOffsetZ * sum);
                    mesh.transform.localScale = new Vector3(1.0f, 1.0f, coinScaleZ * setting.number);
                    
                    // 记录到 Map 中
                    coinIceMap.Add(setting.id, mesh);
                }
            }
        }
        
        public Boolean GetCoinIceMesh(int id, out GameObject mesh)
        {
            return coinIceMap.TryGetValue(id, out mesh);
        }

        public void DestroyCoinIceMesh(int id)
        {
            if (coinIceMap.ContainsKey(id))
            {
                if (coinIceMap.Remove(id, out var mesh))
                {
                    Destroy(mesh);
                }
            }
        }
    }
}
