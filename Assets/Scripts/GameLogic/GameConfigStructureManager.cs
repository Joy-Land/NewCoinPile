using Joyland.GamePlay;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConfig;

public class GameItemManager
{
    public Dictionary<int, GameConfig.GameItemConfigData> AllItemConfigDatas = new Dictionary<int, GameConfig.GameItemConfigData>();

    public GameItemManager(Dictionary<int, GameConfig.GameItemConfigData> allItemDatas)
    {
        var jsonString = JsonConvert.SerializeObject(allItemDatas);
        AllItemConfigDatas = JsonConvert.DeserializeObject<Dictionary<int, GameConfig.GameItemConfigData>>(jsonString);
    }

    public GameConfig.GameItemConfigData GetItemConfigData(ItemID id)
    {
        var key = (int)id;
        GameConfig.GameItemConfigData data = null;
        if (AllItemConfigDatas.TryGetValue(key, out data))
        {
            return data;
        }
        return data;
    }

    public void UpdateItemData(ItemID id, GameConfig.GameItemConfigData itemData)
    {
        var key = (int)id;
        if (AllItemConfigDatas.ContainsKey(key))
        {
            AllItemConfigDatas[key] = itemData;
        }
        else
        {
            AllItemConfigDatas.Add(key, itemData);
        }
    }

    public (ItemCategoryID type, int remainNumber) GetItemCategoryData(ItemID id)
    {
        var itemData = GetItemConfigData(id);
        var len = itemData.categroylist.Count;
        var len = itemData.categorylist.Count;
        for (int i = 0; i < len; i++)
        {
            var categroy = itemData.categorylist[i];
            if (categroy.currentRemainedNumber > 0)
            {
                if (categroy.categoryID == (int)ItemCategoryID.Default)
                {
                    return (ItemCategoryID.Default, categroy.currentRemainedNumber);
                }
                if (categroy.categoryID == (int)ItemCategoryID.Share)
                {
                    return (ItemCategoryID.Share, categroy.currentRemainedNumber);
                }
            }
        }
        return (ItemCategoryID.Video, 99999);
    }

    public void UpdateItemCategoryData(ItemID id, ItemCategoryID categoryID, int operationCount)
    {
        var itemData = GetItemConfigData(id);
        var len = itemData.categorylist.Count;
        for (int i = 0; i < len; i++)
        {
            var categroy = itemData.categorylist[i];
            if (categroy.categoryID == (int)categoryID)
            {
                categroy.currentRemainedNumber = categroy.currentRemainedNumber + operationCount;
            }
        }
    }

}

public class LocalCopyWriteConfig
{
    public Dictionary<string, string> AllCopyWriteConfigDatas = new Dictionary<string, string>();
    public LocalCopyWriteConfig(string json)
    {
        AllCopyWriteConfigDatas = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }
}

public class LocalCollectConfig
{
    public List<GameConfig.CollectConfigData.CollectItemConfigData> AllCollectItemConfigDatas;
    public LocalCollectConfig(string json)
    {
        GameConfig.CollectConfigData AllCollectConfigData = JsonConvert.DeserializeObject<GameConfig.CollectConfigData>(json);
        AllCollectItemConfigDatas = AllCollectConfigData.collect_item_list;
        //console.error(AllCollectConfigData);
        //console.error(AllCollectConfigData.collect_item_list);
    }
}

public class LocalItemUsageConfig
{
    Dictionary<int, ItemUsageConfigData.GameItemUsageConfigData> AllItemUsageConfigDatas;
    public LocalItemUsageConfig(Dictionary<int, ItemUsageConfigData.GameItemUsageConfigData> allItemUsageConfig)
    {
        AllItemUsageConfigDatas = allItemUsageConfig;
    }

    public ItemUsageConfigData.GameItemUsageConfigData GetItemUsageConfigData(int id)
    {
        if(AllItemUsageConfigDatas.ContainsKey(id))
        {
            return AllItemUsageConfigDatas[id];
        }
        return null;
    }
}


