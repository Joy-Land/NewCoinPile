using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRequestHelper
{
    public static void ReqAllItemListDataConfig(Action<ProtoItemListDataStruct> successCb)
    {
        NetworkManager.Instance.GetReq<ProtoItemListDataStruct>("get_item_number_list", null, (res) =>
        {
            if (res.isSuccessed)
            {
                if (res.resData.code == 0)
                {
                    successCb?.Invoke(res.resData.data);
                }
            }
        });
    }

    public static void ReqItemDataConfig(int itemID, Action<ProtoItemDataStruct> successCb)
    {
        NetworkManager.Instance.GetReq<ProtoItemDataStruct>("get_item_number", new {itemId = itemID}, (res) =>
        {
            if (res.isSuccessed)
            {
                if (res.resData.code == 0)
                {
                    successCb?.Invoke(res.resData.data);
                }
            }
        });
    }
}
