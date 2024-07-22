using UnityEngine;
using System.Security.Cryptography;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ThinRL.Core
{
    public static class CommonUtil
    {
        public static class Common
        {
            /// <summary>
            /// 判断字符串是否符合md5规则
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static bool IsMatchMd5Rule(string str)
            {
                // md5规则 从开头到结尾长32个字符，字符串为16进行 0到9 a到f
                var match = Regex.Match(str, @"^[a-f0-9]{32}$");

                return match.Success;
            }


            public struct TimeFormat
            {
                public int day;
                public int hour;
                public int min;
                public int sec;
            }

            public static TimeFormat GetTimeFormat(int totalSec)
            {
                if (totalSec <= 0)
                    return default(TimeFormat);
                TimeFormat obj = new TimeFormat();
                obj.day = totalSec / 86400;
                obj.hour = (totalSec % 86400) / 3600;
                obj.min = (totalSec % 3600) / 60;
                obj.sec = totalSec % 60;
                return obj;
            }


            public static DateTime GetNowTime()
            {
                string ID = TimeZoneInfo.Local.Id;
                DateTime NowTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(ID));
                return NowTime;
            }



            public static string GetCurrentDate()
            {
                long timeStamp = System.DateTime.Now.ToTimestamp();
                DateTime dateTime = timeStamp.ToDateTime();

                return dateTime.ToString("yyyy/MM/dd");
            }

            public static string GetCurrentDateMilliseconds()
            {
                long timeStamp = System.DateTime.Now.ToTimestamp();
                DateTime dateTime = timeStamp.ToDateTime();
                return dateTime.ToString("yyyy/MM/dd HH:mm:ss:fff");
            }



            [ThreadStatic]
            private static System.Text.StringBuilder s_CachedStringBuilder = new System.Text.StringBuilder(1024);
            public static string FormatString(string str, params object[] args)
            {
                if (str == null)
                {
                    UnityEngine.Debug.Log("str is Null,please check it");
                }
                s_CachedStringBuilder.Length = 0;
                s_CachedStringBuilder.AppendFormat(str, args);
                return s_CachedStringBuilder.ToString();
            }

            public static string[] UnpackDate(string data)
            {
                string[] unpackedArray = data.Split('$');
                return unpackedArray;
            }

            public static string PackData(params object[] args)
            {
                string packedString = string.Join("$", args);
                return packedString;
            }
        }

        public static class Random
        {
            public static List<T> ShuffleArray<T>(T[] array)
            {
                List<T> shuffled = new List<T>(array);
                int currentIndex = shuffled.Count - 1;

                while (currentIndex != 0)
                {
                    // const randomIndex = Math.floor(this.GetRandom() * currentIndex);
                    int randomIndex = UnityEngine.Random.Range(0, currentIndex);
                    currentIndex--;
                    (shuffled[currentIndex], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[currentIndex]);
                }

                return shuffled;
            }
        }


    }

}