package com.orcas.core;

import android.Manifest;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.ContentValues;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.MediaRecorder;
import android.media.projection.MediaProjection;
import android.media.projection.MediaProjectionManager;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.provider.MediaStore;
import androidx.annotation.NonNull;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.Surface;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.logging.Logger;

import static android.app.Activity.RESULT_OK;
import static android.content.Context.MEDIA_PROJECTION_SERVICE;

/**
 * Created by chengwen on 2018/5/21.
 */

public class ScreenRecordHelper {
    private static final int RECORD_REQUEST_CODE  = 101;
    private static final int STORAGE_REQUEST_CODE = 102;
    private static final int AUDIO_REQUEST_CODE   = 103;
    private static final int SHOW = 1;
    private static final int CANCEL = 2;
    private static String saveFileNamePrefix = "Game_";
    private static String saveTitleName = "Recording Finished";
    private static String saveTipMsg = "Would you like to share or delete your recording?";
    private static String saveDelete =  "Delete";
    private static String saveShare = "Share";
    private static String shareTitle = "Share";
    private boolean isRecording;
    public boolean enableAudio = false;
    public static Activity currentActivity;
    private MediaProjectionManager mediaProjectionManager;
    private Handler mHandler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            super.handleMessage(msg);
            switch (msg.what) {
                case SHOW:
                    if (isRecording){
//                        Toast.makeText(currentActivity,"录制已开始",Toast.LENGTH_SHORT).show();
                    }else {
                        startScreenCapture();
                        isRecording = true;
                    }
                    break;
                case CANCEL:
                    if (isRecording){
                        stopScreenCapture();
                        isRecording = false;
                    }else {
                    }
                    break;
            }
        }
    };
    private MediaProjection mediaProjection = null;
    private MediaRecorder mediaRecorder = null;
    private VirtualDisplay virtualDisplay = null;
    /**
     * 屏幕的宽度
     */
    private int screenWidth ;
    /**
     * 屏幕的高度
     */
    private int screenHeight ;
    /**
     * 屏幕的像素
     */
    private int screenDpi;
    private DisplayMetrics metrics;
    /**
     * 保存在相册视频的名字
     */
    private String videoName;

    public boolean canRecord = true;


    public void bind(Activity activity) {
        currentActivity = activity;
        try {
            mediaRecorder = new MediaRecorder();
            canRecord = true;
        }
        catch (Exception e)
        {
            canRecord = false;
            Log.e("ScreenRecordHelper", e.getMessage());
        }

//        metrics = new DisplayMetrics();
//        currentActivity.getWindowManager().getDefaultDisplay().getMetrics(metrics);
    }

    private boolean checkPermission()
    {
        ArrayList<String> list = new ArrayList<>();
        if (enableAudio) {
            if (ContextCompat.checkSelfPermission(currentActivity, Manifest.permission.RECORD_AUDIO)
                    != PackageManager.PERMISSION_GRANTED) {
                list.add(Manifest.permission.RECORD_AUDIO);
            }
        }

        if (ContextCompat.checkSelfPermission(currentActivity, Manifest.permission.WRITE_EXTERNAL_STORAGE)
                != PackageManager.PERMISSION_GRANTED) {
            list.add(Manifest.permission.WRITE_EXTERNAL_STORAGE);
        }
        if (list.size() > 0) {
            Log.d("ddd", "request permission");
            ActivityCompat.requestPermissions(currentActivity, list.toArray(new String[list.size()]), STORAGE_REQUEST_CODE);
            return false;
        }
        return true;
    }

    public void SetFileNamePrefix(String name){
        saveFileNamePrefix = name;
    }

    public void SetSaveTips(String tilte, String content, String delete, String share){
        saveTitleName = tilte;
        saveDelete = delete;
        saveTipMsg = content;
        saveShare = share;
    }

    public void SetShareTitle(String title){
        shareTitle = title;
    }

    /**
     * unity调用的方法，需要用一个handler进行处理实现功能，直接无法实现。
     */
    public void stopRecording() {
        if (!canRecord) return;
        mHandler.sendEmptyMessage(CANCEL);
    }
    /**
     * unity调用的方法，需要用一个handler进行处理实现功能，直接无法实现。
     */
    public void startRecording() {
        if (!canRecord) return;
        setVideoSize();
        if (checkPermission()) {
            mHandler.sendEmptyMessage(SHOW);
        }
    }

    private void setVideoSize() {
        metrics = new DisplayMetrics();
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN_MR1) {
            currentActivity.getWindowManager().getDefaultDisplay().getRealMetrics(metrics);
        }
        else {
            currentActivity.getWindowManager().getDefaultDisplay().getMetrics(metrics);
        }
        setConfig(metrics.widthPixels,metrics.heightPixels,metrics.densityDpi);
        restrictVideoSize(720, 1280);
    }

    private void startScreenCapture() {
        Intent captureIntent = null;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.LOLLIPOP) {
            mediaProjectionManager = (MediaProjectionManager)currentActivity.getSystemService(MEDIA_PROJECTION_SERVICE);
            captureIntent = mediaProjectionManager.createScreenCaptureIntent();
        }
        currentActivity.startActivityForResult(captureIntent, RECORD_REQUEST_CODE);
    }

    private void stopScreenCapture() {
        try {
            mediaRecorder.stop();
            mediaRecorder.reset();
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                mediaProjection.stop();
                if (virtualDisplay != null) {
                    virtualDisplay.release();
                }
            }
            insertVideoToMediaStore(getSaveDirectory()+videoName);
        }
        catch (Exception e)
        {
            Log.e("ScreenRecordHelper", "stopScreenCapture "+e.getMessage());
        }
    }

    private void setConfig(int screenWidth, int screenHeight, int screenDpi) {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.screenDpi = screenDpi;
    }

    public boolean onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == RECORD_REQUEST_CODE && resultCode == RESULT_OK) {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                mediaProjection = mediaProjectionManager.getMediaProjection(resultCode, data);
            }
