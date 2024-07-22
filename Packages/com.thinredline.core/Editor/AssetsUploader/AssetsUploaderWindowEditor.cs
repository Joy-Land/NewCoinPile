
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;
using COSXML.Auth;
using COSXML;
using System.Threading.Tasks;
using COSXML.Transfer;
using System.IO;
using COSXML.Model.Bucket;
using COSXML.Model.Tag;
using COSXML.Model.Object;


namespace ThinRL.Editor
{



    public class UxmlLoader
    {
        private readonly static Dictionary<System.Type, string> _uxmlDic = new Dictionary<System.Type, string>();

        /// <summary>
        /// 加载窗口的布局文件
        /// </summary>
        public static UnityEngine.UIElements.VisualTreeAsset LoadWindowUXML<TWindow>() where TWindow : class
        {
            var windowType = typeof(TWindow);

            // 缓存里查询并加载
            if (_uxmlDic.TryGetValue(windowType, out string uxmlGUID))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGUID);
                if (string.IsNullOrEmpty(assetPath))
                {
                    _uxmlDic.Clear();
                    throw new System.Exception($"Invalid UXML GUID : {uxmlGUID} ! Please close the window and open it again !");
                }
                var treeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(assetPath);
                return treeAsset;
            }

            // 全局搜索并加载
            string[] guids = AssetDatabase.FindAssets(windowType.Name);
            if (guids.Length == 0)
                throw new System.Exception($"Not found any assets : {windowType.Name}");

