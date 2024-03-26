using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Utils
{
    /// <summary>
    /// 載入url圖片
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<Sprite> ImageUrlToSprite(string url)
    {
        Sprite sprite = null;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        var asyncOperation = www.SendWebRequest();

        while (!asyncOperation.isDone)
        {
            await Task.Delay(100);//等待100毫秒，避免卡住主線程
        }

        if (www.result == UnityWebRequest.Result.Success)
        {
            // 載入成功，將下載的紋理轉換為Sprite
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        else
        {
            Debug.LogError($"載入url圖片失敗:{www.error}");
        }

        return sprite;
    }

    //觸碰的UI
    private static GraphicRaycaster graphicRaycaster;
    private static EventSystem eventSystem;
    private static PointerEventData eventData;
    /// <summary>
    /// 觸碰的UI物件
    /// </summary>
    /// <returns></returns>
    public static GameObject TouchUIName()
    {
        try
        {
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;
            List<RaycastResult> list = new List<RaycastResult>();
            graphicRaycaster.Raycast(eventData, list);
            foreach (var temp in list)
            {
                if (temp.gameObject.layer.Equals(5))
                {
                    return temp.gameObject;
                }
            }
            return null;
        }
        catch (System.Exception)
        {
            graphicRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
            eventSystem = EventSystem.current;
            eventData = new PointerEventData(eventSystem);
            if (graphicRaycaster == null)
            {
                Debug.Log("未有GraphicRaycaster");
                throw;
            }
            else
            {
                return TouchUIName();
            }
        }
    }
}
