using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System;
using System.IO;
using UnityEditor;

namespace ThinRL.ProfilerHub.Editor.SceneViewDrawMode
{
    [CreateAssetMenu(fileName = "CustomDrawModeAsset", menuName = "ThinRedLine/ProfilerHub/CustomDrawModeAsset", order = 1)]
    public class CustomDrawModeAsset : ScriptableObject
    {
        [Serializable]
        public struct CustomDrawMode
        {
            public string name;
            public string category;
            public string renderType;
            public Shader shader;
        }

        /// 实现方式
        [Serializable]
        public enum ImplemntVersion
        {
            /// 额外渲染一遍，这一边不会画天空盒或clear颜色
            AnotherCamDraw = 0,
            /// 设置相机replacementShader，再执行sceneView.repaint
            RepaintSceneView = 1,
            /// 调用sceneView的replacementShader方法
            SceneViewReplaceShader = 2,


        }
        public ImplemntVersion method = ImplemntVersion.SceneViewReplaceShader;
        public CustomDrawMode[] customDrawModes;
    }

    public static class CustomDrawModeAssetObject
    {
        public static CustomDrawModeAsset cdma;

        public static bool SetUpObject()
        {
            if (cdma == null)
            {
                // 通过脚本目录得出asset目录
                var scriptParentPath = Path.GetDirectoryName(GetClassPath<CustomDrawModeAsset>());
                var assetPath = string.Format("{0}/CustomDrawModeAsset.asset", scriptParentPath);
                cdma = (CustomDrawModeAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(CustomDrawModeAsset));
            }
            if (cdma == null) return false; else return true;
        }

        /// <summary>
        /// 获取一个类的路径 如果没有 返回 "" 空字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetClassPath<T>() where T : ScriptableObject
        {
            var setting = ScriptableObject.CreateInstance<T>();

            MonoScript script = MonoScript.FromScriptableObject(setting);
            UnityEngine.Object.DestroyImmediate(setting);

            if (script == null)
            {
                Debug.Log("无法定义到" + typeof(T).Name + "类");
            }
            else
            {
                return AssetDatabase.GetAssetPath(script);
            }
            return "";
        }
    }
}