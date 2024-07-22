using System;
using System.Collections;
using System.Collections.Generic;
using CoinPileScript;
using TransPileScript;
using CachePileScript;
using UnityEngine;
using Framework;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;

namespace Manager
{
    public class GameManager : SingletonBaseMono<GameManager>
    {
        // Game Manager 的作用
        // 1. 记录当前运钱区的颜色和容量，以及下一个运钱区的颜色和容量
        // 2. 记录每个缓冲区的颜色和容量
        // 3. 每次往运钱区放钱之后，检测当前运钱区是否已经满了，如果满了，就把下一个运钱区替换为当前的运钱区
        // 4. 每次运钱区刷新之后，判断当前的缓冲区中是否有和目前的运钱区相同颜色的，如果有，就将缓冲区中的钱移动到运钱区查找缓冲区，直到找到第一个与钱堆顶部颜色相同的缓冲区，或者是空的缓冲区
        
        // 音效
        [SerializeField] private AudioClip levelPassClip;
        [SerializeField] private float levelPassClipVolume;
        
        [SerializeField] private AudioClip failClip;
        [SerializeField] private float failClipVolume;
        
        // 配置
        public const int TransPileCapacity = 15;
        public const int CachePileCapacity = 5;
        
        // 子组件
        [SerializeField] private GameObject transPilePrefab;
        [SerializeField] private GameObject cachePilePrefab;
        [SerializeField] private GameObject coinPileCollectionPrefab;

        private TransPile transPile;
        private CachePile cachePile;
        private CoinPileCollection coinPileCollection;
        
        // 全局参数
        private Boolean isStopped = false;
        private int levelIndex = 0;
        private Boolean checkCurrentTransPileIsFullLock = false;

        // Start is called before the first frame update
        void Start()
        {
            DOTween.Init();
            Init();
        }

        public void ReStart(Boolean nextLevel)
        {
            var levelListCount = LevelManager.Instance.GetLevelListCount();
            if (nextLevel)
            {
                levelIndex += 1;
            }
            levelIndex = levelIndex % levelListCount;
            Init();
        }

        private void Init()
        {
            // 开始游戏的开关
            isStopped = false;
            
            // 
            checkCurrentTransPileIsFullLock = false;
            
            // 确保 UI 关闭
            UIManager.Instance.HideSuccess();
            UIManager.Instance.HideFail();
            
            if (LevelManager.Instance.GetLevelDataByIndex(levelIndex, out var levelData))
            {
                transPile = transPilePrefab.GetComponent<TransPile>();
                if (transPile == null)
                {
                    throw new Exception("TransPile component missing");
                }

                transPile.Init(levelData.transCoinStackList);
            
                cachePile = cachePilePrefab.GetComponent<CachePile>();
                if (cachePile == null)
                {
                    throw new Exception("CachePile component missing");
                }
                cachePile.Init();
            
                coinPileCollection = coinPileCollectionPrefab.GetComponent<CoinPileCollection>();
                if (coinPileCollection == null)
                {
                    throw new Exception("CoinPileCollection component missing");
                }
                coinPileCollection.Init(levelData.coinLists);
            }
        }
        
        public void GetCurrentTransPile(out CoinColor color, out int freeNumber, out GameObject transGameObject)
        {
            transPile.GetCurrentTransPile(out color, out freeNumber, out transGameObject);
        }
        
        public void GetNextTransPile(out CoinColor color, out int freeNumber, out GameObject transGameObject)
        {
            transPile.GetNextTransPile(out color, out freeNumber, out transGameObject);
        }

        public Action AddCurrenTransPileNumber(int number)
        {
            return transPile.AddCurrenTransPileNumber(number);
        }

        public Boolean FindColorOrEmptyCachePile(CoinColor color, out int cacheIndex, out int freeNumber, out GameObject cacheGameObject)
        {
            return cachePile.FindColorOrEmptyCachePile(color, out cacheIndex, out freeNumber, out cacheGameObject);
        }
        
        public Action AddCachePileNumber(int cacheIndex, CoinColor color, int number)
        {
            return cachePile.AddCachePileNumber(cacheIndex, color, number);
        }

