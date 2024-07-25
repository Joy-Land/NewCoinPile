


#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Event;
using YooAsset;
using UnityEngine.Networking;
using ThinRL.Core;
using ThinRL.Core.FileSystem;
using Cysharp.Threading.Tasks;
using Joyland.GamePlay;
using Newtonsoft.Json;
using UnityEngine.Scripting;






[Serializable]
public class FrontConfig
{
    public string focusVersion = "0.0.0";//最低强更版本
    public string cdn;//可用来切换CDN，替换微信小游戏设置的CDN，使得缓存策略调整
    //yooasset的下载资源路径，后期可以扩展其他的
    public string defaultHostServer;
    public string fallbackHostServer;
}

public class TempGameConfig
{
    public const string PACKAGE_NAME = "DefaultPackage";
    public static FrontConfig frontConfig = new FrontConfig();

    public static string GetFrontUrl()
    {
        string url = GetHotFrontUrlPrefix();
        if (!url.EndsWith("/")) url += "/";
#if UNITY_EDITOR
        url += $"{UnityEditor.EditorUserBuildSettings.activeBuildTarget}.json";
#else
            if (Application.platform == RuntimePlatform.Android)
                url += "Android.json";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                url += "iOS.json";
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
                url += "WebGL.json";
            else
                url += "StandaloneWindows64.json";
#endif
        var epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var unixTime = (long)(DateTime.UtcNow - epochTime).TotalSeconds;
        url += "?ts=" + unixTime;
        return url;
    }

    static string GetHotFrontUrlPrefix()
    {
        string url = "";
        //尝试本地文件获取url
        if (string.IsNullOrEmpty(url) || !url.StartsWith("http"))
        {
            //url = Resources.Load<XIHFrontSetting>(nameof(XIHFrontSetting)).front;
            url = "todo____________________________";
        }
        else
        {
            Debug.Log($"使用外置的Front地址：{url}");
        }
        return url;
    }


}

[Preserve]
public class Boot : MonoBehaviour
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

    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;


    void Awake()
    {
        PlayMode = EPlayMode.WebPlayMode;

        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);

#if UNITY_EDITOR
        PlayMode = EPlayMode.EditorSimulateMode;
#else
#if UNITY_WEBGL
                     PlayMode = EPlayMode.WebPlayMode;
#else
                     PlayMode = EPlayMode.HostPlayMode;
#endif
#endif

        Debug.Log($"资源系统运行模式：{PlayMode}");
    }

    void Start()
    {
        // 游戏管理器
        YooAsset.Sample.GameManager.Instance.Behaviour = this;

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

        EventManager.Instance.AddEvent((-100).ToString(), OnUpdateFinish);


        // // 加载更新页面
        // var go = Resources.Load<GameObject>("PatchWindow");
        // GameObject.Instantiate(go);

        // // 开始补丁更新流程
        // PatchOperation operation = new PatchOperation("DefaultPackage", EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), PlayMode);
        // YooAssets.StartOperation(operation);
        // yield return operation;

        // // 设置默认的资源包
        // var gamePackage = YooAssets.GetPackage("DefaultPackage");
        // YooAssets.SetDefaultPackage(gamePackage);

        // // 切换到主页面场景
        // SceneEventDefine.ChangeToHomeScene.SendEventMessage();
    }

    public async void OnUpdateFinish(object sender, EventArgsPack e)
    {
        //测试

        var asset = Resources.Load<TextAsset>("ui_comp_config");
        var compConfig = JsonConvert.DeserializeObject<Dictionary<int, UICompConfig>>(asset.text);

        //Dictionary<int, UICompConfig> ccc = new Dictionary<int, UICompConfig>();
        //ccc.Add((int)UICompID.UITestComponent, new UICompConfig() { bundleName = "UITestComponent", packageName = "DefaultPackage", preloadAmount = 2 });
        //ccc.Add((int)UICompID.UITest1Component, new UICompConfig() { bundleName = "UITest1Component", packageName = "DefaultPackage", preloadAmount = 2 });
        //UITest1Component
        await UIManager.Instance.InitUICompListConfig(compConfig);
    }


    private static string m_CDNRootPath = "https://cdn.joylandstudios.com/UnityTest/webgl/StreamingAssets/yoo/DefaultPackage";//https://cdn.joylandstudios.com/UnityTest/webgl/StreamingAssets/yoo/DefaultPackage
    //const string cndROOTPath = "https://cdn.joylandstudios.com/UnityTest/DefaultPackage";
    void InitConfig(int tryTime)
    {
        // var www = UnityWebRequest.Get()
#if UNITY_WX && !UNITY_EDITOR
            WX.SetDataCDN(m_CDNRootPath);
#endif
        var json = "";
        TextAsset text = Resources.Load<TextAsset>("version_config");
        json = text.text;
        if (!json.IsNullOrEmpty())
        {
            var config = JsonConvert.DeserializeObject<VersionConfig>(json);
            console.error("cdnURL:",config.debug.cdn, config.release.cdn);

#if JOYLAND_DEBUG
            m_CDNRootPath = config.debug.cdn;
#else
            m_CDNRootPath = config.release.cdn;
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
        var package = YooAssets.CreatePackage(TempGameConfig.PACKAGE_NAME);

        // 编辑器下的模拟模式
        InitializeParameters createParameters = null;
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            createParameters = new EditorSimulateModeParameters()
            {
                SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.ScriptableBuildPipeline.ToString(), TempGameConfig.PACKAGE_NAME)
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
                BuildinQueryServices = new BuildinQueryServices()
            };
            //因为微信小游戏平台的特殊性，需要关闭WebGL的缓存系统，使用微信自带的缓存系统。
            YooAssets.SetCacheSystemDisableCacheOnWebGL();
        }


        var initializationOperation = package.InitializeAsync(createParameters);
        await initializationOperation.ToUniTask();

        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError("错误。。。");
            return;
        }
        UpdatePackageVersionAsync(package).Forget();

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
        var downloader = package.CreateResourceDownloader("Aot2Hot", downloadingMaxNum, failedTryAgain);

        //注册回调方法，这里AOT使用这个是避免裁剪，不然HOT找不到该方法了
        downloader.OnDownloadErrorCallback = OnDownloadError;
        downloader.OnDownloadProgressCallback = OnDownloadProgress;

        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            //GotoAot2HotScene().Forget();
            Debug.LogError("完成，没有要下载的了");

            var gamePackage = YooAssets.GetPackage("DefaultPackage");
            YooAssets.SetDefaultPackage(gamePackage);
            EventManager.Instance.DispatchEvent((-100).ToString(), null, null);

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
            EventManager.Instance.DispatchEvent((-100).ToString(), null, null);
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

    void OnDownloadError(string fileName, string error)
    {
        Debug.LogError($"OnDownloadError:{fileName} > {error}");
    }
    void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        Debug.LogWarning($"正在下载({currentDownloadCount}/{totalDownloadCount}): {(currentDownloadBytes >> 10)}KB/{(totalDownloadBytes >> 10)}KB");
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
            return m_CDNRootPath + "/" + fileName;
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return m_CDNRootPath + "/" + fileName;
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
        public bool Query(string packageName, string fileName, string fileCRC)
        {
            Debug.Log($"BuildinQueryServices {packageName} >> {fileName}   {fileCRC}");
            return false;
        }
    }
}