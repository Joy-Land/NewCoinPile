#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX
using WeChatWASM;
#endif
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;



using UnityEngine;

namespace ThinRL.Core.FileSystem
{
    public static class FileUtil
    {
        private static readonly string RootPath;
        static FileUtil()
        {
#if UNITY_EDITOR
            RootPath = "SaveDir";
#elif UNITY_STANDALONE
            RootPath = Application.streamingAssetsPath;
#elif UNITY_WX
            RootPath = WX.env.USER_DATA_PATH;
//#elif UNITY_DY
//            RootPath = StarkSDKSpace.StarkFileSystemManager.USER_DATA_PATH;
#else
            RootPath = Application.persistentDataPath;
#endif
        }
        // 是否启用stream计数，统计实际的文件io数量
        public static bool EnableRecordStreamCount => true;

        // 项目内目前有两个：BSAZipAccessor（启用）、AndroidJniZipAccessor（未启用），两者都不支持压缩文件
        private static readonly List<IZipAccessor> s_ZipAccessors = new List<IZipAccessor>();

        /// <summary>
        /// editor下测试apk模式，需存在Build/Android/gbf.apk
        /// </summary>
        public static readonly bool s_TestApkInEditorMode = true;

        public static void Register(IZipAccessor zipAccessor)
        {
            if (s_ZipAccessors.Contains(zipAccessor))
                return;
            s_ZipAccessors.Add(zipAccessor);
            s_ZipAccessors.Sort((x, y) => y.Priority.CompareTo(x.Priority));
        }

        public static void Unregister(IZipAccessor zipAccessor)
        {
            s_ZipAccessors.Remove(zipAccessor);
        }

        public static void WriteFile(string relativePath, string content)
        {
            var fullPath = RootPath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            WeChatWASM.WX.GetFileSystemManager().WriteFileSync(fullPath, content);
#elif UNITY_DY
            StarkSDKSpace.StarkSDK.API.GetStarkFileSystemManager().WriteFileSync(fullPath, content);
#else
            File.WriteAllText(fullPath, content, UTF8Encoding.UTF8);
#endif
        }

        public static string ReadFile(string relativePath)
        {
            var fullPath = RootPath + "/" + relativePath;
#if UNITY_WX_WITHOUT_EDITOR
            var exist = "access:ok".Equals(WX.GetFileSystemManager().AccessSync(fullPath));
            if (!exist) return "";
            return WX.GetFileSystemManager().ReadFileSync(fullPath,"utf8");
#elif UNITY_DY
            var exist = StarkSDKSpace.StarkSDK.API.GetStarkFileSystemManager().AccessSync(fullPath);
            if (!exist) return "";
            return StarkSDKSpace.StarkSDK.API.GetStarkFileSystemManager().ReadFileSync(fullPath, "utf8");
#else
            if (!File.Exists(fullPath)) return "";
            return File.ReadAllText(fullPath, UTF8Encoding.UTF8);
#endif
        }

        public static void FileLineWalker(string filePath, Action<StringBuilder> callback)
        {
            if (null == callback)
            {
                return;
            }

            using (var stream = OpenRead(filePath))
            {
                if (stream == null)
                {
                    Debug.LogError("文件打开失败，请检查路径与文件是否存在");

                    return;
                }

                using (var streamReader = new StreamReaderNoAlloc(stream))
                {
                    StringBuilder sb = new StringBuilder(256);

                    while (streamReader.ReadLine(sb))
                    {
                        callback(sb);
                    }
                }
            }
        }

        public static string[] ReadAllLines(string path)
        {
            return ReadAllLines(path, Encoding.UTF8);
        }

        public static string[] ReadAllLines(string path, Encoding encoding)
        {
            if (!IsZipArchive(path))
                return File.ReadAllLines(path, encoding);

            string line;
            List<string> lines = new List<string>();
            using (var stream = OpenReadInZip(path))
            {
                using (StreamReader sr = new StreamReader(stream, encoding, true))
                {
                    while ((line = sr.ReadLine()) != null)
                        lines.Add(line);
                }
            }
            return lines.ToArray();
        }

