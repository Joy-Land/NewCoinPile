using System;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using Newtonsoft.Json;

namespace Joyland.GamePlay
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel : byte
    {
        /// <summary>
        /// 信息级别
        /// </summary>
        Info,

        /// <summary>
        /// 警告级别
        /// </summary>
        Warn,

        /// <summary>
        /// 错误级别
        /// </summary>
        Error
    }

    public class console
    {
        /// <summary>
        /// 日志级别(默认Info)
        /// </summary>
        public static LogLevel LogLevel = LogLevel.Error;
        /// <summary>
        /// 是否使用Unity打印
        /// </summary>
        public static bool UseUnityEngine = true;
        /// <summary>
        /// 是否显示时间
        /// </summary>
        public static bool EnableTime = false;
        /// <summary>
        /// 是否显示堆栈信息
        /// </summary>
        public static bool EnableStack = false;
        /// <summary>
        /// 是否保存到文本
        /// </summary>
        public static bool EnableSave = false;
        /// <summary>
        /// 打印文本流
        /// </summary>
        public static StreamWriter LogFileWriter = null;
        /// <summary>
        /// 日志保存路径(文件夹)
        /// </summary>
        public static string LogFileDir = "";
        /// <summary>
        /// 日志文件名
        /// </summary>
        public static string LogFileName = "";

        //打印格式: {0}-时间 {1}-标签/类名/TAGNAME字段值 {2}-内容
        private static string InfoFormat = "<color=#00FF80>[Info] {0}<color=#00BFFF>{1}</color> {2}</color>";
        private static string WarnFormat = "<color=#FFFF00>[Warn] {0}<color=#00BFFF>{1}</color> {2}</color>";
        private static string ErrorFormat = "<color=#FF0000>[Error] {0}<color=#00BFFF>{1}</color> {2}</color>";

        private static void Internal_Log(string msg, object context = null)
        {
            bool useUnityEngine = UseUnityEngine;
            if (useUnityEngine)
            {
                UnityEngine.Debug.Log(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        private static void Internal_Warn(string msg, object context = null)
        {
            bool useUnityEngine = UseUnityEngine;
            if (useUnityEngine)
            {
                UnityEngine.Debug.LogWarning(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        private static void Internal_Error(string msg, object context = null)
        {
            bool useUnityEngine = UseUnityEngine;
            if (useUnityEngine)
            {
                UnityEngine.Debug.LogError(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        #region Info
        [Conditional("JOYLAND_DEBUG")]
        public static void info(params object[] args)
        {
            if (LogLevel >= LogLevel.Error)
            {
                string msg = string.Empty;

                // 循环处理每个参数
                foreach (var arg in args)
                {
                    if (arg == null)
                    {
                        msg += "null ";
                    }
                    else if (arg.GetType().IsPrimitive || arg is string || arg is char || arg is decimal)
                    {
                        // 基本数据类型，直接转换为字符串
                        msg += arg.ToString() + " ";
                    }
                    else
                    {
                        // 对象类型，转换为 JSON 字符串
                        try
                        {
                            msg += JsonConvert.SerializeObject(arg, Formatting.Indented) + " ";
                        }
                        catch
                        {
                            msg += arg.ToString() + " ";
                        }
                    }
                }
                Internal_Log(msg, null);
                WriteToFile(msg, true);
            }
        }
        #endregion

        #region Warn
        [Conditional("JOYLAND_DEBUG")]
        public static void warn(params object[] args)
        {
            if (LogLevel >= LogLevel.Error)
            {
                string msg = string.Empty;

                // 循环处理每个参数
                foreach (var arg in args)
                {
                    if (arg == null)
                    {
                        msg += "null ";
                    }
                    else if (arg.GetType().IsPrimitive || arg is string || arg is char || arg is decimal)
                    {
                        // 基本数据类型，直接转换为字符串
                        msg += arg.ToString() + " ";
                    }
                    else
                    {
                        // 对象类型，转换为 JSON 字符串
                        try
                        {
                            msg += JsonConvert.SerializeObject(arg, Formatting.Indented) + " ";
                        }
                        catch
                        {
                            msg += arg.ToString() + " ";
                        }
                    }
                }
                Internal_Warn(msg, null);
                WriteToFile(msg, true);
            }
        }
        #endregion

        #region Error
        [Conditional("JOYLAND_DEBUG")]
        public static void error(params object[] args)
        {
            if (LogLevel >= LogLevel.Error)
            {
                string msg = string.Empty;

                // 循环处理每个参数
                foreach (var arg in args)
                {
                    if (arg == null)
                    {
                        msg += "null ";
                    }
                    else if (arg.GetType().IsPrimitive || arg is string || arg is char || arg is decimal)
                    {
                        // 基本数据类型，直接转换为字符串
                        msg += arg.ToString() + " ";
                    }
                    else
                    {
                        // 对象类型，转换为 JSON 字符串
                        try
                        {
                            msg += JsonConvert.SerializeObject(arg, Formatting.Indented) + " ";
                        }
                        catch
                        {
                            msg += arg.ToString() + " ";
                        }
                    }
                }
                Internal_Error(msg, null);
                WriteToFile(msg, true);
            }
        }

        #endregion

        /// <summary>
        /// 获取时间
        /// </summary>
        /// <returns></returns>
        private static string GetLogTime()
        {
            string result = "";
            if (EnableTime)
            {
                result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " ";
            }
            return result;
        }

        /// <summary>
        /// 序列化打印信息
        /// </summary>
        /// <param name="message">打印信息</param>
        /// <param name="EnableStack">是否开启堆栈打印</param>
        private static void WriteToFile(string message, bool EnableStack = false)
        {
            bool flag = !EnableSave;
            return;
        }
    }
}
