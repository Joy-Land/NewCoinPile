using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Manager;
using SOData;

namespace CoinPileScript
{
    public partial class CoinPile : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CoinPileCollection coinPileCollection;
        
        private Boolean pointLock = false;
        private Stack<CoinPileItem> coinList;
        private CoinPileMesh coinPileMesh;
        private CoinPileTransAnim coinPileTransAnim;
        private CoinPileHiddenTubeMesh coinPileHiddenTubeMesh;
        private CoinPileHiddenTubeAnim coinPileHiddenTubeAnim;
        private CoinPileIceMesh coinPileIceMesh;
        private CoinPileIceAnim coinPileIceAnim;

        public void Init(List<CoinItem> coinListData)
        {
            var coinsSettings = new List<CoinPileItem>();
            for (var i = 0; i < coinListData.Count; i++)
            {
                var coinData = coinListData[i];
                coinsSettings.Add(new CoinPileItem()
                {
                    id = i,
                    color = coinData.color,
                    number = coinData.number,
                    isHidden = coinData.isHidden,
                    frozenNumber = coinData.isFrozen ? 3 : 0,
                });
            }
            
            // 初始化 Mesh 和 Anim
            coinPileMesh = GetComponent<CoinPileMesh>();
            if (coinPileMesh == null)
            {
                throw new Exception("coinPileMesh component missing");
            }
            coinPileMesh.Init(coinsSettings);

            coinPileTransAnim = GetComponent<CoinPileTransAnim>();
            if (coinPileTransAnim == null)
            {
                throw new Exception("coinPileTransAnim component missing");
            }
            
            // 初始化 Hidden Tube 的 Mesh 和 Anim
            coinPileHiddenTubeMesh = GetComponent<CoinPileHiddenTubeMesh>();
            if (coinPileHiddenTubeMesh == null)
            {
                throw new Exception("CoinPileHiddenTubeMesh component missing");
            }
            coinPileHiddenTubeMesh.Init(coinsSettings);

            coinPileHiddenTubeAnim = GetComponent<CoinPileHiddenTubeAnim>();
            if (coinPileHiddenTubeAnim == null)
            {
                throw new Exception("CoinPileHiddenTubeAnim component missing");
            }
            
            // 初始化 ICE 的 Mesh 和 Anim
            coinPileIceMesh = GetComponent<CoinPileIceMesh>();
            if (coinPileIceMesh == null)
            {
                throw new Exception("coinPileIceMesh component missing");
            }
            coinPileIceMesh.Init(coinsSettings);

            coinPileIceAnim = GetComponent<CoinPileIceAnim>();
            if (coinPileIceAnim == null)
            {
                throw new Exception("coinPileIceAnim component missing");
            }
            
            // 初始化状态数组
            var tmpCoinsSettings = coinsSettings.ToList();
            tmpCoinsSettings.Reverse();
            coinList = new Stack<CoinPileItem>(tmpCoinsSettings);
        }

        public void SetCoinPileCollection(CoinPileCollection instance)
        {
            this.coinPileCollection = instance;
        }
        
