using System;
using UnityEngine;
using System.Collections.Generic;

namespace ThinRL.Core.NativeUtils
{
    public class NotificationHelper
    {
        private static NotificationHelper instance = null;
        private static ILocalNotificationBridge _bridge;
        public static NotificationHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NotificationHelper();
                }

                return instance;
            }
        }

        public static bool Inited
        {
            get
            {
                if (_bridge == null)
                {
                    return false;
                }
                return _bridge.Inited;
            }
        }

        public void Init()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            _bridge = new LocalNotificationAndroidBridge();
            _bridge.OnStart();
            _bridge.Init();
            #elif UNITY_IOS && !UNITY_EDITOR
            _bridge = new LocalNotificationiOSBridge();
            _bridge.OnStart();
            _bridge.Init();
            #endif
        }
        

        public void RegisterNotify(int id, String pTitle, String pContent, int pDelaySecond, bool pIsDailyLoop)
        {
            _bridge?.RegisterNotify(id, pTitle, pContent, pDelaySecond, pIsDailyLoop);
        }

        public void ClearNotifys(int id)
        {
            _bridge?.ClearNotifys(id);
        }

        public bool PackageInstalled(string packgeName)
        {
            bool result = false;
            if (_bridge != null)
            {
                result = _bridge.PackageInstalled(packgeName);
            }
            return result;
        }

        public void ClearAllNotifications()
        {
            _bridge?.ClearAllNotifications();
        }

    }
}