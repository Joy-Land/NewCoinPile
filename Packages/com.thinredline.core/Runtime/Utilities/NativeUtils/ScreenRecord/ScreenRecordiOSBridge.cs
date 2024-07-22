using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_IOS
using UnityEngine.iOS;
using System.Runtime.InteropServices;
#endif

namespace ThinRL.Core.NativeUtils
{
    public class ScreenRecordiOSBridge : IScreenRecordBridge
    {
        public bool HasStart { get; set; } = false;
        public bool IsAvailable()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return IsAvaliable_iOS();
#endif
            return false;

        }

        public void Start()
        {
#if UNITY_IOS && !UNITY_EDITOR
        StartRecord_iOS(true);
#endif
            HasStart = true;
        }

        public void Stop()
        {
            if (HasStart == false) return;
#if UNITY_IOS && !UNITY_EDITOR
        StartRecord_iOS(false);
#endif
            HasStart = false;
        }

#if UNITY_IOS
    private bool IsAvaliable_iOS()
    {
        return FTReplayKit.APIAvailable();
    }

    private void StartRecord_iOS(bool isStart)
    {
        if (!FTReplayKit.APIAvailable())
        {            
            return;
        }
        try
        {

            if (!FTReplayKit.isRecording() && isStart)
            {
                FTReplayKit.startRecording(true);
            }

            if (FTReplayKit.isRecording() && !isStart)
            {
                FTReplayKit.stopRecording();
            }
        }
        catch (Exception e)
        {
            
        }
    }

#endif
    }
    
#if UNITY_IOS
public class FTReplayKit
{
    [DllImport("__Internal")]
    private static extern int FTReplayKit_APIAvailable();
    [DllImport("__Internal")]
    private static extern int FTReplayKit_startRecording(bool enableMicrophone);
    [DllImport("__Internal")]
    private static extern int FTReplayKit_stopRecording();
    [DllImport("__Internal")]
    private static extern int FTReplayKit_isRecording();

    public static bool APIAvailable()
    {
        return FTReplayKit_APIAvailable() == 1;
    }

    public static bool startRecording(bool enableMicrophone)
    {
        return FTReplayKit_startRecording(enableMicrophone) == 1;
    }

    public static bool stopRecording()
    {
        return FTReplayKit_stopRecording() == 1;
    }

    public static bool isRecording() {
        return FTReplayKit_isRecording() == 1;
    }


}
#endif
    
}
