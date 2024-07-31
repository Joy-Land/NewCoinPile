using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Manager;
using SOData;
using Prefab;

namespace CoinPileScript
{
    public partial class CoinPile : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CoinPileCollection coinPileCollection;

        private int coinListDataLength;
        private List<CoinPileItem> coinList;
        private Boolean hasTunnel;
        private int tunnelStartIndex;
        
        // 动画效果相关的锁
        private Boolean pointLock = false;
        private Boolean shutterLock = false;
        private Boolean tunnelLock = false;
        
        private CoinPileMesh coinPileMesh;
        private CoinPileTransAnim coinPileTransAnim;
        private CoinPileHiddenTubeMesh coinPileHiddenTubeMesh;
        private CoinPileHiddenTubeAnim coinPileHiddenTubeAnim;
        private CoinPileIceMesh coinPileIceMesh;
        private CoinPileIceAnim coinPileIceAnim;
        private CoinPileShutterMesh coinPileShutterMesh;
        private CoinPileShutterAnim coinPileShutterAnim;
        private CoinPileTunnelMesh coinPileTunnelMesh;
        private CoinPileTunnelAnim coinPileTunnelAnim;

        public void Init(List<CoinItem> coinListData, Boolean hasTunnelParams, int tunnelStartIndexParams)
        {
            // 记录 coinListData 配置的初始长度
            coinListDataLength = coinListData.Count;
            
            // 初始化状态数组
            coinList = new List<CoinPileItem>();
            for (var i = 0; i < coinListData.Count; i++)
            {
                var coinData = coinListData[i];
                coinList.Add(new CoinPileItem()
                {
                    id = i,
                    color = coinData.color,
                    number = coinData.number,
                    isHidden = coinData.isHidden,
                    frozenNumber = coinData.isFrozen ? 3 : 0,
                    isShutter = coinData.isShutter,
                    shutterIsOpen = coinData.shutterIsOpen,
                });
            }
            
            // 初始化 Tunnel 相关状态
            this.hasTunnel = hasTunnelParams;
            this.tunnelStartIndex = tunnelStartIndexParams;
            
            // 初始化 Mesh 和 Anim
            coinPileMesh = GetComponent<CoinPileMesh>();
            if (coinPileMesh == null)
            {
                throw new Exception("coinPileMesh component missing");
            }
            coinPileMesh.Init(coinList, hasTunnel, tunnelStartIndex);

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
            coinPileHiddenTubeMesh.Init(coinList);

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
            coinPileIceMesh.Init(coinList);

            coinPileIceAnim = GetComponent<CoinPileIceAnim>();
            if (coinPileIceAnim == null)
            {
                throw new Exception("coinPileIceAnim component missing");
            }
            
            // 初始化 Shutter 的 Mesh 和 Anim
            coinPileShutterMesh = GetComponent<CoinPileShutterMesh>();
            if (coinPileShutterMesh == null)
            {
                throw new Exception("coinPileShutterMesh component missing");
            }
            coinPileShutterMesh.Init(coinList);

            coinPileShutterAnim = GetComponent<CoinPileShutterAnim>();
            if (coinPileShutterAnim == null)
            {
                throw new Exception("coinPileShutterAnim component missing");
            }
            
            // 初始化 Tunnel 的 Mesh 和 Anim
            coinPileTunnelMesh = GetComponent<CoinPileTunnelMesh>();
            if (coinPileTunnelMesh == null)
            {
                throw new Exception("coinPileTunnelMesh component missing");
            }
            coinPileTunnelMesh.Init(coinList, hasTunnel, tunnelStartIndex);

            coinPileTunnelAnim = GetComponent<CoinPileTunnelAnim>();
            if (coinPileTunnelAnim == null)
            {
                throw new Exception("coinPileTunnelAnim component missing");
            }
            
            // // 初始化状态数组
            // var tmpCoinsSettings = coinsSettings.ToList();
            // tmpCoinsSettings.Reverse();
            // coinList = new Stack<CoinPileItem>(tmpCoinsSettings);
        }

        public void Destroy()
        {
            coinPileTunnelMesh.DestroyCoinTunnelMesh();
        }
        
        public void SetCoinPileCollection(CoinPileCollection instance)
        {
            this.coinPileCollection = instance;
        }

        public Transform GetCoinPileElementRopePosition(int coinPileElementIndex)
        {
            var sum = 0;
            foreach (var coin in coinList)
            {
                if (coin.id < coinPileElementIndex)
                {
                    sum += coin.number;
                }
            }
            
            if (coinPileMesh.GetCoinGameObject(sum, out var coinGameObject))
            {
                var coinPrefab = coinGameObject.GetComponent<CoinPrefab>();
                if (coinPrefab != null)
                {
                    return coinPrefab.GetRopePoint();
                }
            }

            return null;
        }
        
