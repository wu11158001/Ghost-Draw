using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

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
}
