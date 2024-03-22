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
