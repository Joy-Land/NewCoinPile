using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Linq;

namespace ThinRL.ProfilerHub.ShaderAnalysis
{
    public static class MaliCompilerUtils
    {


        private static int MAX_COMMAND_LENGTH = 8000;

        struct AnalysisResult
        {
            public string passName;
            public string keywords;
        }
        static Dictionary<string, AnalysisResult> analysisResultDic = new Dictionary<string, AnalysisResult>();
        public static Task<(ExecResult, MaliResultInfo)> ShaderMaliAnalysis(string fileName)
        {
            List<string> vertList = new List<string>();
            List<string> fragList = new List<string>();
            List<string> keywardsList = new List<string>();
            if (!File.Exists(fileName))
            {
                throw new Exception("文件不存在: " + fileName);
            }
            Debug.Log("fzy sss:" + fileName);
            StreamReader sr = new StreamReader(fileName);
            while (true)
            {
                string fileDataLine;
                fileDataLine = sr.ReadLine();
                if (fileDataLine == null)
                {
                    break;
                }
                if (fileDataLine.Contains("Keywords: "))
                {
                    var keywordsDataLine = fileDataLine;
                    var pos = keywordsDataLine.IndexOf("Keywords: ");
                    var keywardsArray = keywordsDataLine.Substring(pos + 10).Split(" ").ToList();
                    var keywards = string.Join("-", keywardsArray);
                    keywardsList.Add(keywards);
                }
                if (fileDataLine.Contains("#ifdef VERTEX"))
                {
                    ReadShader(sr, vertList);

                }
                else if (fileDataLine.Contains("#ifdef FRAGMENT"))
                {
                    ReadShader(sr, fragList);
                }
            }
            sr.Close();
            ClearFolderTempFile();
            var result = TempVertFragOutput(vertList, fragList, keywardsList);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return result;
        }

        private static void ClearFolderTempFile()
        {
            string outputDataPath = Application.dataPath.Replace("Assets", "Temp");
            if (!Directory.Exists(outputDataPath + "/ShaderAnalysis"))
            {
                Directory.CreateDirectory(outputDataPath + "/ShaderAnalysis");
            }
            if (!Directory.Exists(outputDataPath + "/ShaderAnalysis/tempOutput"))
            {
                Directory.CreateDirectory(outputDataPath + "/ShaderAnalysis/tempOutput");
            }
            string outputTempFolder = outputDataPath + "/ShaderAnalysis/tempOutput/";
            var fileList = Directory.GetFiles(outputTempFolder);
            foreach (var filePath in fileList)
            {
                File.Delete(filePath);
            }
        }

        private static void ReadShader(StreamReader sr, List<string> outlist)
        {
            int endFlag = 0;
            StringBuilder builder = new StringBuilder();
            string fileDataLine = sr.ReadLine();
            while (fileDataLine != null)
            {
                if (fileDataLine.Contains("#endif"))
                {
                    endFlag--;
                }
                if (fileDataLine.Contains("#if"))
                {
                    endFlag++;
                }
                if (endFlag < 0)
                    break;
                builder.AppendLine(fileDataLine);
                fileDataLine = sr.ReadLine();
            }
            var str = builder.ToString();
            if (str.Contains("#version 300 es"))
            {
                str = str.Replace("#version 300 es", "#version 320 es");
            }
            else if (str.Contains("#version 330"))
            {
                str = str.Replace("#version 330", "#version 320 es");
            }
            else if (str.Contains("#version 310"))
            {
                str = str.Replace("#version 310", "#version 320 es");
            }
            else if (str.Contains("#version 150"))
            {
                str = str.Replace("#version 150", "#version 320 es");
            }
            outlist.Add(str);
            //  Debug.Log(outlist[outlist.Count-1]);
        }

