using GhostDrawProtobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class GameView : BaseView
{
    private const float selectPokerY = 100;

    [Header("用戶訊息")]
    [SerializeField]
    private Image userAvatar_Img;
    [SerializeField]
    private Text userNickName_Txt, userAction_Txt;
    [SerializeField]
    private RectTransform userPokerPoint;
    private List<RectTransform> userPokerList = new List<RectTransform>();
    [Header("對手訊息")]
    [SerializeField]
    private Image matchAvatar_Img;
    [SerializeField]
    private Text matchNickName_Txt, matchAction_Txt;
    [SerializeField]
    private RectTransform matchPokerPoint;
    private List<RectTransform> matchPokerList = new List<RectTransform>();
    [Header("資源")]
    private GameObject gamePokerObj;
    private Sprite[] pokerData;
    private Sprite[] backData;
    private Sprite[] jokerData;
    [Header("工具類按鈕")]
    [SerializeField]
    private Button shuffle_Btn, exit_Btn, draw_Btn;

    [SerializeField]
    private Image showDrawCard_Img;

    private string localUserId;
    private RectTransform currTouchPoker;
    private int drawIndex;
    private bool isAction;

    private void Awake()
    {
        localUserId = PlayerPrefs.GetString(LauncherManager.Instance.LocalData_UserID);
        RequestManager.Instance.RegisterBroadcast(ActionCode.InitGameInfo, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.HandPokers, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.SelectPoker, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.ActionUser, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.DrawCard, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.GameResult, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.ExitRoom, ReciveBroadcast);
    }

    private void OnEnable()
    {
        userAction_Txt.text = "";
        matchAction_Txt.text = "";
        draw_Btn.gameObject.SetActive(false);
        shuffle_Btn.gameObject.SetActive(false);
        isAction = false;
        showDrawCard_Img.gameObject.SetActive(false);
        exit_Btn.gameObject.SetActive(false);

        StartCoroutine(ILoadAssets());
    }

    private void Start()
    {
        //洗牌
        shuffle_Btn.onClick.AddListener(() =>
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Game;
            pack.ActionCode = ActionCode.Shuffle;
            SendRequest(pack);
        });

        //抽牌
        draw_Btn.onClick.AddListener(() =>
        {
            shuffle_Btn.gameObject.SetActive(false);
            draw_Btn.gameObject.SetActive(false);
            isAction = false;

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Game;
            pack.ActionCode = ActionCode.DrawCard;
            GamePack gamePack = new GamePack();
            gamePack.SelectPockerIndex = drawIndex;
            pack.GamePack = gamePack;
            SendRequest(pack);
        });

        //退出
        exit_Btn.onClick.AddListener(() =>
        {
            BackHall();
        });
    }

    private void Update()
    {
        //選牌
        if (isAction && Input.GetMouseButton(0))
        {
            GameObject obj = Utils.TouchUIName();
            if (currTouchPoker == null || obj != currTouchPoker.gameObject)
            {
                //判斷點擊位置是否大於已觸發物件的Y
                if (currTouchPoker != null)
                {
                    Vector2 localMousePosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(currTouchPoker, Input.mousePosition, null, out localMousePosition);
                    if (localMousePosition.y > currTouchPoker.rect.yMax)
                    {
                        return;
                    }
                }

                for (int i = 0; i < matchPokerList.Count; i++)
                {
                    if (obj == matchPokerList[i].gameObject)
                    {
                        drawIndex = i;
                        draw_Btn.gameObject.SetActive(true);

                        matchPokerList[i].anchoredPosition = new Vector2(matchPokerList[i].anchoredPosition.x, -selectPokerY);
                        if (currTouchPoker != null)
                        {
                            currTouchPoker.anchoredPosition = new Vector2(currTouchPoker.anchoredPosition.x, 0);
                            currTouchPoker.GetChild(0).gameObject.SetActive(false);
                        }
                        currTouchPoker = matchPokerList[i];
                        currTouchPoker.GetChild(0).gameObject.SetActive(true);

                        MainPack pack = new MainPack();
                        pack.RequestCode = RequestCode.Game;
                        pack.ActionCode = ActionCode.SelectPoker;
                        GamePack gamePack = new GamePack();
                        gamePack.SelectPockerIndex = i;
                        pack.GamePack = gamePack;
                        SendRequest(pack);
                        break;
                    }
                }
            }            
        }
    }

    /// <summary>
    /// 載入資源
    /// </summary>
    /// <returns></returns>
    private IEnumerator ILoadAssets()
    {
        yield return YooAssetManager.Instance.IGetAsset<GameObject>("GamePoker", (asset) =>
        {
            gamePokerObj = asset;
        });

        yield return YooAssetManager.Instance.IGetScriptableData("PokerData", (asset) =>
        {
            pokerData = asset;
        });
        yield return YooAssetManager.Instance.IGetScriptableData("BackData", (asset) =>
        {
            backData = asset;
        });
        yield return YooAssetManager.Instance.IGetScriptableData("JokerData", (asset) =>
        {
            jokerData = asset;
        });

        yield return new WaitForSeconds(1.5f);

        SendReadyOK();     
    }

    /// <summary>
    /// 發送準備完成
    /// </summary>
    private void SendReadyOK()
    {
        MainPack pack = new MainPack();
        pack.RequestCode = RequestCode.Game;
        pack.ActionCode = ActionCode.ReadyOk;
        SendRequest(pack);
    }

    /// <summary>
    /// 設定遊戲內玩家訊息
    /// </summary>
    /// <param name="userInfoPack"></param>
    private async void SetPlayersInfo(UserInfoPack userInfoPack)
    {
        //設定用戶訊息
        string localUserId = PlayerPrefs.GetString(LauncherManager.Instance.LocalData_UserID);

        Text textObj;
        Image imageObj;
        if (userInfoPack.LoginPack.Googleid == localUserId)
        {
            textObj = userNickName_Txt;
            imageObj = userAvatar_Img;
        }
        else
        {
            textObj = matchNickName_Txt;
            imageObj = matchAvatar_Img;
        }

        textObj.text = userInfoPack.LoginPack.NickName;
        if (!string.IsNullOrEmpty(userInfoPack.LoginPack.ImgUrl))
        {
            imageObj.sprite = await Utils.ImageUrlToSprite(userInfoPack.LoginPack.ImgUrl);
        }
    }

    /// <summary>
    /// 設定手牌
    /// </summary>
    /// <param name="gamePack"></param>
    private void SetHandPokers(GamePack gamePack)
    {
        foreach (var pocks in gamePack.PokerDic)
        {
            if (pocks.Key == localUserId)
            {
                StartCoroutine(ICreateNewPokers(userPokerPoint, userPokerList, pocks.Value.Values.ToList()));
            }
            else
            {
                StartCoroutine(ICreateNewPokers(matchPokerPoint, matchPokerList, pocks.Value.Values.ToList()));
            }
        }
    }

    /// <summary>
    /// 產生新撲克
    /// </summary>
    /// <param name="point">手牌父物件</param>
    /// <param name="currPoker">當前手牌物件</param>
    /// <param name="newPoker">更新手牌內容</param>
    /// <returns></returns>
    private IEnumerator ICreateNewPokers(RectTransform point, List<RectTransform> currPoker, List<int> newPoker)
    {
        //原手牌合併
        for (int i = point.childCount - 1; i >= 0 ; i--)
        {
            Destroy(point.GetChild(i).gameObject);
            yield return null;
        }
        currPoker.Clear();

        //新手牌
        int dir = point == userPokerPoint ? -1 : 1;
        float addPosX = 31 * dir;
        for (int i = 0; i < newPoker.Count; i++)
        {
            Image poker = Instantiate(gamePokerObj).GetComponent<Image>();
            poker.rectTransform.SetParent(point);
            if (point == userPokerPoint)
            {
                poker.sprite = newPoker[i] < 52 ? pokerData[newPoker[i]] : jokerData[0];
            }
            else
            {
                poker.sprite = backData[0];
            }
            float posX = addPosX * (newPoker.Count / 2) - (i * addPosX);
            poker.rectTransform.anchoredPosition = new Vector2(posX, 0);
            poker.rectTransform.SetSiblingIndex(i);
            poker.name = poker.sprite.name;
            poker.rectTransform.GetChild(0).gameObject.SetActive(false);
            currPoker.Add(poker.rectTransform);

            yield return null;
        }  
    }

    /// <summary>
    /// 抽牌
    /// </summary>
    /// <param name="pack"></param>
    /// <returns></returns>
    private IEnumerator IDrawCard(MainPack pack)
    {
        float moveTime = 0.3f;
        float showRotationTime = 0.6f;
        float addY = 550;
        DateTime startTime = DateTime.Now;

        RectTransform rt = pack.GamePack.ActionUserId == localUserId ? 
                           matchPokerList[pack.GamePack.SelectPockerIndex] : 
                           userPokerList[pack.GamePack.SelectPockerIndex];
        int dir = pack.GamePack.ActionUserId == localUserId ? -1 : 1;

        //抽牌效果
        float initY = rt.anchoredPosition.y;
        while ((DateTime.Now - startTime).TotalSeconds < moveTime)
        {
            float progess = (float)(DateTime.Now - startTime).TotalSeconds / moveTime;
            float posY = Mathf.Lerp(initY, initY + (addY * dir), progess);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, posY);
            yield return null;
        }
        rt.gameObject.SetActive(false);

        //顯示抽中的牌
        showDrawCard_Img.gameObject.SetActive(true);
        showDrawCard_Img.sprite = backData[0];
        showDrawCard_Img.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
        startTime = DateTime.Now;
        Sprite pokerSprite = pack.GamePack.DrawPoker < 52 ? pokerData[pack.GamePack.DrawPoker] : jokerData[0];
        while ((DateTime.Now - startTime).TotalSeconds < showRotationTime)
        {
            float progess = (float)(DateTime.Now - startTime).TotalSeconds / showRotationTime;
            float rotZ = Mathf.Lerp(0, 180, progess);
            showDrawCard_Img.rectTransform.rotation = Quaternion.Euler(0, rotZ, 0);
            showDrawCard_Img.sprite = rotZ >= 90 ? pokerSprite : backData[0];
            yield return null;
        }

        yield return new WaitForSeconds(2);

        showDrawCard_Img.gameObject.SetActive(false);
    }

    /// <summary>
    /// 返回大廳
    /// </summary>
    private void BackHall()
    {
        MainPack pack = new MainPack();
        pack.RequestCode = RequestCode.Room;
        pack.ActionCode = ActionCode.ExitRoom;
        SendRequest(pack);

        UIManager.Instance.OpenTransitionView("Hall");
    }

    public override void ReciveBroadcast(MainPack pack)
    {
        base.ReciveBroadcast(pack);
    }

    public override void HandleBroadcast(MainPack pack)
    {
        switch (pack.ActionCode)
        {
            //遊戲初始
            case ActionCode.InitGameInfo:
                foreach (var userInfo in pack.UserInfoPack)
                {
                    SetPlayersInfo(userInfo);
                }

                SetHandPokers(pack.GamePack);
                break;

            //獲取手牌
            case ActionCode.HandPokers:
                SetHandPokers(pack.GamePack);
                exit_Btn.gameObject.SetActive(true);
                break;

            //對手選牌
            case ActionCode.SelectPoker:
                for (int i = 0; i < userPokerList.Count; i++)
                {
                    if (i == pack.GamePack.SelectPockerIndex)
                    {
                        userPokerList[i].anchoredPosition = new Vector2(userPokerList[i].anchoredPosition.x, selectPokerY);
                        userPokerList[i].GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        userPokerList[i].anchoredPosition = new Vector2(userPokerList[i].anchoredPosition.x, 0);
                        userPokerList[i].GetChild(0).gameObject.SetActive(false);
                    }
                }
                break;

            //行動者
            case ActionCode.ActionUser:                
                isAction = pack.GamePack.ActionUserId == localUserId;
                shuffle_Btn.gameObject.SetActive(pack.GamePack.ActionUserId == localUserId);
                userAction_Txt.text = pack.GamePack.ActionUserId == localUserId ? "抽牌" : "";
                matchAction_Txt.text = pack.GamePack.ActionUserId != localUserId ? "抽牌" : "";
                break;

            //抽牌
            case ActionCode.DrawCard:
                StartCoroutine(IDrawCard(pack));
                break;

            //遊戲結果
            case ActionCode.GameResult:
                string resultStr = pack.GamePack.WinnerId == localUserId ? "獲勝" : "失敗";
                UIManager.Instance.OpenTipView(resultStr, () =>
                {
                    BackHall();
                });
                break;

            //離開房間
            case ActionCode.ExitRoom:
                UIManager.Instance.OpenTipView("對手已退出", () =>
                {
                    BackHall();
                });
                break;
        }
    }
}
