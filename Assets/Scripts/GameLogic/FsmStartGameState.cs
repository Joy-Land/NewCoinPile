#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif

using Joyland.GamePlay;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using Cysharp.Threading.Tasks;
using UniFramework.Event;
using UniFramework.Machine;
using UnityEngine;
using UnityEngine.Scripting;
using YooAsset;
using System.IO;

public class FsmStartGameState : IStateNode
{
    [Preserve]
    class VersionConfig
    {
        public class Config
        {
            public string cdn;
        }
        public Config debug;
        public Config release;
    }

    private Sprite m_Bg;

    public const string PACKAGE_NAME = "DefaultPackage";

    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    private StateMachine m_Machine;
    public void OnCreate(StateMachine machine)
    {
        m_Machine = machine;

        PlayMode = EPlayMode.WebPlayMode;

        Application.targetFrameRate = 30;
        Application.runInBackground = true;

#if UNITY_EDITOR
        PlayMode = EPlayMode.EditorSimulateMode;
#else
#if UNITY_WEBGL
        PlayMode = EPlayMode.WebPlayMode;
#else
        PlayMode = EPlayMode.HostPlayMode;
#endif
#endif


    }

    public void OnEnter()
    {
        var m_UIStartGameNode = m_Machine.GetBlackboardValue("_UIStartGameNode") as GameObject;
        m_Bg = Resources.Load<Sprite>("FirstAssets/bg2");

        UIManager.Instance.SetBackground(0, m_Bg);
        UIManager.Instance.GetAndOpenUIViewOnNode<UIStartGame>(m_UIStartGameNode, UIViewID.UIStartGame,
            new UIViewConfig() { bundleName = "", layer = UIViewLayerEnum.Lowest, packageName = "" }, new EventArgsPack((int)LoadingStageEventCode.Finish));

        EventManager.Instance.AddEvent(GameEventGlobalDefine.EverythingIsReady, OnEverythingIsReadyEvent);
        EventManager.Instance.AddEvent(GameEventGlobalDefine.DownloadFinish, OnDownloadFinishEvent);
        // 初始化事件系统
        UniEvent.Initalize();

        if (EPlayMode.WebPlayMode == PlayMode || EPlayMode.HostPlayMode == PlayMode)
        {
            InitConfig(8);
        }
        else
        {
            //非联机模式直接跳到yooasset初始化
            InitConfig(1);
        }

        //加载资源

        //初始化ui框架
    }

    public void OnExit()
    {
        if (m_Bg)
        {
            Resources.UnloadAsset(m_Bg);
        }
        console.error(this.ToString() + "退出");
        EventManager.Instance.RemoveEvent(GameEventGlobalDefine.EverythingIsReady, OnEverythingIsReadyEvent);
        EventManager.Instance.RemoveEvent(GameEventGlobalDefine.DownloadFinish, OnDownloadFinishEvent);

        UIManager.Instance.CloseUI(UIViewID.UIStartGame);
    }

    public void OnUpdate()
    {

    }

    public void OnEverythingIsReadyEvent(object sender, EventArgsPack e)
    {
        m_Machine.ChangeState<FsmHomeState>();
    }

    public async void OnDownloadFinishEvent(object sender, EventArgsPack e)
    {
        //测试
        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.AddProgressBar, null, new EventArgsPack((int)LoadingStageEventCode.InitResource, (float)((int)LoadingStageEventCode.InitResource)));
        var viewConfigAsset = Resources.Load<TextAsset>("ui_view_config");
        var viewConfig = JsonConvert.DeserializeObject<Dictionary<int, UIViewConfig>>(viewConfigAsset.text);
        UIManager.Instance.InitUIViewConfigWithAddingMode(viewConfig);

        var compConfigAsset = Resources.Load<TextAsset>("ui_comp_config");
        var compConfig = JsonConvert.DeserializeObject<Dictionary<int, UICompConfig>>(compConfigAsset.text);