        public static string ReadAllText(string path)
        {
            return ReadAllText(path, Encoding.UTF8);
        }

        public static string ReadAllText(string path, Encoding encoding)
        {
            if (!IsZipArchive(path))
                return File.ReadAllText(path, encoding);

            using (var stream = OpenReadInZip(path))
            {
                using (StreamReader sr = new StreamReader(stream, encoding, true))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static byte[] ReadAllBytes(string path)
        {
            if (!IsZipArchive(path))
                return File.ReadAllBytes(path);

            using (var stream = OpenReadInZip(path))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static Stream OpenRead(string path, bool deletable = false, int bufferSize = 4096)
        {
            if (string.IsNullOrEmpty(path))
            {
                return default;
            }

            Stream result;
            
            UnityEngine.Profiling.Profiler.BeginSample("FileUtil.OpenRead");

            if (!IsZipArchive(path))
            {
                result = OpenReadFileStreamInternal(path, deletable, bufferSize);
            }
            else
            {
                result = OpenReadInZip(path, bufferSize);
            }

            UnityEngine.Profiling.Profiler.EndSample();

            return result;
        }

        /// <summary>
        /// 打开一个stream provider，判定过程比较复杂，且存在一定效率问题，所以尽量只调用一次得到provider即可
        /// provider内部持有一个OpenReadParameter，结合`Stream OpenReadByParameter(OpenReadParameter parameter)`进行io处理
        /// </summary>
        /// <param name="path"></param>
        /// <param name="deletable"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static IAttachableStreamProvider GetStreamProvider(string path, bool deletable = true, int bufferSize = 4096)
        {
            IAttachableStreamProvider result = null;
            
            UnityEngine.Profiling.Profiler.BeginSample("FileUtil.DelegateStreamProvider");

            if (IsZipArchive(path))
            {
                result = GetStreamProviderInZip(path, bufferSize);
            }

            if (result == null)
            {
                result = new DefaultStreamProvider(new OpenReadParameter(path, bufferSize, deletable));
            }

            UnityEngine.Profiling.Profiler.EndSample();

            return result;
        }

        /// <summary>
        /// 通过参数打开stream
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static Stream OpenReadByParameter(OpenReadParameter parameter)
        {
            Stream result = null;

            try
            {
#if UNITY_ANDROID && false
                if (parameter.useAndroidJni)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("FileUtil.AndroidJniZipAccessor.OpenReadStatic");
                    result = AndroidJniZipAccessor.OpenReadStatic(parameter.filePath, parameter.bufferSize);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                else
#endif
                {
                    UnityEngine.Profiling.Profiler.BeginSample("FileUtil.OpenReadFileStream.SubReadOnlyStream.Inner");
                    result = FileUtil.OpenReadFileStreamInternal(parameter.filePath, parameter.shareDelete, parameter.bufferSize);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
            catch (System.Exception ex)
            {
                result?.Dispose();

                Debug.LogException(ex);
                throw ex;
            }

            return result;
        }

        internal static Stream OpenReadFileStream(string file, int bufferSize = 4096)
        {
            return OpenReadFileStreamInternal(file, false, bufferSize);
        }

        internal static Stream OpenReadFileStreamDeletable(string file, int bufferSize = 4096)
        {
            return OpenReadFileStreamInternal(file, true, bufferSize);
        }

        internal static Stream OpenReadFileStreamInternal(string file, bool deletable, int bufferSize = 4096)
        {
            var share = deletable ? FileShare.Read | FileShare.Delete : FileShare.Read;

            Stream result = new FileStream(file, FileMode.Open, FileAccess.Read, share, bufferSize);


            if (EnableRecordStreamCount)
            {
                result = new CountableStreamProxy(result);
            }

            return result;
        }

        public static bool Exists(string path)
        {
            if (!IsZipArchive(path))
                return File.Exists(path);

            return ExistsInZip(path);
        }

        private static Stream OpenReadInZip(string path, int bufferSize = 4096)
        {
            // 现在只启用了BSAZipAccessor，整个文件用UNITY_ANDROID宏包住了
            for (int i = 0; i < s_ZipAccessors.Count; i++)
            {
                IZipAccessor zipAccessor = s_ZipAccessors[i];
                if (zipAccessor.Support(path))
                    return zipAccessor.OpenRead(path, bufferSize);
            }

#if UNITY_ANDROID
            //if (path.IndexOf(".obb", StringComparison.OrdinalIgnoreCase) > 0 && log.IsWarnEnabled)
            //    log.Warn("Unable to read the content in the \".obb\" file, please click the link for help, and enable access to the OBB file. https://github.com/cocowolf/loxodon-framework/blob/master/docs/faq.md");
#endif

            throw new NotSupportedException(path);
        }

        private static IAttachableStreamProvider GetStreamProviderInZip(string path, int bufferSize = 4096)
        {
            for (int i = 0; i < s_ZipAccessors.Count; i++)
            {
                IZipAccessor zipAccessor = s_ZipAccessors[i];
                if (zipAccessor.Support(path))
                    return zipAccessor.GetStreamProvider(path, bufferSize);
            }

#if UNITY_ANDROID
            //if (path.IndexOf(".obb", StringComparison.OrdinalIgnoreCase) > 0 && log.IsWarnEnabled)
            //    log.Warn("Unable to read the content in the \".obb\" file, please click the link for help, and enable access to the OBB file. https://github.com/cocowolf/loxodon-framework/blob/master/docs/faq.md");
#endif

            return null;
        }

        private static bool ExistsInZip(string path)
        {
            for (int i = 0; i < s_ZipAccessors.Count; i++)
            {
                IZipAccessor zipAccessor = s_ZipAccessors[i];
                if (zipAccessor.Support(path) && zipAccessor.Exists(path))
                {
                    return true;
                }
            }

            return false;
        }

        static readonly string[] s_ZipFilePrefix = new string[] { "jar:file:///" };
        static readonly string[] s_ZipFileSuffix = new string[] { ".jar", ".apk", ".obb", ".zip" };

        /// <summary>
        /// 判断是不是zip形式，正则匹配：@"(jar:file:///)|(\.jar)|(\.apk)|(\.obb)|(\.zip)"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsZipArchive(string path)
        {
#if UNITY_ANDROID
            if (Application.isEditor && s_TestApkInEditorMode && ExistsInZip(path))
            {
                return true;
            }
#endif

            UnityEngine.Profiling.Profiler.BeginSample("FileUtil.IsZipArchive");

            bool result = false;

            if (!string.IsNullOrEmpty(path))
            {
                foreach (var prefix in s_ZipFilePrefix)
                {
                    if (path.StartsWithQuickly(prefix))
                    {
                        result = true;
                        break;
                    }
                }

                if (!result)
                {
                    foreach (var suffix in s_ZipFileSuffix)
                    {
                        if (path.ContainsQuickly(suffix))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();

            return result;
        }
        public static void ClearDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                if (File.Exists(dir))
                {
                    File.Delete(dir);
                }

                return;
            }

            var directoryInfo = new DirectoryInfo(dir);

            // 删除所有内部文件
            directoryInfo.GetFiles().ToList().ForEach(fi => fi.Delete());

            // 删除所有内部文件夹
            directoryInfo.GetDirectories().ToList().ForEach(di => di.Delete(true));
        }

        public interface IZipAccessor
        {
            int Priority { get; }

            bool Support(string path);

            Stream OpenRead(string path, int bufferSize = 4096);

            IAttachableStreamProvider GetStreamProvider(string path, int bufferSize = 4096);

            bool Exists(string path);
        }
    }
}