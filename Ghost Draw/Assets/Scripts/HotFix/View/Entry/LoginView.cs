using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using GhostDrawProtobuf;

public class LoginView : BaseView
{
    /// <summary>
    /// 用戶登入
    /// </summary>
    public void Start()
    {
        string googleid = "";
        string nickName = "";
        string imgUrl = "";
#if !UNITY_EDITOR
        //Google登入
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                googleid = PlayGamesPlatform.Instance.GetUserId();
                nickName = PlayGamesPlatform.Instance.GetUserDisplayName();
                imgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

                Debug.Log($"Google 登入。");
                SendLoginRequest(googleid, nickName, imgUrl);
            }
            else
            {
                Debug.LogError("Google 登入失敗!!!");
            }
        });
#else
        googleid = "TestAccount";
        nickName = "伍鈞遠";
        imgUrl = "";        
        SendLoginRequest(googleid, nickName, imgUrl);
#endif
    }

    /// <summary>
    /// 發送登入協議
    /// </summary>
    /// <param name="googleid"></param>
    private void SendLoginRequest(string googleid, string nickName, string imgUrl)
    {
        PlayerPrefs.SetString(LauncherManager.Instance.LocalData_UserID, googleid);
        PlayerPrefs.SetString(LauncherManager.Instance.LocalData_NickName, nickName);
        PlayerPrefs.SetString(LauncherManager.Instance.LocalData_ImgUrl, imgUrl);

        MainPack pack = new MainPack();
        pack.RequestCode = RequestCode.User;
        pack.ActionCode = ActionCode.Login;

        LoginPack loginPack = new LoginPack();
        loginPack.Googleid = googleid;
        loginPack.NickName = nickName;
        loginPack.ImgUrl = imgUrl;
        pack.LoginPack = loginPack;
        SendRequest(pack);
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
        //登入
        if (pack.ActionCode == ActionCode.Login)
        {
            if (pack.ReturnCode == ReturnCode.Succeed)
            {
                Debug.Log("登入成功。");
                UIManager.Instance.Transition("Hall");
            }
            else if (pack.ReturnCode == ReturnCode.Fail)
            {
                Debug.Log("登入失敗!!");
            }
            else if (pack.ReturnCode == ReturnCode.Duplicated)
            {
                Debug.Log("重複登入");
            }
        }
    }
}
