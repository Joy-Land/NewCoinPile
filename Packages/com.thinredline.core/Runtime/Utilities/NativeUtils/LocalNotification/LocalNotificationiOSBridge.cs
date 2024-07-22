using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    public class LocalNotificationiOSBridge : ILocalNotificationBridge
    {
        public List<int> RegisterQueue { get; set; }
        public bool Inited { get; set; } = false;
        public bool OnStart()
        {
            #if UNITY_IOS 
            Inited =
                (UnityEngine.iOS.NotificationServices.enabledNotificationTypes != UnityEngine.iOS.NotificationType.None);
            #endif
            RegisterQueue = new List<int>();
            return Inited;
        }

        public void Init()
        {
#if UNITY_IOS
		UnityEngine.iOS.NotificationServices.RegisterForNotifications (
			UnityEngine.iOS.NotificationType.Alert |
			UnityEngine.iOS.NotificationType.Badge |
			UnityEngine.iOS.NotificationType.Sound);
#endif
            Inited = true;
        }


        public bool PackageInstalled(string packgeName)
        {
            return false;
        }

        public void RegisterNotify(int id, string pTitle, string pContent, int pDelaySecond, bool pIsDailyLoop)
        {
            if (Inited == false)
                return;
#if UNITY_IOS && !UNITY_EDITOR
        FTNotificationIOS.addLocalNotification(id.ToString(), pContent, pDelaySecond, pIsDailyLoop);
#endif
        }

        public void ClearAllNotifications()
        {
            // 清空队列
            // 清空队列
            foreach (var type in RegisterQueue)
            {
                ClearNotifys(type);
            }

            // iOS清除小红点
#if UNITY_IOS
		UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification (); 
		l.applicationIconBadgeNumber = -1;
		UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow (l); 
		UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
		UnityEngine.iOS.NotificationServices.ClearRemoteNotifications();
#endif
        }

        public void ClearNotifys(int id)
        {
            if (Inited == false)
                return;
            
#if UNITY_IOS && !UNITY_EDITOR
        FTNotificationIOS.removeLocalNotification(id.ToString());
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
