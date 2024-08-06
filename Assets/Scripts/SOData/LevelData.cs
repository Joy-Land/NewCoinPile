using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SOData
{
    [CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Level")]
    public class LevelData : ScriptableObject
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<CoinList> coinLists = new List<CoinList>();
        public TransCoinStackList transCoinStackList;
        public CoinPileRopeList coinPileRopeList;
        public CoinPilePanelList coinPilePanelList;
    }
}
