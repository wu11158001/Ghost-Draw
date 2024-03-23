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

        yield return YooAssets.GetPackage(LauncherManager.Instance.assetsPackageName).LoadSceneAsync(nextScene);

        UIManager.Instance.SetCanvas();
        switch (nextScene)
        {
            case "Hall":
                UIManager.Instance.OpenView<RectTransform>(ViewName.HallView);
                break;
        }

        yield return new WaitForSeconds(1.5f);

        gameObject.SetActive(false);
    }
}
