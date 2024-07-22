using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ThinRL.Core.Editor;

namespace ThinRL.ProfilerHub.Editor.AssetMemoryStats
{
    public class AssetMemoryStatsWindow : EditorWindow
    {
        EditorAssetUtil.CommonAssetData m_AssetMemoryStats;
        Vector2 m_ScrollPosition;

        bool m_FilterAsset = true;
        string m_AnalyzeTextureStr = "图片";
        bool m_isAnalyzeAtlas = false;
        EditorAssetUtil.SpriteMemoryMode m_AtlasMode = EditorAssetUtil.SpriteMemoryMode.Texture;
        long memSizeWithoutFilteredAsset = 0;
        static bool s_ShowCurrentPlatformMemory = true;

        private SerializedObject m_SerializedObj;
        private SerializedProperty m_PrefixFilterList;
        private SerializedProperty m_SuffixFilterList;
        [SerializeField]
        private List<string> m_PathPrefixFilterList =new List<string>();
        [SerializeField]
        private List<string> m_PathSuffixFilterList = new List<string>();

        // 检查一个资源及其依赖的内存，并以窗口显示
        //[MenuItem("Assets/资源分析/显示内存占用", false, 100)]//菜单将丢弃，替换下面的
        //[MenuItem("ThinRL/ProfilerHub/资源内存分析")]
        [MenuItem("ThinRedLine/ProfilerHub/资源内存分析",false,100)]
        public static void ExportChartByRightClick()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorAssetUtil.CommonAssetData data = EditorAssetUtil.GetAssetCommonData(assetPath, s_ShowCurrentPlatformMemory, EditorAssetUtil.SpriteMemoryMode.Texture);
            if (data != null)
            {
                Debug.Log(EditorAssetUtil.GetMBString(data.memorySize));
                OpenWindow(data);
            }
            else
            {
                Debug.Log("不支持此格式文件");
            }
        }
        static void OpenWindow(EditorAssetUtil.CommonAssetData assetMemoryStats)
        {
            AssetMemoryStatsWindow w = (AssetMemoryStatsWindow)EditorWindow.GetWindow(typeof(AssetMemoryStatsWindow));
            w.Show();
            w.m_AssetMemoryStats = assetMemoryStats;

            w.memSizeWithoutFilteredAsset = EditorAssetUtil.GetMemorySizeWithoutFilteredAsset(assetMemoryStats, null, null);
        }

        private void OnEnable()
        {
            m_SerializedObj = new SerializedObject(this);

            m_PrefixFilterList = m_SerializedObj.FindProperty("m_PathPrefixFilterList");
            m_SuffixFilterList = m_SerializedObj.FindProperty("m_PathSuffixFilterList");

            m_PathPrefixFilterList.Clear();
            m_PathPrefixFilterList.Add("Assets/Resources");
        }

