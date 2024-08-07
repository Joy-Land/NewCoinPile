using UnityEngine;
using System.Security.Cryptography;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ThinRL.Core;
using Joyland.GamePlay;
using DG.Tweening;


namespace Joyland.GamePlay
{
    public static class CommonUtil
    {
        public static void SetImageWithAsync(this UnityEngine.UI.Image image, string atlasName, string spriteName, string packageName = "DefaultPackage")
        {
            var package = YooAsset.YooAssets.GetPackage(packageName);
            package.LoadSubAssetsAsync<Sprite>(atlasName).Completed += (handle) =>
            {
                var target = handle.GetSubAssetObject<Sprite>(spriteName);
                image.sprite = target;
            };
        }

        public static void SetMaterialWithAsync(this UnityEngine.UI.Image image, string materialName, string packageName = "DefaultPackage")
        {
            var package = YooAsset.YooAssets.GetPackage(packageName);
            package.LoadAssetAsync<Material>(materialName).Completed += (handle) =>
            {
                var target = handle.AssetObject as Material;
                image.material = target;
            };
        }

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

            public static string ConvertToChineseCurrencyUnit(long number)
            {
                if (number == 0)
                {
                    return "零";
                }

                string[] units = { "", "万", "十万", "百万", "千万", "亿", "十亿", "百亿", "千亿" };
                int unitIndex = 0;

                while (number >= 10000 && unitIndex < units.Length)
                {
                    number /= 10000;
                    unitIndex++;
                }

                if (unitIndex == units.Length)
                {
                    return "千亿以上";
                }

                return number + units[unitIndex];
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

            public static string GenerateRandomCodeWithTimestamp()
            {
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(); // 获取当前时间戳（毫秒）
                string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                string randomCode = "";

                // 生成随机字符部分
                System.Random random = new System.Random();
                for (int i = 0; i < 10; i++)
                {
                    int randomIndex = random.Next(characters.Length);
                    randomCode += characters[randomIndex];
                }

                // 将时间戳转换为指定格式并加入随机码中
                string timestampStr = timestamp.ToString();
                randomCode += timestampStr.Substring(timestampStr.Length - 6); // 添加时间戳后 6 位

                return randomCode;
            }
        }

        public static class Assets
        {

        }

        public static class Tween
        {
            public static void DoNumberFloat(float s, float e, Action<float> updateCB, Action finishCB, float duration = 1f)
            {
                DOTween.To(value => { updateCB?.Invoke(value); }, startValue: s, endValue: e, duration: duration).OnComplete(()=>
                {
                    finishCB?.Invoke();
                });
            }

            public static void DoNumberInt(int s, int e, Action<int> updateCB, Action finishCB, float duration = 1f)
            {
                DOTween.To(value => { updateCB?.Invoke(Mathf.FloorToInt(value)); }, startValue: s, endValue: e, duration: duration).OnComplete(()=>
                {
                    finishCB?.Invoke();
                });
            }
        }


    }

}