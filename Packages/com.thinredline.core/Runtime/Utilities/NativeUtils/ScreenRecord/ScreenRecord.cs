using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ThinRL.Core.NativeUtils
{
    public class ScreenRecord
    {
        private static IScreenRecordBridge _bridge;

        public static void Init()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _bridge = new ScreenRecordAndroidBridge();
#elif UNITY_IOS && !UNITY_EDITOR
            _bridge = new ScreenRecordiOSBridge();
#endif
        }
        
        public bool IsAvailable()
        {
            bool result = false;
            if (_bridge != null)
            {
                result = _bridge.IsAvailable();
            }
            return result;
        }

        public void Start()
        {
            _bridge?.Start();
        }

        public void Stop()
        {
            _bridge?.Stop();
        }

        /// <summary>
        /// 设置Android保存到相册的文件名前缀
        /// 默认是Game_
        /// </summary>
        /// <param name="prefix"></param>
        public void SetSaveFileNamePrefix(string prefix)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_bridge != null)
            {
                if (_bridge is ScreenRecordAndroidBridge)
                {
                    var aBridge = _bridge as ScreenRecordAndroidBridge;
                    aBridge?.SetSaveFileNamePrefix(prefix);
                }
            }
#endif
        }
        
        /// <summary>
        /// 设置Android保存提示的文案
        /// 如果不设置。默认为
        /// tilte: Recording Finished
        /// contenet: Would you like to share or delete your recording?
        /// delete: Delete
        /// share: Share
        /// </summary>
        /// <param name="tilte">标题</param>
        /// <param name="content">内容</param>
        /// <param name="delete">删除按钮</param>
        /// <param name="share">保存</param>
        public void SetScreenRecordSaveTips(string tilte, string content, string delete, string share)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_bridge != null)
            {
                if (_bridge is ScreenRecordAndroidBridge)
                {
                    var aBridge = _bridge as ScreenRecordAndroidBridge;
                    aBridge?.SetScreenRecordSaveTips(tilte, content, delete, share);
                }
            }
#endif
        }
        
        /// <summary>
        /// 设置Android选择分享按钮后，提示标题
        /// 默认为Share
        /// </summary>
        /// <param name="tilte"></param>
        public void SetScreenRecordShareTitle(string tilte)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_bridge != null)
            {
                if (_bridge is ScreenRecordAndroidBridge)
                {
                    var aBridge = _bridge as ScreenRecordAndroidBridge;
                    aBridge?.SetScreenRecordShareTitle(tilte);
                }
            }
#endif
        }
    }
}