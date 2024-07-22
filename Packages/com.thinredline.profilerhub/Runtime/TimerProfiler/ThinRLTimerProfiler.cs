using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace ThinRL.ProfilerHub.TimerProfiler
{
    public class ProfilerData
    {
        public string name;
        public string resolution;
        public string rasterResolution;
        public string initParamsTimespan;
        public string rasterCameraRenderTimespan;
        public string readbackTimespan;
        public string generateCombineDataTimespan;
        public string meshCombineTimespan;
        public string totalTimespan;
    }
    public class ThinRLProfiler 
    {
        public static readonly string NAME_KEY = "物体名称";
        public static readonly string RESOLUTION_KEY = "体素化分辨率";
        public static readonly string RASTER_RESOLUTION_KEY = "光栅化相机分辨率";
        public static readonly string INIT_PARAMS_TIMESPAN_KEY = "初始化数据耗时";
        public static readonly string RASTER_CAMERA_RENDER_TIMESPAN_KEY = "光栅化相机渲染耗时(ms)";
        public static readonly string READBACK_TIMESPAN_KEY = "gpu回读耗时(ms)";
        public static readonly string GENERATE_COMBINE_DATA_TIMESPAN_KEY = "生成合并数据耗时(ms)";
        public static readonly string MESH_COMBINE_TIMESPAN_KEY = "合并mesh耗时(ms)";
        public static readonly string TOTAL_TIMESPAN_KEY = "总耗时(ms)";

        private DataTable m_Table;
        Dictionary<string, System.Diagnostics.Stopwatch> timerDic = new Dictionary<string, System.Diagnostics.Stopwatch>();

        public DataTable Table { get => m_Table; }

        public ThinRLProfiler()
        {
            m_Table = new DataTable("Sheet1");

            m_Table.Columns.Add(NAME_KEY);
            m_Table.Columns.Add(RESOLUTION_KEY);
            m_Table.Columns.Add(RASTER_RESOLUTION_KEY);
            m_Table.Columns.Add(INIT_PARAMS_TIMESPAN_KEY);
            m_Table.Columns.Add(RASTER_CAMERA_RENDER_TIMESPAN_KEY);
            m_Table.Columns.Add(READBACK_TIMESPAN_KEY);
            m_Table.Columns.Add(GENERATE_COMBINE_DATA_TIMESPAN_KEY);
            m_Table.Columns.Add(MESH_COMBINE_TIMESPAN_KEY);
            m_Table.Columns.Add(TOTAL_TIMESPAN_KEY);

            //timerDic.Add(NAME_KEY, new System.Diagnostics.Stopwatch());
            //timerDic.Add(RESOLUTION_KEY, new System.Diagnostics.Stopwatch());
            //timerDic.Add(RASTER_RESOLUTION_KEY, new System.Diagnostics.Stopwatch());
            timerDic.Add(INIT_PARAMS_TIMESPAN_KEY, new System.Diagnostics.Stopwatch());
            timerDic.Add(RASTER_CAMERA_RENDER_TIMESPAN_KEY, new System.Diagnostics.Stopwatch());
            timerDic.Add(READBACK_TIMESPAN_KEY, new System.Diagnostics.Stopwatch());
            timerDic.Add(GENERATE_COMBINE_DATA_TIMESPAN_KEY, new System.Diagnostics.Stopwatch());
            timerDic.Add(MESH_COMBINE_TIMESPAN_KEY, new System.Diagnostics.Stopwatch());
            timerDic.Add(TOTAL_TIMESPAN_KEY, new System.Diagnostics.Stopwatch());
        }

        public Dictionary<int, ProfilerData> dataDic = new Dictionary<int, ProfilerData>();

        private int m_HasKey = -1;
        public void SetHeader(int hashKey, string name, string rasterRes)
        {
            m_HasKey = hashKey;
            if (dataDic.ContainsKey(hashKey) == false)
            {
                dataDic.Add(hashKey, new ProfilerData());
            }

            dataDic[hashKey].name = name;
            dataDic[hashKey].rasterResolution = rasterRes;

        }

        public void SetParams(string key, string value)
        {
            if (dataDic.ContainsKey(m_HasKey))
            {
                if (key == RESOLUTION_KEY)
                {
                    dataDic[m_HasKey].resolution = value;
                }
            }
        }

        public void BeginProfilerTime(string key)
        {
            if (dataDic.ContainsKey(m_HasKey) && timerDic.ContainsKey(key))
            {
                timerDic[key].Reset();
                timerDic[key].Start();
            }
        }

        public void EndProfilerTime(string key)
        {
            if (dataDic.ContainsKey(m_HasKey) && timerDic.ContainsKey(key))
            {
                timerDic[key].Stop();
                var timeStr = timerDic[key].Elapsed.TotalMilliseconds.ToString();
                if (key == READBACK_TIMESPAN_KEY)
                {
                    dataDic[m_HasKey].readbackTimespan = timeStr;
                }
                else if (key == MESH_COMBINE_TIMESPAN_KEY)
                {
                    dataDic[m_HasKey].meshCombineTimespan = timeStr;
                }
                else if (key == TOTAL_TIMESPAN_KEY)
                {
                    dataDic[m_HasKey].totalTimespan = timeStr;
                }
                else if (key == GENERATE_COMBINE_DATA_TIMESPAN_KEY)
                {
                    dataDic[m_HasKey].generateCombineDataTimespan = timeStr;
                }
                else if (key == RASTER_CAMERA_RENDER_TIMESPAN_KEY)
                {
                    dataDic[m_HasKey].rasterCameraRenderTimespan = timeStr;
                }
                else if (key == INIT_PARAMS_TIMESPAN_KEY)
                {
                    dataDic[m_HasKey].initParamsTimespan = timeStr;
                }

            }
        }

        public void SetEnd()
        {
            DataRow dr = m_Table.NewRow();
            var data = dataDic[m_HasKey];
            dr[NAME_KEY] = data.name;
            dr[RESOLUTION_KEY] = data.resolution;
            dr[RASTER_RESOLUTION_KEY] = data.rasterResolution;
            dr[INIT_PARAMS_TIMESPAN_KEY] = data.initParamsTimespan;
            dr[RASTER_CAMERA_RENDER_TIMESPAN_KEY] = data.rasterCameraRenderTimespan;
            dr[READBACK_TIMESPAN_KEY] = data.readbackTimespan;
            dr[GENERATE_COMBINE_DATA_TIMESPAN_KEY] = data.generateCombineDataTimespan;
            dr[MESH_COMBINE_TIMESPAN_KEY] = data.meshCombineTimespan;
            dr[TOTAL_TIMESPAN_KEY] = data.totalTimespan;

            m_Table.Rows.Add(dr);
        }

        public override string ToString()
        {
            string str = string.Empty;
            foreach (var item in dataDic.Values)
            {
                str += item.ToString() + "\n";
            }
            return str;
        }
        public void Print()
        {
            foreach (var item in dataDic.Values)
            {
                Debug.Log("TimeProfiler TimeSnap:" + item.name + "  " + item.resolution + "  " + item.rasterResolution + "  " + item.readbackTimespan + "  " + item.meshCombineTimespan + "  " + item.totalTimespan);
            }
        }

        public void CreateAndSaveRecordData(string fileName)
        {
            string filePath = Application.dataPath + $"\\{fileName}.csv";
            SaveCSV(filePath, this.Table);
        }

        private StringBuilder m_StringBuilder = new StringBuilder(32);
        private void SaveCSV(string CSVPath, DataTable mSheet)
        {
#if !UNITY_EDITOR
            return;
#endif
            if (mSheet.Rows.Count < 1)
                return;

            int rowCount = mSheet.Rows.Count;
            int colCount = mSheet.Columns.Count;

            m_StringBuilder.Clear();
            for (int i = 0; i < mSheet.Columns.Count; i++)
            {
                m_StringBuilder.Append(mSheet.Columns[i].ColumnName + ",");
            }
            m_StringBuilder.Append("\r\n");
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    m_StringBuilder.Append(mSheet.Rows[i][j] + ",");
                }
                m_StringBuilder.Append("\r\n");
            }

            using (FileStream fileStream = new FileStream(CSVPath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    textWriter.Write(m_StringBuilder.ToString());
                }
            }

#if UNITY_EDITOR
                        UnityEditor.AssetDatabase.Refresh();
#endif

        }
    }
}
