using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GamePlayerDataManager
{
    //和服务端同步的金币，客户端没权力更改他
    private float m_ServerAllCoins;


    private float m_LocalAllCoins;
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


}
