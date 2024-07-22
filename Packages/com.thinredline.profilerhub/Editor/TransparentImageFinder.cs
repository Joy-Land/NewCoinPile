using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ThinRL.ProfilerHub.Editor
{

    public class TransparentImageFinder : EditorWindow
    {
        public static string dataSavePath;
        private FileStream m_Fs = null;
        private StreamWriter m_Sw = null;
        private bool m_IsFolder = true;
        private bool m_TurnOnCullTransparent = true;
        private string m_FolderPath = "Assets";
        [MenuItem("ThinRedLine/ProfilerHub/检测预制体中alpha为0的Image")]
        public static void ShowWindow()
        {
            Rect windowRect = new Rect(0, 0, 310, 500);
            TransparentImageFinder window = (TransparentImageFinder)EditorWindow.GetWindowWithRect(typeof(TransparentImageFinder), windowRect, true, "TransparentImageFinder");
            window.Show();
        }


        private void OnGUI()
        {

            GUILayout.BeginVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("要扫描的路径(可拖拽)：");
            var folderPathRect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
            m_FolderPath = EditorGUI.TextField(folderPathRect, m_FolderPath);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择存储位置");
            EditorGUILayout.TextField(dataSavePath);
            if (GUILayout.Button("选择"))
            {
                dataSavePath = EditorUtility.OpenFolderPanel("选择存储位置", dataSavePath, "");
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


            //如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位置在文本输入框内  
            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited)
              && folderPathRect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    m_FolderPath = DragAndDrop.paths[0];
                    m_IsFolder = !Path.HasExtension(m_FolderPath);
                }
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("扫描到的组件开启CullTransparent\t");
            m_TurnOnCullTransparent = GUILayout.Toggle(m_TurnOnCullTransparent,"");
            GUILayout.EndHorizontal();
            if (GUILayout.Button("扫描"))
            {
                if (dataSavePath == null || dataSavePath.Length == 0) { ShowNotification(new GUIContent("请选择存储路径")); return; }
                string date = System.DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
                string pathF = string.Format("{0}/TransparentImagePrefabList{1}.txt", dataSavePath, date);
                m_Fs = new FileStream(pathF, FileMode.Create, FileAccess.Write);
                m_Sw = new StreamWriter(m_Fs, Encoding.UTF8);
                if (m_IsFolder)
                {
                    DoDetectionImageInFolder(m_FolderPath);
                }
                else
                {
                    DoDetectionImage(m_FolderPath);
                }
                m_Sw.Close();
                m_Fs.Close();
                OpenFolder();
            }

            GUILayout.EndVertical();
        }


        public void OpenFolder()
        {
            string path = dataSavePath.Replace("/", "\\");
            System.Diagnostics.Process.Start("explorer.exe", path);
        }


        private void DoDetectionImageInFolder(string folderPath)
        {
            var assetGUIDResult = AssetDatabase.FindAssets("t:Prefab", new string[1] { folderPath });
            var count = assetGUIDResult.Length;
            var idx = 0;
            EditorUtility.DisplayProgressBar("统计中", "统计中...", 0.0f);
            foreach (var guid in assetGUIDResult)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                DoDetectionImage(assetPath);
                EditorUtility.DisplayProgressBar("统计中", "统计中...", (float)(idx++) / count);
            }
            EditorUtility.ClearProgressBar();
        }

        static bool firstNode = true;
        private void DoDetectionImage(string assetPath)
        {
            var UILayer = LayerMask.NameToLayer("UI");
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset.layer == UILayer)
            {
                var index = assetPath.IndexOf(".prefab");
                //var splitIndx = assetPath.LastIndexOf("/");
                firstNode = true;
                RecursionCheck(asset, assetPath.Substring(0, index), asset.name, 0);
                if(firstNode == false)
                {
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                 // Debug.Log("fzy path:" + assetPath);
            }
        }

        private void RecursionCheck(GameObject prefabObj, string assetPath, string rootObjName, int depth)
        {
            var imgComp = prefabObj.GetComponent<UnityEngine.UI.Image>();
            var cavRenderComp = prefabObj.GetComponent<UnityEngine.CanvasRenderer>();
            if (prefabObj == null) return;
            int childCount = prefabObj.transform.childCount;
            
            if (imgComp !=null && cavRenderComp.cullTransparentMesh == false && imgComp.color.a <= (1.0f/255.0f))
            {
                if(m_TurnOnCullTransparent)
                {
                    cavRenderComp.cullTransparentMesh = true;
                }
                if (firstNode)
                    m_Sw.WriteLine($"FirstNode = {prefabObj.name}     " + $"FullPath = {assetPath}      " + $"AssetsFindKey = t:prefab {rootObjName}");
                else
                    m_Sw.WriteLine(GetRetract(depth) + $"NodeName = {prefabObj.name}     " + $"FullPath = {assetPath}");
                firstNode = false;
            }

            for (int i = 0; i < childCount; i++)
            {
                var childObj = prefabObj.transform.GetChild(i).gameObject;
                RecursionCheck(childObj, assetPath + "/" + childObj.name, rootObjName, depth + 1);
            }
        }

        private string GetRetract(int depth)
        {
            string result = "";
            for (int i = 0; i < depth; i++)
            {
                result += "\t";
            }
            return result;
        }
    }
}

