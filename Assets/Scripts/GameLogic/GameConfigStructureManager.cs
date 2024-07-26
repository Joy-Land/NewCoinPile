using Joyland.GamePlay;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItemManager
{
    public Dictionary<int, GameConfig.GameItemConfigData> m_LocalAllItemConfigDatas = new Dictionary<int, GameConfig.GameItemConfigData>();

    public GameItemManager(Dictionary<int, GameConfig.GameItemConfigData> allItemDatas)
    {
        var jsonString = JsonConvert.SerializeObject(allItemDatas);
        m_LocalAllItemConfigDatas = JsonConvert.DeserializeObject<Dictionary<int, GameConfig.GameItemConfigData>>(jsonString);
    }

    public GameConfig.GameItemConfigData GetItemConfigData(ItemID id)
    {
        var key = (int)id;
        GameConfig.GameItemConfigData data = null;
        if (m_LocalAllItemConfigDatas.TryGetValue(key, out data))
        {
            return data;
        }
        return data;
    }

    public void UpdateItemData(ItemID id, GameConfig.GameItemConfigData itemData)
    {
        var key = (int)id;
        if (m_LocalAllItemConfigDatas.ContainsKey(key))
        {
            m_LocalAllItemConfigDatas[key] = itemData;
        }
        else
        {
            m_LocalAllItemConfigDatas.Add(key, itemData);
        }
    }

    public (ItemCategoryID type, int remainNumber) GetItemCategoryData(ItemID id)
    {
        var itemData = GetItemConfigData(id);
        var len = itemData.categroylist.Count;
        for (int i = 0; i < len; i++)
        {
            var categroy = itemData.categroylist[i];
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
        var len = itemData.categroylist.Count;
        for (int i = 0; i < len; i++)
        {
            var categroy = itemData.categroylist[i];
            if (categroy.categoryID == (int)categoryID)
            {
                categroy.currentRemainedNumber = categroy.currentRemainedNumber + operationCount;
            }
        }
    }

}


