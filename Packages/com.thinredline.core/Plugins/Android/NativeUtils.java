package com.orcas.core;

import android.app.Activity;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.net.Uri;
import android.os.Vibrator;

public class NativeUtils {
    private Vibrator vib;
    private Context _context;
    private Activity _activity;

    public NativeUtils(Context ctx, Activity activity){
        _context = ctx;
        _activity = activity;
        vib = (Vibrator)activity.getSystemService(Service.VIBRATOR_SERVICE);
    }

    public static NativeUtils getNativeUtilsImpl(Context ctx, Activity activity){
        return new NativeUtils(ctx, activity);
    }

    public void performVibrate(long duration){
        if(vib != null && vib.hasVibrator()){
            vib.vibrate(duration);
        }
    }

    public boolean isDarkMode()
    {
        int currentNightMode  = _context.getResources().getConfiguration().uiMode & Configuration.UI_MODE_NIGHT_MASK;
        return currentNightMode == Configuration.UI_MODE_NIGHT_NO;
    }

    public void sendEmail(String addr, String title, String content)
    {
        Intent email = new Intent(Intent.ACTION_SEND);
        email.setData(Uri.parse("mailto:")); // only email apps should handle this
        email.putExtra(Intent.EXTRA_EMAIL, new String[]{ addr});
        email.putExtra(Intent.EXTRA_SUBJECT, title);
        email.putExtra(Intent.EXTRA_TEXT, content);

        _activity.startActivity(email);
    }
}
