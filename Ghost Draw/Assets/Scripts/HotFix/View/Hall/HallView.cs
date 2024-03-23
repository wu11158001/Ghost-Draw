using GhostDrawProtobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HallView : BaseView
{
    [SerializeField]
    private Image avatar_Img;
    [SerializeField]
    private Text nickName_Txt;

    private async void OnEnable()
    {
        //設定用戶訊息
        nickName_Txt.text = PlayerPrefs.GetString(LauncherManager.Instance.LocalData_NickName);
        string imgUrl = PlayerPrefs.GetString(LauncherManager.Instance.LocalData_ImgUrl);
        if (!string.IsNullOrEmpty(imgUrl))
        {
            avatar_Img.sprite = await Utils.ImageUrlToSprite(imgUrl);
        }        
    }

    public override void SendRequest(MainPack pack)
    {
        base.SendRequest(pack);
    }

    public override void ReciveRequest(MainPack pack)
    {
        base.ReciveRequest(pack);
    }

    public override void HandleRequest(MainPack pack)
    {
        base.HandleRequest(pack);
    }
}
