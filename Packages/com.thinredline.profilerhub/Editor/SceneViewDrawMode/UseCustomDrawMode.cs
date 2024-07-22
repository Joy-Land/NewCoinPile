using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace ThinRL.ProfilerHub.Editor.SceneViewDrawMode
{
    //====================== The custom draw modes =========================
    //internal static readonly PrefColor kSceneViewWire = new PrefColor("Scene/Wireframe", 0.0f, 0.0f, 0.0f, 0.5f);
    //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Annotation/SceneRenderModeWindow.cs
    //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/SceneView/SceneView.cs
    //GameView? https://forum.unity.com/threads/creating-a-totally-custom-scene-editor-in-the-editor.118065/

    [InitializeOnLoad]
    public class UseCustomDrawMode
    {
        private static Camera cam;
        private static bool delegateSceneView = false;

        static bool s_NeedRecoverSceneVeiwState = false;
        static SceneView.SceneViewState s_ScenceViewOrigState;

        static UseCustomDrawMode()
        {
            EditorApplication.update += Update;
        }

        static void Update()
        {
            if (!delegateSceneView && SceneView.sceneViews.Count > 0)
            {
                SceneView.beforeSceneGui -= OnBeforeDrawScene;
                SceneView.beforeSceneGui += OnBeforeDrawScene;
                SceneView.duringSceneGui -= OnScene;
                SceneView.duringSceneGui += OnScene;
                delegateSceneView = true;
            }

            if (SceneView.sceneViews.Count == 0)
            {
                SceneView.beforeSceneGui -= OnBeforeDrawScene;
                SceneView.duringSceneGui -= OnScene;
                delegateSceneView = false;
            }

        }

        // sceneView回调，在渲染之后调用
        private static void OnScene(SceneView sceneView)
        {
            RunDrawMode();
        }

        // sceneView回调，在渲染之前调用
        private static void OnBeforeDrawScene(SceneView sceneView)
        {
            // overdraw需要黑色背景来标明颜色叠加程度
            if (sceneView.cameraMode.drawMode == DrawCameraMode.UserDefined)
            {
                var lowerName = sceneView.cameraMode.name.ToLower();
                if (lowerName.Contains("overdraw"))
                {
                    sceneView.camera.backgroundColor = Color.black;
                    // editor内部在setupCamera和OnBeforeDrawScene之间会执行ClearCamera
                    // 所以只设置bgColor无效，需要自己在clear一次
                    Handles.ClearCamera(sceneView.camera.rect, sceneView.camera);
                }
                else if (lowerName.Contains("mipmap"))
                {
                    // rt分辨率影响mipmap，这里设定一个标准的用于查看mipmap的分辨率高度，再在shader里进行缩放
                    // 改sceneView rt尺寸的方式虽然可以，但是影响鼠标点选的范围，所以不采用。
                    RenderTexture sceneRt = sceneView.camera.targetTexture;
                    const int MipmapStarndardHeight = 720;
                    Shader.SetGlobalFloat("_MipmapScale", 1.0f * sceneRt.height / MipmapStarndardHeight);
                }
            }

        }

        static bool AcceptedDrawMode(SceneView.CameraMode cameraMode)
        {
            if (
                cameraMode.drawMode == DrawCameraMode.Wireframe ||
                cameraMode.drawMode == DrawCameraMode.TexturedWire ||
                cameraMode.drawMode == DrawCameraMode.Textured ||
                //cameraMode.drawMode == DrawCameraMode.Normal ||
                cameraMode.drawMode == DrawCameraMode.UserDefined
                )
                return true;

            return true;
        }

        static void GetCurrentSceneCam()
        {
            if (SceneView.currentDrawingSceneView == null)
            {
                if (SceneView.lastActiveSceneView != null)
                {
                    cam = SceneView.lastActiveSceneView.camera;
                }
            }
            else
            {
                cam = SceneView.currentDrawingSceneView.camera;
            }
        }

        static void OnDrawModeChange(SceneView.CameraMode mode)
        {
            // overdraw时不显示天空盒，而是现实clear color
            // 不是overdraw时恢复之前的值
            if (mode.drawMode == DrawCameraMode.UserDefined && mode.name.ToLower().Contains("overdraw"))
            {
                if (s_NeedRecoverSceneVeiwState == false)
                {
                    s_ScenceViewOrigState = new SceneView.SceneViewState(SceneView.lastActiveSceneView.sceneViewState);
                    SceneView.lastActiveSceneView.sceneViewState.SetAllEnabled(true);
                    SceneView.lastActiveSceneView.sceneViewState.showSkybox = false;
                }
                s_NeedRecoverSceneVeiwState = true;
            }
            else if (s_NeedRecoverSceneVeiwState)
            {
                s_NeedRecoverSceneVeiwState = false;
                SceneView.lastActiveSceneView.sceneViewState = s_ScenceViewOrigState;
            }

        }

        //******************************************* */

        static void RunDrawMode()
        {
            //Setup object
            if (!CustomDrawModeAssetObject.SetUpObject()) return;

            //Set camera
            GetCurrentSceneCam();

            //Setup draw mode
            SceneView.ClearUserDefinedCameraModes();
            for (int i = 0; i < CustomDrawModeAssetObject.cdma.customDrawModes.Length; i++)
            {
                var dm = CustomDrawModeAssetObject.cdma.customDrawModes[i];
                if (dm.name != "" && dm.category != "")
                    SceneView.AddCameraMode(dm.name, dm.category);
            }
            ArrayList sceneViewArray = SceneView.sceneViews;
            foreach (SceneView sceneView in sceneViewArray)
            {
                sceneView.onValidateCameraMode -= AcceptedDrawMode; // Clean up
                sceneView.onValidateCameraMode += AcceptedDrawMode;
                sceneView.onCameraModeChanged -= OnDrawModeChange;
                sceneView.onCameraModeChanged += OnDrawModeChange;
            }


            //Render with selected draw mode

            if (cam != null)
            {
                ArrayList sceneViewsArray = SceneView.sceneViews;
                foreach (SceneView sceneView in sceneViewsArray)
                {
                    bool m1success = false;
                    bool m2success = false;
                    bool m3success = false;
                    for (int i = 0; i < CustomDrawModeAssetObject.cdma.customDrawModes.Length; i++)
                    {
                        var dm = CustomDrawModeAssetObject.cdma.customDrawModes[i];
                        if (dm.name != "" && sceneView.cameraMode.name == dm.name)
                        {
                            if (dm.shader != null)
                            {
                                var config = CustomDrawModeAssetObject.cdma;
                                if (config.method == CustomDrawModeAsset.ImplemntVersion.AnotherCamDraw)
                                {
                                    cam.RenderWithShader(dm.shader, dm.renderType);
                                    m1success = true;
                                }
                                else if (config.method == CustomDrawModeAsset.ImplemntVersion.RepaintSceneView)
                                {
                                    // dont workk
                                    cam.SetReplacementShader(dm.shader, dm.renderType);
                                    sceneView.Repaint();
                                    m2success = true;
                                }
                                else if (config.method == CustomDrawModeAsset.ImplemntVersion.SceneViewReplaceShader)
                                {
                                    cam.backgroundColor = Color.black;
                                    sceneView.SetSceneViewShaderReplace(dm.shader, dm.renderType);
                                    m3success = true;
                                }
                            }
                            break;
                        }
                    }
                    //If nothing can be found
                    if (!m1success || !m2success)
                    {
                        cam.ResetReplacementShader();
                    }
                    if (!m3success)
                    {
                        sceneView.SetSceneViewShaderReplace(null, "");
                    }
                }
            }

        }

    }
}