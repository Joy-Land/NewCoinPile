package com.orcas.core;
import android.app.*;
import android.os.Build;
import android.util.Log;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import java.util.Calendar;
import java.util.List;
//import static android.content.Context.NOTIFICATION_SERVICE;
import androidx.core.app.NotificationCompat;
//import static javafx.scene.input.KeyCode.R;

public class AndroidNotificator
        extends BroadcastReceiver
{
    private static NotificationManager notifManager;
    public static void ShowNotification(int id, String pAppName, String pTitle, String pContent, int pDelaySecond, boolean pIsDailyLoop)
            throws IllegalArgumentException
    {
        Log.i("zyy----" ,"zyy----" );
        if (pDelaySecond < 0) {
            throw new IllegalArgumentException("The param: pDelaySecond < 0");
        }
        Activity curActivity = UnityPlayer.currentActivity;

        Intent intent = new Intent("UNITY_NOTIFICATOR");
        intent.putExtra("appname", pAppName);
        intent.putExtra("title", pTitle);
        intent.putExtra("content", pContent);
        intent.putExtra("id", id);
        intent.setClass(UnityPlayer.currentActivity, AndroidNotificator.class);
        intent.setData(Uri.parse("tel:" + id));
        intent.putExtra("activity", "com.jykplugin.googleiab.AndroidAgent");
        PendingIntent pi = PendingIntent.getBroadcast(curActivity, id, intent, 0);

        AlarmManager am = (AlarmManager)curActivity.getSystemService("alarm");
        Calendar calendar = Calendar.getInstance();
        calendar.add(13, pDelaySecond);
        long alarmTime = calendar.getTimeInMillis();

        Log.i("zyyalarmTime----"+alarmTime ,"zyy----" );
        if (pIsDailyLoop) {
            am.setRepeating(
                    0,
                    alarmTime,
                    86400000L,
                    pi);
        } else {
            am.set(
                    0,
                    alarmTime,
                    pi);
        }
    }

    public static void ClearNotification(int id)
    {
        NotificationManager notificationManager = (NotificationManager)UnityPlayer.currentActivity.getApplicationContext().getSystemService("notification");
        notificationManager.cancelAll();
        AlarmManager am = (AlarmManager)UnityPlayer.currentActivity.getSystemService("alarm");
        if (am != null)
        {
            Intent intent = new Intent("UNITY_NOTIFICATOR");
            intent.setClass(UnityPlayer.currentActivity, AndroidNotificator.class);
            intent.setData(Uri.parse("tel:" + id));
            PendingIntent sender = PendingIntent.getBroadcast(UnityPlayer.currentActivity, id, intent, 536870912);
            if (sender != null) {
                am.cancel(sender);
            }
        }
    }

    public static String[] GetPackageNameList()
    {
        UnityPlayer.currentActivity.getApplicationContext();
        PackageManager pm = UnityPlayer.currentActivity.getApplicationContext().getPackageManager();
        List<PackageInfo> list = pm.getInstalledPackages(0);
        String[] re = new String[list.size()];
        for (int i = 0; i < list.size(); i++) {
            re[i] = ((PackageInfo)list.get(i)).packageName;
        }
        return re;
    }

    public static boolean PackageInstalled(String packageName)
    {
        UnityPlayer.currentActivity.getApplicationContext();
        PackageManager pm = UnityPlayer.currentActivity.getApplicationContext().getPackageManager();
        try
        {
            pm.getPackageInfo(packageName, 64);
            return true;
        }
        catch (PackageManager.NameNotFoundException e) {}
        return false;
    }

    public void createWeidgt() {}
    @Override
    public void onReceive(Context pContext, Intent pIntent)
    {
        Bundle bundle = pIntent.getExtras();

        String unityClass = pIntent.getStringExtra("activity");
        Class<?> unityClassActivity = null;
        try {
            unityClassActivity = Class.forName(unityClass);
        } catch (Exception e) {
            if (e != null) {
                e.printStackTrace();
            }
            return;
        }

        if(Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
//            String imgname = "stat_notify_chat";
//            Log.i("zyy-k---", "zyy-k---");
//            int imgid = pContext.getResources().getIdentifier(imgname, "drawable", pContext.getPackageName());
//            Log.i("zyy-k---" + imgid, "zyy-k---" + imgid);
            if ((pIntent == null) || (!pIntent.getAction().equals("UNITY_NOTIFICATOR"))) {
                return;
            }

            Intent notificationIntent = new Intent(pContext, unityClassActivity);

            ApplicationInfo applicationInfo = null;
            PackageManager pm = pContext.getPackageManager();
            try {
                applicationInfo = pm.getApplicationInfo(pContext.getPackageName(), 128);
            } catch (Exception ex) {
                if (ex != null) {
                    ex.printStackTrace();
                }
                return;
            }



            PendingIntent contentIntent = PendingIntent.getActivity(
                    pContext,
                    0,
                    notificationIntent,
                    0);


            Bitmap btm = BitmapFactory.decodeResource(pContext.getResources(), applicationInfo.icon);
            Notification notification = new Notification.Builder(pContext)
                    .setContentTitle((String) bundle.get("title"))
                    .setContentText((String) bundle.get("content"))
                    .setSmallIcon(applicationInfo.icon)
                    .setTicker((String) bundle.get("appname"))
                    .setWhen(System.currentTimeMillis())
                    .setContentIntent(contentIntent)
                    .setLargeIcon(btm)
                    .build();
            notification.flags = 16;

            NotificationManager nm = (NotificationManager) pContext.getSystemService("notification");
            nm.notify(bundle.getInt("id"), notification);
        }else {
            final int NOTIFY_ID = 0; // ID of notification
//            String id = pContext.getString(R.string.default_notification_channel_id); // default_channel_id
//            String title = pContext.getString(R.string.default_notification_channel_title); // Default Channel
            String id = "pContext.getString(R.string.default_notification_channel_id)"; // default_channel_id
            String title = (String) bundle.get("title"); // Default Channel
            Intent intent;
            PendingIntent pendingIntent;
            NotificationCompat.Builder builder;
            if (notifManager == null) {
                notifManager = (NotificationManager)pContext.getSystemService(Context.NOTIFICATION_SERVICE);
            }
            int importance = NotificationManager.IMPORTANCE_HIGH;
            NotificationChannel mChannel = notifManager.getNotificationChannel(id);
            if (mChannel == null) {
                mChannel = new NotificationChannel(id, title, importance);
                mChannel.enableVibration(true);
                mChannel.setVibrationPattern(new long[]{100, 200, 300, 400, 500, 400, 300, 200, 400});
                notifManager.createNotificationChannel(mChannel);
            }



            builder = new NotificationCompat.Builder(pContext, id);

            intent = new Intent(pContext, unityClassActivity);
            intent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
            pendingIntent = PendingIntent.getActivity(pContext, 0, intent, 0);
            builder.setContentTitle((String) bundle.get("title"))                            // required
                    .setSmallIcon(android.R.drawable.ic_popup_reminder)   // required
                    .setContentText((String) bundle.get("content")) // required
                    .setDefaults(Notification.DEFAULT_ALL)
                    .setAutoCancel(true)
                    .setContentIntent(pendingIntent)
                    .setTicker((String) bundle.get("appname"))
                    .setVibrate(new long[]{100, 200, 300, 400, 500, 400, 300, 200, 400});
            Notification notification = builder.build();
            notifManager.notify(NOTIFY_ID, notification);
        }
    }
}
