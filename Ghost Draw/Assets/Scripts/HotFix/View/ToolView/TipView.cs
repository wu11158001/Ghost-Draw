using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TipView : MonoBehaviour
{
    [SerializeField]
    private Text content_Txt, confirm_Txt;
    [SerializeField]
    private Button confirm_Btn, cancel_Btn;

    /// <summary>
    /// 設置提示
    /// </summary>
    /// <param name="content">內容</param>
    /// <param name="confirmCallBack">確認回傳</param>
    /// <param name="confirmStr">確認按鈕文字</param>
    /// <param name="isCancel">是否有取消按鈕</param>
    /// <param name="cancelCallBack">取消回傳</param>
    public void SetTip(string content, UnityAction confirmCallBack = null, string confirmStr = "確認", bool isCancel = false, UnityAction cancelCallBack = null)
    {
        content_Txt.text = content;
        confirm_Txt.text = confirmStr;
        confirm_Btn.onClick.AddListener(() =>
        {
            if (confirmCallBack != null)
            {
                confirmCallBack();
            }
            gameObject.SetActive(false);
        });

        cancel_Btn.gameObject.SetActive(isCancel);
        if (isCancel)
        {
            cancel_Btn.onClick.AddListener(() =>
            {
                if (cancelCallBack != null)
                {
                    cancelCallBack();
                }
                gameObject.SetActive(false);
            });
        }
    }
}