        private static async Task<(ExecResult, MaliResultInfo)> TempVertFragOutput(List<string> vertList, List<string> fragList, List<string> keywardList)
        {
            string outputDataPath = Application.dataPath.Replace("Assets", "Temp");
            //Debug.Log("fzy xxxx:" + outputDataPath);
            string outputTempFolder = outputDataPath + "/ShaderAnalysis/tempOutput/";
            int minCount = Math.Min(vertList.Count, fragList.Count);
            List<string> commandList = new List<string>();
            List<string> newKeywardList = new List<string>();
            string fragCommand = "malioc.exe  -f ";
            string vertCommand = "malioc.exe  -v ";
            string vertFileName = "vertexVarying{0}.vert";
            string fragFileName = "fragmentVarying{0}.frag";
            var command = string.Empty;
            StreamWriter sw;
            for (int i = 0; i < minCount; i++)
            {
                string tempVert = string.Format(vertFileName, i);
                string outputVertPath = outputTempFolder + tempVert;
                sw = new StreamWriter(outputVertPath, false);
                sw.Write(vertList[i]);
                sw.Close();
                command = vertCommand + $"{Application.dataPath.Replace("Assets", "Temp")}/ShaderAnalysis/tempOutput/" + tempVert + " -c Mali-G77 --format json" + " && ";
                commandList.Add(command + "exit");
                newKeywardList.Add(keywardList[i]);

                string tempFrag = string.Format(fragFileName, i);
                string outputFragPath = outputTempFolder + tempFrag;
                sw = new StreamWriter(outputFragPath, false);
                sw.Write(fragList[i]);
                sw.Close();
                command = fragCommand + $"{Application.dataPath.Replace("Assets", "Temp")}/ShaderAnalysis/tempOutput/" + tempFrag + " -c Mali-G77 --format json" + " && ";
                commandList.Add(command + "exit");
                newKeywardList.Add(keywardList[i]);
            }
            //Debug.Log("fzy xxxx:" + command);
            //if (minCount < vertList.Count)
            //{
            //    for (int i = minCount; i < vertList.Count; i++)
            //    {
            //        string tempVert = string.Format(vertFileName, i);
            //        string outputVertPath = outputTempFolder + tempVert;
            //        sw = new StreamWriter(outputVertPath, false);
            //        sw.Write(vertList[i]);
            //        sw.Close();
            //        command = vertCommand + $"{Application.dataPath.Replace("Assets", "Temp")}/ShaderAnalysis/tempOutput/" + tempVert + " --format json" + " && ";
            //        commandList.Add(command + "exit");
            //    }
            //}

            //if (minCount < fragList.Count)
            //{
            //    for (int i = minCount; i < fragList.Count; i++)
            //    {
            //        string tempFrag = string.Format(fragFileName, i);
            //        string outputFragPath = outputTempFolder + tempFrag;
            //        sw = new StreamWriter(outputFragPath, false);
            //        sw.Write(fragList[i]);
            //        sw.Close();
            //        command = fragCommand + $"{Application.dataPath.Replace("Assets", "Temp")}/ShaderAnalysis/tempOutput/" + tempFrag + " --format json" + " && ";
            //        commandList.Add(command + "exit");
            //    }
            //}

            //command += "exit";
            //Debug.Log(command);

            (ExecResult, MaliResultInfo) analysisOutput = await MaliCompilerOutput(commandList, newKeywardList);
            File.WriteAllText(outputDataPath + "/ShaderAnalysis/FinalOutput.txt", analysisOutput.Item1.Output);
            return analysisOutput;
        }


