using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ThinRL.Core.Tools;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    public class NativeUtils
    {
        private static INativeUtilsBridge _bridge;

        private static void LazyInit()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_bridge == null)
            {
                _bridge = new NativeUtilsAndroidBridge();
            }
#elif UNITY_IOS && !UNITY_EDITOR
            if (_bridge == null)
            {
                _bridge = new NativeUtilsiOSBridge();
            }
#endif
        }

        public static void Vibrate(long duration)
        {
            if (PlayerPrefsManager.GetInt(PlayerPrefHelper.VibrationSwitch, 0) == 0)
                return;
            LazyInit();
            _bridge?.Vibrate(duration);
        }

        public static string GetDefaultLanguage()
        {
            LazyInit();
            return _bridge != null ? _bridge.GetDefaultLanguage() : "en-Editor";
        }

        public static void SendEmail(string addr, string title, string message)
        {
#if UNITY_EDITOR
            Uri uri = new Uri(string.Format("mailto:{0}?subject={1}&body={2}", addr, title, message));
            Application.OpenURL(uri.AbsoluteUri);
#else
            LazyInit();
            _bridge?.SendEmail(addr, title, message);   
#endif
        }

        public static string GetDeviceIdfa()
        {
            LazyInit();
            return _bridge != null ? _bridge.GetDeviceIdfa() : "";
        }

        public static void SaveDeviceCode(string dcode)
        {
            LazyInit();
            _bridge?.SaveDeviceCode(dcode);
        }


        public static string GetDeviceCode()
        {
            LazyInit();
            return _bridge != null ? _bridge.GetDeviceCode() : SystemInfo.deviceUniqueIdentifier + "";
        }

        public static void RequestAppTrackingAuthorization()
        {
            LazyInit();
            _bridge?.RequestAppTrackingAuthorization();
        }

        public static string GetCountryCode()
        {
            LazyInit();
            return _bridge?.GetCountryCode();
        }

        public static bool CheckIsDarkMode()
        {
            LazyInit();
            return _bridge != null ? _bridge.CheckIsDarkMode() : false;
        }
    }
}
