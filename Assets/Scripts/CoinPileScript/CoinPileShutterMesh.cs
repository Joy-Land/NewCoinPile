using System;
using System.Collections;
using System.Collections.Generic;
using Item;
using UnityEngine;

namespace CoinPileScript
{
    public class CoinPileShutterMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinShutter;
        
        private readonly float startZ = -0.11f;
        private readonly float coinOffsetZ = 0.18f;
        private readonly float coinScaleY = 0.9f;

        private Dictionary<int, GameObject> coinShutterMap;
        
        public void Init(in List<CoinPileItem> coinsSettings)
        {
            coinShutterMap = new Dictionary<int, GameObject>();

            var sum = 0;
            foreach (var setting in coinsSettings)
            {
                if (setting.isShutter)
                {
                    // 生成 CoinHiddenTube 的 Mesh
                    var mesh = Instantiate(coinShutter, this.transform);
                    mesh.transform.localPosition = new Vector3(0, 0, startZ + coinOffsetZ * sum);
                    mesh.transform.localRotation = new Quaternion(-0.707106829f, 0, 0, 0.707106829f);
                    mesh.transform.localScale = new Vector3(1.0f, coinScaleY * setting.number, 1.0f);
                    
                    // 记录到 Map 中
                    coinShutterMap.Add(setting.id, mesh);

                    if (setting.shutterIsOpen)
                    {
                        var coinShutterComponent = mesh.GetComponent<CoinShutter>();
                        if (coinShutterComponent != null)
                        {
                            coinShutterComponent.ChangeShutterStatus(true, true, null, null);
                        }
                    }
                    else
                    {
                        var coinShutterComponent = mesh.GetComponent<CoinShutter>();
                        if (coinShutterComponent != null)
                        {
                            coinShutterComponent.ChangeShutterStatus(false, true, null, null);
                        }
                    }
                }
                
                sum += setting.number;
            }
        }
        
        public Boolean GetCoinShutterMesh(int id, out GameObject mesh)
        {
            return coinShutterMap.TryGetValue(id, out mesh);
        }

        public void DestroyCoinShutterMesh(int id)
        {
            if (coinShutterMap.ContainsKey(id))
            {
                if (coinShutterMap.Remove(id, out var mesh))
                {
                    Destroy(mesh);
                }
            }
        }
    }
}
