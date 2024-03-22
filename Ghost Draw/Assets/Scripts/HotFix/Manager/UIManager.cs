using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : UnitySingleton<UIManager>
{
    private Transform canvas;
    private Transform toolCanvas;

    public override void Awake()
    {
        base.Awake();
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
}
