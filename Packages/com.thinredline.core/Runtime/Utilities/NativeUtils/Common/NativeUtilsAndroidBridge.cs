using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ThinRL.Core.Tools;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    public class NativeUtilsAndroidBridge : INativeUtilsBridge
    {
        //private AndroidJavaObject utilJavaObject;

        private void LazyInit()
        {
            //var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            //var currentContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            //AndroidJavaClass androidClass = new AndroidJavaClass("com.orcas.core.NativeUtils");
            //utilJavaObject = androidClass.CallStatic<AndroidJavaObject>("getNativeUtilsImpl", currentContext, currentActivity);
        }

        public void Vibrate(long duration)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            //LazyInit();
            //utilJavaObject.Call ("performVibrate", duration);
#endif
        }

        public string GetDefaultLanguage()
        {
//#if UNITY_ANDROID && !UNITY_EDITOR
//            return GetDefaultLanguage_Android();
//#else
//            return "Editor_English";
//#endif
            return "Editor_English";
        }

        public void SendEmail(string addr, string title, string message)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            SendAndroidEmail(addr, title, message);
#endif
        }

        public string GetDeviceIdfa()
        {
            return "";
        }

        public void SaveDeviceCode(string dcode)
        {

        }

        public string GetDeviceCode()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public string GetUdid()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public void RequestAppTrackingAuthorization()
        {

        }

        public bool CheckIsDarkMode()
        {
//#if UNITY_ANDROID && !UNITY_EDITOR
//            LazyInit();
//            return utilJavaObject.Call<bool>("isDarkMode");
//#endif
            return false;
        }

        private string GetDefaultLanguage_Android()
        {
            string lan = "en";
            //try
            //{
            //    AndroidJavaClass localeClass = new AndroidJavaClass("java.util.Locale");
            //    AndroidJavaObject locale = localeClass.CallStatic<AndroidJavaObject>("getDefault");
            //    lan = locale.Call<string>("getLanguage");//.eg German:de_DE
            //}
            //catch (Exception ex)
            //{
            //    Debug.LogError(ex.Message);
            //}
            //if (lan == null)
            //{
            //    lan = "en";
            //}
            return lan;
        }

        private void SendAndroidEmail(string addr, string title, string message)
        {
            //AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            //AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            //intentObject = intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SENDTO"));
            //AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            //AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "mailto: " + addr);
            //intentObject = intentObject.Call<AndroidJavaObject>("setData", uriObject);
            //intentObject = intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), title);
            //intentObject = intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), message);
            //AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            //currentActivity.Call("startActivity", intentObject);
        }

        public string GetCountryCode()
        {
            string country = "";
            //try
            //{
            //    AndroidJavaClass localeClass = new AndroidJavaClass("java.util.Locale");
            //    AndroidJavaObject locale = localeClass.CallStatic<AndroidJavaObject>("getDefault");
            //    country = locale.Call<string>("getCountry");
            //}
            //catch (Exception ex)
            //{
            //    Debug.LogError(ex.Message);
            //}
            //if (country == null)
            //{
            //    country = "";
            //}
            return country;
        }
    }
}
