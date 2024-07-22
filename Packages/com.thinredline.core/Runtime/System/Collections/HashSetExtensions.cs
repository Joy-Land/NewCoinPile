/*
功能描述：
    本模块用于维护HashSetExtensions
*/

using System.Collections.Generic;
using System.Reflection;


namespace ThinRL.Core.SystemCollections
{
    public static class HashSetExtensions
    {
        static readonly Dictionary<System.Type, MethodInfo> s_MIs = new Dictionary<System.Type, MethodInfo>();

        /// <summary>
        /// 初始化Capacity，现阶段仅限刚初始化之后调用，防止后续无意义的重新分配空间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <param name="capacity">capacity</param>
        public static void InitializeCapacity<T>(this HashSet<T> hs, int capacity)
        {
            if (hs == null)
            {
                return;
            }

            if (hs.Count != 0)
            {
                throw new Exception<HashSet<T>>("re initialize");
            }

            if (!s_MIs.TryGetValue(hs.GetType(), out var mi))
            {
                mi = hs.GetType().GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

                s_MIs.Add(hs.GetType(), mi);
            }

            if (mi == null)
            {
                throw new Exception<HashSet<T>>("no method found");
            }

            mi.Invoke(hs, new object[] { capacity });
        }

        public static void Clear()
        {
            s_MIs.Clear();
        }
    }
}
