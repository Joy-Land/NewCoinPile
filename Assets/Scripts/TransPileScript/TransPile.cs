using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using SOData;

namespace TransPileScript
{
    public class TransPile : MonoBehaviour
    {
        // 状态
        private List<TransCoinStackItem> transPileList;
        private int currentTransIndex;
        private int nextTransIndex;
        
        // 子组件
        private TransPileMesh transPileMesh;
        private TransPileAnim transPileAnim;
        
        public void Init(TransCoinStackList transCoinStackListData)
        {
            transPileList = new List<TransCoinStackItem>(transCoinStackListData.data.Count);
            foreach (var transCoinStackColor in transCoinStackListData.data)
            {
                transPileList.Add(new TransCoinStackItem(){ Color = transCoinStackColor, Number = 0, IsPushingNumber = TransCoinStackItem.IsPushingNumberConst });
            }

            currentTransIndex = 0;
            nextTransIndex = 1;
            
            transPileMesh = GetComponent<TransPileMesh>();
            if (transPileMesh == null)
            {
                throw new Exception("transPileMesh component missing");
            }
            transPileMesh.Init(currentTransIndex < transPileList.Count ? transPileList[currentTransIndex] : null, nextTransIndex < transPileList.Count ? transPileList[nextTransIndex] : null, nextTransIndex + 1 < transPileList.Count ?transPileList[nextTransIndex+1] : null);
    
            transPileAnim = GetComponent<TransPileAnim>();
            if (transPileAnim == null)
            {
                throw new Exception("transPileAnim component missing");
            }
            transPileAnim.Init();
        }
        
        public void GetCurrentTransPile(out CoinColor color, out int freeNumber, out GameObject transGameObject)
        {
            if (currentTransIndex < transPileList.Count)
            {
                color = transPileList[currentTransIndex].Color;
                freeNumber = GameManager.TransPileCapacity - transPileList[currentTransIndex].Number;
                transGameObject = transPileMesh.GetCurrentTransGameObject();
                return;
            }
            color = CoinColor.Empty;
            freeNumber = 0;
            transGameObject = null;
        }

        public void GetNextTransPile(out CoinColor color, out int freeNumber, out GameObject transGameObject)
        {
            if (nextTransIndex < transPileList.Count)
            {
                color = transPileList[nextTransIndex].Color;
                freeNumber = GameManager.TransPileCapacity - transPileList[nextTransIndex].Number;
                transGameObject = transPileMesh.GetNextTransGameObject();
                return;
            }
            color = CoinColor.Empty;
            freeNumber = 0;
            transGameObject = null;
        }
        
        public Action AddCurrenTransPileNumber(int number)
        {
            // number 为 0 时，不重复触发
            if (number == 0) return null;
            transPileList[currentTransIndex] = new TransCoinStackItem()
            {
                Color = transPileList[currentTransIndex].Color,
                Number = transPileList[currentTransIndex].Number + number,
                IsPushingNumber = transPileList[currentTransIndex].IsPushingNumber + 1,
            };
            return () =>
            {
                transPileList[currentTransIndex].IsPushingNumber -= 1;
            };
        }

        public Boolean CheckCurrentTransPileIsPushing()
        {
            return transPileList[currentTransIndex].IsPushingNumber != TransCoinStackItem.IsPushingNumberConst;
        }
        
        public Boolean CheckCurrentTransPileFull()
        {
            return transPileList[currentTransIndex].Number == GameManager.TransPileCapacity;
        }
        
        public Boolean CheckTransPileListFull()
        {
            if (currentTransIndex == transPileList.Count)
            {
                return true;
            }
    
            return false;
        }
        
        public void FetchNextTransPile(Action onComplete)
        {
            // 播放运钱区移动动画
            transPileAnim.MoveTransferPile(transPileList[currentTransIndex].Color,
                transPileMesh.GetCurrentTransGameObject(), transPileMesh.GetNextTransGameObject(), transPileMesh.GetLastTransGameObject(),
                () =>
                {
                    // 动画结束后，交换当前与下一个运钱区的索引
                    currentTransIndex = nextTransIndex;
                    nextTransIndex = nextTransIndex + 1;
                    
                    // 动画结束后
                    // 1. 生成新的运钱区 GameObject，并设置为 last
                    // 2. 交换 last、next 和 next、current 的索引
                    transPileMesh.ExChangeCurrentAndNextGameObject();
                    transPileMesh.SetLastTransGameObject(nextTransIndex + 1 < transPileList.Count
                        ? transPileList[nextTransIndex + 1]
                        : null);
                    
                    onComplete();
                });
        }
    }
}
