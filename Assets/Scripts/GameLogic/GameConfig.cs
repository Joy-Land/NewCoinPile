using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//using AllItemConfigDatasMapping = System.Collections.Generic.Dictionary<int, GameConfig.GameItemConfigData>;
//using TTT = GameConfig;
public class GameConfig
{
    public class GameItemConfigData
    {
        public string itemName;
        public List<(int categoryID, int currentRemainedNumber)> categroylist;
    }

    private static GameItemManager m_GameItemManager;
    public static GameItemManager GameItemManager
    {
        get
        {
            return m_GameItemManager;
        }
    }



    private GameConfig() { }
    private static GameConfig m_Instance;
    public static GameConfig Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new GameConfig();
            return m_Instance;
        }
    }

    /// <summary>
    /// 客户端没权力更新它，它始终和服务器同步
    /// </summary>
    private Dictionary<int, GameItemConfigData> m_ServerAllItemConfigDatas = new Dictionary<int, GameItemConfigData>();


    public void SetAllItemData(List<ProtoItemDataStruct> itemDataList)
    {
        if (itemDataList == null || itemDataList.Count == 0)
            return;
        this.m_ServerAllItemConfigDatas.Clear();

        var len = itemDataList.Count;
        for (int i = 0; i < len; i++)
        {
            var itemData = itemDataList[i];
            var gConf = new GameItemConfigData() { itemName = itemData.itemName , categroylist = new List<(int categroyID, int currentRemainedNumber)>()};
            for (int j = 0; j < itemData.categoryList.Count; j++)
            {
                var data = itemData.categoryList[j];
                gConf.categroylist.Add(new ValueTuple<int,int>(data.categoryId, data.currentRemainedNumber));
            }
            if(m_ServerAllItemConfigDatas.ContainsKey(itemData.itemId) == false)
            {
                m_ServerAllItemConfigDatas.Add(itemData.itemId, gConf);
            }
        }

        m_GameItemManager = new GameItemManager(m_ServerAllItemConfigDatas);
    }

    public void SetItemData(ProtoItemDataStruct itemData)
    {
        if (itemData == null)
            return;
        var gConf = new GameItemConfigData() { itemName = itemData.itemName, categroylist = new List<(int categroyID, int currentRemainedNumber)>() };
        for (int j = 0; j < itemData.categoryList.Count; j++)
        {
            var data = itemData.categoryList[j];
            gConf.categroylist.Add(new ValueTuple<int, int>(data.categoryId, data.currentRemainedNumber));
        }
        if (m_ServerAllItemConfigDatas.ContainsKey(itemData.itemId) == false)
        {
            m_ServerAllItemConfigDatas.Add(itemData.itemId, gConf);
        }
        else
        {
            m_ServerAllItemConfigDatas[itemData.itemId] = gConf;
        }
        m_GameItemManager = new GameItemManager(m_ServerAllItemConfigDatas);
    }

}
