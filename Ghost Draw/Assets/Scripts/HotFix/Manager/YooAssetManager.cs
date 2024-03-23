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
    public IEnumerator GetAsset<T>(string assetName, UnityAction<T> callBack) where T : Object
    {
        AssetHandle handle = YooAssets.GetPackage(LauncherManager.Instance.assetsPackageName).LoadAssetAsync<T>(assetName);

        yield return handle;

        assetHandleLias.Add(handle);
        T asset = handle.AssetObject as T;
        callBack(asset);
    }

    private void OnDestroy()
    {
        foreach (var handle in assetHandleLias)
        {
            handle.Release();
        }
    }
}