//            setConfig(metrics.widthPixels,metrics.heightPixels,metrics.densityDpi);
            startRecord();
            return true;
        } else if (requestCode == RECORD_REQUEST_CODE) {
            isRecording = false;
        }
        return false;
    }

    public boolean onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        if (requestCode == STORAGE_REQUEST_CODE) {
            boolean permitted = true;
            for (int i = 0; i < grantResults.length; i++) {
                if (grantResults[i] == PackageManager.PERMISSION_DENIED) {
                    permitted = false;
                    Log.e("ScreenRecord", "permission denied " + permissions[i]);
                }
            }
            if (permitted == true) startRecording();
            return true;
        }
        return false;
    }

    private boolean startRecord() {
        initRecorder();
//        createVirtualDisplay();
        try {
            mediaRecorder.start();
        }
        catch (Exception e)
        {
            Log.e("ScreenRecordHelper", "startRecord "+ e.getMessage());
            e.printStackTrace();
        }

        return true;
    }


//    private void createVirtualDisplay() {
//        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
//            try {
//                Surface surface = mediaRecorder.getSurface();
//                if (surface != null) {
//                    virtualDisplay = mediaProjection.createVirtualDisplay("MainScreen", screenWidth, screenHeight, screenDpi,
//                            DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR, surface, null, null);
//                }
//            }
//            catch (Exception e)
//            {
//                Log.e("ScreenRecordHelper", "createVirtualDisplay fail " + e.getMessage());
//                e.printStackTrace();
//            }
//        }
//    }

    /*
    by liqing  2018-11-19
    针对mediaRecorder.getSurface() 崩溃做的修改 见https://play.google.com/apps/publish/?account=6571611572201924211#AndroidMetricsErrorsPlace:p=com.sports.real.golf.rival.online&appid=4975500561698790577&appVersion=PRODUCTION&clusterName=apps/com.sports.real.golf.rival.online/clusters/b2efc629&detailsAppVersion=PRODUCTION&detailsSpan=7
    根据文档，如果getSurface在prepare之前或者stop之后调用就会抛出异常，根据代码逻辑，有可能在prepare或者其之前就发生了崩溃，但是被catch住了，导致prepare
    没有成功，因此，将两部分代码合并到一个try-catch里（不是百分百确定，这样改一下试试看）
     */
    private void initRecorder() {
        try {
            if (enableAudio) {
                mediaRecorder.setAudioSource(MediaRecorder.AudioSource.MIC);
            }
            mediaRecorder.setVideoSource(MediaRecorder.VideoSource.SURFACE);

            mediaRecorder.setOutputFormat(MediaRecorder.OutputFormat.THREE_GPP);

            videoName = saveFileNamePrefix + System.currentTimeMillis() + ".mp4";
            mediaRecorder.setOutputFile(getSaveDirectory() + videoName);
            mediaRecorder.setVideoSize(screenWidth, screenHeight);
            if (enableAudio) {
                mediaRecorder.setAudioEncoder(MediaRecorder.AudioEncoder.AMR_NB);
            }
            mediaRecorder.setVideoEncoder(MediaRecorder.VideoEncoder.H264);
            mediaRecorder.setVideoEncodingBitRate(5*1024*1024);
            mediaRecorder.setVideoFrameRate(24);

            mediaRecorder.prepare();

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                Surface surface = mediaRecorder.getSurface();
                if (surface != null) {
                    virtualDisplay = mediaProjection.createVirtualDisplay("MainScreen", screenWidth, screenHeight, screenDpi,
                            DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR, surface, null, null);
                }
            }

        }
        catch (Exception e) {
            Log.e("ScreenRecordRecord", "InitRecord " + e.getMessage());
            e.printStackTrace();
        }
    }

    private void restrictVideoSize(int maxWidth, int maxHeight) {
        float ratio;
        if (screenWidth > maxWidth) {
            ratio = (float)screenWidth/maxWidth;
            screenWidth = maxWidth;
            screenHeight = (int) (screenHeight / ratio);
        }
        else if (screenHeight > maxHeight) {
            ratio = (float)screenHeight/maxHeight;
            screenHeight = maxHeight;
            screenWidth = (int)(screenWidth / ratio);
        }
    }

    private String getSaveDirectory() {
        if (Environment.getExternalStorageState().equals(Environment.MEDIA_MOUNTED)) {
            String screenRecordPath =  Environment.getExternalStorageDirectory().getAbsolutePath() + File.separator + "DCIM"+File.separator+"Camera"+File.separator;
            return screenRecordPath;
        } else {
            return null;
        }
    }


    private  void insertVideoToMediaStore(final String filePath) {
        ContentValues values = new ContentValues();
        values.put(MediaStore.MediaColumns.DATA,filePath);
        // video/*
        values.put(MediaStore.MediaColumns.MIME_TYPE, "video/mp4");
        final Uri uri = currentActivity.getContentResolver().insert(MediaStore.Video.Media.EXTERNAL_CONTENT_URI, values);

        AlertDialog.Builder builder = new AlertDialog.Builder(currentActivity, AlertDialog.THEME_DEVICE_DEFAULT_LIGHT);
        builder.setTitle(saveTitleName);
        builder.setMessage(saveTipMsg);
        builder.setNegativeButton(saveDelete, new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialogInterface, int i) {
                String where = MediaStore.Images.Media.DATA + "='" + filePath + "'";
                currentActivity.getContentResolver().delete(MediaStore.Video.Media.EXTERNAL_CONTENT_URI, where, null);
            }
        });
        builder.setPositiveButton(saveShare, new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialogInterface, int i) {
                shareVideo(uri);
            }
        });
        builder.show();

    }

    private void shareVideo(Uri uri)
    {
        Intent share_intent = new Intent();
        share_intent.setAction(Intent.ACTION_SEND);//设置分享行为
        share_intent.setType("video/*");//设置分享内容的类型
        share_intent.putExtra(Intent.EXTRA_SUBJECT, shareTitle);//添加分享内容标题
        share_intent.putExtra(Intent.EXTRA_TEXT, videoName);//添加分享内容
        share_intent.putExtra(Intent.EXTRA_STREAM, uri);
        //创建分享的Dialog
        share_intent = Intent.createChooser(share_intent, saveShare);
        currentActivity.startActivity(share_intent);
    }

}
