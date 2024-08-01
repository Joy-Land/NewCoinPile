using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager;
using Sirenix.OdinInspector;

namespace SOData
{
    [System.Serializable]
    public struct CoinItem
    {
        public CoinColor color;
        public int number;
        public Boolean isHidden;
        public Boolean isFrozen;
        public Boolean isShutter;
        public Boolean shutterIsOpen;
    }
    
    [System.Serializable]
    public class CoinList 
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<CoinItem> data = new List<CoinItem>();
        public Boolean hasTunnel;
        public int tunnelStartIndex;
        public Vector2 position;
    }
}
