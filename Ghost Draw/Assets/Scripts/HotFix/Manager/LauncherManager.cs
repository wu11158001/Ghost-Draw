using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;
using System;
using HybridCLR;

public class LauncherManager : UnitySingleton<LauncherManager>
{
    public string AssetsPackageName { get; } = "AssetsPackage";
    public string HotFixPackageName { get; } = "HotFixPackage";

    public string LocalData_UserID { get; } = "GhostDraw_UserId";
    public string LocalData_NickName { get; } = "GhostDraw_NickName";
    public string LocalData_ImgUrl { get; } = "GhostDraw_ImgUrl";

    /// <summary>
    /// 補充AOT
    /// </summary>
    private static List<string> additionalAOT { get; } = new List<string>()
    {
        
    };

    /// <summary>
    /// 熱更Dll
    /// </summary>
    private static List<string> HotFixDllName { get; } = new List<string>()
    {
        "Assembly-CSharp.dll",
    };

    private ResourcePackage hotFixPackage;

    private Dictionary<string, Assembly> assmblyList = new Dictionary<string, Assembly>();
    private List<RawFileHandle> rawHandleList = new List<RawFileHandle>();

    public override void Awake()
    {
        base.Awake();
    }

    private IEnumerator Start()
    {
        hotFixPackage = YooAssets.GetPackage(HotFixPackageName);

        yield return LoadAdditionalAssemblies();
        yield return LoadHotFixDll();

        Debug.Log("熱更新完成。");

        //啟動腳本
        gameObject.AddComponent<ClientManager>();
        gameObject.AddComponent<RequestManager>();
        gameObject.AddComponent<UnityMainThreadDispatcher>();
        gameObject.AddComponent<UIManager>();
        gameObject.AddComponent<YooAssetManager>();
    }

    /// <summary>
    /// 載入補充AOT
    /// </summary>
    private IEnumerator LoadAdditionalAssemblies()
    {
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in additionalAOT)
        {
            RawFileHandle handle = hotFixPackage.LoadRawFileAsync(aotDllName);
            yield return handle;
            rawHandleList.Add(handle);
            byte[] dllData = handle.GetRawFileData();
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllData, mode);
            Debug.Log($"{aotDllName}  補充AOT加載 : {err}");
        }
    }

    /// <summary>
    /// 載入熱更Dll
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadHotFixDll()
    {
        foreach (var dllName in HotFixDllName)
        {
            RawFileHandle handle = hotFixPackage.LoadRawFileAsync(dllName);
            yield return handle;
            rawHandleList.Add(handle);
            byte[] assemblyData = handle.GetRawFileData();
            var ass = Assembly.Load(assemblyData);

            assmblyList.Add(dllName, ass);
            Debug.Log($"已載入熱更Dll:{dllName}");
        }
    }

    /// <summary>
    /// 獲取腳本
    /// </summary>
    /// <param name="dllName">程序集名稱</param>
    /// <param name="csName">腳本名稱</param>
    /// <returns></returns>
    public Type GetAssemblyType(string dllName, string csName)
    {
        return assmblyList[dllName].GetType(csName);
    }

    private void OnDestroy()
    {
        foreach (var raw in rawHandleList)
        {
            raw.Release();
        }

        YooAssets.GetPackage(AssetsPackageName).UnloadUnusedAssets();
        YooAssets.GetPackage(HotFixPackageName).UnloadUnusedAssets();
        YooAssets.Destroy();
    }
}
