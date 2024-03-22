using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;
using System.Reflection;
using System;
using HybridCLR;

public class Entry : MonoBehaviour
{
    private static Entry _instance;
    public static Entry Instance { get { return _instance; } }

    private const string remotePath = "http://192.168.1.172:8080/Solitaire/";
    private const string assetsPackageName = "AssetsPackage";
    private const string hotFixPackageName = "HotFixPackage";

    private const float rotationSpeed = 180;

    private ResourcePackage assetsPackage;
    private ResourcePackage hotFixPackage;

    private int currSpriteIndex;
    private float lastYRotation = 90;

    [SerializeField]
    private bool isShowDebug;

    [SerializeField]
    private GameObject debugTool, tip_Obj;
    [SerializeField]
    private Sprite[] rotationSprite;
    [SerializeField]
    private Image pocker_Img;
    [SerializeField]
    private Text update_Txt, tip_Txt;
    [SerializeField]
    private Button tipConfirm_Btn;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        debugTool.SetActive(isShowDebug);
        tip_Obj.SetActive(false);
        pocker_Img.sprite = rotationSprite[currSpriteIndex];
    }

    private void Start()
    {
        StartCoroutine(ILoad());
    }

    private void Update()
    {
        //撲克旋轉
        pocker_Img.transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
        lastYRotation += rotationSpeed * Time.deltaTime;
        if (lastYRotation >= 180)
        {
            lastYRotation = 0;
            currSpriteIndex = (currSpriteIndex + 1) % rotationSprite.Length;
            pocker_Img.sprite = rotationSprite[currSpriteIndex];
        }
    }

    /// <summary>
    /// 加載資源
    /// </summary>
    /// <returns></returns>
    private IEnumerator ILoad()
    {
        YooAssets.Initialize();

        //熱更包
        hotFixPackage = YooAssets.CreatePackage(hotFixPackageName);
        yield return ILoadYooAsset(hotFixPackage, hotFixPackageName);
        yield return IPatchDownload(hotFixPackageName);
        yield return LoadAOTAssemblies();

        //資源包
        assetsPackage = YooAssets.CreatePackage(assetsPackageName);
        yield return ILoadYooAsset(assetsPackage, assetsPackageName);
        yield return IPatchDownload(assetsPackageName);

        update_Txt.text = "更新完成。";

        //啟動腳本
        RawFileHandle handle = hotFixPackage.LoadRawFileAsync("HotFixDefinition.dll");
        yield return handle;
        byte[] loadDllData = handle.GetRawFileData();
        var ass = Assembly.Load(loadDllData);
        Type type = ass.GetType("LauncherManager");
        GameObject go = new GameObject("LauncherManager");
        go.AddComponent(type);
    }

    /// <summary>
    /// 載入AOT
    /// </summary>
    private IEnumerator LoadAOTAssemblies()
    {
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
        {
            RawFileHandle handle = hotFixPackage.LoadRawFileAsync(aotDllName);
            yield return handle;
            byte[] dllData = handle.GetRawFileData();
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllData, mode);
            Debug.Log($"{aotDllName}  AOT加載 : {err}");
        }
    }

    /// <summary>
    /// 載入資源
    /// </summary>
    /// <param name="package"></param>
    /// <param name="packageName"></param>
    /// <param name="remotePath"></param>
    /// <returns></returns>
    private IEnumerator ILoadYooAsset(ResourcePackage package, string packageName)
    {
        string defaultHostServer = $"{remotePath}{packageName}";
        string fallbackHostServer = $"{remotePath}{packageName}";

        var initParameters = new HostPlayModeParameters();
        initParameters.BuildinQueryServices = new GameQueryServices();
        initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
        InitializationOperation initOperation = package.InitializeAsync(initParameters);

        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"【{packageName}】資源包初始化成功。");
            update_Txt.text = $"【{packageName}】資源包初始化成功。";
        }
        else
        {
            Debug.LogError($"【{packageName}】資源包初始化成功失敗 : {initOperation.Error}");
            OpenTipView($"【{packageName}】資源包初始化成功失敗!!!");
        }

        //獲取資源版本
        var operation = package.UpdatePackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            //更新失败
            Debug.LogError(operation.Error);
            OpenTipView("熱更新失敗");
            yield break;
        }

        //更新成功
        string packageVersion = operation.PackageVersion;
        Debug.Log($"【{packageName}】更新版本 : {packageVersion}");
        update_Txt.text = $"【{packageName}】更新版本 : {packageVersion}";

        //更新補丁清單
        var operation2 = package.UpdatePackageManifestAsync(packageVersion, true);
        yield return operation2;

        if (operation2.Status != EOperationStatus.Succeed)
        {
            //更新失败
            Debug.LogError(operation.Error);
            OpenTipView("熱更新失敗");
            yield break;
        }
    }

    /// <summary>
    /// 補丁包下載
    /// </summary>
    /// <returns></returns>
    private IEnumerator IPatchDownload(string packageName)
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var package = YooAssets.GetPackage(packageName);
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            yield break;
        }

        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        //注册回调方法
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //开启下载
        downloader.BeginDownload();
        yield return downloader;

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //下载成功
            Debug.Log($"{packageName}:補丁包更新完成");
            update_Txt.text = $"{packageName}:補丁包更新完成";
        }
        else
        {
            //下载失败
            Debug.Log($"{packageName}:補丁包更新失敗");
            OpenTipView($"{packageName}:補丁包更新失敗");
        }
    }

    /// <summary>
    /// 開始下載
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log($"開始下載 : {fileName}，文件大小 : {sizeBytes}");
        update_Txt.text = $"開始下載 : {fileName}，文件大小 : {sizeBytes}";
    }

    /// <summary>
    /// 下載完成
    /// </summary>
    /// <param name="isSucceed"></param>
    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("下載" + (isSucceed ? "成功" : "失敗"));
        update_Txt.text = "下載" + (isSucceed ? "成功" : "失敗");
    }

    /// <summary>
    /// 更新中
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        Debug.Log($"文件總數 : {totalDownloadCount}， 已下載文件數 : {currentDownloadCount}， 下載總大小 : {totalDownloadBytes}， 已下載大小 : {currentDownloadBytes}");
        update_Txt.text = $"文件總數 : {totalDownloadCount}， 已下載文件數 : {currentDownloadCount}， 下載總大小 : {totalDownloadBytes}， 已下載大小 : {currentDownloadBytes}";
    }

    /// <summary>
    /// 下載出錯
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.LogError($"下載出錯! 文件名:{fileName}，錯誤訊息 : {error}");
        update_Txt.text = $"下載出錯! 文件名:{fileName}，錯誤訊息 : {error}";
        OpenTipView("下載出錯!!!");
    }

    /// <summary>
    /// 開啟提示
    /// </summary>
    /// <param name="conent"></param>
    private void OpenTipView(string content)
    {
        StopAllCoroutines();
        YooAssets.DestroyPackage(hotFixPackageName);
        YooAssets.DestroyPackage(assetsPackageName);
        YooAssets.Destroy();

        tip_Obj.SetActive(true);
        pocker_Img.gameObject.SetActive(false);
        tip_Txt.text = content;
        tipConfirm_Btn.onClick.AddListener(() =>
        {
            StartCoroutine(ILoad());
            tip_Obj.SetActive(false);
            pocker_Img.gameObject.SetActive(true);
        });
    }
}
