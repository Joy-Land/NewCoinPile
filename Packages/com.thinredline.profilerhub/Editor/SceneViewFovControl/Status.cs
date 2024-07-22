// https://github.com/anchan828/unitejapan2014/tree/master/SyncCamera/Assets
// todo: near/far clip control
// todo: skybox doesn't follow FoV
#define SCENE_VIEW_FOV_CONTROL_USE_GUI_BUTTON

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

#if !UNITY_EDITOR
#error This script must be placed under "Editor/" directory.
#endif

namespace ThinRL.ProfilerHub.Editor.SceneViewFovControl
{

    class Status 
    {
        float fov = 0.0f;
        bool reset = false;
        bool autoFov = true;
        float lastOnSceneGuiFov = 0.0f;
        WeakReference slaveCamera = new WeakReference(null);
        // 游戏相机同步给场景相机，还是反过来
        bool syncToSceneCamera = true;
        // 猜测哪个game view相机正在生效
        bool guessActualGameCam = false;

        bool previousAutoFov = false;
        System.Object previousSlaveCamera = null;

        const string ButtonStringFovAuto = "FoV:Auto";
        const string ButtonStringFovUser = "FoV:{0:0.00}";
        const string ButtonStringFovAutoWithSlave = "FoV:Auto{0}{1}";
        const string ButtonStringFovUserWithSlave = "FoV:{0:0.00}{1}{2}";
        const string SlaveCameraSubMenu = "同步sceneView相机/{0}{1}";

    #if SCENE_VIEW_FOV_CONTROL_USE_GUI_BUTTON
        string buttonString = "";

        enum MouseButton {
            None,
            Left,
            Right,
            Middle
        }
    #else
        GUIContent ButtonContent = null;
    #endif

        public void OnScene(SceneView sceneView) {
            if(sceneView == null
            || sceneView.camera == null
            || sceneView.in2DMode
            ) {
                return;
            }

            Camera sceneCam = sceneView.camera;

            if(!autoFov) {
                if(fov == 0.0f || reset) {
                    fov = sceneCam.fieldOfView;
                    reset = false;
                }

                var ev = Event.current;
                var settings = Settings.Data;
                float deltaFov = 0.0f;

                if(ev.modifiers == settings.ModifiersNormal || ev.modifiers == settings.ModifiersQuick) {
                    if(ev.type == EventType.ScrollWheel) {
                        // note : In MacOS, ev.delta becomes zero when "Shift" pressed.  I don't know the reason.
                        deltaFov = ev.delta.y;
                        ev.Use();
                    } else if(ev.type == EventType.KeyDown && ev.keyCode == settings.KeyCodeIncreaseFov) {
                        deltaFov = +1.0f;
                        ev.Use();
                    } else if(ev.type == EventType.KeyDown && ev.keyCode == settings.KeyCodeDecreaseFov) {
                        deltaFov = -1.0f;
                        ev.Use();
                    }
                }

                if(deltaFov != 0.0f) {
                    deltaFov *= settings.FovSpeed;
                    if(ev.modifiers == settings.ModifiersQuick) {
                        deltaFov *= settings.FovQuickMultiplier;
                    }
                    fov += deltaFov;
                    fov = Mathf.Clamp(fov, settings.MinFov, settings.MaxFov);
                }

                sceneCam.fieldOfView = fov;
            }

            if (guessActualGameCam)
            {
                // 把能渲染default层的最好深度相机同步到scene
                var cam = FindMaxDepthCameraWithCullMaskLayer(LayerMask.NameToLayer("Default"));
                if (cam != null)
                {
                    if (syncToSceneCamera)
                        CopyCameraInfo(from: cam, to: sceneCam);
                    else
                        CopyCameraInfo(from: sceneCam, to: cam);
                }
            }
            else if(HasSlaveCamera()) {
                if (syncToSceneCamera)
                    CopyCameraInfo(from: GetSlaveCamera(), to: sceneCam);
                else
                    CopyCameraInfo(from: sceneCam, to: GetSlaveCamera());
            }
        }

        Camera FindMaxDepthCameraWithCullMaskLayer(int layer)
        {
            float maxDepth = float.MinValue;
            Camera result = null;
            foreach (var c in Camera.allCameras)
            {
                if ((c.cullingMask & (1 << layer)) != 0 && c.depth > maxDepth)
                {
                    result = c;
                    maxDepth = c.depth;
                }
            }
            return result;
        }

        public static void CopyCameraInfo(Camera from, Camera to) {
            if(from == null || to == null) {
                return;
            }
            to.fieldOfView = from.fieldOfView;
            to.orthographic = from.orthographic;
            to.orthographicSize = from.orthographicSize;
            to.nearClipPlane = from.nearClipPlane;
            to.farClipPlane = from.farClipPlane;
            to.gameObject.transform.position = from.transform.position;
            to.gameObject.transform.rotation = from.transform.rotation;
        }

