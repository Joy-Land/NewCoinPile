/*
功能描述：
    本模块用于维护字符串扩展
*/

namespace ThinRL.Core
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 比原生快30~100倍的方法
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool StartsWithQuickly(this string str, string value, int start = 0)
        {
            var lenStr = str.Length;
            var lenValue = value.Length;

            var indexStr = start;
            var indexValue = 0;

            while (indexStr < lenStr && indexValue < lenValue && str[indexStr] == value[indexValue])
            {
                indexStr++;
                indexValue++;
            }

            return (indexValue == lenValue);
        }


        /// <summary>
        /// 比原生快30~100倍的方法
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool EndsWithQuickly(this string str, string value)
        {
            var indexStr = str.Length - 1;
            var indexValue = value.Length - 1;

            while (indexStr >= 0 && indexValue >= 0 && str[indexStr] == value[indexValue])
            {
                indexStr--;
                indexValue--;
            }

            return (indexValue < 0);
        }

        /// <summary>
        /// 不知道比原生快多少倍的方法，但还不是最快的方法，考虑多次StartsWithQuickly在重叠空间的冗余运算
        /// "123.4.45.456.4567".ContainsQuickly(".456");
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsQuickly(this string str, string value)
        {
            var index = 0;

            while(index < str.Length)
            {
                if (str[index] == value[0] && str.StartsWithQuickly(value, index))
                {
                    return true;
                }

                ++index;
            }

            return false;
        }

        /// <summary>
        /// 不知道比原生快多少倍的方法，但还不是最快的方法，考虑多次StartsWithQuickly在重叠空间的冗余运算
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOfQuickly(this string str, string value)
        {
            var index = 0;

            while (index < str.Length)
            {
                if (str[index] == value[0] && str.StartsWithQuickly(value, index))
                {
                    break;
                }

                ++index;
            }

            return index;
        }

        /// <summary>
        /// 不知道比原生快多少倍的方法，但还不是最快的方法，考虑多次StartsWithQuickly在重叠空间的冗余运算
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int LastIndexOfQuickly(this string str, string value)
        {
            var index = str.Length - 1;

            while (index >= 0)
            {
                if (str[index] == value[0] && str.StartsWithQuickly(value, index))
                {
                    break;
                }

                --index;
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="times">匹配次数</param>
        /// <returns></returns>
        public static int LastIndexOfQuickly(this string str, char value, int times = 1)
        {
            if (str.IsNullOrEmpty())
            {
                return -1;
            }

            if (times <= 0)
            {
                return -1;
            }

            int index = str.Length - 1;

            while (index >= times - 1)
            {
                if (str[index] == value)
                {
                    times--;

                    if (times == 0)
                    {
                        return index;
                    }
                }

                index--;
            }

            return -1;
        }
    }
}
