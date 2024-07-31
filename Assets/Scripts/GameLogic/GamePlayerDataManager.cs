using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GamePlayerDataManager
{
    //和服务端同步的金币，客户端没权力更改他
    private float m_ServerAllCoins;

    //以服务端数据为基准的用户当前stage
    private int m_ServerUserCurrentStage = 0;

    private float m_LocalAllCoins;
    /// <summary>
    /// 用户当前总资产
    /// </summary>
    public float AllCoins
    {
        set
        {
            m_LocalAllCoins = value;
        }
        get
        {
            return m_ServerAllCoins;
        }
    }

    private int m_LocalUserCurrentStage = 0;
    /// <summary>
    /// 用户当前所处stage
    /// </summary>
    public int UserCurrentStage
    {
        get
        {
            return m_LocalUserCurrentStage;
        }
    }


    private bool m_StageHasIncrese = false;
    /// <summary>
    /// 如果检测到为true，需要手动置为false
    /// </summary>
    public bool StageHasIncrease
    {
        set
        {
            m_StageHasIncrese = value;
        }
        get
        {
            return m_StageHasIncrese;
        }
    }

    /// <summary>
    /// 检查是否stage增加了
    /// </summary>
    /// <param name="additionCoins">新增的金币数量</param>
    /// <returns></returns>
    public bool CheckStageIncrease(float additionCoins)
    {
        var searchStage = 0;
        var coins = AllCoins + additionCoins;
        var list = GameConfig.LocalCollectManager.AllCollectItemConfigDatas;
        var len = list.Count;
        for (int i = 0; i < len; i++)
        {
            if (coins >= list[i].limit)
            {
                searchStage = i;
                break;
            }
        }

        if (searchStage > UserCurrentStage)
        {
            m_StageHasIncrese = true;
            return m_StageHasIncrese;
        }
        return m_StageHasIncrese;
    }

}