        private void OnDisable()
        {

        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("选中的资源:");
            EditorGUILayout.PropertyField(m_PrefixFilterList);
            var selectedObj = EditorGUILayout.ObjectField(Selection.activeObject, typeof(Object), false);
            if (selectedObj != null && GUILayout.Button("分析被选资源"))
            {
                var path = AssetDatabase.GetAssetOrScenePath(selectedObj);
                EditorAssetUtil.CommonAssetData data = EditorAssetUtil.GetAssetCommonData(path, s_ShowCurrentPlatformMemory, m_AtlasMode);
                if (data != null)
                {
                    OpenWindow(data);
                }
                else
                {
                    Debug.Log("不支持此格式文件");
                }
                GUILayout.EndHorizontal();
                //return;
            }
            else
                GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (m_AssetMemoryStats == null)
                return;

            var stats = m_AssetMemoryStats;
            var mainAsset = AssetDatabase.LoadAssetAtPath<Object>(stats.assetPath);

            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.yellow;

            GUILayout.BeginHorizontal();

            GUILayout.Label("此处显示资源在移动端的内存占用，跟编辑器inspector显示的贴图内存可能不一样");
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("主资源", GUILayout.Width(150));
            mainAsset = EditorGUILayout.ObjectField(mainAsset, typeof(Object), false);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("选项", GUILayout.Width(150));
            style.normal.textColor = Color.green;
            if (GUILayout.Button($"刷新", style))
            {
                EditorAssetUtil.CommonAssetData data = EditorAssetUtil.GetAssetCommonData(stats.assetPath, s_ShowCurrentPlatformMemory, m_AtlasMode);
                if (data != null)
                {
                    OpenWindow(data);
                }
                else
                {
                    Debug.Log("不支持此格式文件");
                }
                //return;
            }
            if (GUILayout.Button($"显示当前平台实际大小"))
            {
                s_ShowCurrentPlatformMemory = true;
                EditorAssetUtil.CommonAssetData data = EditorAssetUtil.GetAssetCommonData(stats.assetPath, s_ShowCurrentPlatformMemory, m_AtlasMode);
                if (data != null)
                {
                    OpenWindow(data);
                }
                else
                {
                    Debug.Log("不支持此格式文件");
                }
                //return;
            }
            if (GUILayout.Button($"显示Android平台大小"))
            {
                s_ShowCurrentPlatformMemory = false;
                EditorAssetUtil.CommonAssetData data = EditorAssetUtil.GetAssetCommonData(stats.assetPath, s_ShowCurrentPlatformMemory, m_AtlasMode);
                if (data != null)
                {
                    OpenWindow(data);
                }
                else
                {
                    Debug.Log("不支持此格式文件");
                }
                //return;
            }
            m_AnalyzeTextureStr = "图片";
            m_AtlasMode = EditorAssetUtil.SpriteMemoryMode.Texture;

            bool filterAssetNew = GUILayout.Toggle(m_FilterAsset, "过滤资源");

            if (filterAssetNew != m_FilterAsset)
            {
                m_FilterAsset = filterAssetNew;
                EditorAssetUtil.CommonAssetData data = EditorAssetUtil.GetAssetCommonData(stats.assetPath, s_ShowCurrentPlatformMemory, m_AtlasMode);
                OpenWindow(data);
                //return;
            }
            GUILayout.EndHorizontal();
            if (m_FilterAsset == true)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(100);
                GUILayout.BeginVertical();
                
                EditorGUILayout.PropertyField(m_PrefixFilterList);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.Label($"资源内存:{EditorAssetUtil.GetMBString(stats.memorySize)} MB", GUILayout.Width(150));
            GUILayout.Label($"过滤后资源内存 :{EditorAssetUtil.GetMBString(memSizeWithoutFilteredAsset)} MB", GUILayout.Width(150));
            GUILayout.Label($"当前统计:{m_AnalyzeTextureStr}", GUILayout.Width(150));

            GUILayout.Space(20);

            GUILayout.Label($"依赖资源(过滤{(m_FilterAsset ? "开启" : "关闭")})");

            GUILayout.BeginHorizontal();
            const int col1Width = 70;
            const int col2Width = 100;
            const int col3Width = 200;
            GUILayout.Label("累加MB", GUILayout.Width(col1Width));
            GUILayout.Label("自身内存MB", GUILayout.Width(col1Width));
            GUILayout.Label("包含依赖MB", GUILayout.Width(col2Width));
            GUILayout.Label("文件对象", GUILayout.Width(col3Width));
            GUILayout.Label("文件路径");
            GUILayout.EndHorizontal();

            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            if (stats.depsList != null)
            { 
                long memSum = 0;

                foreach (var dep in stats.depsList)
                {
                    if (m_FilterAsset && EditorAssetUtil.IsFilteredAsset(dep.assetPath, m_PathPrefixFilterList, m_PathSuffixFilterList))
                        continue;

                    GUILayout.BeginHorizontal();
                    memSum += dep.memorySize;
                    GUILayout.Label(EditorAssetUtil.GetMBString(memSum), GUILayout.Width(col1Width));
                    GUILayout.Label(EditorAssetUtil.GetMBString(dep.memorySize), GUILayout.Width(col1Width));
                    EditorGUILayout.ObjectField(dep.assetObj, typeof(Object), false, GUILayout.Width(col3Width));
                    GUILayout.Label($"{dep.assetPath} : [{dep.assetObj.GetType()}]");
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();

        }

        // 如果asset有对应类型的过滤配置，就用对应类型的，否则用默认
        //static void FindFilterConfig(string mainAssetPath, out List<string> pathPrefixFilter, out List<string> fileSuffixFilter)
        //{
        //    //foreach (var v in AssetAnalyzerConfig.Instance.memoryAnalyzetionSheetConfig)
        //    //{
        //    //    if (mainAssetPath.StartsWith(v.assetPath))
        //    //    {
        //    //        if (v.useCustomDepMemoryFilter)
        //    //        {
        //    //            pathPrefixFilter = v.customDepMemoryFilterPathPrefix;
        //    //            fileSuffixFilter = v.customDepMemoryFilterFileSuffix;
        //    //            return;
        //    //        }
        //    //        break;
        //    //    }
        //    //}
        //    pathPrefixFilter = m_PathPrefixFilterList;
        //    fileSuffixFilter = null;
        //}
    }

}
