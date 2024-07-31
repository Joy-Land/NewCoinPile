using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRequestHelper
{
    public GameRequestHelper()
    {

    }
    public void ReqAllItemListDataConfig(Action<ProtoItemListDataStruct> successCb)
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

    public void ReqItemDataConfig(int itemID, Action<ProtoItemDataStruct> successCb)
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

    public void UpdateItemData(int itemID, int categroyID, int updateNumber, int ttl, Action<ProtoItemDataStruct> successCb)
    {
        NetworkManager.Instance.GetReq<ProtoItemDataStruct>("update_item_number", new { itemId = itemID, categoryId = categroyID, updateNumber = updateNumber, ttl = ttl }, (res) =>
        {
            if(res.isSuccessed)
            {
                if(res.resData.code == 0)
                {
                    successCb?.Invoke(res.resData.data);
                }
            }
        });
    }

    public void UploadUserInfo(string nickName, string avatarUrl, Action successCb)
    {
        NetworkManager.Instance.PostReq<ProtoEmptyStruct>("set_user_name_and_avatar", new { userName = nickName, userAvatar = avatarUrl }, (res) =>
        {
            if(res.isSuccessed)
            {
                if(res.resData.code == 0)
                {
                    console.info("头像昵称上报成功");
                }
            }
        });
    }
}
