using System;
using UnityEngine;

namespace ThinRL.Core
{
    public static class ExtensionUtil
    {
        // 有则返回，没有则创建新组件
        public static T AddComponentIfNotExist<T>(this GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c == null)
                c = go.AddComponent<T>();
            return c;
        }
        public static T AddOrReplaceComponent<T>(this GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c != null)
                GameObject.DestroyImmediate(c);
            else
                c = go.AddComponent<T>();
            return c;
        }

        /// <summary>
        /// 时间转时间戳
        /// </summary>
        /// <param name="_dataTime">时间</param>
        /// <param name="MilliTime">毫秒计时</param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime _dataTime, bool Millisecond = true)
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(ID));
            DateTime NowTime = TimeZoneInfo.ConvertTime(_dataTime, TimeZoneInfo.FindSystemTimeZoneById(ID));
            long timeStamp;
            if (Millisecond)
                timeStamp = (long)(NowTime - startTime).TotalMilliseconds; // 相差毫秒数
            else
                timeStamp = (long)(NowTime - startTime).TotalSeconds; // 相差秒数
            return timeStamp;
        }

        public static DateTime ToDateTime(this long stamp, bool Millisecond = true)
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(ID));
            DateTime dt;
            if (Millisecond)
                dt = startTime.AddMilliseconds(stamp);
            else
                dt = startTime.AddSeconds(stamp);
            Console.WriteLine(dt.ToString("yyyy/MM/dd HH:mm:ss:fff"));
            return dt;
        }

    }
}