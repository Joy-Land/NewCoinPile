using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

#if !UNITY_EDITOR
#error This script must be placed under "Editor/" directory.
#endif

namespace ThinRL.ProfilerHub.Editor.SceneViewFovControl
{

    [InitializeOnLoad]
    public static class SceneViewFovControl
    {
        static Dictionary<int, Status> statuses = new Dictionary<int, Status>();
        static bool EnableFlag = false;

        static SceneViewFovControl()
        {
            Enable(true);
        }

        public static void Enable(bool enable)
        {
            if (enable != EnableFlag)
            {
                if (enable)
                {
                    SceneView.beforeSceneGui -= OnScene;
                    SceneView.beforeSceneGui += OnScene;
                    SceneView.duringSceneGui -= OnSceneGUI;
                    SceneView.duringSceneGui += OnSceneGUI;
                }
                else
                {
                    SceneView.beforeSceneGui -= OnScene;
                    SceneView.duringSceneGui -= OnSceneGUI;
                }
                EnableFlag = enable;
            }
            SceneView.RepaintAll();
        }

        static Status GetOrAddStatus(SceneView sceneView)
        {
            Status s = null;
            if (sceneView == null || sceneView.camera == null)
            {
                return s;
            }
            int key = sceneView.GetInstanceID();
            if (!statuses.TryGetValue(key, out s))
            {
                s = new Status();
                statuses[key] = s;
            }
            return s;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            Status s = GetOrAddStatus(sceneView);
            if (s != null)
            {
                Handles.BeginGUI();
                s.OnSceneGUI(sceneView);
                Handles.EndGUI();
            }
        }

        static void OnScene(SceneView sceneView)
        {
            Status s = GetOrAddStatus(sceneView);
            if (s != null)
            {
                s.OnScene(sceneView);
            }
        }
    }

} // namespace
