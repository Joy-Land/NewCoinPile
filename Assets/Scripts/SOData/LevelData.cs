using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SOData
{
    [CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Level")]
    public class LevelData : ScriptableObject
    {
        public List<CoinList> coinLists = new List<CoinList>();
        public TransCoinStackList transCoinStackList;
        public CoinPileRopeList coinPileRopeList;
    }
}
