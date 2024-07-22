using System.Reflection;
using System.Diagnostics;

namespace Joyland.GamePlay
{
    /// <summary>
    /// 自定义Debuger类的扩展类
    /// </summary>
    public static class DebugerExtension
    {
        [Conditional("JOYLAND_DEBUG")]
        public static void info(this object obj, string message)
        {
            if (console.LogLevel >= LogLevel.Info)
            {
                console.info(GetLogTag(obj), message);
            }
        }

        [Conditional("JOYLAND_DEBUG")]
        public static void info(this object obj, string format, params object[] args)
        {
            if (console.LogLevel >= LogLevel.Info)
            {
                string message = string.Format(format, args);
                console.info(GetLogTag(obj), message);
            }
        }

        [Conditional("JOYLAND_DEBUG")]
        public static void warn(this object obj, string message)
        {
            if (console.LogLevel >= LogLevel.Warn)
            {
                console.warn(GetLogTag(obj), message);
            }
        }

        [Conditional("JOYLAND_DEBUG")]
        public static void warn(this object obj, string format, params object[] args)
        {
            if (console.LogLevel >= LogLevel.Warn)
            {
                string message = string.Format(format, args);
                console.warn(GetLogTag(obj), message);
            }
        }

        [Conditional("JOYLAND_DEBUG")]
        public static void error(this object obj, string message)
        {
            if (console.LogLevel >= LogLevel.Error)
            {
                console.error(GetLogTag(obj), message);
            }
        }

        [Conditional("JOYLAND_DEBUG")]
        public static void error(this object obj, string format, params object[] args)
        {
            if (console.LogLevel >= LogLevel.Error)
            {
                string message = string.Format(format, args);
                console.error(GetLogTag(obj), message);
            }
        }
        /// <summary>
        /// 获取调用打印的类名称或者标记有TAGNAME的字段
        /// 有TAGNAME字段的，触发类名称用TAGNAME字段对应的赋值
        /// 没有用类的名称代替
        /// </summary>
        /// <param name="obj">触发Log对应的类</param>
        /// <returns></returns>
        private static string GetLogTag(object obj)
        {
            FieldInfo field = obj.GetType().GetField("TAGNAME");
            bool flag = field != null;
            string result;
            if (flag)
            {
                result = (string)field.GetValue(obj);
            }
            else
            {
                result = obj.GetType().Name;
            }
            return result;
        }
    }
}
