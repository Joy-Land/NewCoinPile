using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ThinRL.Core.NativeUtils
{
    public class ScreenRecordAndroidBridge : IScreenRecordBridge
    {
        public bool HasStart { get; set; } = false;
        public bool IsAvailable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return IsAvaliable_Android();
#endif
            return false;
        }

        public void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartRecord_Android(true);
#endif
            HasStart = true;
        }

        public void Stop()
        {
            if (HasStart == false) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        StartRecord_Android(false);
#endif
            HasStart = false;
        }

        public void SetSaveFileNamePrefix(string prefix)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidObj().Call("SetScreenRecordFileNamePrefix", prefix);
#endif
        }

        public void SetScreenRecordSaveTips(string tilte, string content, string delete, string share)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidObj().Call("SetScreenRecordSaveTips", tilte, content, delete, share);
#endif
        }

        public void SetScreenRecordShareTitle(string tilte)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidObj().Call("SetScreenRecordSaveTips", tilte);
#endif
        }
        
        
#if UNITY_ANDROID
        private static AndroidJavaObject androidObj = null;

        private AndroidJavaObject AndroidObj()
        {
            if (androidObj == null)
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                androidObj = jc.GetStatic<AndroidJavaObject>("currentActivity");
            }

            return androidObj;
        }

        private bool IsAvaliable_Android(){
            return AndroidObj().Get<bool>("isScreenRecordAvaliable");
        }

        private void StartRecord_Android(bool isStart)
        {
            try
            {
                if (isStart)
                {
                    AndroidObj().Call("StartScreenRecord");
                }
                else
                {
                    AndroidObj().Call("StopScreenRecord");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("录屏出错 " + e.Message);
            }
        }

#endif
    }
}
