using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class DllHandle : MonoBehaviour
{
    private static readonly List<string> dllList = new List<string>()
    {
        "Assembly-CSharp.dll",
        "HotFixDefinition.dll",
    };

    private static readonly string targetFolderPath = $"Assets/HotFixAssets/HotFix";

    [MenuItem("MyTool/DllHandle")]
    private static void CopyDll()
    {
        // 移除每個檔案
        string[] files = Directory.GetFiles(targetFolderPath, "*.bytes");
        foreach (string file in files)
        {
            File.Delete(file);
        }
        
        foreach (var dll in dllList)
        {
            string sourceFilePath = $"E:/MyUnityProject/Solitaire Fild/Solitaire/HybridCLRData/HotUpdateDlls/Android/{dll}";
            
            string targetFilePath = Path.Combine(targetFolderPath, dll);

            // 複製文件
            if (File.Exists(sourceFilePath))
            {
                File.Copy(sourceFilePath, targetFilePath, true);

                // 更改檔名
                if (File.Exists(targetFilePath))
                {
                    string newTargetFileName = $"{dll}.bytes";
                    string newTargetFilePath = Path.Combine(targetFolderPath, newTargetFileName);
                    File.Move(targetFilePath, newTargetFilePath);
                }
                else
                {
                    Debug.LogError($"{dll}:目標文件不存在，檔名更改失敗。");
                }
            }
            else
            {
                Debug.LogError($"{dll}源文件不存在，複製失敗。");
            }

            Debug.Log($"{dll} 已複製。");
        }

        Debug.Log("Dll複製完成。");
    }
}
