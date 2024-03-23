using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GhostDrawProtobuf;

public class BaseView : MonoBehaviour
{
    /// <summary>
    /// 發送協議
    /// </summary>
    /// <param name="pack"></param>
    public virtual void SendRequest(MainPack pack)
    {
        RequestManager.Instance.Send(pack, ReciveRequest);
    }

    /// <summary>
    /// 接收協議
    /// </summary>
    /// <param name="pack"></param>
    public virtual void ReciveRequest(MainPack pack)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            HandleRequest(pack);
        });
    }

    /// <summary>
    /// 處理協議
    /// </summary>
    /// <param name="pack"></param>
    public virtual void HandleRequest(MainPack pack)
    {

    }

    /// <summary>
    /// 接收廣播訊息
    /// </summary>
    /// <param name="pack"></param>
    public virtual void ReciveBroadcast(MainPack pack)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            HandleBroadcast(pack);
        });
    }

    /// <summary>
    /// 處理廣播訊息
    /// </summary>
    /// <param name="pack"></param>
    public virtual void HandleBroadcast(MainPack pack)
    {
        
    }
}