        public void SetCoinPileElementRopePinStatus(int coinPileElementIndex, Boolean show)
        {
            var sum = 0;
            foreach (var coin in coinList)
            {
                if (coin.id < coinPileElementIndex)
                {
                    sum += coin.number;
                }
            }
            
            if (coinPileMesh.GetCoinGameObject(sum, out var coinGameObject))
            {
                var coinPrefab = coinGameObject.GetComponent<CoinPrefab>();
                if (coinPrefab != null)
                {
                    if (show)
                    {
                        coinPrefab.ShowRopePin();
                    }
                    else
                    {
                        coinPrefab.HideRopePin();
                    }
                }
            }
        }

        public Boolean CheckCoinPileIsEmpty()
        {
            return coinList.Count == 0;
        }
        
        public Boolean CheckCoinTopHasSameColor(CoinColor color)
        {
            if (coinList.Count > 0)
            {
                var stackTop = coinList[0];
                return stackTop.color == color;
            }
            
            return false;
        }
        
        public Boolean CheckTopCoinIsFrozen()
        {
            if (coinList.Count > 0)
            {
                var stackTop = coinList[0];
                return stackTop.frozenNumber > 0;
            }
            
            return false;
        }

        public Boolean CheckTopCoinIsShuttered()
        {
            if (coinList.Count > 0)
            {
                var stackTop = coinList[0];
                return stackTop.isShutter && !stackTop.shutterIsOpen ;
            }
            
            return false;
        }

        public Boolean CheckIsBelowTopCoin(int coinElementIndex)
        {
            if (coinList.Count > 0)
            {
                var stackTop = coinList[0];
                return stackTop.id < coinElementIndex;
            }
            
            return false;
        }