        public class ExecResult
        {
            public string Output { get; set; }
            /// <summary>
            /// 程序正常执行后的错误输出，需要根据实际内容判断是否成功。如果Output为空但Error不为空，则基本可以说明发生了问题或错误，但是可以正常执行结束
            /// </summary>
            public string Error { get; set; }
            /// <summary>
            /// 执行发生的异常，表示程序没有正常执行并结束
            /// </summary>
            public Exception ExceptError { get; set; }
        }
        private static List<string> m_ResultStringList = new List<string>(10);
        public async static System.Threading.Tasks.Task<(ExecResult, MaliResultInfo)> MaliCompilerOutput(List<string> commands, List<string> keywards)
        {
            m_ResultStringList.Clear();
            var myPackage = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/thinrl.profilerhub");
            var path = Application.dataPath + $"/../Packages/thinrl.profilerhub@{myPackage.version}/Editor/ShaderAnalysis/mali_offline_compiler";
            ExecResult excResult = new ExecResult();
            for (int i = 0; i < commands.Count; i++)
            {
                excResult = await RunAsync(commands[i], path);

                var str = excResult.Output;
                //Debug.Log("fzy firstRes:" + str);
                var pos = str.LastIndexOf("exit");
                str = str.Substring(pos + 4);
                var p = str.IndexOf('{');
                str = str.Insert(p + 1, $"\"keyward\":\"{keywards[i]}\",");
                m_ResultStringList.Add(str);
                Debug.Log("fzy result:" + str);
            }
            string res = "{\"data\":[";
            for (int i = 0; i < m_ResultStringList.Count; i++)
            {
                var cc = ",";
                if (i == m_ResultStringList.Count - 1)
                {
                    cc = "";
                }
                res += m_ResultStringList[i] + cc;
            }
            res += "]}";
            var resultInfo = JsonConvert.DeserializeObject<MaliResultInfo>(res);

            return (excResult, resultInfo);
        }

        public static async Task<ExecResult> RunAsync(string command, string workDirectory = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException();
            }

            return await Task.Run(() =>
            {
                command = command.Trim().TrimEnd('&') + "&exit";  //说明：不管命令是否成功均执行exit命令

                string cmdFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "cmd.exe");// @"C:\Windows\System32\cmd.exe";
                using (var p = new System.Diagnostics.Process())
                {
                    var result = new ExecResult();
                    try
                    {
                        p.StartInfo.FileName = cmdFileName;
                        p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                        p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                        p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                        p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                        p.StartInfo.CreateNoWindow = true;          //不显示程序窗口

                        if (!string.IsNullOrWhiteSpace(workDirectory))
                        {
                            p.StartInfo.WorkingDirectory = workDirectory;
                        }

                        p.Start();//启动程序

                        //向cmd窗口写入命令
                        p.StandardInput.WriteLine(command);
                        p.StandardInput.AutoFlush = true;
                        result.Output = p.StandardOutput.ReadToEnd();
                        result.Error = p.StandardError.ReadToEnd();

                        p.WaitForExit();
                        p.Close();
                    }
                    catch (Exception ex)
                    {
                        result.ExceptError = ex;
                    }
                    return result;
                }
            });
        }

    }
}


