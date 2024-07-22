using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    public interface ILocalNotificationBridge 
    {
        List<int> RegisterQueue { get; set; }
        
        bool Inited { get; set; }
        bool OnStart();

        void Init();
        bool PackageInstalled(string packgeName);
        void RegisterNotify(int id, string pTitle, string pContent, int pDelaySecond, bool pIsDailyLoop);
        void ClearAllNotifications();
        void ClearNotifys(int id);
    }
}
