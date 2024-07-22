package com.orcas.core;

import android.app.job.JobParameters;
import android.app.job.JobService;
import android.content.Intent;
import android.os.Build;
import android.os.Handler;
import android.os.Process;
import androidx.annotation.RequiresApi;

@RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
public class MyJobService extends JobService {
    private final static String TAG = "MyJobService";

    @Override
    public void onDestroy() {
        super.onDestroy();
        DebugLoger.Log(TAG, "onDestroy");
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        DebugLoger.Log(TAG, "onStartCommand: ");
        return START_STICKY;
    }

    @Override
    public boolean onStartJob(JobParameters jobParameters) {
        DebugLoger.Log(TAG, "onStartJob: true the 'isRunning'");
        exitApp(jobParameters);
        return true;
    }

    @Override
    public boolean onStopJob(JobParameters jobParameters) {
        DebugLoger.Log(TAG, "onStopJob: false the 'isRunning'");
        return false;
    }

    private void exitApp(final JobParameters jobParameters) {
        DebugLoger.Log(TAG, "service is running: 10s. time over");
        jobFinished(jobParameters, false);
        DebugLoger.Log(TAG, "service is running: jobFinished and processPid is " + Process.myPid());
        stopSelf();
        DebugLoger.Log(TAG, "service is running: exitApp: stop self");

        new Handler().postDelayed(new Runnable() {
            @Override
            public void run() {
                try {
                    if (!((OrcasMultiDexApplication) MyJobService.this.getApplication()).isForeground) {
                        Process.killProcess(Process.myPid());
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        }, 100);

        DebugLoger.Log(TAG, "service is running:  Process.killProcess");
    }

}
