using GhostDrawProtobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class HallView : BaseView
{
    private DateTime startPairTime;

    [SerializeField]
    private RectTransform timing_Rt;
    [SerializeField]
    private Image avatar_Img;
    [SerializeField]
    private Text nickName_Txt, timing_Txt;
    [SerializeField]
    private Button pair_Btn, cancelPair_Btn;

    private void Awake()
    {
        RequestManager.Instance.RegisterBroadcast(ActionCode.StartGame, ReciveBroadcast);

        timing_Rt.gameObject.SetActive(false);
    }

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

    private void Start()
    {
        //配對
        pair_Btn.onClick.AddListener(() =>
        {
            startPairTime = DateTime.Now;
            timing_Rt.gameObject.SetActive(true);
            timing_Rt.anchoredPosition = new Vector2(0, timing_Rt.rect.height);

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Room;
            pack.ActionCode = ActionCode.Pair;
            SendRequest(pack);
        });

        //取消配對
        cancelPair_Btn.onClick.AddListener(() =>
        {
            timing_Rt.gameObject.SetActive(false);

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Room;
            pack.ActionCode = ActionCode.ExitRoom;
            SendRequest(pack);
        });
    }

    private void Update()
    {
        //計時器
        if (timing_Rt.gameObject.activeSelf)
        {
            if (timing_Rt.anchoredPosition.y > -100)
            {
                timing_Rt.Translate(new Vector3(0, -500 * Time.deltaTime, 0), Space.Self);
            }

            TimeSpan timing = DateTime.Now - startPairTime;
            timing_Txt.text = $"{(int)timing.TotalMinutes} : {timing.Seconds:00}";
        }
    }

    public override void ReciveBroadcast(MainPack pack)
    {
        base.ReciveBroadcast(pack);
    }

    public override void HandleBroadcast(MainPack pack)
    {
        switch (pack.ActionCode)
        {
            case ActionCode.StartGame:
                Debug.Log("配對成功，遊戲開始。");
                timing_Rt.gameObject.SetActive(false);
                UIManager.Instance.OpenTransitionView("Game");
                break;

            case ActionCode.ExitRoom:
                break;
        }
    }
}