        await UIManager.Instance.InitUICompListConfig(compConfig);
        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.AddProgressBar, null, new EventArgsPack((int)LoadingStageEventCode.InitResource, (float)((int)LoadingStageEventCode.InitResource) + 1));

        await LoadNecessaryConfig();

        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.AddProgressBar, null, new EventArgsPack((int)LoadingStageEventCode.Finish, (float)((int)LoadingStageEventCode.Finish)));
    }

    private async UniTask LoadNecessaryConfig()
    {
        //文案配置
        var copyWrite = YooAssets.LoadAssetAsync<TextAsset>("CopyWriteConfig");
        copyWrite.Completed += (handle) =>
        {
            var res = handle.AssetObject as TextAsset;
            GameConfig.Instance.LoadLocalCopyWriteConfig(res.text);
            console.info(GameConfig.LocalCopyWriteManager.AllCopyWriteConfigDatas);
        };
        await copyWrite.ToUniTask();

        //收藏配置
        var collect = YooAssets.LoadAssetAsync<TextAsset>("CollectConfig");
        collect.Completed += (handle) =>
        {
            var res = handle.AssetObject as TextAsset;
            GameConfig.Instance.LoadLocalCollectConfig(res.text);
            console.info(GameConfig.LocalCollectManager.AllCollectItemConfigDatas);
        };
        await collect.ToUniTask();


        var itemUsage = YooAssets.LoadAssetAsync<TextAsset>("ItemUsageConfig");
        itemUsage.Completed += (handle) =>
        {
            var res = handle.AssetObject as TextAsset;
            GameConfig.Instance.LoadLocalItemUsageConfig(res.text);
        };
        await itemUsage.ToUniTask();

        //别的配置
    }

    private static string m_RemoteBundlePath = "https://cdn.joylandstudios.com/UnityTest/webgl/StreamingAssets/yoo/DefaultPackage";
    private static string m_CDNRootPath = "https://cdn.joylandstudios.com/UnityTest/webgl/StreamingAssets/yoo/DefaultPackage";//https://cdn.joylandstudios.com/UnityTest/webgl/StreamingAssets/yoo/DefaultPackage
    void InitConfig(int tryTime)
    {

        // var www = UnityWebRequest.Get()
        var json = "";
        TextAsset text = Resources.Load<TextAsset>("version_config");
        json = text.text;
        if (!json.IsNullOrEmpty())
        {
            var config = JsonConvert.DeserializeObject<VersionConfig>(json);
            console.error("cdnURL:", config.debug.cdn, config.release.cdn);

#if JOYLAND_DEBUG
            m_RemoteBundlePath = config.debug.cdn;
#else
            m_RemoteBundlePath = config.release.cdn;
#endif

            var pathSplit = config.debug.cdn.Split("/StreamingAssets");
            m_CDNRootPath = pathSplit[0];
            console.info("cdnRoot:", m_CDNRootPath);
            if (string.IsNullOrEmpty(m_CDNRootPath))
            {
                console.error("路径错误");
                return;
            }
#if UNITY_WX && !UNITY_EDITOR
            WX.SetDataCDN(m_CDNRootPath);
            GetBundleCacheFileList("/StreamingAssets"+pathSplit[1]);
#endif

        }
        else
        {
            console.error("错误，没version_config无内容");
            return;
        }

        this.InitYooAssetStart();
    }

    async void InitYooAssetStart()
    {
        // 初始化资源系统
        YooAssets.Initialize();
        // 创建资源包裹类
        var package = YooAssets.CreatePackage(PACKAGE_NAME);
        // 编辑器下的模拟模式
        InitializeParameters createParameters = null;
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            createParameters = new EditorSimulateModeParameters()
            {
                SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.ScriptableBuildPipeline.ToString(), PACKAGE_NAME)
            };
        }
        // 单机运行模式
        else if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            createParameters = new OfflinePlayModeParameters();
        }
        // 联机运行模式
        else if (PlayMode == EPlayMode.HostPlayMode)
        {
            createParameters = new HostPlayModeParameters()
            {
                RemoteServices = new RemoteServices(),
                BuildinQueryServices = new BuildinQueryServices()
            };
        }
        // WebGL运行模式
        else if (PlayMode == EPlayMode.WebPlayMode)
        {
            createParameters = new WebPlayModeParameters()
            {
                RemoteServices = new RemoteServices(),
                BuildinQueryServices = new BuildinQueryServices(),
                WechatQueryServices = new WechatCacheQueryService()
            };
            //因为微信小游戏平台的特殊性，需要关闭WebGL的缓存系统，使用微信自带的缓存系统。
            YooAssets.SetCacheSystemDisableCacheOnWebGL();
        }


        var initializationOperation = package.InitializeAsync(createParameters);
        await initializationOperation.ToUniTask();

        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            console.error("错误。。。");
            return;
        }
        UpdatePackageVersionAsync(package).Forget();
    }

    public static HashSet<string> CacheFileNameSet = new HashSet<string>();
    public void GetBundleCacheFileList(string fileCachePath)
    {
        CacheFileNameSet.Clear();
        var cachePath = WX.PluginCachePath;
        var fileSystem = WX.GetFileSystemManager();

        var path = cachePath + fileCachePath;

        var cacheFileList = new string[0];
        try
        {
            cacheFileList = fileSystem.ReaddirSync(path);
        }
        catch
        {
            cacheFileList = new string[0];
        }

        foreach (var file in cacheFileList)
        {
            CacheFileNameSet.Add(file);
        }
        console.info("fzypp:", cachePath, path, cacheFileList, Application.streamingAssetsPath, WX.env.USER_DATA_PATH);
        console.info("fzy new pp:", cacheFileList);
    }

    async UniTaskVoid UpdatePackageVersionAsync(ResourcePackage package)
    {
        var yooOp = package.UpdatePackageVersionAsync();
        var uniOp = yooOp.ToUniTask();
        await uniOp;
        if (uniOp.Status == UniTaskStatus.Succeeded)
        {
            //更新成功
            Debug.Log($"Updated package Version : {yooOp.PackageVersion}");
            UpdatePackageManifest(package, yooOp.PackageVersion).Forget();
        }
        else
        {
            QuitGame();
        }
    }

    //UpdatePackageManifest
    async UniTaskVoid UpdatePackageManifest(ResourcePackage package, string packageVersion)
    {
        var yooOp = package.UpdatePackageManifestAsync(packageVersion, true);
        var uniOp = yooOp.ToUniTask();
        await uniOp;
        if (uniOp.Status == UniTaskStatus.Succeeded)
        {
            //更新成功
            DownloadAot2HotRes(package).Forget();
        }
        else
        {
            QuitGame();
        }
    }

    async UniTaskVoid DownloadAot2HotRes(ResourcePackage package)
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var downloader = package.CreateResourceDownloader("ResDownloader", downloadingMaxNum, failedTryAgain);

        //注册回调方法，这里AOT使用这个是避免裁剪，不然HOT找不到该方法了
        downloader.OnDownloadErrorCallback = OnDownloadError;
        downloader.OnDownloadProgressCallback = OnDownloadProgress;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFile;

        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.LogError("完成，没有要下载的了");

            var gamePackage = YooAssets.GetPackage("DefaultPackage");
            YooAssets.SetDefaultPackage(gamePackage);
            EventManager.Instance.DispatchEvent(GameEventGlobalDefine.AddProgressBar, null, new EventArgsPack((int)LoadingStageEventCode.DownloadPackage, (float)((int)LoadingStageEventCode.DownloadPackage) + 1));
            EventManager.Instance.DispatchEvent(GameEventGlobalDefine.DownloadFinish, null, null);

            return;
        }
        //开启下载
        downloader.BeginDownload();
        await downloader.ToUniTask();

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //GotoAot2HotScene().Forget();
            Debug.LogError("全部下载完成");

            var gamePackage = YooAssets.GetPackage("DefaultPackage");
            YooAssets.SetDefaultPackage(gamePackage);
            EventManager.Instance.DispatchEvent(GameEventGlobalDefine.AddProgressBar, null, new EventArgsPack((int)LoadingStageEventCode.DownloadPackage, (float)((int)LoadingStageEventCode.DownloadPackage) + 1));
            EventManager.Instance.DispatchEvent(GameEventGlobalDefine.DownloadFinish, null, null);
            //var gamePackage = YooAssets.GetPackage("DefaultPackage");
            //YooAssets.SetDefaultPackage(gamePackage);
            //SceneEventDefine.ChangeToHomeScene.SendEventMessage();

            //LOAD_DATA_FROM_SUBPACKAGE
        }
        else
        {
            QuitGame();
        }
    }

    void OnStartDownloadFile(string fileName, long sizeBytes)
    {
        //console.error($"xxxx:{fileName}");
    }
    void OnDownloadError(string fileName, string error)
    {
        //console.error($"OnDownloadError:{fileName} > {error}");
    }
    void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.AddProgressBar, this, new EventArgsPack((int)LoadingStageEventCode.DownloadPackage, (float)((int)LoadingStageEventCode.DownloadPackage) + (float)currentDownloadCount / (float)totalDownloadCount, currentDownloadCount, totalDownloadCount));
        //console.warn($"正在下载({currentDownloadCount}/{totalDownloadCount}): {(currentDownloadBytes >> 10)}KB/{(totalDownloadBytes >> 10)}KB");
    }

    void QuitGame()
    {
        Application.Quit();//尝试多次失败直接退出游戏，不让玩
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        // Debug.LogError($"本地调试报错解决方法：找到Assets/Resources/{nameof(XIHFrontSetting)}.asset和参考{nameof(AotConfig.InitFrontConfig)}方法修改为你本地的web地址且删除项目根目录下的XIHWebServerRes/Front文件夹，然后重新运行程序自动生成它们；\n Windows下菜单栏 XIHUtil/Server/WebSvr 即可开启本地web服务；\n Mac用户请自行搭建web服务，且设置web根路径为 XIHWebServerRes (与Assets同层级)");
#endif
    }


    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return m_RemoteBundlePath + "/" + fileName;
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return m_RemoteBundlePath + "/" + fileName;
        }
    }
    /// <summary>
    /// 小游戏启动时加载StreamingAsset内资源会被转换为www请求，此时走小游戏缓存可直接返回之前下载的资源
    /// 微信小游戏对于CDN下，带有StreamingAsset的资源可以缓存，所以我们设置YooAsset的下载路径和小游戏读取内置路径的url保持一致即可，
    /// 这样即时首包不包含任何内容，我们触发正常的YooAsset下载，也会被微信缓存了
    /// 所以为了先缓存资源，游戏启动需要先完成资源更新，将远程StreamingAsset资源全部缓存到本地，后面就不需要再从远程获取了
    /// </summary>
    private class BuildinQueryServices : IBuildinQueryServices
    {
        /// <summary>
        /// 查询内置文件的时候，是否比对文件哈希值
        /// </summary>
        public static bool CompareFileCRC = true;

        public bool Query(string packageName, string fileName, string fileCRC)
        {
            return false;
            //console.info($"BuildinQueryServices {packageName} >> {fileName}   {fileCRC}");
            //var res = JStreamingAssetsHelper.FileExists(packageName, fileName, fileCRC);
            //return res;
        }
    }

    private class WechatCacheQueryService : IWechatQueryServices
    {
        public bool Query(string packageName, string fileName, string fileCRC)
        {
            var res = CacheFileNameSet.Contains(fileName);
            //console.error("wxquery:", res, packageName, fileName);
            return res;
        }
    }



