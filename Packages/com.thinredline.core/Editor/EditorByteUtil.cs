using System;

namespace ThinRL.Core.Editor
{
    static public class ByteUtil
    {
        // 转换string的guid为二进制数组，注意字节序有略微差异
        // lightmapData的二进制数据中就有这种记录，见https://git.youle.game/TC/TAG/tag-documents/-/issues/251
        public static byte[] ConvertStringGuidToBinary(string guid)
        {
            // string guid: 24c6......
            // bytes[]: 426c.....
            if (string.IsNullOrEmpty(guid) || guid.Length != 32) return null;

            var res = new byte[16];
            for (int i = 0; i < 16; ++i)
            {
                int low = Convert.ToInt32(guid[i * 2].ToString(), 16);
                int high = Convert.ToInt32(guid[i * 2 + 1].ToString(), 16);
                byte v = (byte)(high << 4 | low);
                res[i] = v;
            }
            return res;
        }

        // 查找byte数组中的内容，返回索引
        // https://stackoverflow.com/questions/5132890/c-sharp-replace-bytes-in-byte
        public static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            // handle the complete source array
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }


        // 替换byte数组中的内容，可替换所有匹配的
        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
            byte[] dst = null;
            byte[] temp = null;
            int index = FindBytes(src, search);
            while (index >= 0)
            {
                if (temp == null)
                    temp = src;
                else
                    temp = dst;

                dst = new byte[temp.Length - search.Length + repl.Length];

                // before found array
                Buffer.BlockCopy(temp, 0, dst, 0, index);
                // repl copy
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                // rest of src array
                Buffer.BlockCopy(
                    temp,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    temp.Length - (index + search.Length));


                index = FindBytes(dst, search);
            }
            return dst;
        }
    }

}
