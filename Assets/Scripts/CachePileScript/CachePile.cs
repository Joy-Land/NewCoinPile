using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Manager;
using UnityEngine;

namespace CachePileScript
{
    public class CachePile : MonoBehaviour
    {
        // 状态
        private CacheCoinStackItem[] cachePileList;

        // 子组件
        private CachePileMesh cachePileMesh;
        private CachePileAnim cachePileAnim;

        public void Init()
        {
            // 初始化组件
            cachePileAnim = GetComponent<CachePileAnim>();
            if (cachePileAnim == null)
            {
                throw new Exception("cachePileAnim component missing");
            }

            cachePileMesh = GetComponent<CachePileMesh>();
            if (cachePileMesh == null)
            {
                throw new Exception("CachePileMesh component missing");
            }
            cachePileMesh.Init();
            cachePileMesh.GetCacheGameObjectList(out var cacheGameObjectList);

            // 通过存钱区预制体，初始化
            cachePileList = new CacheCoinStackItem[cacheGameObjectList.Length];
            for (var i = 0; i < cacheGameObjectList.Length; i++)
            {
                cachePileList[i] = new CacheCoinStackItem()
                {
                    Color = CoinColor.Empty,
                    Number = 0,
                    IsPushing = false,
                    IsPopping = false,
                };
            }
        }

        public Action AddCachePileNumber(int cacheIndex, CoinColor color, int number)
        {
            cachePileList[cacheIndex] = new CacheCoinStackItem()
            {
                Color = color,
                Number = cachePileList[cacheIndex].Number + number,
                IsPushing = true,
                IsPopping = false,
            };
            return () => { cachePileList[cacheIndex].IsPushing = false; };
        }

        public Boolean FindColorOrEmptyCachePile(CoinColor color, out int cacheIndex, out int freeNumber,
            out GameObject cacheGameObject)
        {
            for (int i = 0; i < cachePileList.Length; i++)
            {
                cacheIndex = i;
                freeNumber = GameManager.CachePileCapacity - cachePileList[i].Number;
                cacheGameObject = cachePileMesh.GetCacheGameObjectByIndex(i);
                if ((cachePileList[i].Color == color || cachePileList[i].Color == CoinColor.Empty) && freeNumber > 0 &&
                    cachePileList[i].IsPushing == false && cachePileList[i].IsPopping == false)
                {
                    return true;
                }
            }

            cacheIndex = -1;
            freeNumber = GameManager.CachePileCapacity;
            cacheGameObject = null;
            return false;
        }

        // Check if cache pile has the same color with current trans pile
        public Boolean CheckCachePileListIsFull()
        {
            foreach (var cachePile in cachePileList)
            {
                // 如果有空的或者正在进行动画的，就认为目前未满
                if (cachePile.Color == CoinColor.Empty  || cachePile.IsPushing || cachePile.IsPopping)
                {
                    return false;
                }
            }

            var checkFlag = true; 
            foreach (var cachePile in cachePileList)
            {
                // 还有一种情况需要特殊处理，那就是虽然目前的缓冲区数量未满，但是钱堆里面已经没有同样的颜色了
                if (cachePile.Number < GameManager.CachePileCapacity)
                {
                    // 检查当前的所有的钱堆堆顶的颜色，是否和当前缓冲区颜色相同
                    if (GameManager.Instance.CheckCoinPilesHasColor(cachePile.Color, out var coinIndexList))
                    {
                        // 只要有一个相同的，直接返回 false
                        checkFlag = false;
                        break;
                    }
                }
            }

            return checkFlag;
        }

        public Boolean FindSameColorCachePile(CoinColor transPileColor, out int cachePileIndex)
        {
            for (int i = 0; i < cachePileList.Length; i++)
            {
                if (cachePileList[i].Color == transPileColor && cachePileList[i].Number > 0 &&
                    cachePileList[i].IsPushing == false && cachePileList[i].IsPopping == false)
                {
                    cachePileIndex = i;
                    return true;
                }
            }

            cachePileIndex = -1;
            return false;
        }

        public void MoveCachePileToTransPile(int cachePileIndex, Action onComplete)
        {
            // 1. 获取当前缓冲区的颜色和数量。以及对应的预制体
            var cachePile = cachePileList[cachePileIndex];
            GameManager.Instance.GetCurrentTransPile(out var transPleColor, out var transPileFreeNumber,
                out var transGameObject);
            // 2. 检测缓冲区的数量和运钱区空余数量的大小
            var destStartIndex = GameManager.TransPileCapacity - transPileFreeNumber;
            if (transPileFreeNumber >= cachePile.Number)
            {
                // 2-1. 如果运钱区空余数量比缓冲区大，直接将缓冲区中的所有钱币转移到运钱区
                Action unLockTansPile = GameManager.Instance.AddCurrenTransPileNumber(cachePile.Number);
                cachePileList[cachePileIndex].IsPopping = true;
                cachePileAnim.TransferCoinsFromCache(cachePile.Number,
                    cachePileMesh.GetCacheGameObjectByIndex(cachePileIndex), cachePile.Number - 1, transGameObject,
                    destStartIndex, () =>
                    {
                        cachePileList[cachePileIndex] =
                            new CacheCoinStackItem()
                            {
                                Color = CoinColor.Empty,
                                Number = 0,
                                IsPushing = false,
                                IsPopping = false,
                            };
                        if (unLockTansPile != null)
                        {
                            unLockTansPile();
                        }

                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    });
            }
            else
            {
                if (transPileFreeNumber > 0)
                {
                    // 2-2. 如果运钱区空余数量比缓冲区小，只将空余数量的钱币转移到运钱区
                    Action unLockTansPile = GameManager.Instance.AddCurrenTransPileNumber(transPileFreeNumber);
                    cachePileList[cachePileIndex].IsPopping = true;
                    cachePileAnim.TransferCoinsFromCache(transPileFreeNumber,
                        cachePileMesh.GetCacheGameObjectByIndex(cachePileIndex), cachePile.Number - 1, transGameObject,
                        destStartIndex, () =>
                        {
                            cachePileList[cachePileIndex] =
                                new CacheCoinStackItem()
                                {
                                    Color = cachePile.Color,
                                    Number = cachePile.Number - transPileFreeNumber,
                                    IsPushing = false,
                                    IsPopping = false,
                                };
                            if (unLockTansPile != null)
                            {
                                unLockTansPile();
                            }
                            
                            if (onComplete != null)
                            {
                                onComplete();
                            }
                        });
                }
                else
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                }
            }
        }
    }
}