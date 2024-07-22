using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    public class NativeUtilsiOSBridge : INativeUtilsBridge
    {
        public void Vibrate(long duration)
        {
#if UNITY_IOS && !UNITY_EDITOR
		    performVibrate ();
#endif
        }

        public string GetDefaultLanguage()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return GetCurrLanguage();
#else    
            return "Editor_English";
#endif
        }

        public void SendEmail(string addr, string title, string message)
        {
#if UNITY_IOS
            Uri uri = new Uri(string.Format("mailto:{0}?subject={1}&body={2}", addr, title, message));
            Application.OpenURL(uri.AbsoluteUri);
#endif
        }

        public string GetDeviceIdfa()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return GetIDFA();
#else
            return "";
#endif
        }

        public void SaveDeviceCode(string dcode)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!string.IsNullOrEmpty(dcode))
            {
                while (GetUdid() != dcode)
                    saveUDID(dcode);
            }
#endif
        }

        public string GetDeviceCode()
        {
#if UNITY_IOS && !UNITY_EDITOR
            string dcode = GetUDID();
            if (string.IsNullOrEmpty(dcode))
            {
                dcode = SystemInfo.deviceUniqueIdentifier;
                SaveDeviceCode(dcode);
            }
            return dcode;
#else
            return "";
#endif
        }

        public string GetUdid()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return getUDID();
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }

        public void RequestAppTrackingAuthorization()
        {
#if UNITY_IOS && !UNITY_EDITOR
            RequestAppTrackingAuthorization_iOS();
#endif
        }

        public string GetCountryCode()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _GetCurrCountry();
#endif
            return "";
        }

        public bool CheckIsDarkMode()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return CheckIsDarkMode_iOS();
#endif
            return false;
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void performVibrate(); //震动

        [DllImport("__Internal")]
        private static extern string GetCurrLanguage();

        [DllImport("__Internal")]
        private static extern string getIDFA();
        private static string GetIDFA()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var idfa = getIDFA();
                if (string.IsNullOrEmpty(idfa) == false && idfa != "00000000-0000-0000-0000-000000000000")
                    return idfa;
            }
            return "";
        }

        [DllImport("__Internal")]
        private static extern string getUDID();
        private static string GetUDID()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return getUDID();
            }
            return SystemInfo.deviceUniqueIdentifier;
        }

        [DllImport("__Internal")]
        private static extern string GetCurrCountry();
        private static string _GetCurrCountry()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return GetCurrCountry();
            }
            return "";
        }

        [DllImport("__Internal")]
        private static extern void saveUDID(string data);
        private static void SaveUDID(string data)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                saveUDID(data);
            }
        }


        [DllImport("__Internal")]

        private static extern void RequestATTAuthorization();
        public static void RequestAppTrackingAuthorization_iOS()
        {
            RequestATTAuthorization();
        }

        [DllImport("__Internal")]

        private static extern bool CheckIsUserDarkMode();
        public static bool CheckIsDarkMode_iOS()
        {
            return CheckIsUserDarkMode();
        }

#endif

    }
}
