using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.NativeUtils
{
    public interface INativeUtilsBridge
    {
        void Vibrate(long duration);

        string GetDefaultLanguage();

        void SendEmail(string addr, string title, string message);

        string GetDeviceIdfa();

        void SaveDeviceCode(string dcode);

        string GetDeviceCode();

        string GetUdid();

        void RequestAppTrackingAuthorization();
        string GetCountryCode();

        bool CheckIsDarkMode();
    }

}


