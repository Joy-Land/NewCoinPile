using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SOData;

namespace CoinPileScript
{
    public class CoinPilePanelMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinPilePanel;
        
        private Dictionary<GameObject, Dictionary<int, GameObject>> coinPilePanelMap;

        public void Init(List<GameObject> coinGameObjectList, List<CoinPilePanelData> coinPilePanelItemList)
        {
            
        }
    }
}