            foreach (string assetGUID in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType == typeof(UnityEngine.UIElements.VisualTreeAsset))
                {
                    _uxmlDic.Add(windowType, assetGUID);
                    var treeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(assetPath);
                    return treeAsset;
                }
            }
            throw new System.Exception($"Not found UXML file : {windowType.Name}");
        }
    }

    public class DropableBox : Box, INotifyValueChanged<string[]>
    {
        public static Color DefaultColor = new Color(112 / 255f, 128 / 255f, 144 / 255f, 0.5f);
        public static Vector2 DefaultSize = new Vector2(200, 50);
        private Label m_TipLabel;
        private static string s_FlowGraphBigTitleLabel = "FlowGraphBigTitleLabel";

        private string[] m_Value;
        public string[] value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (m_Value != null)
                {
                    if (value.Length != 0)
                    {
                        if (m_Value.Length == value.Length)
                        {
                            int cc = 0;
                            for (int i = 0; i < m_Value.Length; i++)
                            {
                                if (value[i] == m_Value[i])
                                {
                                    cc++;
                                }
                            }
                            Debug.Log("fzy cc:" + cc + "  " + m_Value.Length + "  " + value.Length);
                            if (cc == m_Value.Length)
                            {
                                return;
                            }
                        }
                    }

                }
                Debug.Log("fzy cc:" + m_Value + " " + value);
                Debug.Log(value.Length);
                using (var pooled = ChangeEvent<string[]>.GetPooled(m_Value, value))
                {
                    pooled.target = this;

                    SetValueWithoutNotify(value);

                    SendEvent(pooled);
                }
            }
        }

        public DropableBox()
        {
            VisualElement elementSpace = new VisualElement();
            elementSpace.style.flexDirection = FlexDirection.Column;
            this.Add(elementSpace);

            this.style.backgroundColor = new StyleColor(DefaultColor);
            this.style.width = DefaultSize.x;
            this.style.height = DefaultSize.y;
            this.style.alignContent = Align.Center;
            this.style.alignItems = Align.Center;
            this.style.unityTextAlign = TextAnchor.MiddleCenter;
            this.style.alignSelf = Align.Center;

            m_TipLabel = new Label("拖动文件/文件夹到这里");
            m_TipLabel.style.fontSize = 20;
            m_TipLabel.style.alignContent = Align.Center;
            m_TipLabel.style.alignItems = Align.Center;
            m_TipLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            m_TipLabel.style.alignSelf = Align.Center;
            m_TipLabel.style.marginTop = 10;
            elementSpace.Add(m_TipLabel);
        }

        public void SetValueWithoutNotify(string[] newValue)
        {
            m_Value = newValue;
            Debug.Log("new vv:");
        }
    }

    [Serializable]
    public class AssetBundleCollectorGroup
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDesc = string.Empty;

        /// <summary>
        /// 资源分类标签
        /// </summary>
        public string AssetTags = string.Empty;


    }
    public class AssetsUploaderWindowEditor : EditorWindow
    {


        [MenuItem("ThinRedLine/工具/上传资源")]
        public static void ShowWindow()
        {
            Rect windowRect = new Rect(0, 0, 300, 500);
            AssetsUploaderWindowEditor window = EditorWindow.GetWindow<AssetsUploaderWindowEditor>("AssetsUploaderWindow", true);
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private ScrollView _collectorScrollView;
        private void AddUploaderBtn_clicked()
        {

            AssetsUploader uploader = new AssetsUploader();
            AssetsUploaderSettingData.CreateUploader(uploader);
            FillCollectorViewData();

        }
        private void RemoveUploaderBtn_clicked(AssetsUploader selectUploader)
        {
            if (selectUploader == null)
                return;

            AssetsUploaderSettingData.RemoveUploader(selectUploader);
            FillCollectorViewData();
        }


        private void FillCollectorViewData()
        {
            var uploaders = AssetsUploaderSettingData.Setting.Uploaders;

            _collectorScrollView.Clear();
            for (int i = 0; i < uploaders.Count; i++)
            {
                VisualElement element = MakeUploaderListViewItem();
                BindUploadListViewItem(element, i);
                _collectorScrollView.Add(element);
            }
        }

        private void BindUploadListViewItem(VisualElement element, int index)
        {
            var uploader = AssetsUploaderSettingData.Setting.Uploaders[index];

            // Foldout
            var foldout = element.Q<Foldout>("FileListFoldout");

            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    RefreshFoldout(foldout, uploader);
                else
                    foldout.Clear();
            });

            // Remove Button
            var removeBtn = element.Q<Button>("Button1");
            removeBtn.clicked += () =>
            {
                RemoveUploaderBtn_clicked(uploader);
            };

            //Box Field
            var box0 = element.Q<DropableBox>("BoxField0");
            box0.RegisterValueChangedCallback(evt =>
            {
                uploader.filesPath = box0.value.ToList();
                AssetsUploaderSettingData.ModifyUploader(uploader);
                if (foldout.value)
                {
                    RefreshFoldout(foldout, uploader);
                }
            });

            //cdn path
            var textFiled0 = element.Q<TextField>("TextField0");
            textFiled0.SetValueWithoutNotify(uploader.bucketKey);
            textFiled0.RegisterValueChangedCallback(evt =>
            {
                uploader.bucketKey = evt.newValue;
                AssetsUploaderSettingData.ModifyUploader(uploader);
            });

            var toggle0 = element.Q<Toggle>("Toggle0");
            toggle0.SetValueWithoutNotify(uploader.coverMode);
            toggle0.RegisterValueChangedCallback(evt =>
            {
                uploader.coverMode = evt.newValue;
                AssetsUploaderSettingData.ModifyUploader(uploader);
            });
        }

        private void RefreshFoldout(Foldout foldout, AssetsUploader uploader)
        {
            foldout.Clear();
            if (uploader.filesPath != null)
            {
                foreach (var path in uploader.filesPath)
                {
                    VisualElement elementRow = new VisualElement();
                    elementRow.style.flexDirection = FlexDirection.Row;
                    foldout.Add(elementRow);

                    string showInfo = path;
                    FileAttributes attributes = File.GetAttributes(path);
                    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        showInfo = "文件夹：" + showInfo;
                    }
                    else
                    {
                        showInfo = "文件：" + showInfo;
                    }

                    var label = new Label();
                    label.text = showInfo;
                    label.style.width = 300;
                    label.style.marginLeft = 0;
                    label.style.flexGrow = 1;
                    elementRow.Add(label);
                }

            }

        }

        private Button _saveButton;
        private Button _uploadButton;
        private string[] currentDragPaths;
        void CreateGUI()
        {
            VisualElement root = this.rootVisualElement;
            var visualAsset = UxmlLoader.LoadWindowUXML<AssetsUploaderWindowEditor>();
            if (visualAsset == null)
                return;

            visualAsset.CloneTree(root);
            Debug.Log("asfasfs");

            _collectorScrollView = root.Q<ScrollView>("CollectorScrollView");
            _collectorScrollView.style.height = new Length(100, LengthUnit.Percent);
            _collectorScrollView.viewDataKey = "scrollView";


            var collectorAddContainer = root.Q("CollectorAddContainer");
            {
                var addBtn = collectorAddContainer.Q<Button>("AddBtn");
                addBtn.clicked += AddUploaderBtn_clicked;
            }


            // 配置保存按钮
            _saveButton = root.Q<Button>("SaveButton");
            _saveButton.clicked += SaveBtn_clicked;

            _uploadButton = root.Q<Button>("UploadButton");
            _uploadButton.clicked += UploadBtn_clicked;

            FillCollectorViewData();
        }

        private CosXml cosXml;
        private void SaveBtn_clicked()
        {
            AssetsUploaderSettingData.SaveFile();
        }

        private async void UploadBtn_clicked()
        {
            CosXmlConfig config = new CosXmlConfig.Builder()
            .SetRegion("ap-beijing") // 设置默认的地域, COS 地域的简称请参照 https://intl.cloud.tencent.com/document/product/436/6224
            .Build();


            string secretId = "AKIDteDwqvHXh0DDzXwDOOn0kejmkcP7lLzW";   // 云 API 密钥 SecretId, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi
            string secretKey = "Sjq8Xu8QlGlErimxxqeOG9KXdoy2EMPJ"; // 云 API 密钥 SecretKey, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi
            long durationSecond = 600;          //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(secretId,
              secretKey, durationSecond);

            cosXml = new CosXmlServer(config, qCloudCredentialProvider);

            var uploader = AssetsUploaderSettingData.Setting.Uploaders;
            for (int i = 0; i < uploader.Count; i++)
            {
                var uploaderItem = uploader[i];
                var filePaths = GetAllFilePaths(uploaderItem.filesPath.ToArray());
                var remoteUrl = GenerateRemotePaths(uploaderItem.filesPath.ToArray());
                if (uploaderItem.coverMode == false)
                {
                    String nextMarker = null;

                    // 循环请求直到没有下一页数据
                    do
                    {
                        // 存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer
                        string bucket = "wxgame-hotload-1325692694";
                        string prefix = uploaderItem.bucketKey + "/"; //指定前缀
                        GetBucketRequest listRequest = new GetBucketRequest(bucket);
                        //获取 folder1/ 下的所有对象以及子目录
                        listRequest.SetPrefix(prefix);
                        listRequest.SetMarker(nextMarker);
                        //执行列出对象请求
                        GetBucketResult listResult = cosXml.GetBucket(listRequest);
                        ListBucket info = listResult.listBucket;
                        // 对象列表
                        List<ListBucket.Contents> objects = info.contentsList;
                        // 下一页的下标
                        nextMarker = info.nextMarker;

                        DeleteMultiObjectRequest deleteRequest = new DeleteMultiObjectRequest(bucket);
                        //设置返回结果形式
                        deleteRequest.SetDeleteQuiet(false);
                        //对象列表
                        List<string> deleteObjects = new List<string>();
                        foreach (var content in objects)
                        {
                            deleteObjects.Add(content.key);
                        }
                        if (deleteObjects.Count == 0)
                            break;
                        deleteRequest.SetObjectKeys(deleteObjects);
                        //执行批量删除请求
                        DeleteMultiObjectResult deleteResult = cosXml.DeleteMultiObjects(deleteRequest);
                        //打印请求结果
                        Debug.Log(deleteResult.GetResultInfo());
                    } while (nextMarker != null);

                }
                for (int idx = 0; idx < filePaths.Count; idx++)
                {
                    await TransferUploadFile(uploaderItem.bucketKey, filePaths[idx], remoteUrl[idx]);
                }

            }

        }



        public static List<string> GenerateRemotePaths(params string[] paths)
        {
            List<string> remotePaths = new List<string>();

            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    // 是文件，直接返回文件名作为remote路径
                    remotePaths.Add(Path.GetFileName(path));
                }
                else if (Directory.Exists(path))
                {
                    // 是文件夹，递归遍历文件夹
                    remotePaths.AddRange(GenerateRemotePathsInDirectory(path, Path.GetFileName(path)));
                }
                else
                {
                    // 路径无效，提示错误信息（可以根据需要选择是否输出到控制台）
                    Console.WriteLine($"路径 '{path}' 不存在。");
                }
            }

            return remotePaths;
        }

        private static List<string> GenerateRemotePathsInDirectory(string directoryPath, string currentPrefix)
        {
            List<string> remotePaths = new List<string>();

            // 获取文件夹中的所有文件
            string[] files = Directory.GetFiles(directoryPath);

            // 将所有文件的路径添加到列表，并添加前缀
            foreach (string file in files)
            {
                remotePaths.Add($"{currentPrefix}/{Path.GetFileName(file)}");
            }

            // 递归遍历子文件夹
            string[] subdirectories = Directory.GetDirectories(directoryPath);
            foreach (string subdirectory in subdirectories)
            {
                string subdirectoryName = Path.GetFileName(subdirectory);
                remotePaths.AddRange(GenerateRemotePathsInDirectory(subdirectory, $"{currentPrefix}/{subdirectoryName}"));
            }

            return remotePaths;
        }

        public static List<string> GetAllFilePaths(params string[] paths)
        {
            List<string> filePaths = new List<string>();

            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    // 是文件，直接添加到列表，并修正路径分隔符
                    filePaths.Add(path.Replace('\\', '/'));
                }
                else if (Directory.Exists(path))
                {
                    // 是文件夹，递归遍历文件夹，并修正路径分隔符
                    filePaths.AddRange(GetAllFilePathsInDirectory(path).ConvertAll(p => p.Replace('\\', '/')));
                }
                else
                {
                    // 路径无效，提示错误信息（可以根据需要选择是否输出到控制台）
                    Console.WriteLine($"路径 '{path}' 不存在。");
                }
            }

            return filePaths;
        }

        private static List<string> GetAllFilePathsInDirectory(string directoryPath)
        {
            List<string> filePaths = new List<string>();

            // 获取文件夹中的所有文件
            string[] files = Directory.GetFiles(directoryPath);

            // 将所有文件的路径添加到列表，并修正路径分隔符
            filePaths.AddRange(files.Select(f => f.Replace('\\', '/')).ToArray());

            // 递归遍历子文件夹
            string[] subdirectories = Directory.GetDirectories(directoryPath);
            foreach (string subdirectory in subdirectories)
            {
                filePaths.AddRange(GetAllFilePathsInDirectory(subdirectory).ConvertAll(p => p.Replace('\\', '/')));
            }

            return filePaths;
        }

        public async Task TransferUploadFile(string bucketKey, string file, string remoteUrl)
        {
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();
            // 手动设置开始分块上传的大小阈值为10MB，默认值为5MB
            transferConfig.DivisionForUpload = 10 * 1024 * 1024;
            // 手动设置分块上传中每个分块的大小为2MB，默认值为1MB
            transferConfig.SliceSizeForUpload = 2 * 1024 * 1024;

            // 初始化 TransferManager
            TransferManager transferManager = new TransferManager(cosXml, transferConfig);
            // 存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer
            String bucket = "wxgame-hotload-1325692694";
            String cosPath = bucketKey + "/" + remoteUrl; //对象在存储桶中的位置标识符，即称对象键
            String srcPath = file;//本地文件绝对路径

            // 上传对象
            COSXMLUploadTask uploadTask = new COSXMLUploadTask(bucket, cosPath);
            uploadTask.SetSrcPath(srcPath);

            uploadTask.progressCallback = delegate (long completed, long total)
            {
                //Debug.Log(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
            };

            uploadTask.successCallback = (e) =>
            {
                Debug.Log("fzy file ok:" + e.Key);
            };

            try
            {
                //Debug.LogError("???"+file
                COSXMLUploadTask.UploadTaskResult result = await transferManager.UploadAsync(uploadTask);
                //Debug.LogError("???xxx"+remoteUrl);
                Debug.Log("上传成功" + file + "   " + result.GetResultInfo());
                //string eTag = result.eTag;
            }
            catch (Exception e)
            {
                Debug.Log("CosException: " + e);
            }

        }

        public void OnDestroy()
        {
            if (AssetsUploaderSettingData.IsDirty)
                AssetsUploaderSettingData.SaveFile();
        }


        private bool m_DragEnter = false;
        private VisualElement MakeUploaderListViewItem()
        {
            VisualElement element = new VisualElement();

            VisualElement elementTop = new VisualElement();
            elementTop.style.flexDirection = FlexDirection.Row;
            element.Add(elementTop);

            VisualElement elementBottom = new VisualElement();
            elementBottom.style.flexDirection = FlexDirection.Row;
            element.Add(elementBottom);

            VisualElement elementFoldout = new VisualElement();
            elementFoldout.style.flexDirection = FlexDirection.Row;
            element.Add(elementFoldout);

            VisualElement elementSpace = new VisualElement();
            elementSpace.style.flexDirection = FlexDirection.Column;
            element.Add(elementSpace);


            // Top VisualElement
            {
                var button = new Button();
                button.name = "Button1";
                button.text = "-";
                button.style.unityTextAlign = TextAnchor.MiddleCenter;
                button.style.flexGrow = 0f;
                elementTop.Add(button);
            }

            {
                //DragEnterEvent
                var box = new DropableBox();

                box.RegisterCallback<DragEnterEvent>((e) =>
                {
                    var pp = DragAndDrop.paths;
                    if (pp != null && pp.Length > 0)
                    {
                        currentDragPaths = pp;
                        m_DragEnter = true;
                    }
                });
                box.RegisterCallback<DragExitedEvent>((element) =>
                {
                    var pp = currentDragPaths;
                    for (int i = 0; i < pp.Length; i++)
                    {
                        Debug.Log("fzy path:" + pp[i]);
                    }
                    box.value = pp;
                    m_DragEnter = false;
                    currentDragPaths = null;
                });

                box.RegisterCallback<DragLeaveEvent>((e) =>
                {

                    m_DragEnter = false;
                });
                box.name = "BoxField0";
                box.style.unityTextAlign = TextAnchor.MiddleLeft;
                box.style.flexGrow = 1f;
                elementTop.Add(box);
            }


            // Bottom VisualElement
            {
                var label = new Label();
                label.style.width = 40;
                elementBottom.Add(label);
            }
            {
                var textField = new TextField();
                textField.name = "TextField0";
                textField.label = "存储桶对象键";
                textField.style.width = 200;
                textField.style.marginLeft = 20;
                textField.style.flexGrow = 1;
                elementBottom.Add(textField);
                var label = textField.Q<Label>();
                label.style.minWidth = 40;
            }

            {
                var toggle = new Toggle();
                toggle.name = "Toggle0";
                toggle.label = "覆盖模式";
                toggle.style.width = 50;
                toggle.style.marginLeft = 0;
                toggle.style.flexGrow = 0.35f;
                elementBottom.Add(toggle);
                var label = toggle.Q<Label>();
                label.style.minWidth = 50;
                label.style.maxWidth = 50;
            }

            // Foldout VisualElement
            {
                var label = new Label();
                label.style.width = 90;
                elementFoldout.Add(label);
            }
            {
                var foldout = new Foldout();
                foldout.name = "FileListFoldout";
                foldout.value = false;
                foldout.text = "待上传文件路径";
                elementFoldout.Add(foldout);
            }

            // Space VisualElement
            {
                var label = new Label();
                label.style.height = 10;
                elementSpace.Add(label);
            }

            return element;
        }
        private VisualElement MakeGroupListViewItem()
        {
            VisualElement element = new VisualElement();

            {
                var label = new Label();
                label.name = "Label1";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.flexGrow = 1f;
                label.style.height = 20f;
                element.Add(label);
            }

            return element;
        }
        private void BindGroupListViewItem(VisualElement element, int index)
        {
            // var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
            // if (selectPackage == null)
            //     return;

            // var group = selectPackage.Groups[index];

            // var textField1 = element.Q<Label>("Label1");
            // if (string.IsNullOrEmpty(group.GroupDesc))
            //     textField1.text = group.GroupName;
            // else
            //     textField1.text = $"{group.GroupName} ({group.GroupDesc})";

            // // 激活状态
            // IActiveRule activeRule = AssetBundleCollectorSettingData.GetActiveRuleInstance(group.ActiveRuleName);
            // bool isActive = activeRule.IsActiveGroup();
            // textField1.SetEnabled(isActive);
        }


        public void Update()
        {
            if (m_DragEnter == true)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            if (_saveButton != null)
            {
                if (AssetsUploaderSettingData.IsDirty)
                {
                    if (_saveButton.enabledSelf == false)
                        _saveButton.SetEnabled(true);
                }
                else
                {
                    if (_saveButton.enabledSelf)
                        _saveButton.SetEnabled(false);
                }
            }
        }

    }
}