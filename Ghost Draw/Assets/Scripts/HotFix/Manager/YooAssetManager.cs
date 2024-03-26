using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using UnityEngine.Events;

public class YooAssetManager : UnitySingleton<YooAssetManager>
{
    private List<AssetHandle> assetHandleLias = new List<AssetHandle>();

    public override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 獲取資源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetName"></param>
    /// <param name="callBack"></param>
    public IEnumerator IGetAsset<T>(string assetName, UnityAction<T> callBack) where T : Object
    {
        AssetHandle handle = YooAssets.GetPackage(LauncherManager.Instance.AssetsPackageName).LoadAssetAsync<T>(assetName);

        yield return handle;

        assetHandleLias.Add(handle);
        T asset = handle.AssetObject as T;
        callBack(asset);
    }

    /// <summary>
    /// 獲取圖集資源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public IEnumerator IGetScriptableData(string assetName, UnityAction<Sprite[]> callBack)
    {
        yield return IGetAsset<ScriptableObject_SpriteData>(assetName, (asset) =>
        {
            if (asset != null)
            {
                SpriteData spriteData = asset.sprites;
                if (spriteData != null)
                {
                    Sprite[] spriteArray = spriteData.spritesList;
                    if (spriteArray != null)
                    {
                        callBack(spriteArray);
                    }
                    else
                    {
                        Debug.LogError($"{assetName}:圖像合集內容為空");
                    }
                }
                else
                {
                    Debug.LogError($"{assetName}:圖像合集 null !!!");
                }
            }
            else
            {
                Debug.LogError($"沒有找到圖像合集:{assetName}");
            }            
        });
    }

    private void OnDestroy()
    {
        foreach (var handle in assetHandleLias)
        {
            handle.Release();
        }
    }
}
