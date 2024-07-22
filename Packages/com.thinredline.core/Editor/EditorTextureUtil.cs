using ThinRL.Core;

using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.Profiling;

namespace ThinRL.Core.Editor
{
    public static class EditorTextureUtil
    {
        public static float GetBytesPerPixel(TextureImporterFormat format)
        {
            switch (format)
            {
                case TextureImporterFormat.RGBA32:
                    return 4;
                case TextureImporterFormat.RGB24:
                    return 3;
                case TextureImporterFormat.RGBA16:
                    return 2;
                case TextureImporterFormat.Alpha8:
                // 4x4 pixel in 128 bits(16Bytes)
                case TextureImporterFormat.ASTC_4x4:
                case TextureImporterFormat.BC5:
                case TextureImporterFormat.AutomaticCompressed:
                case TextureImporterFormat.BC7:
                case TextureImporterFormat.DXT5:
                    return 1;
                // 5x5 pixel in 128 bits(16Bytes)
                case TextureImporterFormat.ASTC_5x5:
                    return 0.64f;
                case TextureImporterFormat.DXT1:
                case TextureImporterFormat.PVRTC_RGB4:
                case TextureImporterFormat.PVRTC_RGBA4:
                    return 0.5f;
                case TextureImporterFormat.ASTC_6x6:
                    return 0.4444f;
                case TextureImporterFormat.ASTC_8x8:
                case TextureImporterFormat.ETC2_RGBA8:
                case TextureImporterFormat.PVRTC_RGB2:
                case TextureImporterFormat.PVRTC_RGBA2:
                    return 0.25f;
                case TextureImporterFormat.ASTC_10x10:
                    return 0.16f;
                case TextureImporterFormat.ASTC_12x12:
                    return 0.11111f;
                case TextureImporterFormat.ETC_RGB4:
                case TextureImporterFormat.ETC2_RGB4:
                    return 0.125f;
                default:
                    Debug.LogError("Unhandle Format:" + format);
                    // 用很大的书暴露未处理的问题
                    return 10;
            }
        }



        //若raw为True则不换算至Android平台，否则返回Android平台内存
        public static long GetRuntimeMemorySize(Object inst, bool showCurrentPlatformMemory)
        {
            if (inst == null) return 0;

            long count = 0;
            long size = 0;
            // 高频调用Profiler.GetRuntimeMemorySizeLong会随机出现为0的错误返回，需要多调用几次来保证结果正确性
            // 经测试 一般在30万次调用以内可以取到正确结果
            do
            {
                size = Profiler.GetRuntimeMemorySizeLong(inst);
                count++;
            } while (size == 0 && count < 300000);
            if (count >= 300000)
            {
                Debug.Log(("zerosizeinst", inst, size));
            }

            // 编辑器里算texture, mesh内存是实际的2倍
            var meshInst = inst as Mesh;
            var texInst = inst as Texture2D;
            var cubeMap = inst as Cubemap;
            if (meshInst != null && meshInst.isReadable == false)
            {
                size /= 2;
            }
            else if (texInst != null)
            {
                return GetTextureBytesConsiderPlatform(texInst, texInst.format, showCurrentPlatformMemory);
            }
            else if (cubeMap != null)
            {
                return GetTextureBytesConsiderPlatform(cubeMap, cubeMap.format, showCurrentPlatformMemory) * 6;
            }

            return size;
        }

        // 根据平台获取贴图内存大小

        // Profiler.GetRuntimeMemorySizeLong获取贴图内存，大多数情况都是按readable x2的，但有时不x2
        // 只好用自定义实现来屏蔽这种不一致
        static long GetTextureBytesConsiderPlatform(Texture texInst, TextureFormat rFormat, bool showCurrentPlatformMemory)
        {
            // 如果设定为显示原始内存，则直接返回原始size
            if (showCurrentPlatformMemory)
            {
                return TextureUtil.GuessTextureBytes(texInst, TextureUtil.GetBytesPerPixel(rFormat));
            }

            //获取Android下对应ASTC压缩格式进行计算
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texInst)) as TextureImporter;
            if (importer)
            {
                // 编辑器inpsctor没显示有安卓平台时，会返回automatic，会跟真实的不一致
                var importFormat = importer.GetAutomaticFormat("Android");
                return TextureUtil.GuessTextureBytes(texInst, GetBytesPerPixel(importFormat));
            }
            else
            {
                // subAsset没有导入设置，用运行时类型计算
                return TextureUtil.GuessTextureBytes(texInst, TextureUtil.GetBytesPerPixel(rFormat));
            }
        }
    }
}
