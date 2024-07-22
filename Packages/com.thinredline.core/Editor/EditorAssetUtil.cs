using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine.Profiling;

namespace ThinRL.Core.Editor
{
    public class EditorAssetUtil
    {

        // sprite计算内存的模式
        public enum SpriteMemoryMode
        {
            [LabelText("散图")]
            Texture = 0,            // 按sprite直接对应的贴图算
        }

        //通用检测的信息类
        public class CommonAssetData
        {
            public long memorySize;

            public string assetPath;

            public Object assetObj;

            public List<CommonAssetData> depsList;

        }

        //资源信息类
        public class AssetInfo
        {
            public string assetPath;

            public Object depObject;

            public bool isAtlas;

            // 个别资源需要制定额外的比率，比如对不压缩的raw atlas散图，我们希望分析内存时按其压缩大小来算
            public float customScale = 1;

        }

        public const double MBSize = 1024.0 * 1024.0;

        public static string GetMBString(long size)
        {
            var sizeStr = string.Format("{0:N3}", GetMBNumber(size));
            return sizeStr;
        }
        public static float GetMBNumber(long size)
        {
            return (float)(size / MBSize);
        }

        // 是否为被过滤掉的资源
        // filenameSuffix 为.扩展名之前的部分
        public static bool IsFilteredAsset(string assetPath, List<string> pathPrefix = null, List<string> filenameSuffix = null)
        {
            if (pathPrefix != null)
            {
                foreach (var p in pathPrefix)
                    if (string.IsNullOrEmpty(p) == false && assetPath.StartsWith(p))
                        return true;
            }

            if (filenameSuffix != null)
            {
                var filename = Path.GetFileNameWithoutExtension(assetPath);
                foreach (var s in filenameSuffix)
                    if (string.IsNullOrEmpty(s) == false && filename.EndsWith(s))
                        return true;
            }

            return false;
        }

        // 一个资源出去被过滤的依赖之后的内存值
        public static long GetMemorySizeWithoutFilteredAsset(CommonAssetData assetInfo, List<string> pathPrefix = null, List<string> filenameSuffix = null)
        {
            long memSize = 0;
            if (assetInfo.depsList == null)
            {
                return assetInfo.memorySize;
            }
            foreach (var ast in assetInfo.depsList)
            {
                if (!IsFilteredAsset(ast.assetPath, pathPrefix, filenameSuffix))
                    memSize += ast.memorySize;
            }
            return memSize;
        }

        //基于反射的方法来获取图集的内存占用
        public static long GetAtlasMemorySize(string assetPath)
        {
            long size = 0;
            var atlasInst = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);

            if (atlasInst == null)
            {
                Debug.LogError($"没有图集资源：{assetPath}，其内存算作0");
                return size;
            }
            var nowSetting = SpriteAtlasExtensions.GetPlatformSettings(atlasInst, "Android");
            MethodInfo getPreviewTextureMI = typeof(SpriteAtlasExtensions).GetMethod("GetPreviewTextures", BindingFlags.Static | BindingFlags.NonPublic);
            Texture2D[] atlasTextures = (Texture2D[])getPreviewTextureMI.Invoke(null, new Object[] { atlasInst });

            for (int i = 0; i < atlasTextures.Length; i++)
            {
                //Debug.Log(atlasTextures[i] + " " + size = Profiler.GetRuntimeMemorySizeLong(atlasTextures[i]));
                size += TextureUtil.GuessTextureBytes(atlasTextures[i], EditorTextureUtil.GetBytesPerPixel(nowSetting.format));
            }

            return size;
        }

