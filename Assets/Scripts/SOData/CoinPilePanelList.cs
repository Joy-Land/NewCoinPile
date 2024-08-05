using System.Collections;
using System.Collections.Generic;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SOData
{
    [System.Serializable]
    public struct CoinPanelBlendShape
    {
        [MinValue(0f)]
        [MaxValue(100f)]
        public float height;
        
        [MinValue(0f)]
        [MaxValue(100f)]
        public float width;
        
        [MinValue(0f)]
        [MaxValue(100f)]
        public float cross;
        
        [MinValue(0f)]
        [MaxValue(100f)]
        public float triangle;
        
        [MinValue(0f)]
        [MaxValue(100f)]
        public float triangle2;
        
        [MinValue(0f)]
        [MaxValue(100f)]
        public float triangle3;
        
        [MinValue(0f)]
        [MaxValue(100f)]
        public float baklava;
        
        [MinValue(0f)]
        [MaxValue(100f)]
        public float heart;
    }
    
    [System.Serializable]
    public struct CoinPanelMesh
    {
        public Vector3 position;
        public CoinColor color;
    }
    
    [System.Serializable]
    public struct CoinPanelItem
    {
        public int coinPileIndex;
        public int coinElementIndex;
    }
    
    [System.Serializable]
    public struct CoinPanelItemList
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<CoinPanelItem> data;
    }
    
    [System.Serializable]
    public class CoinPilePanelData
    {
        public CoinPanelBlendShape coinPanelBlendShape;
        public CoinPanelMesh coinPanelMesh;
        public CoinPanelItemList coinPanelItemList;
    }
    
    [System.Serializable]
    public class CoinPilePanelList
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<CoinPilePanelData> data;
    }
}