        public Boolean CheckCoinTopHasSameColor(CoinColor color)
        {
            if (coinList.Count > 0)
            {
                var stackTop = coinList.Peek();
                return stackTop.color == color;
            }
            
            return false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Debug.Log("Clicked");
            // 如果钱堆为空，或者被锁住了，不做任何操作
            if (coinList.Count <= 0 || pointLock || GameManager.Instance.IsGameStopped()) return;
            pointLock = true;
            
            // 获取栈顶状态
            var stackTop = coinList.Peek();
            
            // 判定是否是冰块
            if (stackTop.frozenNumber > 0)
            {
                // 目前栈顶是冰块，不能继续点击
                if (coinPileIceMesh.GetCoinIceMesh(stackTop.id, out var coinIceMesh) && coinPileMesh.GetTopCoinGroup(out var topCoinGroup))
                {
                    coinPileIceAnim.ShakeIce(coinIceMesh, topCoinGroup, () =>
                    {
                        pointLock = false;
                    });
                }
                return;
            }
            
            List<int> coinTransNumberList = new List<int>(); // 转移的硬币数量
            List<Stack<GameObject>> coinGoList = new List<Stack<GameObject>>(); // 转移的硬币
            List<GameObject> destGoList = new List<GameObject>(); // 硬币转移的区域
            List<int> destStartIndexList = new List<int>(); // 硬币转移的位置索引
            List<Action> onCompleteList = new List<Action>(); // 完成函数列表

            // 通过 GameManager 判断目前运钱区的颜色和数量
            GameManager.Instance.GetCurrentTransPile(out CoinColor transPileColor, out int transPileFreeNumber,
                out GameObject transGameObject);

            // 更改状态，收集动画信息
            if (stackTop.color == transPileColor && transPileFreeNumber > 0)
            {
                // 2. 如果目前运钱区的颜色和钱堆顶部颜色相同
                destGoList.Add(transGameObject);
                destStartIndexList.Add(GameManager.TransPileCapacity - transPileFreeNumber);
                if (transPileFreeNumber >= stackTop.number)
                {
                    // 2-1. 判断运钱区容量。如果容量大于钱堆顶部数量，则将钱堆全部放入运钱区
                    int transNumber = stackTop.number;
                    Action unlockTransPile = GameManager.Instance.AddCurrenTransPileNumber(transNumber);
                    coinTransNumberList.Add(transNumber);
                    coinGoList.Add(coinPileMesh.GetTopCoins(transNumber));
                    onCompleteList.Add(() =>
                    {
                        coinList.Pop();
                        if (unlockTransPile != null)
                        {
                            unlockTransPile();
                        }
                        
                        GameManager.Instance.CheckCurrentTransPileIsFull();
                    });
                }
                else
                {
                    // 2-2. 如果容量小于钱堆顶部数量，则只将容量数量的钱放入运钱区
                    Action unlockTransPile = GameManager.Instance.AddCurrenTransPileNumber(transPileFreeNumber);
                    coinTransNumberList.Add(transPileFreeNumber);
                    coinGoList.Add(coinPileMesh.GetTopCoins(transPileFreeNumber));
                    onCompleteList.Add(() =>
                    {
                        coinList.Pop();
                        coinList.Push(new CoinPileItem(){ id = stackTop.id, color = stackTop.color, number = stackTop.number - transPileFreeNumber });
                        if (unlockTransPile != null)
                        {
                            unlockTransPile();
                        }
                        
                        GameManager.Instance.CheckCurrentTransPileIsFull();
                    });
                }
            }
            else
            {
                // 3. 如果目前运钱区的颜色和钱堆顶部颜色不同，则查找缓冲区，直到找到第一个与钱堆顶部颜色相同的缓冲区，或者是空的缓冲区
                while (true)
                {
                    if (GameManager.Instance.FindColorOrEmptyCachePile(stackTop.color, out int cachePileIndex,
                            out int cachePileFreeNumber, out GameObject cacheGameObject))
                    {
                        // 3-1. 如果找到了这样的缓冲区，查看当前空余的容量是否大于钱堆顶部数量
                        destGoList.Add(cacheGameObject);
                        destStartIndexList.Add(GameManager.CachePileCapacity - cachePileFreeNumber);
                        if (cachePileFreeNumber >= stackTop.number)
                        {
                            // 3-1-1. 如果缓冲空余容量大于钱堆顶部数量，则把所有钱堆放入缓冲区
                            int transNumber = stackTop.number;
                            CoinColor transColor = stackTop.color;
                            Action unLockCachePile =
                                GameManager.Instance.AddCachePileNumber(cachePileIndex, transColor,
                                    transNumber);
                            coinTransNumberList.Add(transNumber);
                            coinGoList.Add(coinPileMesh.GetTopCoins(transNumber));
                            onCompleteList.Add(() =>
                            {
                                coinList.Pop();
                                if (unLockCachePile != null)
                                {
                                    unLockCachePile();
                                }
                                
                                // 判断是否可以从当前缓冲区向当前运钱区转移
                                GameManager.Instance.GetCurrentTransPile(out CoinColor transPileColorTmp, out int transPileFreeNumberTmp, out GameObject _);
                                if (transColor == transPileColorTmp && transPileFreeNumberTmp > 0)
                                {
                                    // 条件有二：颜色相同，并且运钱区有空余容量
                                    GameManager.Instance.MoveCachePileToTransPile(cachePileIndex, () =>
                                    {
                                        // 移动缓冲区到运钱区动画播放完成后，需要检测运钱区是否满了
                                        GameManager.Instance.CheckCurrentTransPileIsFull();
                                    });
                                }
                                
                                // 进行死亡判定前，需要先判断缓冲区是否满了，如果都没满，就不可能有死亡判定
                                if (GameManager.Instance.CheckCachePileListIsFull())
                                {
                                    // 重新获取一下当前运钱区的数量
                                    GameManager.Instance.GetCurrentTransPile(out CoinColor _, out int transPileFreeNumberLocal, out GameObject _);
                                    if (transPileFreeNumberLocal == 0)
                                    {
                                        // 如果当前运钱区满了，那就判断下一个运钱区的颜色，然后查找是否能找到和其颜色相同的缓冲区
                                        GameManager.Instance.GetNextTransPile(out CoinColor nextTransColor, out int _,
                                            out GameObject _);
                                        if (!GameManager.Instance.FindSameColorCachePile(nextTransColor, out int _))
                                        {
                                            // 如果所有缓冲区中，根本没有和下一个运钱区颜色相同的缓冲区，则游戏失败
                                            GameManager.Instance.GameIsFailed();
                                        }
                                    }
                                    else
                                    {
                                        // 如果当前运钱区没满，则直接判定死亡
                                        GameManager.Instance.GameIsFailed();
                                    }
                                };
                            });
                            break;
                        }

                        // 3-1-2. 如果缓冲空余容量小于钱堆顶部数量，则只将缓冲区容量数量的钱放入缓冲区，剩下的钱按照上述步骤，再循环检测一遍
                        stackTop = new CoinPileItem(){ id = stackTop.id, color = stackTop.color, number = stackTop.number - cachePileFreeNumber };
                        // 更新此时的栈顶状态
                        coinList.Pop();
                        coinList.Push(stackTop);
                        Action unLockCachePileParent = GameManager.Instance.AddCachePileNumber(cachePileIndex, stackTop.color,
                            cachePileFreeNumber);
                        coinTransNumberList.Add(cachePileFreeNumber);
                        coinGoList.Add(coinPileMesh.GetTopCoins(cachePileFreeNumber));
                        onCompleteList.Add(() =>
                        {
                            if (unLockCachePileParent != null)
                            {
                                unLockCachePileParent();
                            }
                            
                            if (GameManager.Instance.CheckCachePileListIsFull())
                            {
                                GameManager.Instance.GameIsFailed();
                            };
                        });
                    }
                    else
                    {
                        // 3-2. 如果找不到这样的缓冲区，就结束游戏
                        break;
                    }
                }
            }

            // 播放动画
            if (coinTransNumberList.Count != 0 && destGoList.Count != 0 && destStartIndexList.Count != 0 && onCompleteList.Count != 0)
            {
                coinPileTransAnim.TransferCoins(coinTransNumberList, coinGoList, destGoList, destStartIndexList, onCompleteList, () =>
                {
                    // Debug.Log("Complete");
                    pointLock = false;
                    // GameManager.Instance.CheckCurrentTransPileIsFull();
                }, () =>
                {
                    // 动画播放开始前，调用所有钱堆的冰冻消融效果
                    if (coinPileCollection != null)
                    {
                        coinPileCollection.CheckCoinPileCanMeltIce();
                    }
                }, () =>
                {
                    if (coinList.Count > 1)
                    {
                        // 动画播放结束，检查目前栈顶项目是否是隐藏的，如果是隐藏的，则播放隐藏动画
                        var stockTopItem = coinList.Skip(1).First();
                        if (stockTopItem.isHidden)
                        {
                            // 获取对应的 coinHiddenTubeMesh
                            if (coinPileHiddenTubeMesh.GetCoinHiddenTubeMesh(stockTopItem.id, out var coinHiddenTubeMesh))
                            {
                                // 播放隐藏动画
                                coinPileHiddenTubeAnim.HideCoinTube(coinHiddenTubeMesh, stockTopItem.number, () =>
                                {
                                    // 销毁对应的 coinHiddenTubeMesh
                                    coinPileHiddenTubeMesh.DestroyCoinHiddenTubeMesh(stockTopItem.id);
                                });
                            }
                        }
                    }
                });
            }
            else
            {
                pointLock = false;
                GameManager.Instance.CheckCurrentTransPileIsFull();
            }
        }

        public void TryMeltIce()
        {
            if (coinList.Count > 0)
            {
                // 首先检查目前栈顶是不是冰冻
                var stackTop = coinList.Peek();

                if (stackTop.frozenNumber > 0)
                {
                    int newFrozenNumber = stackTop.frozenNumber - 1;
                
                    // 如果是冰冻状态，减少冰冻数量，并播放相应动画
                    coinList.Pop();
                    coinList.Push(new CoinPileItem()
                    {
                        id = stackTop.id,
                        color = stackTop.color,
                        number = stackTop.number,
                        isHidden = stackTop.isHidden,
                        frozenNumber = newFrozenNumber,
                    });
                
                    if (coinPileIceMesh.GetCoinIceMesh(stackTop.id, out var coinIceMesh))
                    {
                        coinPileIceAnim.MeltIce(coinIceMesh, newFrozenNumber, () =>
                        {
                            // 如果此时 newFrozenNumber 为 0，需要销毁此时的 GameObject
                            if (newFrozenNumber <= 0)
                            {
                                coinPileIceMesh.DestroyCoinIceMesh(stackTop.id);
                            }
                        });
                    }
                }
            }
        }
    }
}