#if UNITY_EDITOR
    public sealed class JStreamingAssetsHelper
    {
        public static void Init() { }
        public static bool FileExists(string packageName, string fileName, string fileCRC)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, StreamingAssetsDefine.RootFolderName, packageName, fileName);
            if (File.Exists(filePath))
            {
                if (BuildinQueryServices.CompareFileCRC)
                {
                    string crc32 = YooAsset.HashUtility.FileCRC32(filePath);
                    return crc32 == fileCRC;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
#else
public sealed class JStreamingAssetsHelper
{
    private class PackageQuery
    {
        public readonly Dictionary<string, BuildinFileManifest.Element> Elements = new Dictionary<string, BuildinFileManifest.Element>(1000);
    }

    private static bool _isInit = false;
    private static readonly Dictionary<string, PackageQuery> _packages = new Dictionary<string, PackageQuery>(10);

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        if (_isInit == false)
        {
            _isInit = true;

            var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
            if (manifest != null)
            {
                foreach (var element in manifest.BuildinFiles)
                {
                    if (_packages.TryGetValue(element.PackageName, out PackageQuery package) == false)
                    {
                        package = new PackageQuery();
                        _packages.Add(element.PackageName, package);
                    }
                    package.Elements.Add(element.FileName, element);
                }
            }
        }
    }

    /// <summary>
    /// 内置文件查询方法
    /// </summary>
    public static bool FileExists(string packageName, string fileName, string fileCRC32)
    {
        if (_isInit == false)
            Init();

        if (_packages.TryGetValue(packageName, out PackageQuery package) == false)
            return false;

        if (package.Elements.TryGetValue(fileName, out var element) == false)
            return false;

        if (BuildinQueryServices.CompareFileCRC)
        {
            return element.FileCRC32 == fileCRC32;
        }
        else
        {
            return true;
        }
    }
}
#endif

}
