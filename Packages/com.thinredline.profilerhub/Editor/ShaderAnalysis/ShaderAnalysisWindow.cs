using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Reflection;

namespace ThinRL.ProfilerHub.ShaderAnalysis
{
    public class ShaderAnalysisWindow : EditorWindow
    {
        //机型GPU，核心数，分辨率，GPU频率        （4 * 650M / (60 * 2340 * 1080)
        //估算机型：小米10（Adreno650，8核，2340*1080，587MHZ），Quest2（Adreno865，8核，3664*1920，525MHZ）

        ShaderAnalysisWindow()
        {
            this.titleContent = new GUIContent("Shader Analysis");
        }

        private string m_AnalysisResult = "";
        private Vector2 scrollPos;
        private Vector2 scrollPos2;

        [MenuItem("ThinRedLine/ProfilerHub/Shader Analysis Window")]
        static void showWindow()
        {
            var window = EditorWindow.GetWindow(typeof(ShaderAnalysisWindow));
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            ShaderListArea();

            GUILayout.Space(5);
            GUI.skin.label.fontSize = 18;
            GUILayout.Label("Analysis Shader Result");
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2, GUILayout.MaxHeight(650));
            if (m_AnalysisResult == "")
            {
                GUILayout.TextArea(m_AnalysisResult, GUILayout.Height(440));
            }
            else
            {
                GUILayout.TextArea(m_AnalysisResult);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUI.skin.label.fontSize = 11;
            GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
            GUILayout.Label("Output file path: Temp/ShaderAnalysis/FinalOutput.txt");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("分析选中的Shader", GUILayout.Width(120)))
            {
                //OnClickResetButton();
                var folderPath = Application.dataPath;
                folderPath = folderPath.Replace("Assets", "Temp");
                var shader = Selection.activeObject;
                if (shader != null && shader is Shader)
                {
                    var name = shader.name;
                    var resName = "Compiled-" + name.Replace("/", "-") + ".shader";
                    resName = resName.Replace(folderPath + "\\", "");
                    //Debug.Log("fzy 11:" + resName + "       " + folderPath + "    " + Path.Combine(folderPath, resName));
                    var filePath = Path.Combine(folderPath, resName);
                    if (!File.Exists(filePath))
                    {
                        GenerateCompileCodeWithTargetShader(shader as Shader);
                    }
                    OnClickAnalysisButton(filePath);
                }

            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void GenerateCompileCodeWithTargetShader(Shader shader)
        {
            Assembly asm = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
            System.Type t2 = asm.GetType("UnityEditor.ShaderUtil");
            var refInspectorPlatformType = asm.GetType("UnityEditor.ShaderInspectorPlatformsPopup");
            var refCurrentMode = refInspectorPlatformType.GetProperty("currentMode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            var refCurrentPlatformMask = refInspectorPlatformType.GetProperty("currentPlatformMask", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            var refcurrentVariantStripping = refInspectorPlatformType.GetProperty("currentVariantStripping", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            var currentvariantStripping = int.Parse(refcurrentVariantStripping.ToString());
            MethodInfo method = t2.GetMethod("OpenCompiledShader", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            {
                method.Invoke(null, new System.Object[] { shader, refCurrentMode, refCurrentPlatformMask, currentvariantStripping == 0, false, false });
            }
            //var s = method.Invoke()
        }

        private void ShaderListArea()
        {
            GUILayout.Space(10);
            GUI.skin.label.fontSize = 18;
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            var folderPath = Application.dataPath;
            folderPath = folderPath.Replace("Assets", "Temp");

            var fileList = Directory.GetFiles(folderPath);
            List<string> shaderList = new List<string>();
            foreach (var fileName in fileList)
            {
                if (fileName.Contains(".shader") && fileName.Contains("Compiled"))
                {
                    shaderList.Add(fileName);
                }
            }
            var _shaderList = shaderList.ToArray();
            int minCount = _shaderList.Length;
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(125));

            for (int i = 0; i < minCount; i++)
            {
                var shaderPath = _shaderList[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(shaderPath.Replace(folderPath + "\\", ""));
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Analysis", GUILayout.Width(100)))
                {
                    OnClickAnalysisButton(shaderPath);

                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);
            }

            GUILayout.EndScrollView();
            GUI.skin.label.fontSize = 11;
            GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
            if (minCount == 0)
                GUILayout.Label("There is no COMPILED shader file.");
            else
            {
                GUILayout.Space(5);
            }

        }

        private MaliResultInfo m_ResultInfoData;
        private async void OnClickAnalysisButton(string shaderPath)
        {
            var scriptObj = MonoScript.FromScriptableObject(this);
            var pp = AssetDatabase.GetAssetPath(scriptObj);
            Debug.Log("fzy PPP:" + pp);
            var res = await MaliCompilerUtils.ShaderMaliAnalysis(shaderPath);
            m_ResultInfoData = res.Item2;
            m_AnalysisResult = "";
            //m_AnalysisResult = m_ResultInfoData.ToString();
            var resStr = "";
            for (int i = 0;i<m_ResultInfoData.data.Length;i++)
            {
                var data = m_ResultInfoData.data[i];
                float avg_arith_total = 0;
                float avg_arith_fma = 0;
                float avg_arith_cvt = 0;
                float avg_arith_suf = 0;
                float avg_load_store = 0;
                float avg_texture = 0;
                float[] avgArray = new float[6] { avg_arith_total, avg_arith_fma, avg_arith_cvt, avg_arith_suf, avg_load_store, avg_texture };
                for(int j =0;j< data.shaders.Length;j++)
                {
                    if(data.shaders[j].variants == null)
                    {
                        Debug.LogError("fzy 又出错内容，注意检查");
                        continue;
                    }
                    int c = data.shaders[j].variants.Length;
                    for (int k = 0; k < c; k++)
                    {
                        avgArray[0] += data.shaders[j].variants[k].performance.total_cycles.cycle_count[0];
                        avgArray[1] += data.shaders[j].variants[k].performance.total_cycles.cycle_count[1];
                        avgArray[2] += data.shaders[j].variants[k].performance.total_cycles.cycle_count[2];
                        avgArray[3] += data.shaders[j].variants[k].performance.total_cycles.cycle_count[3];
                        avgArray[4] += data.shaders[j].variants[k].performance.total_cycles.cycle_count[4];
                        avgArray[5] += data.shaders[j].variants[k].performance.total_cycles.cycle_count[5];
                    }
                    avgArray[0] /= c;
                    avgArray[1] /= c;
                    avgArray[2] /= c;
                    avgArray[3] /= c;
                    avgArray[4] /= c;
                    avgArray[5] /= c;
                    float max = -100.0f;
                    for(int t = 0; t < 6; t++)
                    {
                        max = Mathf.Max(max, avgArray[t]);
                    }
                    var bounds = string.Join(",", data.shaders[j].variants[0].performance.total_cycles.bound_pipelines);
                    m_AnalysisResult += data.ToString();
                    m_AnalysisResult += $"可能存在瓶颈的部分为：{bounds}  =>  Max Bounds={max} \n";
                    //（4 * 650M / (60 * 2340 * 1080)
                    //小米10（Adreno650，8核，2340*1080，587MHZ），Quest2（Adreno865，8核，3664*1920，525MHZ）
                    float a1 = (float)((8 * 587 * 1000000.0) / (60 * 2340 * 1080.0)); //mi10
                    float a2 = (float)((8 * 525 * 1000000.0) / (60 * 2340 * 1080.0)); //quest2
                    m_AnalysisResult += $"小米10下，要跑满60FPS，可执行cycle能力估计为：{a1}     overdraw承受力估计：{a1/Mathf.Max(1,max)} \n";
                    m_AnalysisResult += $"quest2下，要跑满60FPS，可执行cycle能力估计为：{a2}     overdraw承受力估计：{a2/ Mathf.Max(1, max)}  \n";
                    if(max>29)
                    {
                        m_AnalysisResult += "  结论：有比较大的消耗！！！！！！！\n";
                    }
                    else if(max>14)
                    {
                        m_AnalysisResult += "  结论：有中度消耗！\n";
                    }
                    else if(max >6)
                    {
                        m_AnalysisResult += "  结论：微小的消耗，可忽略不计\n";
                    }
                    else
                    {
                        m_AnalysisResult += "  结论：无性能问题\n";
                    }
                    m_AnalysisResult += "\n\n";
                }
                

            }

            //m_AnalysisResult += "fasdfasfds";
            Debug.Log(m_ResultInfoData.ToString());
        }

        private void OnClickResetButton()
        {
            m_AnalysisResult = "";
        }

        private void OnClickClearButton()
        {
            if (EditorUtility.DisplayDialog("Clear File", "Delete ALL compiled shader file?", "Delete", "Cancle"))
            {
                var folderPath = Application.dataPath;
                folderPath = folderPath.Replace("Assets", "Temp");

                var fileList = Directory.GetFiles(folderPath);
                foreach (var fileName in fileList)
                {
                    if (fileName.Contains(".shader") && fileName.Contains("Compiled"))
                    {
                        File.Delete(fileName);
                    }
                }
            }
            m_AnalysisResult = "";
        }

    }

}
