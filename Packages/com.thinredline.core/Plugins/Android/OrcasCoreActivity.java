package com.orcas.core;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.os.Build.VERSION;
import androidx.annotation.NonNull;
import android.util.Log;
import com.unity3d.player.UnityPlayerActivity;


public class OrcasCoreActivity extends UnityPlayerActivity {
    static final String TAG = "ORCAS_CORE_PLUGIN";
    public ScreenRecordHelper screenRecordHelper;
    public boolean isScreenRecordAvaliable;

    public OrcasCoreActivity() {
    }

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        this.screenRecordHelper = new ScreenRecordHelper();
        this.screenRecordHelper.bind(this);
        this.isScreenRecordAvaliable = this.isScreenRecordAvaliable();
    }


    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if(!this.screenRecordHelper.onActivityResult(requestCode, resultCode, data)) {
            Log.d("ORCAS_CORE_PLUGIN", "onActivityResult(" + requestCode + "," + resultCode + "," + data);
        }
    }

    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        if(!this.screenRecordHelper.onRequestPermissionsResult(requestCode, permissions, grantResults)) {
            super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    private boolean isScreenRecordAvaliable() {
        if (VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            return this.screenRecordHelper.canRecord;
        }
        return false;
    }


    public void StartScreenRecord() {
        this.screenRecordHelper.startRecording();
    }


    public void SetScreenRecordFileNamePrefix(String name){
        this.screenRecordHelper.SetFileNamePrefix(name);
    }

    public void SetScreenRecordSaveTips(String tilte, String content, String delete, String share){
        this.screenRecordHelper.SetSaveTips(tilte, content, delete, share);
    }

    public void SetScreenRecordShareTitle(String title){
        this.screenRecordHelper.SetShareTitle(title);
    }

    public void StopScreenRecord() {
        this.screenRecordHelper.stopRecording();
    }

    public void BackToHome(){
        moveTaskToBack(false);
    }
}