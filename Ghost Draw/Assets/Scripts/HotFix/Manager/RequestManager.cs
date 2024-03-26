using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GhostDrawProtobuf;

public class RequestManager : UnitySingleton<RequestManager>
{
    public override void Awake()
    {
        base.Awake();
    }

    private Dictionary<ActionCode, UnityAction<MainPack>> requsetDic = new Dictionary<ActionCode, UnityAction<MainPack>>();
    private Dictionary<ActionCode, UnityAction<MainPack>> broadcastDic = new Dictionary<ActionCode, UnityAction<MainPack>>();

    /// <summary>
    /// 清除協議紀錄
    /// </summary>
    public void ClearRequestDic()
    {
        requsetDic.Clear();
        broadcastDic.Clear();
    }

    /// <summary>
    /// 註冊下發協議
    /// </summary>
    /// <param name="actionCode"></param>
    /// <param name="callback"></param>
    public void RegisterRequest(ActionCode actionCode, UnityAction<MainPack> callback)
    {
        if (!requsetDic.ContainsKey(actionCode))
        {
            requsetDic.Add(actionCode, callback);
        }
    }

    /// <summary>
    /// 註冊廣播事件
    /// </summary>
    /// <param name="actionCode"></param>
    /// <param name="callback"></param>
    public void RegisterBroadcast(ActionCode actionCode, UnityAction<MainPack> callback)
    {
        broadcastDic.Add(actionCode, callback);
    }

    /// <summary>
    /// 發送協議
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="callback"></param>
    public void Send(MainPack pack, UnityAction<MainPack> callback)
    {
        ClientManager.Instance.Send(pack);

        if (!requsetDic.ContainsKey(pack.ActionCode))
        {
            requsetDic.Add(pack.ActionCode, callback);
        }
    }

    /// <summary>
    /// 處理回覆
    /// </summary>
    /// <param name="pack"></param>
    public void HandleResponse(MainPack pack)
    {
        if (pack.SendModeCode == SendModeCode.RoomBroadcast)
        {
            //房間廣播
            if (broadcastDic.ContainsKey(pack.ActionCode))
            {
                broadcastDic[pack.ActionCode](pack);

                if (requsetDic.ContainsKey(pack.ActionCode))
                {
                    requsetDic.Remove(pack.ActionCode);
                }
            }
            else
            {
                Debug.LogError("沒有相關房間廣播協議:" + pack.ActionCode);
            }
        }
        else
        {
            //一般協議
            if (requsetDic.ContainsKey(pack.ActionCode))
            {
                requsetDic[pack.ActionCode](pack);
                requsetDic.Remove(pack.ActionCode);
            }
            else
            {
                Debug.LogWarning("沒有相關協議:" + pack.ActionCode);
            }
        }
    }
}
