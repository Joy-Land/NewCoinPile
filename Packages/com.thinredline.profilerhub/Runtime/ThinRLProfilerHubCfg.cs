using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThinRL.Core;
using Sirenix.OdinInspector;

namespace ThinRL.ProfilerHub
{
    internal class ThinRLProfilerHubCfg : ThinRLSettingBase
    {
        // ------------- 子类必须实现的指定签名的静态单例方法才能显示在ProjectSetting界面
        static ThinRLProfilerHubCfg s_Inst;
        public static ThinRLProfilerHubCfg Instance { get { return s_Inst ?? (s_Inst = LoadOrCreateHelper<ThinRLProfilerHubCfg>(true)); } }
        // --------------------------------------------
        [LabelText("OverdrawMonitor")]
        public OverdrawMonitor.ConfigDefine overdrawMonitorConfig;
    }

}
