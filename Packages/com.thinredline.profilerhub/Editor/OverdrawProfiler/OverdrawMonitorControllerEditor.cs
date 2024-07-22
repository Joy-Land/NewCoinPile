using System.Collections;
using System.Collections.Generic;
using ThinRL.ProfilerHub.OverdrawMonitor;
using UnityEditor;
using UnityEngine;

namespace OverdrawForURP.Editor
{
    public class OverdrawMonitorControllerEditor
    {

        [MenuItem("ThinRedLine/ProfilerHub/����OverdrawProfiler")]
        public static void EnableOverdrawDebug()
        {
            EditorPrefs.SetBool("enable_overdraw_profiler", true);
            OverdrawMonitorManager.OnAfterSceneLoadRuntimeMethod();
        }


        [MenuItem("ThinRedLine/ProfilerHub/����OverdrawProfiler")]
        public static void DisableOverdrawDebug()
        {
            EditorPrefs.SetBool("enable_overdraw_profiler", false);
            OverdrawMonitorManager.OnAfterSceneLoadRuntimeMethod();
        }
    }

}
