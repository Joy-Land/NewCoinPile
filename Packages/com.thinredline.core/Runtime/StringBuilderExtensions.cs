/*
功能描述：
    本模块用于维护StringBuilderExtensions
*/


using ThinRL.Core;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;


public static class StringBuilderExtensions
{
    /// <summary>
    /// 返回指定字符在StringBuilder中第一次出现处的索引,如果此字符串中没有这样的字符,则返回 -1
    /// </summary>
    /// <param name="sb">buffer</param>
    /// <param name="c">目标字符</param>
    /// <param name="start">起始索引位置</param>
    /// <returns></returns>
    public static int IndexOf(this StringBuilder sb, char c, int start = 0)
    {
        if (sb != null)
        {
            for (int pos = start; pos < sb.Length; ++pos)
            {
                if (sb[pos] == c)
                {
                    return pos;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// 返回指定字符在StringBuilder中最后一次出现处的索引,如果此字符串中没有这样的字符,则返回 -1
    /// </summary>
    /// <param name="sb">buffer</param>
    /// <param name="c">目标字符</param>
    /// <param name="start">起始索引位置</param>
    /// <returns></returns>
    public static int LastIndexOf(this StringBuilder sb, char c, int start = -1)
    {
        if (start < 0)
        {
            start = start == -1 ? sb.Length - 1 : 0;
        }

        if (sb != null)
        {
            for (int i = Math.Min(sb.Length - 1, start); i >= 0; --i)
            {
                if (sb[i] == c)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// InsertQuickly相比原生方法Insert能减少GC.Alloc
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static StringBuilder InsertQuickly(this StringBuilder sb, int index, string value)
    {
        if (index < 0 || sb == null || value.IsNullOrEmpty())
        {
            return sb;
        }

        var lengthOld = sb.Length;
        int lengthNew = lengthOld + value.Length;

        sb.Capacity = Math.Max(sb.Capacity, lengthNew);
        sb.Length = lengthNew;

        for (int i = 1; i <= lengthOld - index; ++i)
        {
            sb[lengthNew - i] = sb[lengthOld - i];
        }

        for (int i = 0; i < value.Length; ++i)
        {
            sb[index + i] = value[i];
        }

        return sb;
    }

    //public static StringBuilder InsertQuickly(this StringBuilder sb, int index, char value)
    //{
    //    if (index < 0 || sb == null || value == '\0')
    //    {
    //        return sb;
    //    }

    //    return sb.Insert(index, value);
    //}
}