[Serializable]
public class Schema
{
    public string name;
    public int version;
}
[Serializable]
public class Producer
{
    public string name;
    public int[] version;
    public string build;
    public string documentation;
}
[Serializable]
public class Shaders
{
    [Serializable]
    public class Shader
    {
        public string api;
        public string type;
    }
    [Serializable]
    public class Pipelines
    {
        public string description;
        public string display_name;
        public string name;
    }
    [Serializable]
    public class Hardware
    {
        public string architecture;
        public string core;
        public string version;
        public Pipelines[] pipelines;
    }
    [Serializable]
    public class Properties
    {
        public string description;
        public string display_name;
        public string name;
        public bool value;
    }
    [Serializable]
    public class Performance
    {
        [Serializable]
        public class LongestPathCycles
        {
            public string[] bound_pipelines;
            public float[] cycle_count;
        }
        [Serializable]
        public class ShortestPathCycles
        {
            public string[] bound_pipelines;
            public float[] cycle_count;
        }
        [Serializable]
        public class TotalCycles
        {
            public string[] bound_pipelines;
            public float[] cycle_count;
        }
        public LongestPathCycles longest_path_cycles;
        public string[] pipelines;
        public ShortestPathCycles shortest_path_cycles;
        public TotalCycles total_cycles;
    }
    [Serializable]
    public class Variants
    {
        [Serializable]
        public class Properties
        {
            public string description;
            public string display_name;
            public string name;
            public object value;
        }
        public string name;
        public Performance performance;
        public Properties[] properties;
    }
    public string filename;
    public Hardware hardware;
    public string driver;
    public Shader shader;
    public string[] warnings;
    public Properties[] properties;
    public Variants[] variants;

}
[Serializable]
public class SingleMaliResultInfo
{
    StringBuilder sb = new StringBuilder(1 << 7);
    public Schema schema;
    public Producer producer;
    public Shaders[] shaders;
    public string keyward;
    public override string ToString()
    {
        sb.Clear();
        for (int j = 0; j < shaders.Length; j++)
        {
            string split = "\t";
            sb.AppendLine($"Shader {j + 1}:{shaders[j].shader.api}=>{shaders[j].shader.type}  ,  文件路径：{shaders[j].filename}");
            sb.AppendLine($"模拟GPU型号: {shaders[j].hardware.core}");
            sb.AppendLine($"Pass: {keyward}");
            for (int k = 0; k < shaders[j].properties.Length; k++)
            {
                var prop = shaders[j].properties[k];
                switch (prop.name)
                {
                    case "has_uniform_computation":
                        sb.AppendLine($"{split}是否有uniform变量计算的优化（最好不要有，可以放在cpu中去完成）：{prop.value}");
                        break;
                    case "has_side_effects":
                        sb.AppendLine($"{split}是否有管线外额外的内存操作（例如ssbo、atomic op、imagesStore）：{prop.value}");
                        break;
                    case "modifies_coverage":
                        sb.AppendLine($"{split}是否丢弃像素的行为等修改coverage的操作（会打断earlyz的操作）：{prop.value}");
                        break;
                    case "uses_late_zs_test":
                        sb.AppendLine($"{split}是否有深度修改操作（会打断earlyz的操作）：{prop.value}");
                        break;
                    case "uses_late_zs_update":
                        sb.AppendLine($"{split}是否有强制延迟z/s更新的操作（例如gl_LastFragDepthARM/gl_LastFragStencilARM）：{prop.value}");
                        break;
                    case "reads_color_buffer":
                        sb.AppendLine($"{split}是否有framebufferfetch操作（gl_LastFragColorARM）：{prop.value}");
                        break;
                    default:
                        break;
                }
            }

            split += "\t";
            //shader propertices:https://developer.arm.com/documentation/101863/0705/Using-Mali-Offline-Compiler/Performance-analysis/Shader-properties
            //Mali在针对vertexshader，在硬件级别上进行了重新设计（Bifrost和Valhall架构上）。会将vertex编译为两份 https://developer.arm.com/documentation/101863/0705/Using-Mali-Offline-Compiler/Performance-analysis/IDVS-shader-variants
            //流程大致是position-》cull-》varing
            //一份为position，只用来计算顶点位置变换
            //一份为varing，只针对变换后剔除完可见的图元进行非位置上属性的计算
            //所以这里统计采用两个阶段取平均的方法（为了简化显示）
            float avg_arith_total = 0;
            float avg_arith_fma = 0;
            float avg_arith_cvt = 0;
            float avg_arith_suf = 0;
            float avg_load_store = 0;
            float avg_texture = 0;
            int c = shaders[j].variants.Length;
            for (int k = 0; k < c; k++)
            {
                avg_arith_total += shaders[j].variants[k].performance.total_cycles.cycle_count[0];
                avg_arith_fma += shaders[j].variants[k].performance.total_cycles.cycle_count[1];
                avg_arith_cvt += shaders[j].variants[k].performance.total_cycles.cycle_count[2];
                avg_arith_suf += shaders[j].variants[k].performance.total_cycles.cycle_count[3];
                avg_load_store += shaders[j].variants[k].performance.total_cycles.cycle_count[4];
                avg_texture += shaders[j].variants[k].performance.total_cycles.cycle_count[5];
            }
            sb.AppendLine($"{split}算数指令总体消耗：{avg_arith_total / c}");
            sb.AppendLine($"{split}算数乘法单元消耗：{avg_arith_fma / c}");
            sb.AppendLine($"{split}算数整数运算和类型转换单元消耗：{avg_arith_cvt / c}");
            sb.AppendLine($"{split}算数复杂运算指令单元消耗：{avg_arith_suf / c}");
            sb.AppendLine($"{split}非纹理内存访问总体消耗：{avg_load_store / c}");
            sb.AppendLine($"{split}纹理单元采样、过滤等总体消耗：{avg_texture / c}");

            split += "\t";
            for (int k = 0; k < c; k++)
            {
                var p = shaders[j].variants[k].properties;
                for (int index = 0; index < p.Length; index++)
                {
                    var prop = p[index];
                    switch (prop.name)
                    {
                        case "work_registers_used":
                            sb.AppendLine($"{split}使用的读写寄存器的数量：{prop.value}");
                            break;
                        //case "thread_occupancy":
                        //    sb.AppendLine($"{split}：{prop.value}");
                        //    break;
                        case "uniform_registers_used":
                            sb.AppendLine($"{split}只读uniform寄存器使用数：{prop.value}");
                            break;
                        case "has_stack_spilling":
                            sb.AppendLine($"{split}是否有变量存储在栈中而不是寄存器中（说明存满了）：{prop.value}");
                            break;
                        case "stack_spill_bytes":
                            sb.AppendLine($"{split}存储在栈中的字节数：{prop.value}");
                            break;
                        case "fp16_arithmetic":
                            var v = 0;
                            if (prop.value != null && prop.value is string)
                            {
                                v = int.Parse(prop.value.ToString());
                            }
                            sb.AppendLine($"{split}16位算数运算的计算量比例：{v}%");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        return sb.ToString();
    }
}
[Serializable]
public class MaliResultInfo
{
    StringBuilder sb = new StringBuilder(1 << 7);
    public SingleMaliResultInfo[] data;
    public override string ToString()
    {
        sb.Clear();
        if (data == null || data.Length == 0) return "noData";
        var d = data[0];
        sb.AppendLine($"{d.producer.name}版本：{d.producer.version[0]}.{d.producer.version[1]}.{d.producer.version[2]}");
        for (int i = 0; i < data.Length; i++)
        {
            sb.AppendLine(data[i].ToString());
            sb.AppendLine("\n");
        }
        return sb.ToString();
    }
}


//返回数据示例
/*
 {
"schema": {
    "name": "performance",
    "version": 0
},
"producer": {
    "name": "Mali Offline Compiler",
    "version": [7, 2, 0],
    "build": "624fbd",
    "documentation": "https://developer.arm.com/mobile-studio/"
},
"shaders": [
    {
        "filename": "./samples/opengles/shader.frag",
        "hardware": {
            "architecture": "Valhall",
            "core": "Mali-G78",
            "revision": "r1p1",
            "pipelines": [
                {
                    "description": "Arithmetic Convert pipeline.",
                    "display_name": "Arith CVT",
                    "name": "arith_cvt"
                },
                {
                    "description": "Arithmetic Fused Multiply-Add pipeline.",
                    "display_name": "Arith FMA",
                    "name": "arith_fma"
                },
                {
                    "description": "Load/Store pipeline.",
                    "display_name": "Load/Store",
                    "name": "load_store"
                },
                {
                    "description": "Arithmetic Special Function Unit pipeline.",
                    "display_name": "Arith SFU",
                    "name": "arith_sfu"
                },
                {
                    "description": "Texture pipeline.",
                    "display_name": "Texture",
                    "name": "texture"
                },
                {
                    "description": "Varying pipeline.",
                    "display_name": "Varying",
                    "name": "varying"
                }
            ]
        },
        "driver": "r25p0-00rel0",
        "shader": {
            "api": "OpenGL ES",
            "type": "Fragment"
        },
        "warnings": [],
        "properties": [
            {
                "description": "If true the shader contains computation which produces the same value for every invocation in a draw call or compute dispatch.",
                "display_name": "Has uniform computation",
                "name": "has_uniform_computation",
                "value": true
            },
            {
                "description": "If true the shader has in-memory side-effects that are visible to the application. Side-effecting shaders often disable optimizations, such as hidden surface removal.",
                "display_name": "Has side-effects",
                "name": "has_side_effects",
                "value": false
            },
            {
                "description": "If true the shader can modify fragment coverage mask. Shaders with modifiable coverage cannot do early-ZS update and cannot be hidden surface occluders.",
                "display_name": "Modifies coverage",
                "name": "modifies_coverage",
                "value": false
            },
            {
                "description": "If true the shader uses a late ZS test. Shaders using a late ZS test that are killed are a potential source of inefficiency.",
                "display_name": "Uses late ZS test",
                "name": "uses_late_zs_test",
                "value": false
            },
            {
                "description": "If true the shader uses a late ZS update. Shaders using a late ZS update can cause later fragment at the same coordinate to stall due a depth dependency.",
                "display_name": "Uses late ZS update",
                "name": "uses_late_zs_update",
                "value": false
            },
            {
                "description": "If true the shader programmatically reads from the color buffer. Shaders that read from the color buffer are treated as transparent, and cannot be hidden surface occluders.",
                "display_name": "Reads color buffer",
                "name": "reads_color_buffer",
                "value": false
            }
        ],
        "variants": [
            {
                "name": "Main",
                "performance": {
                    "longest_path_cycles": {
                        "bound_pipelines": [
                            "arith_fma"
                        ],
                        "cycle_count": [
                            1.40625,
                            0.078125,
                            0.8125,
                            0.0,
                            0.75,
                            0.25
                        ]
                    },
                    "pipelines": [
                        "arith_fma",
                        "arith_cvt",
                        "arith_sfu",
                        "load_store",
                        "varying",
                        "texture"
                    ],
                    "shortest_path_cycles": {
                        "bound_pipelines": [
                            "arith_fma"
                        ],
                        "cycle_count": [
                            1.40625,
                            0.046875,
                            0.8125,
                            0.0,
                            0.75,
                            0.25
                        ]
                    },
                    "total_cycles": {
                        "bound_pipelines": [
                            "arith_fma"
                        ],
                        "cycle_count": [
                            1.40625,
                            0.078125,
                            0.8125,
                            0.0,
                            0.75,
                            0.25
                        ]
                    }
                },
                "properties": [
                    {
                        "description": "Number of read-write work registers used.",
                        "display_name": "Work Registers Used",
                        "name": "work_registers_used",
                        "value": 32
                    },
                    {
                        "description": "Number of read-only uniform registers used.",
                        "display_name": "Uniform Registers Used",
                        "name": "uniform_registers_used",
                        "value": 18
                    },
                    {
                        "description": "If true one or more variables had to be stored on the stack.",
                        "display_name": "Has Stack Spilling",
                        "name": "has_stack_spilling",
                        "value": false
                    },
                    {
                        "description": "Number of bytes stored on the stack instead of a register.",
                        "display_name": "Stack Spill Size (bytes)",
                        "name": "stack_spill_bytes",
                        "value": 0
                    },
                    {
                        "description": "The percentage of narrow arithmetic ops.",
                        "display_name": "16-bit arithmetic",
                        "name": "fp16_arithmetic",
                        "value": 0
                    }
                ]
            }
        ]
    }
]
}

 */
