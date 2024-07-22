using System.Collections;
using System.Collections.Generic;
using ThinRL.Core.Tools;
using UnityEngine;

namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    public static class OverdrawMonitorController
    {
        public static bool GetEnableState()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetBool("enable_overdraw_profiler", false);
#else
            var v = PlayerPrefsManager.GetInt("enableOverdrawProfiler", 0);
            return v != 0;
#endif


        }
    }
}