        public void OnSceneGUI(SceneView sceneView) {
            {
                bool f = false;
                if(!autoFov && lastOnSceneGuiFov != fov) {
                    lastOnSceneGuiFov = fov;
                    f = true;
                }
                if(previousAutoFov != autoFov) {
                    previousAutoFov = autoFov;
                    f = true;
                }
                if(GetSlaveCamera() as System.Object != previousSlaveCamera) {
                    previousSlaveCamera = GetSlaveCamera() as System.Object;
                    f = true;
                }
    #if !SCENE_VIEW_FOV_CONTROL_USE_GUI_BUTTON
                if(ButtonContent == null) {
                    f = true;
                }
    #endif
                if(f) {
                    string s;
                    var dirChar = syncToSceneCamera ? "<" : ">";
                    if(autoFov) {
                        if(HasSlaveCamera()) {
                            s = string.Format(ButtonStringFovAutoWithSlave, dirChar, GetSlaveCameraName());
                        } else {
                            s = ButtonStringFovAuto;
                        }
                    } else {
                        if(HasSlaveCamera()) {
                            s = string.Format(ButtonStringFovUserWithSlave, fov, dirChar, GetSlaveCameraName());
                        } else {
                            s = string.Format(ButtonStringFovUser, fov);
                        }
                    }
    #if SCENE_VIEW_FOV_CONTROL_USE_GUI_BUTTON
                    buttonString = s;
    #else
                    ButtonContent = new GUIContent(s);
    #endif
                }
            }

    #if SCENE_VIEW_FOV_CONTROL_USE_GUI_BUTTON
            MouseButton mouseButton = MouseButton.None;
            {
                var e = Event.current;
                if(e != null && e.type == EventType.MouseUp) {
                    switch(e.button) {
                    default:
                    case 0: mouseButton = MouseButton.Left;     break;
                    case 1: mouseButton = MouseButton.Right;    break;
                    case 2: mouseButton = MouseButton.Middle;   break;
                    }
                }
            }
            GUIStyle style = EditorStyles.miniButton;
            if (GUI.Button(new Rect(8, 8, 160, 24), buttonString, style)) {
                switch(mouseButton) {
                default:                                                    break;
                case MouseButton.Left:  OnFovButtonLeftClicked(sceneView);  break;
                case MouseButton.Right: OnFovButtonRightClicked(sceneView); break;
                }
            }
    #else
            GUIStyle style = EditorStyles.toolbarDropDown;
            sceneView.DoToolbarRightSideGUI(ButtonContent, style, (rect) => {
                int btn = -1;
                if(Event.current.type == EventType.MouseUp) {
                    btn = Event.current.button;
                }
                if (GUI.Button(rect, ButtonContent, style)) {
                    if(btn == 1) {
                        OnFovButtonRightClicked(sceneView);
                    } else {
                        OnFovButtonLeftClicked(sceneView);
                    }
                }
            });
    #endif
        }

        void SetAutoFov(bool auto) {
            if(autoFov != auto) {
                autoFov = auto;
                if(!autoFov) {
                    reset = true;
                }
            }
        }

        // This procedure will be called when "FoV" button is left-clcked.
        void OnFovButtonLeftClicked(SceneView sceneView) {
            SetAutoFov(!autoFov);
        }

        // This procedure will be called when "FoV" button is right-clcked.
        void OnFovButtonRightClicked(SceneView sceneView) {
            var menu = new GenericMenu();

            menu.AddItem(
                new GUIContent("FoV : 默认")
                , autoFov
                , (obj) => {
                    SetAutoFov(true);
                }
                , 0
            );

            menu.AddItem(
                new GUIContent("FoV : 手动")
                , !autoFov
                , (obj) => {
                    SetAutoFov(false);
                }
                , 0
            );

            menu.AddSeparator(string.Empty);

            menu.AddItem(
                new GUIContent("解除sceneView相机同步")
                , false
                , (obj) => {
                    guessActualGameCam = false;
                    SetSlaveCamera(null);
                }
                , 0
            );

            {
                menu.AddSeparator(string.Format(SlaveCameraSubMenu, string.Empty, string.Empty));
                // guess which actual game is in use, and sync to scene view cam
                menu.AddItem(
                    new GUIContent(string.Format(SlaveCameraSubMenu, "from:", "auto"))
                    , guessActualGameCam
                    , () => {
                        guessActualGameCam = true;
                        syncToSceneCamera = true;
                    }
                );

                // game cam to scene view cam
                foreach(var camera in Camera.allCameras) {
                    menu.AddItem(
                        new GUIContent(string.Format(SlaveCameraSubMenu, "from:", camera.name))
                        , IsSlaveCamera(camera, true)
                        , (obj) => {
                            guessActualGameCam = false;
                            syncToSceneCamera = true;
                            SetSlaveCamera(obj as Camera);
                        }
                        , camera
                    );
                }
                
                // scene view cam to game came
                foreach(var camera in Camera.allCameras) {
                    menu.AddItem(
                        new GUIContent(string.Format(SlaveCameraSubMenu, "to:", camera.name))
                        , IsSlaveCamera(camera, false)
                        , (obj) => {
                            guessActualGameCam = false;
                            syncToSceneCamera = false;
                            SetSlaveCamera(obj as Camera);
                        }
                        , camera
                    );

                }
            }

            menu.ShowAsContext();
        }

        bool HasSlaveCamera() {
            return GetSlaveCamera() != null;
        }

        bool IsSlaveCamera(Camera camera, bool syncToSceneCamera) {
            return camera == GetSlaveCamera() && this.syncToSceneCamera == syncToSceneCamera;
        }

        Camera GetSlaveCamera() {
            if(! slaveCamera.IsAlive) {
                return null;
            }
            return slaveCamera.Target as Camera;
        }

        string GetSlaveCameraName() {
            var camera = GetSlaveCamera();
            if(camera != null) {
                return camera.name;
            }
            return "Camera(null)";
        }

        void SetSlaveCamera(Camera camera) {
            slaveCamera = new WeakReference(camera);
        }
    }

} // namespace
