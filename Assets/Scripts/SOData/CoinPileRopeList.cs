using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SOData
{
    [System.Serializable]
    public struct CoinPileRopeItem
    {
        public int srcCoinPileIndex;
        public int srcCoinElementIndex;
        public int destCoinPileIndex;
        public int destCoinElementIndex;
    }
    
    [System.Serializable]
    public class CoinPileRopeList
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<CoinPileRopeItem> data;
    }
}
