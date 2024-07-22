package com.orcas.core;

import android.app.Notification;
import android.app.Service;
import android.content.ComponentName;
import android.content.Intent;
import android.os.Handler;
import android.os.IBinder;
import android.os.Process;

public class MyService extends Service {
    private final String TAG = "MyService";

    private boolean isRunning = false;


    public MyService() {
    }

    @Override
    public void onCreate() {
        super.onCreate();
        DebugLoger.Log(TAG,"onCreate");
        isRunning = true;
        new Handler().postDelayed(new Runnable() {
            @Override
            public void run() {
                try {
                    if (isRunning) {
                        DebugLoger.Log(TAG, "timer start:" + Thread.currentThread().getName());
                        MyService.this.stopSelf();
                        DebugLoger.Log(TAG, "run: stopSelf:" + Process.myPid());
                        if (!((OrcasMultiDexApplication)MyService.this.getApplication()).isForeground) {
                            Process.killProcess(Process.myPid());
                        }
                        DebugLoger.Log(TAG, "run: killProcess");
                    } else {
                        MyService.this.stopSelf();
                        DebugLoger.Log(TAG, "run: is not running and stop self");
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        },JobSchedulerTool.getDurationTime() * 1000);
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        isRunning = true;
        DebugLoger.Log(TAG,"onStartCommand");
        return START_STICKY;
    }

    @Override
    public boolean onUnbind(Intent intent) {
        DebugLoger.Log(TAG,"onUnbind");
        isRunning = false;
        return super.onUnbind(intent);
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        isRunning = false;
        DebugLoger.Log(TAG,"onDestory");
    }

    @Override
    public IBinder onBind(Intent intent) {
        // TODO: Return the communication channel to the service.
        DebugLoger.Log(TAG, "onBind: ");
        isRunning = true;
        return null;
    }
}
