using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoinPileScript
{
    public class CoinPileHiddenTubeMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinHiddenTube;
        
        private readonly float startZ = -0.09f;
        private readonly float coinOffsetZ = 0.18f;
        private readonly float coinScaleZ = 0.9f;

        private Dictionary<int, GameObject> coinHiddenTubeMap;
        
        public void Init(in List<CoinPileItem> coinsSettings)
        {
            coinHiddenTubeMap = new Dictionary<int, GameObject>();

            var sum = 0;
            foreach (var setting in coinsSettings)
            {
                sum += setting.number;
                
                if (setting.isHidden)
                {
                    // 生成 CoinHiddenTube 的 Mesh
                    var mesh = Instantiate(coinHiddenTube, this.transform);
                    mesh.transform.localPosition = new Vector3(0, 0, startZ + coinOffsetZ * sum);
                    mesh.transform.localScale = new Vector3(1.0f, 1.0f, coinScaleZ * setting.number);
                    
                    // 记录到 Map 中
                    coinHiddenTubeMap.Add(setting.id, mesh);
                }
            }
        }
        
        public Boolean GetCoinHiddenTubeMesh(int id, out GameObject mesh)
        {
            return coinHiddenTubeMap.TryGetValue(id, out mesh);
        }

        public void DestroyCoinHiddenTubeMesh(int id)
        {
            if (coinHiddenTubeMap.ContainsKey(id))
            {
                if (coinHiddenTubeMap.Remove(id, out var mesh))
                {
                    Destroy(mesh);
                }
            }
        }
    }
}
