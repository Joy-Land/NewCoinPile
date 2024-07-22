// Better Streaming Assets, Piotr Gwiazdowski <gwiazdorrr+github at gmail.com>, 2017

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using Better;
using Better.StreamingAssets;
using Better.StreamingAssets.ZipArchive;
using ThinRL.Core;

#if UNITY_EDITOR
using BetterStreamingAssetsImp = BetterStreamingAssets.EditorImpl;
#elif UNITY_ANDROID
using BetterStreamingAssetsImp = BetterStreamingAssets.ApkImpl;
#else
using BetterStreamingAssetsImp = BetterStreamingAssets.LooseFilesImpl;
#endif

public static partial class BetterStreamingAssets
{
    internal struct ReadInfo
    {
        public string readPath;
        public long size;
        public long offset;
        public uint crc32;
    }

    public static string Root
    {
        get { return BetterStreamingAssetsImp.s_root; }
    }

    public static void Initialize()
    {
        BetterStreamingAssetsImp.Initialize(Application.dataPath, Application.streamingAssetsPath);
    }

#if UNITY_EDITOR
    public static void InitializeWithExternalApk(string apkPath)
    {
        BetterStreamingAssetsImp.ApkMode = true;
        BetterStreamingAssetsImp.Initialize(apkPath, "jar:file://" + apkPath + "!/assets/");
    }

    public static void InitializeWithExternalDirectories(string dataPath, string streamingAssetsPath)
    {
        BetterStreamingAssetsImp.ApkMode = false;
        BetterStreamingAssetsImp.Initialize(dataPath, streamingAssetsPath);
    }
#endif

    public static bool FileExists(string path)
    {
        ReadInfo info;
        return BetterStreamingAssetsImp.TryGetInfo(path, out info);
    }

    public static bool DirectoryExists(string path)
    {
        return BetterStreamingAssetsImp.DirectoryExists(path);
    }

    public static AssetBundleCreateRequest LoadAssetBundleAsync(string path, uint crc = 0)
    {
        var info = GetInfoOrThrow(path);
        return AssetBundle.LoadFromFileAsync(info.readPath, crc, (ulong)info.offset);
    }

    public static AssetBundle LoadAssetBundle(string path, uint crc = 0)
    {
        var info = GetInfoOrThrow(path);
        return AssetBundle.LoadFromFile(info.readPath, crc, (ulong)info.offset);
    }

    public static System.IO.Stream OpenRead(string path)
    {
        if ( path == null )
            throw new ArgumentNullException("path");
        if ( path.Length == 0 )
            throw new ArgumentException("Empty path", "path");

        return BetterStreamingAssetsImp.OpenRead(path);
    }

    public static System.IO.StreamReader OpenText(string path)
    {
        Stream str = OpenRead(path);
        try
        {
            return new StreamReader(str);
        }
        catch (System.Exception)
        {
            if (str != null)
                str.Dispose();
            throw;
        }
    }

    public static string ReadAllText(string path)
    {
        using ( var sr = OpenText(path) )
        {
            return sr.ReadToEnd();
        }
    }

    public static string[] ReadAllLines(string path)
    {
        string line;
        var lines = new List<string>();

        using ( var sr = OpenText(path) )
        {
            while ( ( line = sr.ReadLine() ) != null )
            {
                lines.Add(line);
            }
        }

        return lines.ToArray();
    }

    public static byte[] ReadAllBytes(string path)
    {
        if ( path == null )
            throw new ArgumentNullException("path");
        if ( path.Length == 0 )
            throw new ArgumentException("Empty path", "path");

        return BetterStreamingAssetsImp.ReadAllBytes(path);
    }

