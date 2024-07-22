using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    
    public class NativeUtilsListener : MonoBehaviour
    {

        public delegate void RequestAttResult(string result);

        public static RequestAttResult AttCallback;
        private static bool HasInit = false;
        public static void Init()
        {
            if (HasInit == false)
            {
                GameObject obj = new GameObject("NativeUtilsListener");
                obj.AddComponent<NativeUtilsListener>();
            }
        }
        
        void Awake()
        {
            if (!HasInit)
            {
                HasInit = true;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        
        
        /// <summary>
        /// 请求ATT弹窗的回调
        /// </summary>
        /// <param name="str">
        /// ATTrackingManagerAuthorizationStatus枚举int值转string
        /// case authorized = 3
        /// The value returned if the user authorizes access to app-related data that can be used for tracking the user or the device.
        /// case denied = 2
        /// The value returned if the user denies authorization to access app-related data that can be used for tracking the user or the device.
        /// case notDetermined = 0
        /// The value returned if a user has not yet received an authorization request to authorize access to app-related data that can be used for tracking the user or the device.
        /// case restricted = 1
        /// The value returned if authorization to access app-related data that can be used for tracking the user or the device is restricted.
        /// </param>
        public void OnRequestATTPermissionFinished(string str)
        {
            if (AttCallback != null)
            {
                AttCallback(str);
            }
        }
    }
}