        public Boolean IsGameStopped()
        {
            return isStopped;
        }

        public void GameIsFailed()
        {
            isStopped = true;
            UIManager.Instance.ShowFail();
            SoundFXManager.Instance.PlaySoundFXClip(failClip, Vector3.zero, failClipVolume);
        }

        private void GameIsSuccessful()
        {
            isStopped = true;
            UIManager.Instance.ShowSuccess();
            SoundFXManager.Instance.PlaySoundFXClip(levelPassClip, Vector3.zero, levelPassClipVolume);
        }
        
        public Boolean CheckCoinPilesHasColor(CoinColor color, out List<int> coinIndexList)
        {
            return coinPileCollection.CheckCoinPilesHasColor(color, out coinIndexList);
        }

        public Boolean CheckCachePileListIsFull()
        {
            return cachePile.CheckCachePileListIsFull();
        }
        
        // Check if the current pile is full
        public void CheckCurrentTransPileIsFull()
        {
            // 锁定 CheckCurrentTransPileIsFull 函数，直到动画结束后再解锁
            if (checkCurrentTransPileIsFullLock || isStopped) return;
            checkCurrentTransPileIsFullLock = true;
            
            if (transPile.CheckCurrentTransPileFull())
            {
                // 如果当前运钱区满了，且没有其他动画占用运钱区，才播放移动动画
                if (!transPile.CheckCurrentTransPileIsPushing())
                {
                    transPile.FetchNextTransPile(() =>
                    {
                        // 移动动画播放完成后，检测是否需要移动缓冲区到运钱区
                        CheckMoveCachePileToTransPile(() =>
                        {
                            // 移动缓冲区到运钱区动画播放完成后，解锁函数
                            checkCurrentTransPileIsFullLock = false;
                    
                            if (transPile.CheckTransPileListFull())
                            {
                                // Game is successful
                                GameIsSuccessful();
                                return;
                            }
                            
                            // 移动缓冲区到运钱区动画播放完成后，需要检测运钱区是否满了
                            CheckCurrentTransPileIsFull();
                        });
                    });
                }
                else
                {
                    checkCurrentTransPileIsFullLock = false;
                }
            }
            else
            {
                // 如果当前运钱区没满，直接解锁函数
                CheckMoveCachePileToTransPile(() =>
                {
                    checkCurrentTransPileIsFullLock = false;
                    
                    if (transPile.CheckCurrentTransPileFull())
                    {
                        // 移动缓冲区到运钱区动画播放完成后，需要检测运钱区是否满了
                        CheckCurrentTransPileIsFull();
                    }
                });
                // checkCurrentTransPileIsFullLock = false;
            }
        }
        
        public void MoveCachePileToTransPile(int cachePileIndex, Action onComplete)
        {
            cachePile.MoveCachePileToTransPile(cachePileIndex, onComplete);
        }
        
        public Boolean FindSameColorCachePile(CoinColor transPileColor, out int cachePileIndex)
        {
            return cachePile.FindSameColorCachePile(transPileColor, out cachePileIndex);
        }

        private Boolean CheckIfCanMoveCachePileToTransPile(out int cachePileIndex)
        {
            GetCurrentTransPile(out var currentTransColor, out var currentTransFreeNumber, out var currentTransGameObject);
            if (currentTransFreeNumber > 0 && FindSameColorCachePile(currentTransColor, out cachePileIndex))
            {
                return true;
            }

            cachePileIndex = -1;
            return false;
        }
        
        private void CheckMoveCachePileToTransPile(Action onComplete)
        {
            if (!isStopped)
            {
                if(CheckIfCanMoveCachePileToTransPile(out var cachePileIndex))
                {
                    // onComplete 中解除对于 CheckCurrentTransPileIsFull 函数的锁定
                    MoveCachePileToTransPile(cachePileIndex, () =>
                    {
                        CheckMoveCachePileToTransPile(onComplete);
                    });
                }
                else
                {
                    onComplete();
                }
            }
        }
    }
}
