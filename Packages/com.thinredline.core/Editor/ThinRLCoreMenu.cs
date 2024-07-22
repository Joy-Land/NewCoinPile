using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public static class ThinRLCoreMenu
{
    
    [MenuItem ("ThinRedLine/Game Data Management/Clear Preferences")]
    static void ClearPreferences () 
    {
        PlayerPrefs.DeleteAll();
    }
    
    
    [MenuItem("ThinRedLine/生成安卓Gradle和Manifest模板", false, 1)]
    public static void CopyGradleAndManifestTemplate()
    {
        string dstFullPath = Path.GetFullPath("Assets/Plugins/Android");
        bool dirExist = Directory.Exists(dstFullPath);
        
        if (!dirExist)
        {
            Directory.CreateDirectory(dstFullPath);
        }

        string[] copyFileName = new[] {"AndroidManifest.xml", "mainTemplate.gradle", "launcherTemplate.gradle"};
        string[] srcPath = new string[copyFileName.Length];
        string[] dstPath = new string[copyFileName.Length];        
        string allExistFile = "";
        for (int i = 0; i < copyFileName.Length; i++)
        {
            srcPath[i] = Path.GetFullPath("Packages/orcas.core/Editor/Template/" + copyFileName[i]);
            dstPath[i] = Path.GetFullPath("Assets/Plugins/Android/" + copyFileName[i]);
            allExistFile += (copyFileName[i] + ", ");
        }

        if (allExistFile.Length > 0)
        {
            string msg = "当前Plugins/Android目录下面存在" + allExistFile + "操作将会覆盖文件。\n是否确定要覆盖文件？";
            if (EditorUtility.DisplayDialog("警告", msg, "确定", "取消"))
            {
                for (int i = 0; i < srcPath.Length; i++)
                {
                    File.Copy(srcPath[i], dstPath[i], true);    
                }
                AssetDatabase.Refresh();
                Debug.Log("覆盖Gradle和Manifest完成");
            }
        }
        else
        {
            for (int i = 0; i < srcPath.Length; i++)
            {
                File.Copy(srcPath[i], dstPath[i], true);    
            }
            AssetDatabase.Refresh();
            Debug.Log("拷贝Gradle和Manifest完成");
        }
    }
}
