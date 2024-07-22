using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThinRL.ProfilerHub.RTDebug
{
    public class RTDebugUpdater : MonoBehaviour
    {
        private ThinRLRTDebugSettingsAsset m_DebuggerAsset;
        private static RTDebugUpdater m_Instance = null;

        public static RTDebugUpdater Instance
        {
            get
            {
                var com = FindObjectOfType<RTDebugUpdater>();
                if (com != null)
                {
                    m_Instance = com;
                }
                else
                {
                    m_Instance = null;
                    if (m_Instance == null)
                    {
                        GameObject go = new GameObject();
                        //DontDestroyOnLoad(go);
                        go.name = "Mono Singleton:" + typeof(RTDebugUpdater).ToString();
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localEulerAngles = Vector3.zero;
                        go.transform.localScale = Vector3.one;
                        m_Instance = go.AddComponent<RTDebugUpdater>();
                    }
                }
                return m_Instance;
            }
        }


#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void OnInitialize()
        {
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= OnPlayStateChanged;
                UnityEditor.EditorApplication.playModeStateChanged += OnPlayStateChanged;

                // DestroyInstance();
                // s_Instance = null;
                EditorSceneManager.activeSceneChangedInEditMode -= ChangedActiveScene;
                EditorSceneManager.activeSceneChangedInEditMode += ChangedActiveScene;
            }

        }

        static void OnPlayStateChanged(PlayModeStateChange playModeStateChange)
        {

        }

        static void ChangedActiveScene(Scene current, Scene next)
        {

        }

#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnAfterSceneLoadRuntimeMethod()
        {
        }

        static void ShowOverdrawText()
        {

        }

        static void DestroyInstance()
        {
            var com = FindObjectOfType<RTDebugUpdater>();
            if (com == null)
            {
                m_Instance = null;
                return;
            }
#if UNITY_EDITOR
            bool isHierarchyGo = !EditorUtility.IsPersistent(com);
#else
                bool isHierarchyGo = true;
#endif
            if (isHierarchyGo)
            {
                GameObject.DestroyImmediate(com.gameObject);
            }

            m_Instance = null;
        }


        private void OnEnable()
        {
            RTDebugger.Init();
#if UNITY_EDITOR
            //���� debugSetting��ֻ���˱༭���µ�debug��û���ݴ����ġ�Ҫ��runtime�õĻ�����΢�ӵ㶫��Ҳ�ܼ�
            var path = AssetDatabase.GUIDToAssetPath("b4215d735ae1a4048b26e11e311d8e9c");
            m_DebuggerAsset = AssetDatabase.LoadAssetAtPath<ThinRLRTDebugSettingsAsset>(path);
#else
            m_DebuggerAsset = ScriptableObject.CreateInstance<ThinRLRTDebugSettingsAsset>();
#endif

        }

        // Update is called once per frame
        void Update()
        {
            //����shader���� Ϊ�˸�debuggerʹ��
            Shader.SetGlobalFloat(Shader.PropertyToID("_InBlack"), m_DebuggerAsset.ColorRangeMin * 255);
            Shader.SetGlobalFloat(Shader.PropertyToID("_InWhite"), m_DebuggerAsset.ColorRangeMax * 255);
            Shader.SetGlobalInt(Shader.PropertyToID("_InGamma"), m_DebuggerAsset.Gamma == true ? 0 : 1);
        }
    }
}

