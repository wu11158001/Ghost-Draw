using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : UnitySingleton<UIManager>
{
    private Transform canvas;
    private Transform toolCanvas;

    private Dictionary<ViewName, RectTransform> viewDic = new Dictionary<ViewName, RectTransform>();
    private Dictionary<ViewName, RectTransform> toolDic = new Dictionary<ViewName, RectTransform>();
    private Stack<RectTransform> viewStack = new Stack<RectTransform>();

    public override void Awake()
    {
        base.Awake();

        SetCanvas();
    }

    /// <summary>
    /// 設定場景Canvas
    /// </summary>
    public void SetCanvas()
    {
        canvas = GameObject.Find("Canvas").transform;

        if (toolCanvas == null)
        {
            toolCanvas = GameObject.Find("ToolCanvas").transform;
            DontDestroyOnLoad(toolCanvas.gameObject);
        }
    }

    /// <summary>
    /// 清除紀錄資料
    /// </summary>
    public void ClearData()
    {
        viewDic.Clear();
        viewStack.Clear();
    }

    /// <summary>
    /// 初始化View
    /// </summary>
    /// <param name="rt"></param>
    private void InitView(RectTransform rt)
    {
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.eulerAngles = Vector3.zero;
        rt.localScale = Vector3.one;
    }

    /// <summary>
    /// 轉場
    /// </summary>
    /// <param name="nextScene"></param>
    public void OpenTransitionView(string nextScene)
    {
        OpenToolView<TransitionView>(ViewName.TransitionView, (transitionView) =>
        {
            StartCoroutine(transitionView.ITransition(nextScene));
        });
    }

    /// <summary>
    /// 開啟提示
    /// </summary>
    /// <param name="content">內容</param>
    /// <param name="confirmCallBack">確認回傳</param>
    /// <param name="confirmStr">確認按鈕文字</param>
    /// <param name="isCancel">是否有取消按鈕</param>
    /// <param name="cancelCallBack">取消回傳</param>
    public void OpenTipView(string content, UnityAction confirmCallBack = null, string confirmStr = "確認", bool isCancel = false, UnityAction cancelCallBack = null)
    {
        OpenToolView<TipView>(ViewName.TipView, (TipView) =>
        {
            TipView.SetTip(content,
                           confirmCallBack,
                           confirmStr,
                           isCancel,
                           cancelCallBack);
        });
    }

    /// <summary>
    /// 開啟工具介面
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="callBack"></param>
    public void OpenToolView<T>(ViewName viewName, UnityAction<T> callBack = null) where T : Component
    {
        StartCoroutine(IOpenToolView(viewName, callBack));
    }
    private IEnumerator IOpenToolView<T>(ViewName viewName, UnityAction<T> callBack = null) where T : Component
    {
        RectTransform view = null;
        if (toolDic.ContainsKey(viewName))
        {
            view = toolDic[viewName];
            view.gameObject.SetActive(true);
        }
        else
        {
            yield return YooAssetManager.Instance.IGetAsset<GameObject>(viewName.ToString(), (viewAsset) =>
            {
                view = Instantiate(viewAsset).GetComponent<RectTransform>();
                view.SetParent(toolCanvas);
                view.transform.SetSiblingIndex(101);
                InitView(view);
                toolDic.Add(viewName, view);
            });
        }

        callBack?.Invoke(view.GetComponent<T>());
    }

    /// <summary>
    /// 開啟介面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="callBack"></param>
    public void OpenView<T>(ViewName viewName, UnityAction<T> callBack = null) where T : Component
    {
        StartCoroutine(IOpenView(viewName, callBack));
    }
    private IEnumerator IOpenView<T>(ViewName viewName, UnityAction<T> callBack = null) where T : Component
    {
        if (viewStack.Count > 0)
        {
            viewStack.Peek().gameObject.SetActive(false);
        }

        RectTransform view = null;
        if (viewDic.ContainsKey(viewName))
        {
            view = viewDic[viewName];
            view.gameObject.SetActive(true);
        }
        else
        {
            yield return YooAssetManager.Instance.IGetAsset<GameObject>(viewName.ToString(), (asset) =>
            {
                view = Instantiate(asset).GetComponent<RectTransform>();
                view.SetParent(canvas);
                InitView(view);
                viewDic.Add(viewName, view);
            });
        }

        viewStack.Push(view);
        callBack?.Invoke(view.GetComponent<T>());
    }
}