        public Boolean GetCoinGroupsAboveIndex(int coinGroupIndex, out List<GameObject> coinGroupList)
        {
            return coinPileMesh.GetCoinGroupsAboveIndex(coinGroupIndex, out coinGroupList);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            // Debug.Log("Clicked");
            // 如果钱堆为空，或者被锁住了，不做任何操作
            if (coinList.Count <= 0 || pointLock || tunnelLock || shutterLock || GameManager.Instance.IsGameStopped()) return;
            pointLock = true;
            
            // 获取栈顶状态
            var stackTop = coinList[0];
            
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
            
            // 判断是否是开关
            if (stackTop.isShutter && !stackTop.shutterIsOpen)
            {
                // 目前栈顶是开关，并且是关闭的状态，不能继续点击
                if (coinPileShutterMesh.GetCoinShutterMesh(stackTop.id, out var coinShutterMesh) && coinPileMesh.GetTopCoinGroup(out var topCoinGroup))
                {
                    coinPileShutterAnim.ShakeShutter(coinShutterMesh, topCoinGroup, () =>
                    {
                        pointLock = false;
                    });
                }
                return;
            }
            
            // 判断是否被连线挡住了
            if (coinPileCollection != null && coinPileCollection.CheckCoinPileIsBlockByRope(this.gameObject, stackTop.id,
                    () =>
                    {
                        pointLock = false;
                    }))
            {
                // 如果被挡住了，阻止进一步点击
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
                        coinList.RemoveAt(0);
                        if (unlockTransPile != null)
                        {
                            unlockTransPile();
                        }
                        
                        // 判断游戏是否结束
                        GameManager.Instance.CheckGameIsFailed();
                        
                        // 判断运钱区是否满了，需要转移
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
                        coinList[0] = new CoinPileItem()
                        {
                            id = stackTop.id,
                            color = stackTop.color,
                            number = stackTop.number - transPileFreeNumber,
                            isHidden = stackTop.isHidden,
                            frozenNumber = stackTop.frozenNumber,
                            isShutter = stackTop.isShutter,
                            shutterIsOpen = stackTop.shutterIsOpen,
                        };
                        if (unlockTransPile != null)
                        {
                            unlockTransPile();
                        }
                        
                        // 判断游戏是否结束
                        GameManager.Instance.CheckGameIsFailed();
                        
                        // 判断运钱区是否满了，需要转移
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
                                coinList.RemoveAt(0);
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
                                
                                // 判断游戏是否结束
                                GameManager.Instance.CheckGameIsFailed();
                            });
                            break;
                        }

                        // 3-1-2. 如果缓冲空余容量小于钱堆顶部数量，则只将缓冲区容量数量的钱放入缓冲区，剩下的钱按照上述步骤，再循环检测一遍
                        stackTop = new CoinPileItem()
                        {
                            id = stackTop.id,
                            color = stackTop.color,
                            number = stackTop.number - cachePileFreeNumber,
                            isHidden = stackTop.isHidden,
                            frozenNumber = stackTop.frozenNumber,
                            isShutter = stackTop.isShutter,
                            shutterIsOpen = stackTop.shutterIsOpen,
                        };
                        // 更新此时的栈顶状态
                        coinList[0] = stackTop;
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
                            
                            // 判断游戏是否结束
                            GameManager.Instance.CheckGameIsFailed();
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
                    if (coinPileCollection != null)
                    {
                        // 动画播放开始前，调用所有钱堆的冰冻消融效果
                        coinPileCollection.CheckCoinPileCanMeltIce();
                        
                        // 检查此时栈顶是否是开关
                        if(stackTop.isShutter && stackTop.shutterIsOpen)
                        {
                            // 如果是开关，并且是打开的状态，则开关消失
                            coinPileShutterMesh.DestroyCoinShutterMesh(stackTop.id);
                            
                            // 且要改变一次全局的开关状态，并且这次改变要排除掉自身
                            coinPileCollection.CheckCoinPileCanOpenShutter(this.gameObject, stackTop.id);
                        }
                        else if (!stackTop.isShutter)
                        {
                            // 如果不是开关，则直接改变一次全局的开关状态
                            coinPileCollection.CheckCoinPileCanOpenShutter(null, -1);
                        }
                        
                        // 检查是否具有 tunnel
                        if (hasTunnel)
                        {
                            // 检查此时是否只剩下最后一种颜色
                            if (coinList.Count == 1)
                            {
                                // 销毁 tunnel
                                // TODO: 并播放相应的销毁动画
                                if (coinPileTunnelMesh.GetCoinTunnelMesh(out var coinTunnelGameObject))
                                {
                                    coinPileTunnelAnim.TunnelDisappear(coinTunnelGameObject, () =>
                                    {
                                        coinPileTunnelMesh.DestroyCoinTunnelMesh();
                                    });
                                }
                            }
                        }

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
                        
                        // 存在 Tunnel 的情况下
                        if (hasTunnel)
                        {
                            if (stockTopItem.id >= tunnelStartIndex)
                            {
                                // 需要一边显示钱币，一边整体移动
                                // 获取 topCoinGroup 对应的 coins 列表
                                if (coinPileMesh.GetTopCoinGroup(out var topCoinGroup))
                                {
                                    CoinPrefab[] coinChildren = topCoinGroup.GetComponentsInChildren<CoinPrefab>();
                                    if (coinChildren.Length > 0)
                                    {
                                        // 播放钱币移出的动画之前，先上锁，此时不能点击
                                        tunnelLock = true;
                                        
                                        // 播放钱币移出的动画
                                        coinPileTunnelAnim.EmergeFromTunnel(this.gameObject, coinChildren, () =>
                                        {
                                            tunnelLock = false;
                                        });
                                        
                                        // 并修改提示板上的数字
                                        coinPileTunnelMesh.ChangeCoinTunnelNumber(coinListDataLength - stockTopItem.id - 1);
                                    }
                                }
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
                var stackTop = coinList[0];

                if (stackTop.frozenNumber > 0)
                {
                    int newFrozenNumber = stackTop.frozenNumber - 1;
                
                    // 如果是冰冻状态，减少冰冻数量，并播放相应动画
                    coinList[0] = new CoinPileItem()
                    {
                        id = stackTop.id,
                        color = stackTop.color,
                        number = stackTop.number,
                        isHidden = stackTop.isHidden,
                        frozenNumber = newFrozenNumber,
                        isShutter = stackTop.isShutter,
                        shutterIsOpen = stackTop.shutterIsOpen,
                    };
                
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

        public void TryOpenShutter(int excludeCoinId)
        {
            if (coinList.Count > 0)
            {
                // 锁定点击
                shutterLock = true;
                var coinListIndexList = new List<int>();
                
                // 遍历 coinList，查找与 excludeCoinPileId 不同的 coin
                for (var i = 0; i < coinList.Count; i++)
                {
                    var coin = coinList[i];
                    if (coin.isShutter && coin.id != excludeCoinId)
                    {
                        coinListIndexList.Add(i);
                    }
                }
            
                var coinShutterMeshList = new List<GameObject>(coinListIndexList.Count);
                var coinShutterStatusList = new List<Boolean>(coinListIndexList.Count);
                var coinOnCompleteList = new List<Action>(coinListIndexList.Count);
                
                // 修改其状态，并播放开关切换的动画
                foreach (var index in coinListIndexList)
                {
                    var coin = coinList[index];
                    
                    if (coinPileShutterMesh.GetCoinShutterMesh(coin.id, out var coinShutterMesh))
                    {
                        coinShutterMeshList.Add(coinShutterMesh);
                        coinShutterStatusList.Add(!coin.shutterIsOpen);
                        var index1 = index;
                        coinOnCompleteList.Add(() =>
                        {
                            coinList[index1] = new CoinPileItem()
                            {
                                id = coin.id,
                                color = coin.color,
                                number = coin.number,
                                isHidden = coin.isHidden,
                                frozenNumber = coin.frozenNumber,
                                isShutter = coin.isShutter,
                                shutterIsOpen = !coin.shutterIsOpen,
                            };
                        });
                    }
                }
            
                if (coinListIndexList.Count > 0)
                {
                    coinPileShutterAnim.OpenShutter(coinShutterMeshList, coinShutterStatusList, coinOnCompleteList, () =>
                    {
                        shutterLock = false;
                    });
                }
                else
                {
                    shutterLock = false;
                }
            }
        }
    }
}
