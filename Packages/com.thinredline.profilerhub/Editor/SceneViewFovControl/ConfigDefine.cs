using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ThinRL.ProfilerHub.Editor.SceneViewFovControl
{
    [System.Serializable]
    public class ConfigDefine
    {
        [LabelText(" ")]
        [InfoBox("这里没有要配置的东西")]
        [ReadOnly]
        public bool placeHolder = false; 
    }
}
