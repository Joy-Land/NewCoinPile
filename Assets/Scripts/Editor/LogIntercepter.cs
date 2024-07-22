using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace M.Editor
{
    /// <summary>
    /// 日志拦截器
    /// </summary>
    internal sealed class LogIntercepter
    {
        private static LogIntercepter _current;
        private static LogIntercepter Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new LogIntercepter();
                }
                return _current;
            }
        }

        private Type _consoleWindowType;
        private FieldInfo _activeTextInfo;
        private FieldInfo _consoleWindowInfo;
        private MethodInfo _setActiveEntry;
        private object[] _setActiveEntryArgs;
        private object _consoleWindow;

        private LogIntercepter()
        {
            _consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
            _activeTextInfo = _consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            _consoleWindowInfo = _consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            _setActiveEntry = _consoleWindowType.GetMethod("SetActiveEntry", BindingFlags.Instance | BindingFlags.NonPublic);
            _setActiveEntryArgs = new object[] { null };
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            UnityEngine.Object instance = EditorUtility.InstanceIDToObject(instanceID);
            if (AssetDatabase.GetAssetOrScenePath(instance).EndsWith(".cs"))
            {
                return Current.OpenAsset();//双击会触发这里
            }
            return false;
        }

        private bool OpenAsset()
        {
            string stackTrace = GetStackTrace();
            if (stackTrace != "")
            {
                //结合条件  可以用来过滤是否使用了原始接口还是自定义的
                if (stackTrace.Contains("[Info]") || stackTrace.Contains("[Warn]") || stackTrace.Contains("[Error]"))
                {
                    string[] paths = stackTrace.Split('\n');

                    for (int i = 0; i < paths.Length; i++)
                    {
                        //过滤日志封装类和日志扩展类
                        if (!paths[i].Contains("Debuger.cs") && !paths[i].Contains("DebugerExtension.cs") && paths[i].Contains(" (at "))
                        {
                            return OpenScriptAsset(paths[i]);
                        }
                    }
                }
            }
            return false;
        }

        private bool OpenScriptAsset(string path)
        {
            int startIndex = path.IndexOf(" (at ") + 5;
            int endIndex = path.IndexOf(".cs:") + 3;
            string filePath = path.Substring(startIndex, endIndex - startIndex);
            string lineStr = path.Substring(endIndex + 1, path.Length - endIndex - 2);
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (asset != null)
            {
                int line;
                if (int.TryParse(lineStr, out line))
                {
                    object consoleWindow = GetConsoleWindow();
                    _setActiveEntry.Invoke(consoleWindow, _setActiveEntryArgs);

                    EditorGUIUtility.PingObject(asset);
                    AssetDatabase.OpenAsset(asset, line);
                    return true;
                }
            }
            return false;
        }

        private string GetStackTrace()
        {
            object consoleWindow = GetConsoleWindow();

            if (consoleWindow != null)
            {
                if (consoleWindow == EditorWindow.focusedWindow as object)
                {
                    object value = _activeTextInfo.GetValue(consoleWindow);
                    return value != null ? value.ToString() : "";
                }
            }
            return "";
        }

        private object GetConsoleWindow()
        {
            if (_consoleWindow == null)
            {
                _consoleWindow = _consoleWindowInfo.GetValue(null);
            }
            return _consoleWindow;
        }
    }
}
