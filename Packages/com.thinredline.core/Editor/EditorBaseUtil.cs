using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace ThinRL.Core.Editor
{
    /// <summary>
    /// 为编辑器下的相关公共接口
    /// </summary>
    public class BaseEditorUtil
    {
        //是否为贴图
        public static bool IsTexture(string assetName)
        {
            return assetName.ToLower().EndsWith(".png") || assetName.ToLower().EndsWith(".jpg")
                || assetName.ToLower().EndsWith(".tga");
        }

        //拼接路径为Assets开头的全路径
        public static string GetRelativeFilePath(string fullFilePath)
        {
            StringBuilder filePath = new StringBuilder();
            filePath.Append("Assets").
                    Append(Path.GetFullPath(fullFilePath).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/'));
            return filePath.ToString();
        }

        //是否为meta文件
        public static bool IsMeta(string assetName)
        {
            return assetName.ToLower().EndsWith(".meta");
        }

        //是否为cs script

        public static bool IsCSharpScript(string assetName)
        {
            return assetName.ToLower().EndsWith(".cs");
        }

        //是否为shader文件
        public static bool IsShaderFile(string assetName)
        {
            return assetName.ToLower().EndsWith(".shader")
            || assetName.ToLower().EndsWith(".compute");
        }

        //是否为dll文件
        public static bool IsDllFile(string assetName)
        {
            return assetName.ToLower().EndsWith(".dll");
        }

        //是否为图集文件
        public static bool IsSpriteAtlas(string assetName)
        {
            return assetName.ToLower().EndsWith(".spriteatlas");
        }
        static public string[] AssetGUID2Path(string[] guids)
        {
            List<string> pathList = new List<string>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                pathList.Add(path);
            }
            return pathList.ToArray();
        }

        static public string GetScripableObjectPath<T>() where T : ScriptableObject
        {
            var obj = ScriptableObject.CreateInstance<T>();

            MonoScript script = MonoScript.FromScriptableObject(obj);
            UnityEngine.Object.DestroyImmediate(obj);

            if (script == null)
            {
                Debug.Log("无法定义ScriptableObject " + typeof(T).Name + "类");
                return "";
            }
            else
            {
                return AssetDatabase.GetAssetPath(script);
            }

        }

        static public string GetMonoBehaviourPath<T>() where T : MonoBehaviour
        {
            var go = new GameObject("temp");
            var cpnt = go.AddComponent<T>();

            MonoScript script = MonoScript.FromMonoBehaviour(cpnt);
            GameObject.DestroyImmediate(go);

            if (script == null)
            {
                Debug.Log("无法定义MonoBehaviour " + typeof(T).Name + "类");
                return "";
            }
            else
            {
                return AssetDatabase.GetAssetPath(script);
            }

        }

        /// <summary>
        /// 准备需要文件夹
        /// </summary>
        /// <param name="targetFilePath"></param>
        public static void PrepareDirectory(string targetFilePath)
        {
            if (AssetDatabase.IsValidFolder(targetFilePath))
            {
                return;
            }

            string[] pathList = targetFilePath.Split('/');
            var upperPath = pathList[0];
            string validPath = pathList[0] + "/" + pathList[1];
            var newFolderName = pathList[2];

            //方法本身效率不高，如果前缀路径固定比较好
            for (int i = 1; i < pathList.Length - 1; i++)
            {
                if (!AssetDatabase.IsValidFolder(validPath))
                {
                    AssetDatabase.CreateFolder(upperPath, newFolderName);
                    AssetDatabase.Refresh();
                    AssetDatabase.ImportAsset(validPath);   
                }
                upperPath += "/" + pathList[i];
                validPath += "/" + pathList[i + 1];
                newFolderName = pathList[i + 1];
            }
        }

        public static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
    }
}
