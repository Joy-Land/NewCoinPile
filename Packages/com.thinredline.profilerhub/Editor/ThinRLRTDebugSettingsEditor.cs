
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThinRL.ProfilerHub.RTDebug
{
    [CustomEditor(typeof(ThinRLRTDebugSettingsAsset), editorForChildClasses: true)]
    public class FZYDebugSettingsEditor : UnityEditor.Editor
    {
        public ThinRLRTDebugSettingsAsset asset;

        private UnityEditor.SerializedObject m_SerializedObject;

        public SerializedProperty enableTextureDebugProp;
        public SerializedProperty colorRangeMinProp;
        public SerializedProperty colorRangeMaxProp;
        public SerializedProperty gammaProp;

        public SerializedProperty fullScrrenProp;

        private void OnEnable()
        {

            this.asset = serializedObject.targetObject as ThinRLRTDebugSettingsAsset;
            this.m_SerializedObject = serializedObject;

            this.enableTextureDebugProp = serializedObject.FindProperty("m_EnableTextureDebug");
            this.colorRangeMinProp = serializedObject.FindProperty("m_ColorRangeMin");
            this.colorRangeMaxProp = serializedObject.FindProperty("m_ColorRangeMax");
            this.gammaProp = serializedObject.FindProperty("m_Gamma");

            this.fullScrrenProp = serializedObject.FindProperty("m_FullScreen");

            //init asset data
            asset.EnableTextureDebug = enableTextureDebugProp.boolValue;
            asset.FullScreen = fullScrrenProp.boolValue;
            //asset.CurTextureIndex = ;
        }
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();


            bool enableTexDebug = enableTextureDebugProp.boolValue;
            enableTexDebug = EditorGUILayout.Toggle("开启RT调试", enableTexDebug);
            enableTextureDebugProp.boolValue = enableTexDebug;
            asset.EnableTextureDebug = enableTexDebug;


            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gammaProp);
            float m_leftValue = colorRangeMinProp.floatValue;
            float m_rightValue = colorRangeMaxProp.floatValue;
            EditorGUILayout.MinMaxSlider("色阶", ref m_leftValue, ref m_rightValue, 0f, 1f);
            colorRangeMinProp.floatValue = m_leftValue;
            colorRangeMaxProp.floatValue = m_rightValue;
            EditorGUI.indentLevel--;


            bool fullScreen = fullScrrenProp.boolValue;
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (fullScreen)
            {
                if (GUILayout.Button("全屏上张", GUILayout.Width(150)))
                {
                    int maxIndex = asset.TextureCount - 1;
                    if (asset.TextureCount == 0) maxIndex = 0;
                    int idx = asset.CurTextureIndex;
                    if (idx - 1 < 0) idx = maxIndex;
                    else idx = idx - 1;
                    //Debug.Log("fzy cccccccccccccccccccccc:" + maxIndex+"  "+ idx);
                    asset.CurTextureIndex = idx;
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(fullScreen == true ? "还原显示" : "全屏显示", GUILayout.Width(90)))
            {
                fullScreen = !fullScreen;
                fullScrrenProp.boolValue = fullScreen;
                asset.FullScreen = fullScreen;
            }
            GUILayout.FlexibleSpace();
            if (fullScreen)
            {
                if (GUILayout.Button("全屏下张", GUILayout.Width(150)))
                {
                    int maxIndex = asset.TextureCount - 1;
                    if (asset.TextureCount == 0) maxIndex = 0;
                    int idx = asset.CurTextureIndex;
                    if (idx + 1 > (maxIndex)) idx = 0;
                    else idx = idx + 1;
                    //Debug.Log("fzy cccccccccccccccccccccc:" + maxIndex + "  " + idx);
                    asset.CurTextureIndex = idx;
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("恢复到默认色阶[0-1]"))
            {
                colorRangeMinProp.floatValue = 0f;
                colorRangeMaxProp.floatValue = 1f;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {

        }
    }
}