    public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return BetterStreamingAssetsImp.GetFiles(path, searchPattern, searchOption);
    }

    public static string[] GetFiles(string path)
    {
        return GetFiles(path, null);
    }

    public static string[] GetFiles(string path, string searchPattern)
    {
        return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    private static ReadInfo GetInfoOrThrow(string path)
    {
        ReadInfo result;
        if ( !BetterStreamingAssetsImp.TryGetInfo(path, out result) )
            ThrowFileNotFound(path);
        return result;
    }

    private static void ThrowFileNotFound(string path)
    {
        throw new FileNotFoundException("File not found", path);
    }

    static partial void AndroidIsCompressedFileStreamingAsset(string path, ref bool result);

#if UNITY_EDITOR
    internal static class EditorImpl
    {
        public static bool ApkMode = false;

        public static string s_root
        {
            get { return ApkMode ? ApkImpl.s_root : LooseFilesImpl.s_root; }
        }

        internal static void Initialize(string dataPath, string streamingAssetsPath)
        {
            if ( ApkMode )
            {
                ApkImpl.Initialize(dataPath, streamingAssetsPath);
            }
            else
            {
                LooseFilesImpl.Initialize(dataPath, streamingAssetsPath);
            }
        }

        internal static bool TryGetInfo(string path, out ReadInfo info)
        {
            if ( ApkMode )
                return ApkImpl.TryGetInfo(path, out info);
            else
                return LooseFilesImpl.TryGetInfo(path, out info);
        }

        internal static bool DirectoryExists(string path)
        {
            if ( ApkMode )
                return ApkImpl.DirectoryExists(path);
            else
                return LooseFilesImpl.DirectoryExists(path);
        }

        internal static Stream OpenRead(string path)
        {
            if ( ApkMode )
                return ApkImpl.OpenRead(path);
            else
                return LooseFilesImpl.OpenRead(path);
        }

        internal static byte[] ReadAllBytes(string path)
        {
            if ( ApkMode )
                return ApkImpl.ReadAllBytes(path);
            else
                return LooseFilesImpl.ReadAllBytes(path);
        }

        internal static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if ( ApkMode )
                return ApkImpl.GetFiles(path, searchPattern, searchOption);
            else
                return LooseFilesImpl.GetFiles(path, searchPattern, searchOption);
        }
    }
#endif

#if UNITY_EDITOR || !UNITY_ANDROID
    internal static class LooseFilesImpl
    {
        public static string s_root;
        private static string[] s_emptyArray = new string[0];

        public static void Initialize(string dataPath, string streamingAssetsPath)
        {
            s_root = Path.GetFullPath(streamingAssetsPath).Replace('\\', '/').TrimEnd('/');
        }

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if (!Directory.Exists(s_root))
                return s_emptyArray;

            // this will throw if something is fishy
            path = PathUtil.NormalizeRelativePath(path, forceTrailingSlash : true);

            Debug.Assert(s_root.Last() != '\\' && s_root.Last() != '/' && path.StartsWithQuickly("/"));

            var files = Directory.GetFiles(s_root + path, searchPattern ?? "*", searchOption);

            for ( int i = 0; i < files.Length; ++i )
            {
                Debug.Assert(files[i].StartsWithQuickly(s_root));
                files[i] = files[i].Substring(s_root.Length + 1).Replace('\\', '/');
            }

#if UNITY_EDITOR
            // purge meta files
            {
                int j = 0;
                for ( int i = 0; i < files.Length; ++i )
                {
                    if ( !files[i].EndsWithQuickly(".meta") )
                    {
                        files[j++] = files[i];
                    }
                }
                Array.Resize(ref files, j);
            }

#endif
            return files;
        }

        public static bool TryGetInfo(string path, out ReadInfo info)
        {
            path = PathUtil.NormalizeRelativePath(path);

            info = new ReadInfo();

            var fullPath = s_root + path;
            if ( !File.Exists(fullPath) )
                return false;

            info.readPath = fullPath;
            return true;
        }

        public static bool DirectoryExists(string path)
        {
            var normalized = PathUtil.NormalizeRelativePath(path);
            return Directory.Exists(s_root + normalized);
        }

        public static byte[] ReadAllBytes(string path)
        {
            ReadInfo info;

            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            return File.ReadAllBytes(info.readPath);
        }

        public static System.IO.Stream OpenRead(string path)
        {
            ReadInfo info;
            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            Stream fileStream = File.OpenRead(info.readPath);
            try
            {
                return new SubReadOnlyStream(fileStream, leaveOpen: false);
            }
            catch ( System.Exception )
            {
                fileStream.Dispose();
                throw;
            }
        }
    }
#endif

