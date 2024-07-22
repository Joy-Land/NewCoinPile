using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Log
{
    public class GameLog
    {
        public class LogConfig
        {
            public static readonly float LogFileFlushInterval = 2.0f;
            public static readonly float LogFileFlushRate = 0.5f;
#if UNITY_EDITOR || THINRL_DEV
            public static readonly bool IsEnableLogFile = false;

            public static readonly bool OpenLogInfo = true;
            public static readonly bool OpenLogError = true;
            public static readonly bool OpenLogWarning = true;
            public static readonly bool OpenLogException = true;
#else
        //正式包模式开始手机日志 且只收集Error和Exception
        public static readonly bool IsEnableLogFile = true;

        public static readonly bool OpenLogInfo = false;
        public static readonly bool OpenLogError = true;
        public static readonly bool OpenLogWarning = false;
        public static readonly bool OpenLogException = true;
#endif
        }

        public static bool IsEnableDebugMode()
        {
#if UNITY_EDITOR || THINRL_DEV
            return true;
#else
        return false;
#endif
        }

        // 是否已初始化
        private static bool _is_inited = false;
        // 日志文件路径
        public static string _log_file_path = "Game.log";

        // 缓存空间
        private const int _cache_max_size = 2048;
        private static byte[] _cache_pool = null;
        private static int _cache_pool_size = 0;

        // 日志前缀，只用于写到文件时
        private static Dictionary<LogType, string> _logPrefix = new Dictionary<LogType, string>();
        // 上一次写入到文件的时间
        private static float _last_flush_ts = -1;

        public static bool StopLog = false;

        internal static bool isMandatoryOpenLogs = false;
        static GameLog()
        {
            _is_inited = false;
            _cache_pool_size = 0;
            _cache_pool = new byte[_cache_max_size];

            _logPrefix.Add(LogType.Error, "[Err]");
            _logPrefix.Add(LogType.Assert, "[Asrt]");
            _logPrefix.Add(LogType.Warning, "[Warn]");
            _logPrefix.Add(LogType.Log, "[Log]");
            _logPrefix.Add(LogType.Exception, "[Excp]");


        }

        //在GameEntry中注册
        public static void RegisterLog()
        {
            _log_file_path = CheckLogFile();

            Application.logMessageReceived += LogCallBack;
            _is_inited = true;

        }

        #region Log写入接口

        public static void Log(object message)
        {
            if (LogConfig.OpenLogInfo && !StopLog || isMandatoryOpenLogs)
            {
                UnityEngine.Debug.Log(message);
            }
        }
        internal static void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }


        public static void LogYellow(object message)
        {
            Log(string.Format("<color=yellow>{0}</color>", message));
        }
        internal static void LogYellowFormat(string format, params object[] args)
        {
            LogYellow(string.Format(format, args));
        }


        public static void LogWarning(object message)
        {
            if (LogConfig.OpenLogWarning && !StopLog || isMandatoryOpenLogs)
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }
        internal static void LogWarningFormat(string format, params object[] args)
        {
            LogWarning(string.Format(format, args));
        }


        public static void LogError(object message)
        {
            if (LogConfig.OpenLogError && !StopLog || isMandatoryOpenLogs)
            {
                UnityEngine.Debug.LogError(message);
            }
        }
        internal static void LogErrorFormat(string format, params object[] args)
        {
            LogError(string.Format(format, args));
        }


        public static void LogException(System.Exception e)
        {
            if (LogConfig.OpenLogException || isMandatoryOpenLogs)
            {
                Debug.LogException(e);
            }

            //编辑器和DEV包下将重新抛出异常
            if (IsEnableDebugMode())
            {
                throw e;
            }
        }

        #endregion

        #region 写入文件
        //获取log日志文件名
        private static string CheckLogFile()
        {
            string _path = string.Format("{0}/Log", Application.persistentDataPath);
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            var date = DateTime.Now;
            string fileNamePath = string.Format("GameLog {0}{1}{2}",
                date.Year,
                date.Month,
                date.Day);
            _path = string.Format("{0}/{1}", _path, fileNamePath);
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            fileNamePath = string.Format("{0}/{1}{2}{3}{4}{5}{6}.log",
                _path,
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                date.Second);
            return fileNamePath;
        }

        private static void CacheLog(string msg)
        {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(msg);

            // 如果缓存空间不足，立即刷新
            int bsOffset = 0;
            if (_cache_pool_size + bs.Length >= _cache_max_size)
            {
                while (bsOffset < bs.Length)
                {
                    int bsLength = Math.Min(bs.Length - bsOffset, _cache_max_size - _cache_pool_size);
                    Buffer.BlockCopy(bs, bsOffset, _cache_pool, _cache_pool_size, bsLength);
                    _cache_pool_size += bsLength;
                    bsOffset += bsLength;
                    if (!FlushToStream())
                    {
                        UnityEngine.Debug.LogError("CacheLog flush error.");
                        break;
                    }
                }
            }
            else
            {
                Buffer.BlockCopy(bs, 0, _cache_pool, _cache_pool_size, bs.Length);
                _cache_pool_size += bs.Length;
            }
        }

        // 刷新数据到文件 , 在GameEntry LateUpdate中调用
        public static bool FlushLog()
        {
            if (!_is_inited)
            {
                return false;
            }

            if (_cache_pool == null || _cache_pool_size <= 0)
            {
                return false;
            }
            try
            {
                bool isFlush = false;
                // 超过最小时间间隔时刷新
                float interval = Time.time - _last_flush_ts;
                if (interval >= LogConfig.LogFileFlushInterval)
                {
                    _last_flush_ts = Time.time;
                    isFlush = true;
                }
                // 缓存空间中字节数超过一定比率时刷新
                if (_cache_pool_size >= (_cache_max_size * LogConfig.LogFileFlushRate))
                {
                    _last_flush_ts = Time.time;
                    isFlush = true;
                }

                if (isFlush)
                {
                    return FlushToStream();
                }
            }
            catch (Exception ex)
            {
                _cache_pool_size = 0;
                UnityEngine.Debug.LogError("LogModule FlushLog error.");
            }
            return false;
        }
        private static bool FlushToStream()
        {
            try
            {
                if (!_is_inited)
                {
                    return false;
                }

                if (_cache_pool == null || _cache_pool_size <= 0)
                {
                    return false;
                }

                using (FileStream fs = new FileStream(_log_file_path, FileMode.Append, FileAccess.Write))
                {
                    fs.Write(_cache_pool, 0, _cache_pool_size);
                    fs.Flush();
                    _cache_pool_size = 0;
                }
                return true;
            }
            catch (Exception ex)
            {
                _cache_pool_size = 0;
                UnityEngine.Debug.LogError("LogModule FlushToStream error.");
            }
            return false;
        }

        #endregion

        #region 日志回调
        // 日志回调
        private static void LogCallBack(string msg, string stackTrace, LogType type)
        {
            // 写入文件
            if (LogConfig.IsEnableLogFile || isMandatoryOpenLogs)
            {
                CacheLog(FormatFileLog(type, msg, stackTrace));
            }
        }

        static StringBuilder formatLogBuilder = new StringBuilder(4096);

        private static string FormatFileLog(LogType type, string msg, string stackTrace)
        {
            string prefix = "[Log]";
            if (_logPrefix.ContainsKey(type))
            {
                prefix = _logPrefix[type];
            }

            formatLogBuilder.Clear();
            formatLogBuilder.Append(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"));
            formatLogBuilder.Append(prefix);
            if (type == LogType.Log)
            {
                formatLogBuilder.AppendLine(msg);
            }
            else
            {
                formatLogBuilder.AppendLine(msg);
                formatLogBuilder.AppendLine(stackTrace);
            }
            return formatLogBuilder.ToString();
        }
        #endregion
    }
}