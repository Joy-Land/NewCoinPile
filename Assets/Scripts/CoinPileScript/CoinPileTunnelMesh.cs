using System;
using System.Collections;
using System.Collections.Generic;
using Item;
using UnityEngine;

namespace CoinPileScript
{
    public class CoinPileTunnelMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinTunnel;
        
        private readonly float startZ = -0.09f;
        private readonly float coinOffsetZ = 0.18f;

        private GameObject coinTunnelGameObject;
        
        public void Init(in List<CoinPileItem> coinsSettings, Boolean hasTunnel, int tunnelStartIndex)
        {
            if (hasTunnel)
            {
                var sum = 0;
                for (var i = 0; i < coinsSettings.Count; i++)
                {
                    if (i >= tunnelStartIndex)
                    {
                        break;
                    }

                    var setting = coinsSettings[i];
                    sum += setting.number;
                }
                
                // 生成 CoinTunnel 的 Mesh
                coinTunnelGameObject = Instantiate(coinTunnel, this.transform);
                coinTunnelGameObject.transform.localPosition = new Vector3(0, 0, startZ + coinOffsetZ * sum);
                
                // 为了不让 tunnel 随着钱堆移动，需要重新设置其 parent
                coinTunnelGameObject.transform.parent = null;
                
                // 设置提示板上显示的数量
                ChangeCoinTunnelNumber(coinsSettings.Count - tunnelStartIndex);
            }
        }
        
        public Boolean GetCoinTunnelMesh(out GameObject mesh)
        {
            if (coinTunnelGameObject != null)
            {
                mesh = coinTunnelGameObject;
                return true;
            }
            else
            {
                mesh = null;
                return false;
            }
        }

        public void DestroyCoinTunnelMesh()
        {
            if (coinTunnelGameObject != null)
            {
                Destroy(coinTunnelGameObject);
                coinTunnelGameObject = null;
            }
        }

        public void ChangeCoinTunnelNumber(int number)
        {
            var coinTunnelComponent = coinTunnelGameObject.GetComponent<CoinTunnel>();
            if (coinTunnelComponent != null)
            {
                coinTunnelComponent.ShowNumber(number);
            }
        }
    }
}
