using Joyland.GamePlay;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConfig.CollectConfigData;


//using AllItemConfigDatasMapping = System.Collections.Generic.Dictionary<int, GameConfig.GameItemConfigData>;
//using TTT = GameConfig;
public class GameConfig
{
    public class GameItemConfigData
    {
        public string itemName;
        public List<(int categoryID, int currentRemainedNumber)> categorylist;
    }

    public class ItemUsageConfigData
    {
        public class GameItemUsageConfigData
        {
            public int itemId;
            public string itemName;
            public int defaultCount;
            public int currentCount;
            public int maxAcquireCount;
            public int currentAcquireCount;
            public string desc;
        }

        public List<GameItemUsageConfigData> itemList;
    }


    public class CollectConfigData
    {
        public class CollectItemConfigData
        {
            public float limit;
            public string desc;
            public string note;
        }
        public List<CollectItemConfigData> collect_item_list;
    }

    public class LocalGameItemConfigData
    {
        public List<ProtoItemDataStruct> itemList;
    }

    private static GameItemManager m_GameItemManager;
    public static GameItemManager GameItemManager
    {
        get
        {
            return m_GameItemManager;
        }
    }

    private static LocalCopyWriteConfig m_LocalCopyWriteManager;
    public static LocalCopyWriteConfig LocalCopyWriteManager
    {
        get
        {
            return m_LocalCopyWriteManager;
        }
    }

    private static LocalCollectConfig m_LocalCollectManager;
    public static LocalCollectConfig LocalCollectManager
    {
        get
        {
            return m_LocalCollectManager;
        }
    }


    private static LocalItemUsageConfig m_LocalItemUsageManager;
    public static LocalItemUsageConfig LocalItemUsageManager
    {
        get
        {
            return m_LocalItemUsageManager;
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

    public void LoadLocalCopyWriteConfig(string json)
    {
        m_LocalCopyWriteManager = new LocalCopyWriteConfig(json);
    }

    public void LoadLocalCollectConfig(string json)
    {
        m_LocalCollectManager = new LocalCollectConfig(json);
    }

    //默认加载一份本地的，服务器拉下来会覆盖之，不会卡住进不去游戏
    public void LoadLocalGameItemConfig(string json)
    {
        var conf = JsonConvert.DeserializeObject<LocalGameItemConfigData>(json);
        console.error(conf, conf.itemList);
        SetAllItemData(conf.itemList);
    }

    public void LoadLocalItemUsageConfig(string json)
    {
        Dictionary<int, ItemUsageConfigData.GameItemUsageConfigData> allItemUsageConfigDatas = new Dictionary<int, ItemUsageConfigData.GameItemUsageConfigData>();

        var itemUsageList = JsonConvert.DeserializeObject<ItemUsageConfigData>(json).itemList;
        foreach (var itemUsage in itemUsageList)
        {
            var gConf = new ItemUsageConfigData.GameItemUsageConfigData() { 
                itemId = itemUsage.itemId,
                defaultCount = itemUsage.defaultCount,
                currentCount = itemUsage.currentCount,
                maxAcquireCount = itemUsage.maxAcquireCount,
                currentAcquireCount = itemUsage.currentAcquireCount,
                itemName = itemUsage.itemName,
                desc = itemUsage.desc,
            };
            allItemUsageConfigDatas.Add(itemUsage.itemId, gConf);
        }
        //TODO:
        m_LocalItemUsageManager = new LocalItemUsageConfig(allItemUsageConfigDatas);
    }

    public void SetAllItemData(List<ProtoItemDataStruct> itemDataList)
    {
        if (itemDataList == null || itemDataList.Count == 0)
            return;
        this.m_ServerAllItemConfigDatas.Clear();

        var len = itemDataList.Count;
        for (int i = 0; i < len; i++)
        {
            var itemData = itemDataList[i];
            var gConf = new GameItemConfigData() { itemName = itemData.itemName , categorylist = new List<(int categroyID, int currentRemainedNumber)>()};
            for (int j = 0; j < itemData.categoryList.Count; j++)
            {
                var data = itemData.categoryList[j];
                gConf.categorylist.Add(new ValueTuple<int,int>(data.categoryId, data.currentRemainedNumber));
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
        var gConf = new GameItemConfigData() { itemName = itemData.itemName, categorylist = new List<(int categroyID, int currentRemainedNumber)>() };
        for (int j = 0; j < itemData.categoryList.Count; j++)
        {
            var data = itemData.categoryList[j];
            gConf.categorylist.Add(new ValueTuple<int, int>(data.categoryId, data.currentRemainedNumber));
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
