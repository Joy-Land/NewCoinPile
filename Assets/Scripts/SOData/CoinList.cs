using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager;


namespace SOData
{
    [System.Serializable]
    public struct CoinItem
    {
        public CoinColor color;
        public int number;
        public Boolean isHidden;
        public Boolean isFrozen;
    }
    
    [System.Serializable]
    public class CoinList 
    {
        public List<CoinItem> data = new List<CoinItem>();
        public Vector2 position;
    }
}