        // 不重复的记录下每个原子资源
        static void TryAddAtomicAsset(Object obj, string assetPath, ref Dictionary<Object, AssetInfo> atomicAssets, SpriteMemoryMode atlasMode = SpriteMemoryMode.Texture)
        {
            if (obj == null || obj as MonoScript != null || obj.GetType().IsSubclassOf(typeof(Component)))
                return;


            Object recordObj = obj;
            float memScale = 1;
            bool isAtlas = false;
            string recordPath = assetPath;

            if (recordObj != null && !atomicAssets.ContainsKey(recordObj))
            {
                AssetInfo nowAsset = new AssetInfo();
                nowAsset.assetPath = recordPath;
                nowAsset.isAtlas = isAtlas;
                nowAsset.depObject = recordObj;
                nowAsset.customScale = memScale;
                atomicAssets.Add(recordObj, nowAsset);
            }
        }
        //获取当前资源依赖的资源信息
        public static List<AssetInfo> GetAssetAndDepsInfo(string rootAssetPath, SpriteMemoryMode atlasMode = SpriteMemoryMode.Texture)
        {
            var depAssetArr = ThinRLAssetDatabase.GetDependencyFilesAccurate(rootAssetPath, true);
            Dictionary<Object, AssetInfo> depDict = new Dictionary<Object, AssetInfo>(depAssetArr.Length);

            foreach (var depPath in depAssetArr)
            {
                if (BaseEditorUtil.IsMeta(depPath) || BaseEditorUtil.IsCSharpScript(depPath) ||
                    BaseEditorUtil.IsDllFile(depPath) || string.IsNullOrEmpty(depPath))
                    continue;

                var mainAsset = AssetDatabase.LoadMainAssetAtPath(depPath);
                TryAddAtomicAsset(mainAsset, depPath, ref depDict, atlasMode);

                // 子资源，比如sprite、material、texture
                var subAssetArr = AssetDatabase.LoadAllAssetRepresentationsAtPath(depPath);
                foreach (var obj in subAssetArr)
                {
                    TryAddAtomicAsset(obj, depPath, ref depDict, atlasMode);
                }
            }

            List<AssetInfo> depList = new List<AssetInfo>();
            foreach (var dep in depDict)
            {
                depList.Add(dep.Value);
            }

            return depList;
        }
        //获取普通资源的数据
        // @param computePrefabWithDepSize, 是否计算被依赖的prefab在包含依赖的情况下的大小
        public static CommonAssetData GetAssetCommonData(string assetPath, bool currentPlatform, SpriteMemoryMode atlasMode = SpriteMemoryMode.Texture)
        {            
            List<AssetInfo> depInfoList = GetAssetAndDepsInfo(assetPath, atlasMode);

            long totalMemBytes = 0;
            CommonAssetData data = new CommonAssetData();
            data.assetPath = assetPath;
            data.depsList = new List<CommonAssetData>();
            for (int j = 0; j < depInfoList.Count; j++)
            {
                var item = depInfoList[j];
                long depMemBytes = 0;
                if (item.isAtlas)
                {
                    //获取对应的图集的大小
                    depMemBytes = GetAtlasMemorySize(item.assetPath);
                }
                else
                {
                    depMemBytes = (long)(EditorTextureUtil.GetRuntimeMemorySize(item.depObject, currentPlatform) * item.customScale);
                }

                totalMemBytes += depMemBytes;

                CommonAssetData depData = new CommonAssetData();
                depData.assetPath = item.assetPath;
                depData.memorySize = depMemBytes;
                depData.assetObj = item.depObject;
                data.depsList.Add(depData);
            }

            data.depsList.Sort((a, b) => b.memorySize.CompareTo(a.memorySize));
            data.memorySize = totalMemBytes;
            return data;
        }

        //测算Prefab实例化消耗内存，待完善
        public static long GetInstantiateMemory(GameObject obj, out long instanceMemorySizeAfterGCCollect, out float instanceTimeMS)
        {
            if (obj == null)
            {
                instanceMemorySizeAfterGCCollect = 0;
                instanceTimeMS = 0;
                return 0;
            }


            // 编辑器下不能设置GarbageCollector.GCMode
            // 无法关闭c# gc，至少先尽量回收空出内存，再尽量不回收
            System.GC.Collect();
            var baseSize = Profiler.GetMonoUsedSizeLong();

            var t0 = Time.realtimeSinceStartup;
            var instObj = Object.Instantiate(obj);
            instanceTimeMS = (Time.realtimeSinceStartup - t0) * 1000;

            var incSize = Profiler.GetMonoUsedSizeLong() - baseSize;
            System.GC.Collect();
            instanceMemorySizeAfterGCCollect = Profiler.GetMonoUsedSizeLong() - baseSize;
            Object.DestroyImmediate(instObj);

            if (incSize < 0)
            {
                Debug.LogError($"{obj.name} inc MB:{EditorAssetUtil.GetMBString(incSize)}");
            }
            return incSize;
        }
    }
}
