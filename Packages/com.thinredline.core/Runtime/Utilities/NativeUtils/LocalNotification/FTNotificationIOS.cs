using System;
using System.Runtime.InteropServices;

public static class FTNotificationIOS
{
#if UNITY_IOS
    [DllImport("__Internal")]
    static extern void FTNotification_cancelLocalNotification(string uid);
    [DllImport("__Internal")]
    static extern void FTNotification_addNotification(string uid, string content, int delaySecond, int loop);


    public static void removeLocalNotification(string uid) {
        FTNotification_cancelLocalNotification(uid);
    }

    public static void addLocalNotification(string uid, string content, int delaySecond, bool loop) {
        FTNotification_addNotification(uid, content, delaySecond, loop ? 1 : 0);
    }
#endif
}

