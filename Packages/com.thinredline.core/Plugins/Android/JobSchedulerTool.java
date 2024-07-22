package com.orcas.core;

import android.app.job.JobInfo;
import android.app.job.JobScheduler;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import androidx.annotation.RequiresApi;


public class JobSchedulerTool {
    private static final String TAG = "JobSchedulerTool";

    public final static int exitAppJobId = 1147;
    private static boolean enableAutoExitApp = false;
    private static boolean enableAutoExitAppHigher = false;
    private static long durationTime = 50;

    public static void autoExitApp(boolean enable) {
        enableAutoExitApp = enable;
        DebugLoger.Log(TAG, "autoExitApp: enter here：" + enable);
    }

    public static void autoExitAppHigher(boolean enable) {
        enableAutoExitAppHigher = enable;
        DebugLoger.Log(TAG, "autoExitAppHigher: enter here:" + enable);
    }

    public static void setDurationTime(int time) {
        try {
            if (time > 0) {
                durationTime = time;
            }
            DebugLoger.Log(TAG, "autoExitAppHigher: enter here param:" + time  + ", durationTime:" + durationTime);
        } catch (Exception e) {
            DebugLoger.Log(TAG, "setDurationTime: exception" + e.getLocalizedMessage());
        }
    }

    public static long getDurationTime() {
        return durationTime;
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    private static void schedulerNotification(final Context context){
        try {
            if (context == null) return;
            final JobScheduler jobScheduler = (JobScheduler) context.getSystemService(Context.JOB_SCHEDULER_SERVICE);
            if (jobScheduler != null) {
                jobScheduler.cancel(exitAppJobId);

                long delayTime = durationTime * 1000L;
                JobInfo.Builder builder = new JobInfo.Builder(exitAppJobId, new ComponentName(context, MyJobService.class));
                builder.setMinimumLatency(delayTime);
//            builder.setOverrideDeadline(delayTime);
                JobInfo jobInfo = builder.build();
                int result = jobScheduler.schedule(jobInfo);
                DebugLoger.Log(TAG, "schedulerNotification: " + result + ", durationTime:" + durationTime);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static void startService(Context context) {
        try {
            DebugLoger.Log(TAG, "startService: service.low version:" + enableAutoExitApp + " higer version:" + enableAutoExitAppHigher);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                if (enableAutoExitAppHigher) {
                    schedulerNotification(context);
                }
            } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                if (enableAutoExitApp) {
                    schedulerNotification(context);
                }
            } else {
                if (enableAutoExitApp && context != null) {
                    context.startService(new Intent(context, MyService.class));
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static void cancelMyService(Context context) {
        try {
            DebugLoger.Log(TAG, "cancelService: service.low version:" + enableAutoExitApp + " higer version:" + enableAutoExitAppHigher);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                if (enableAutoExitAppHigher) {
                    final JobScheduler jobScheduler = (JobScheduler) context.getSystemService(Context.JOB_SCHEDULER_SERVICE);
                    jobScheduler.cancel(exitAppJobId);
                    DebugLoger.Log(TAG, "cancelService: job:" + exitAppJobId);
                }
            } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                if (enableAutoExitApp) {
                    final JobScheduler jobScheduler = (JobScheduler) context.getSystemService(Context.JOB_SCHEDULER_SERVICE);
                    jobScheduler.cancel(exitAppJobId);
                    DebugLoger.Log(TAG, "cancelService: job:" + exitAppJobId);
                }
            } else {
                if (enableAutoExitApp && context != null) {
                    context.stopService(new Intent(context, MyService.class));
                    DebugLoger.Log(TAG, "cancelService: service");
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