#if UNITY_EDITOR || UNITY_ANDROID
    internal static class ApkImpl
    {
        private static string[] s_paths;
        private static Dictionary<string, PartInfo> s_PathPartInfoDictionary;
        public static string s_root;
        /// <summary>
        /// 分包后的apk为多个文件，对多个文件进行记录
        /// </summary>
        public static string[] s_RootPathArray;

        private struct PartInfo
        {
            public long size;
            public long offset;
            public uint crc32;
            public int rootPathIndex;
        }

        public static void Initialize(string dataPath, string streamingAssetsPath)
        {
            Debug.Log("BSA Init");
            s_root = dataPath;
            var searchDirectory = Path.GetDirectoryName(dataPath);
            // var searchDirectory = "/Volumes/zdys_T7_Touch/GitProject/testaab/com.DefaultCompany.buildaab-OSrKjP0Ryz4oMASOpJbtnA==";
            
            Debug.Log($"Bsa search path is {searchDirectory}");
            s_RootPathArray = Directory.GetFiles(searchDirectory, "*.apk", SearchOption.TopDirectoryOnly);

            // var fileArray = new string[]{s_root};

            s_PathPartInfoDictionary = new Dictionary<string, PartInfo>(2 << 14);
            var pathPartInfoDictionary = new Dictionary<string, PartInfo>(2 << 13);

            void Merge<T>(T lh, T rh) where T : Dictionary<string, PartInfo>
            {
                foreach(var rhkv in rh)
                {
                    if(lh.ContainsKey(rhkv.Key))
                    {
                        Debug.LogError($"duplicate path {rhkv.Key}");
                    }
                    else
                    {
                        lh.Add(rhkv.Key, rhkv.Value);
                    }
                }
            }

            // 修改初始化方式，循环读取dataPath所在目录下的所有apk，注意只能遍历一层文件夹，lib/和oat/目录可能有权限问题
            for (int i = 0; i < s_RootPathArray.Length; i++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("BetterStreamingAssets.GetStreamingAssetsInfoFromJar");
                GetStreamingAssetsInfoFromJar(s_RootPathArray[i], pathPartInfoDictionary, i);
                Merge(s_PathPartInfoDictionary, pathPartInfoDictionary);
                pathPartInfoDictionary.Clear();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            if (s_PathPartInfoDictionary.Count == 0 && !Application.isEditor && Path.GetFileName(dataPath) != "base.apk")
            {
                // maybe split?
                var newDataPath = Path.GetDirectoryName(dataPath) + "/base.apk";
                if (File.Exists(newDataPath))
                {
                    s_root = newDataPath;
                    UnityEngine.Profiling.Profiler.BeginSample("BetterStreamingAssets.GetStreamingAssetsInfoFromJar");
                    GetStreamingAssetsInfoFromJar(newDataPath, pathPartInfoDictionary, 0);
                    Merge(s_PathPartInfoDictionary, pathPartInfoDictionary);
                    pathPartInfoDictionary.Clear();
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        public static bool TryGetInfo(string path, out ReadInfo info)
        {
            UnityEngine.Profiling.Profiler.BeginSample("BSA.TryGetInfo");

            // 2021-05-14 23:01:05 Shell Lee
            // 需由上游甚至打包流程，保证路径的统一
            //path = PathUtil.NormalizeRelativePath(path);

            UnityEngine.Profiling.Profiler.BeginSample("BSA.TryGetInfo.New");
            info = new ReadInfo();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("BSA.TryGetInfo.TryGetValue");
            if (!s_PathPartInfoDictionary.TryGetValue(path, out var partInfo))
            {
                UnityEngine.Profiling.Profiler.EndSample();
                return false;
            }
            UnityEngine.Profiling.Profiler.EndSample();


            UnityEngine.Profiling.Profiler.BeginSample("BSA.TryGetInfo.Assign");
            info.crc32 = partInfo.crc32;
            info.offset = partInfo.offset;
            info.size = partInfo.size;
            // info.readPath = s_root;
            info.readPath = s_RootPathArray[partInfo.rootPathIndex];
            UnityEngine.Profiling.Profiler.EndSample();
            
            UnityEngine.Profiling.Profiler.EndSample();

            return true;
            
        }

        public static bool DirectoryExists(string path)
        {
            var normalized = PathUtil.NormalizeRelativePath(path, forceTrailingSlash : true);
            var dirIndex = GetDirectoryIndex(normalized);
            return dirIndex >= 0 && dirIndex < s_paths.Length;
        }

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if ( path == null )
                throw new ArgumentNullException("path");

            var actualDirPath = PathUtil.NormalizeRelativePath(path, forceTrailingSlash : true);

            // find first file there
            var index = GetDirectoryIndex(actualDirPath);
            if ( index < 0 )
                throw new IOException();
            if ( index == s_paths.Length )
                throw new DirectoryNotFoundException();

            Predicate<string> filter;
            if ( string.IsNullOrEmpty(searchPattern) || searchPattern == "*" )
            {
                filter = null;
            }
            else if ( searchPattern.IndexOf('*') >= 0 || searchPattern.IndexOf('?') >= 0 )
            {
                var regex = PathUtil.WildcardToRegex(searchPattern);
                filter = (x) => regex.IsMatch(x);
            }
            else
            {
                filter = (x) => string.Compare(x, searchPattern, true) == 0;
            }

            List<string> results = new List<string>();
            string fixedPath = null;

            for ( int i = index; i < s_paths.Length; ++i )
            {
                var filePath = s_paths[i];

                if ( !filePath.StartsWithQuickly(actualDirPath) )
                    break;

                string fileName;

                var dirSeparatorIndex = filePath.LastIndexOf('/', filePath.Length - 1, filePath.Length - actualDirPath.Length);
                if ( dirSeparatorIndex >= 0 )
                {
                    if ( searchOption == SearchOption.TopDirectoryOnly )
                        continue;

                    fileName = filePath.Substring(dirSeparatorIndex + 1);
                }
                else
                {
                    fileName = filePath.Substring(actualDirPath.Length);
                }

                // now do a match
                if ( filter == null || filter(fileName) )
                {
                    var normalizedPart = filePath.Substring(actualDirPath.Length);

                    if ( fixedPath == null )
                    {
                        fixedPath = PathUtil.FixTrailingDirectorySeparators(path);
                        if ( fixedPath == "/" )
                            fixedPath = string.Empty;
                    }

                    var result = PathUtil.CombineSlash(fixedPath, normalizedPart);
                    results.Add(result);
                }
            }

            return results.ToArray();
        }

        public static byte[] ReadAllBytes(string path)
        {
            ReadInfo info;
            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            byte[] buffer;
            using ( var fileStream = File.OpenRead(info.readPath) )
            {
                if ( info.offset != 0 )
                {
                    if ( fileStream.Seek(info.offset, SeekOrigin.Begin) != info.offset )
                        throw new IOException();
                }

                if ( info.size > (long)int.MaxValue )
                    throw new IOException();

                int count = (int)info.size;
                int offset = 0;

                buffer = new byte[count];
                while ( count > 0 )
                {
                    int num = fileStream.Read(buffer, offset, count);
                    if ( num == 0 )
                        throw new EndOfStreamException();
                    offset += num;
                    count -= num;
                }
            }

            return buffer;
        }

        public static System.IO.Stream OpenRead(string path)
        {
            ReadInfo info;
            if ( !TryGetInfo(path, out info) )
                ThrowFileNotFound(path);

            Stream fileStream = File.OpenRead(info.readPath);
            try
            {
                return new SubReadOnlyStream(fileStream, info.offset, info.size, leaveOpen : false);
            }
            catch ( System.Exception )
            {
                fileStream.Dispose();
                throw;
            }
        }

        private static int GetDirectoryIndex(string path)
        {
            Debug.Assert(s_paths != null);

            // find first file there
            var index = Array.BinarySearch(s_paths, path, StringComparer.OrdinalIgnoreCase);
            if ( index >= 0 )
                return ~index;

            // if the end, no such directory exists
            index = ~index;
            if ( index == s_paths.Length )
                return index;

            for ( int i = index; i < s_paths.Length && s_paths[i].StartsWithQuickly(path); ++i )
            {
                // because otherwise there would be a match
                Debug.Assert(s_paths[i].Length > path.Length);

                if ( path[path.Length - 1] == '/' )
                    return i;

                if ( s_paths[i][path.Length] == '/' )
                    return i;
            }

            return s_paths.Length;
        }

        private static void GetStreamingAssetsInfoFromJar(string apkPath, Dictionary<string, PartInfo> paths, int rootIndex)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            using ( var stream = File.OpenRead(apkPath) )
            using ( var reader = new BinaryReader(stream) )
            {
                if ( !stream.CanRead )
                    throw new ArgumentException();
                if ( !stream.CanSeek )
                    throw new ArgumentException();

                long expectedNumberOfEntries;
                long centralDirectoryStart;
                ZipArchiveUtils.ReadEndOfCentralDirectory(stream, reader, out expectedNumberOfEntries, out centralDirectoryStart);

                try
                {
                    stream.Seek(centralDirectoryStart, SeekOrigin.Begin);

                    long numberOfEntries = 0;

                    ZipCentralDirectoryFileHeader header;

                    const int prefixLength = 7;
                    const string prefix = "assets/";
                    const string assetsPrefix = "assets/bin/";
                    Debug.Assert(prefixLength == prefix.Length);

                    while ( ZipCentralDirectoryFileHeader.TryReadBlock(reader, out header) )
                    {
                        if ( header.CompressedSize != header.UncompressedSize )
                        {
#if UNITY_ASSERTIONS
                            var fileName = Encoding.UTF8.GetString(header.Filename);
                            if (fileName.StartsWithQuickly(prefix) && !fileName.StartsWithQuickly(assetsPrefix))
                            {
                                bool isStreamingAsset = false;
                                AndroidIsCompressedFileStreamingAsset(fileName, ref isStreamingAsset);
                                if (isStreamingAsset)
                                {
                                    Debug.LogAssertionFormat("BetterStreamingAssets: file {0} is where Streaming Assets are put, but is compressed. " +
                                        "If this is a App Bundle build, see README for a possible workaround. " +
                                        "If this file is not a Streaming Asset (has been on purpose by hand or by another plug-in), implement " +
                                        "BetterStreamingAssets.AndroidIsCompressedFileStreamingAsset partial method to prevent this message from appearing again. ",
                                        fileName);
                                }
                            }
#endif
                            // we only want uncompressed files
                        }
                        else
                        {
                            var fileName = Encoding.UTF8.GetString(header.Filename);
                            
                            if (fileName.EndsWithQuickly("/"))
                            {
                                // there's some strangeness when it comes to OBB: directories are listed as files
                                // simply ignoring them should be enough
                                Debug.Assert(header.UncompressedSize == 0);
                            }
                            else if ( fileName.StartsWithQuickly(prefix) )
                            {
                                // ignore normal assets...
                                if ( fileName.StartsWithQuickly(assetsPrefix) )
                                {
                                    // Note: if you put bin directory in your StreamingAssets you will get false negative here
                                }
                                else
                                {
                                    // var relativePath = fileName.Substring(prefixLength - 1);
                                    var relativePath = fileName.Substring(prefixLength - 1);
                                    var entry = new PartInfo()
                                    {
                                        crc32 = header.Crc32,
                                        offset = header.RelativeOffsetOfLocalHeader, // this offset will need fixing later on
                                        size = header.UncompressedSize,
                                        rootPathIndex = rootIndex
                                    };

                                    if (paths.ContainsKey(relativePath) )
                                        throw new System.InvalidOperationException("Paths duplicate! " + fileName);

                                    paths.Add(relativePath, entry);
                                }
                            }
                        }

                        numberOfEntries++;
                    }

                    if ( numberOfEntries != expectedNumberOfEntries )
                        throw new ZipArchiveException("Number of entries does not match");

                }
                catch ( EndOfStreamException ex )
                {
                    throw new ZipArchiveException("CentralDirectoryInvalid", ex);
                }

                // now fix offsets
                foreach (var path in paths.Keys.ToArray())
                {
                    var entry = paths[path];
                    stream.Seek(entry.offset, SeekOrigin.Begin);

                    if ( !ZipLocalFileHeader.TrySkipBlock(reader) )
                        throw new ZipArchiveException("Local file header corrupt");

                    entry.offset = stream.Position;

                    paths[path] = entry;
                }
            }

            sw.Stop();

            Debug.Log($"Get {paths.Count} StreamingAssetsInfoFromJar finish cost {sw.Elapsed.TotalMilliseconds} milliseconds for {apkPath}");
        }
    }
#endif
}

