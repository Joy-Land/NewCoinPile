package com.orcas.core;

import android.app.Activity;
import android.app.Application;
import android.content.Context;
import android.os.Bundle;
import androidx.multidex.MultiDex;

//import com.appsflyer.AppsFlyerConversionListener;
//import com.appsflyer.AppsFlyerLib;
import com.unity3d.player.UnityPlayer;



public class OrcasMultiDexApplication
        extends Application
{
    private static final String TAG = "OrcasMultiDexApplication";
    public static String ConversionData = "";
    public static String AttributionData = "";
//    private static  String af_dev_key="aenSuFdSXYGEa8HyMfspZA";

    private int activityAount = 0;
    public boolean isForeground = false;


    public void onCreate()
    {
        super.onCreate();
        isForeground = true;
        registerActivityLifecycleCallbacks(activityLifecycleCallbacks);
    }


    protected void attachBaseContext(Context base)
    {
        super.attachBaseContext(base);
        MultiDex.install(this);
    }

    /**
     * Activity 生命周期监听，用于监控app前后台状态切换
     */
    Application.ActivityLifecycleCallbacks activityLifecycleCallbacks = new Application.ActivityLifecycleCallbacks() {
        @Override
        public void onActivityCreated(Activity activity, Bundle savedInstanceState) {
        }

        @Override
        public void onActivityStarted(Activity activity) {
            activityAount++;
            isForeground = true;
            JobSchedulerTool.cancelMyService(activity);
        }

        @Override
        public void onActivityResumed(Activity activity) {
        }
        @Override
        public void onActivityPaused(Activity activity) {
        }

        @Override
        public void onActivityStopped(Activity activity) {
            activityAount--;
            if (activityAount == 0) {
                isForeground = false;
                JobSchedulerTool.startService(activity);
                DebugLoger.Log(TAG, "startService");
            }
            DebugLoger.Log(TAG, "onActivityStopped activityCount:" + activityAount);
        }

        @Override
        public void onActivitySaveInstanceState(Activity activity, Bundle outState) {
        }
        @Override
        public void onActivityDestroyed(Activity activity) {
        }
    };
}
