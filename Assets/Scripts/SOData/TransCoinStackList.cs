using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager;
using Sirenix.OdinInspector;

namespace SOData
{
    [System.Serializable]
    public class TransCoinStackList
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<CoinColor> data = new List<CoinColor>();
    }
}
