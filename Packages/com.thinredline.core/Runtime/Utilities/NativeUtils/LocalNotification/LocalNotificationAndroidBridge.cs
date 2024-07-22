using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    public class LocalNotificationAndroidBridge : ILocalNotificationBridge
    {
        public List<int> RegisterQueue { get; set; }
        public bool Inited { get; set; } = false;
        
#if UNITY_ANDROID
        private AndroidJavaClass androidNotificationClass;
#endif
        
        public bool OnStart()
        {
            Inited = true;
            RegisterQueue = new List<int>();
#if UNITY_ANDROID && !UNITY_EDITOR
		androidNotificationClass = new AndroidJavaClass ("com.orcas.core.AndroidNotificator");
#endif
            return Inited;
        }

        public void Init()
        {
            Inited = true;
        }


        public bool PackageInstalled(string packgeName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		return androidNotificationClass.CallStatic<bool> ("PackageInstalled",packgeName);
#endif
            return false;
        }

        public void RegisterNotify(int id, string pTitle, string pContent, int pDelaySecond, bool pIsDailyLoop)
        {
            if (Inited == false)
                return;
#if UNITY_ANDROID && !UNITY_EDITOR
		androidNotificationClass.CallStatic ("ShowNotification", id, Application.productName, pTitle, pContent, pDelaySecond, pIsDailyLoop);
#endif
        }

        public void ClearAllNotifications()
        {
            // 清空队列
            foreach (var type in RegisterQueue)
            {
                ClearNotifys(type);
            }
            RegisterQueue.Clear();
        }

        public void ClearNotifys(int id)
        {
            if (Inited == false)
                return;
#if UNITY_ANDROID && !UNITY_EDITOR
		androidNotificationClass.CallStatic ("ClearNotification", id);
#endif
            for (int i = RegisterQueue.Count - 1; i >= 0; i--)
            {
                if (id == RegisterQueue[i])
                {
                    RegisterQueue.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
