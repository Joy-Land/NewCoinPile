using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.FileSystem
{
    // 用于缓存路径映射，比如相对路径到绝对路径
    // 可以控制这种缓存是否开启，对于大量路径的情况，缓存会长久占用很多内存，采用临时拼接可被gc释放内存的可能更好
    public class PathMapCacheUtil
    {
        // 文件路径到全路径的映射缓存，对于已经处理过的文件，则缓存下来，下次直接用
        Dictionary<string, string> m_FilePathToFullPathMap;

        public readonly bool Enable = false;

        public PathMapCacheUtil(bool enable, int initCapacity = 10)
        {
            if (enable)
            {
                Enable = enable;
                m_FilePathToFullPathMap = new Dictionary<string, string>(initCapacity);
            }
        }

        public bool ContainsKey(string key)
        {
            return (m_FilePathToFullPathMap?.ContainsKey(key)).Value;
        }

        public bool TryGetValue(string key, out string value)
        {
            value = null;
            if (m_FilePathToFullPathMap == null) return false;
            return m_FilePathToFullPathMap.TryGetValue(key, out value);
        }

        public void Add(string key, string value)
        {
            m_FilePathToFullPathMap?.Add(key, value);
        }

        public void Clear()
        {
            m_FilePathToFullPathMap?.Clear();
        }

    }

}