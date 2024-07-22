/*
功能描述：
    本模块用于维护BetterStreamingAssets接入框架，BSA可以直接把apk当文件流打开，然后偏移到某个内部文件的头部位置开始读取，
    使用BSA可以同步读取非bundle文件，而不是基于WWW或UnityWebRequest的异步，同时.bytes操作会造成额外的堆内存分配，导致过高的内存峰值
    https://answer.uwa4d.com/question/5a8105ab2174bd258ff90fc8
    但不支持压缩文件
注意事项：
    1. editor下调试apk内部文件时候，需要先将RelatedFiles/1_1_FileList.txt拷贝到RemoteAssetBundle/Android/RelatedFiles目录下
       解决调试问题（调试时候，apk内部文件存在与否的判定，是依据RemoteAssetBundle相对路径进行组装再判定的）
*/

#if UNITY_ANDROID// || true

using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using BetterStreamingAssetsImp = BetterStreamingAssets.EditorImpl;
#elif UNITY_ANDROID
using BetterStreamingAssetsImp = BetterStreamingAssets.ApkImpl;
#else
using BetterStreamingAssetsImp = BetterStreamingAssets.LooseFilesImpl;
#endif

namespace ThinRL.Core.FileSystem
{
    public partial class BSAZipAccessor : FileUtil.IZipAccessor
    {
        public int Priority => 512;

        static string s_Root;

        const string k_SubFileTag = "!/assets/";

        int m_SubFileTagIndexOffset => k_SubFileTag.Length - 1;  // 8，由于底层存储的是/Android/...，以斜杠开头，取子串的时候要保留"!/assets/"的最后一个"/"

        // 黑科技：单apk文件流模式
        // 读取同一个apk，可以只open有限个stream（每个线程一个），上层的其他stream都基于他们做实际的I/O操作
        public static bool EnableShareApkStream = true;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void OnInitialized()
        {
            FileUtil.Register(new BSAZipAccessor());

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            Debug.Log("BSAZipAccessor.OnInitialized");

#if !UNITY_EDITOR
            BetterStreamingAssets.Initialize();

            s_Root = Application.dataPath;
#else
            s_Root = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Build/Android/gbf.apk").Replace("\\", "/");

            if (File.Exists(s_Root))
            {
                BetterStreamingAssets.InitializeWithExternalApk(s_Root);
            }
#endif

            sw.Stop();

            Debug.Log($"BSAZipAccessor.OnInitialized finish cost {sw.Elapsed.TotalMilliseconds} milliseconds");
        }

        /// <summary>
        /// 获取资源相对路径（如果输入的是绝对路径，那么会有substring消耗，如果输入是相对路径，那么只有简单的find消耗）
        /// </summary>
        /// <param name="path">绝对路径或相对路径</param>
        /// <returns></returns>
        protected string GetAssetFilePath(string path)
        {
            var result = path.Trim();

            if (!string.IsNullOrEmpty(path))
            {
                int start = path.LastIndexOfQuickly(k_SubFileTag);

                if (start >= 0)
                {
                    result = path.Substring(start + m_SubFileTagIndexOffset);
                }
            }

            if (Application.isEditor && result.ContainsQuickly("RemoteAssetBundle"))
            {
                var resultNew = path.Replace("RemoteAssetBundle", string.Empty);

                if (BetterStreamingAssetsImp.TryGetInfo(resultNew, out var info))
                {
                    result = resultNew;
                }
            }

            return result;
        }

        bool TryGetInfo(string path, out BetterStreamingAssets.ReadInfo info)
        {
            if (string.IsNullOrEmpty(path))
            {
                info = default;

                return false;
            }

            path = path.Trim();

            if (!string.IsNullOrEmpty(path))
            {
                int start = path.LastIndexOfQuickly(k_SubFileTag);

                if (start >= 0)
                {
                    path = path.Substring(start + m_SubFileTagIndexOffset);
                }
            }

            bool result = false;
            info = default;

            // editor下只是为了兼容，如果不成功，那么继续向后按原有方式进行
            if (Application.isEditor && path.ContainsQuickly("RemoteAssetBundle"))
            {
                var pathNew = path.Replace("RemoteAssetBundle", string.Empty);

                result = BetterStreamingAssetsImp.TryGetInfo(pathNew, out info);
            }

            result = result || BetterStreamingAssetsImp.TryGetInfo(path, out info);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">绝对路径或相对路径</param>
        /// <returns></returns>
        public bool Exists(string path)
        {
            return BetterStreamingAssets.FileExists(GetAssetFilePath(path));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">绝对路径或相对路径</param>
        /// <returns></returns>
        public Stream OpenRead(string path, int bufferSize = 4096)
        {
            // 逻辑比较复杂，直接使用封装好的函数，而不是重新写一遍，牺牲一点性能，但更好维护
            return new ComposableStreamProxy(GetStreamProvider(path, bufferSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">绝对路径或相对路径</param>
        /// <returns></returns>
        public IAttachableStreamProvider GetStreamProvider(string path, int bufferSize = 4096)
        {
            UnityEngine.Profiling.Profiler.BeginSample("BSAZipAccessor.GetStreamProvider");

            StreamProviderBase result;

            try
            {
                if (TryGetInfo(path, out var info))
                {
                    var parameter = new OpenReadParameter(info.readPath, bufferSize, true, info.offset, info.size);

                    if (EnableShareApkStream)
                    {
                        result = new ThreadBindStreamProvider(parameter);
                    }
                    else
                    {
                        result = new DefaultStreamProvider(parameter);
                    }
                }
                else
                {
                    result = new DefaultStreamProvider(new OpenReadParameter(path, bufferSize, true));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);

                throw;
            }
            finally
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">绝对路径</param>
        /// <returns></returns>
        public bool Support(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

#if UNITY_EDITOR  // 方便editor下测试apk
            return Exists(path);
#endif

            string fullname = path.ToLower();

            if (fullname.IndexOfQuickly(".apk") > 0 && fullname.LastIndexOfQuickly(k_SubFileTag) > 0)
            {
                return true;
            }

            return false;
        }
    }
}
#endif

namespace ThinRL.Core.FileSystem
{
    public partial class BSAZipAccessorWrapper
    {
        public static void OnReInitialized()
        {
#if UNITY_ANDROID
            BSAZipAccessor.OnInitialized();
#endif
        }
    }
}
