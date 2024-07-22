using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ThinRL.Core.Editor
{
    public class EditorTextureImportUtil
    {
        // 统一导入配置的压缩等级，如果是pc平台可以选一种转换即可
        public enum TextureImporterCompressLevel
        {
            LodRaw_TrueColor = -1,
            Lod0_ASTC4x4 = 0,
            Lod1_ASTC5x5 = 1,
            Lod2_ASTC6x6 = 2,
            Lod3_ASTC8x8 = 3,
            Lod4_ASTC10x10 = 4,
            Lod5_ASTC12x12 = 5,
        }

        // 贴图导入最大尺寸
        public enum TextureImporterMaxSize
        {
            _2 = 0,
            _4 = 1,
            _8 = 2,
            _16 = 3,
            _32 = 4,
            _64 = 5,
            _128 = 6,
            _256 = 7,
            _512 = 8,
            _1024 = 9,
            _2048 = 10,
            _4096 = 11,
            _8192 = 12,
        }

        public enum ImporterBoolSetting
        {
            ForceNo = 0,
            ForceYes = 1,
            Free = 2,
        }

        //按传入lod分级ASTC压缩
        public static TextureImporterFormat GetTextureFormatByCompressLod(TextureImporterCompressLevel texLod)
        {
            TextureImporterFormat texImFormat = TextureImporterFormat.ASTC_4x4;
            switch (texLod)
            {
                case TextureImporterCompressLevel.LodRaw_TrueColor:
                    texImFormat = TextureImporterFormat.Automatic;
                    break;
                case TextureImporterCompressLevel.Lod0_ASTC4x4:
                    texImFormat = TextureImporterFormat.ASTC_4x4;
                    break;
                case TextureImporterCompressLevel.Lod1_ASTC5x5:
                    texImFormat = TextureImporterFormat.ASTC_5x5;
                    break;
                case TextureImporterCompressLevel.Lod2_ASTC6x6:
                    texImFormat = TextureImporterFormat.ASTC_6x6;
                    break;
                case TextureImporterCompressLevel.Lod3_ASTC8x8:
                    texImFormat = TextureImporterFormat.ASTC_8x8;
                    break;
                case TextureImporterCompressLevel.Lod4_ASTC10x10:
                    texImFormat = TextureImporterFormat.ASTC_10x10;
                    break;
                case TextureImporterCompressLevel.Lod5_ASTC12x12:
                    texImFormat = TextureImporterFormat.ASTC_12x12;
                    break;
                default:
                    Debug.LogError($"unsupported format{texLod}");
                    texImFormat = TextureImporterFormat.ASTC_4x4;
                    break;
            }
            return texImFormat;
        }

        public static int ConvertTextureSize(TextureImporterMaxSize enumSize)
        {
            switch (enumSize)
            {
                case TextureImporterMaxSize._2: return 2;
                case TextureImporterMaxSize._4: return 4;
                case TextureImporterMaxSize._8: return 8;
                case TextureImporterMaxSize._16: return 16;
                case TextureImporterMaxSize._32: return 32;
                case TextureImporterMaxSize._64: return 64;
                case TextureImporterMaxSize._128: return 128;
                case TextureImporterMaxSize._256: return 256;
                case TextureImporterMaxSize._512: return 512;
                case TextureImporterMaxSize._1024: return 1024;
                case TextureImporterMaxSize._2048: return 2048;
                case TextureImporterMaxSize._4096: return 4096;
                case TextureImporterMaxSize._8192: return 8192;
                default: return 1024;
            }
        }
    }
}
