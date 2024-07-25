using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace ThinRL.Core.Tools
{
    /// <summary>
    /// 使用userkey相关接口时，需要在登录成功取得user唯一id后进行设置
    /// </summary>
    public static class PlayerPrefsManager
    {
        private static string GetUserKey(string key)
        {
            return PlayerPrefs.GetString(PlayerPrefHelper.UserID, "") + key;
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static void SetUserInt(string key, int value)
        {
            PlayerPrefs.SetInt(GetUserKey(key), value);
        }
        public static int GetInt(string key)
        {
            return PlayerPrefs.GetInt(key);
        }
        public static int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
        public static int GetUserInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(GetUserKey(key), defaultValue);
        }

        //bool
        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static void SetUserBool(string key, bool value)
        {
            PlayerPrefs.SetInt(GetUserKey(key), value ? 1 : 0);
        }
        public static bool GetBool(string key)
        {
            return PlayerPrefs.GetInt(key) == 1 ? true : false;
        }
        public static bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue? 1 : 0) == 1 ? true : false;
        }
        public static bool GetUserBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(GetUserKey(key), defaultValue ? 1 : 0) == 1 ? true : false;
        }

        //float
        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
        public static void SetUserFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(GetUserKey(key), value);
        }
        public static float GetFloat(string key)
        {
            return PlayerPrefs.GetFloat(key);
        }
        public static float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }
        public static float GetUserFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(GetUserKey(key), defaultValue);
        }

        //string
        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static void SetUserString(string key, string value)
        {
            PlayerPrefs.SetString(GetUserKey(key), value);
        }

        public static string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }
        public static string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
        public static string GetUserString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(GetUserKey(key), defaultValue);
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void ClearAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Clear(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}