using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class TransitionView : MonoBehaviour
{
    /// <summary>
    /// 轉場
    /// </summary>
    /// <param name="nextScene"></param>
    /// <returns></returns>
    public IEnumerator ITransition(string nextScene)
    {
        transform.SetSiblingIndex(101);

        UIManager.Instance.ClearData();
        RequestManager.Instance.ClearRequestDic();

        yield return YooAssets.GetPackage(LauncherManager.Instance.AssetsPackageName).LoadSceneAsync(nextScene);

        //場景啟動介面
        UIManager.Instance.SetCanvas();
        switch (nextScene)
        {
            case "Hall":
                UIManager.Instance.OpenView<RectTransform>(ViewName.HallView);
                break;
                
            case "Game":
                UIManager.Instance.OpenView<RectTransform>(ViewName.GameView);
                break;
        }

        yield return new WaitForSeconds(1.5f);

        gameObject.SetActive(false);
    }
}
