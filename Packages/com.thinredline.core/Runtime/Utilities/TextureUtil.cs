using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core
{
    // 处理贴图相关的辅助类
    public class TextureUtil
    {
        public static float GetBytesPerPixel(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.RGBA32:
                    return 4;
                case TextureFormat.RGB24:
                    return 3;
                case TextureFormat.RGB565:
                    return 2;
                case TextureFormat.Alpha8:
                // 4x4 pixel in 128 bits(16Bytes)
                case TextureFormat.ASTC_4x4:
                case TextureFormat.BC5:
                case TextureFormat.BC7:
                case TextureFormat.DXT5:
                case TextureFormat.BC6H:
                    return 1;
                // 5x5 pixel in 128 bits(16Bytes)
                case TextureFormat.ASTC_5x5:
                    return 0.64f;
                case TextureFormat.DXT1:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                    return 0.5f;
                case TextureFormat.ASTC_6x6:
                    return 0.4444f;
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                    return 0.25f;
                case TextureFormat.ASTC_10x10:
                    return 0.16f;
                case TextureFormat.ASTC_12x12:
                    return 0.11111f;
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                    return 0.125f;
                default:
                    Debug.LogError("Unhandle Format " + format);
                    // 编辑器中用很大的数暴露未处理的问题
                    if (Application.isEditor)
                        return 100;
                    else
                        return 1;
            }
        }

        public static long GuessTextureBytes(Texture tex, float bytesPerPixcel)
        {
            if (tex == null) return 0;
            var pixelCount = tex.width * tex.height;
            float scale = 1;
            if (tex.mipmapCount > 1)
                scale *= 1.333f;
            if (tex.isReadable)
                scale *= 2;

            long size = (long)(pixelCount * bytesPerPixcel * scale);
            return size;
        }
        public static long GuessTexture2DBytes(Texture2D tex)
        {
            return GuessTextureBytes(tex, GetBytesPerPixel(tex.format));
        }
        public static long GuessCubemapBytes(Cubemap cubemap)
        {
            return GuessTextureBytes(cubemap, GetBytesPerPixel(cubemap.format)) * 6;
        }


    }


}