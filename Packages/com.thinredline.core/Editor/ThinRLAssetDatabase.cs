using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

namespace ThinRL.Core.Editor
{

    public static class ThinRLAssetDatabase
    {

        // 仿照AssetDatabase.GetDependencies，但不会把材质中残留的历史引用算进来
        // 包含自己
        public static string[] GetDependencyFilesAccurate(string pathName, bool recursive)
        {
            Profiler.BeginSample("GetDependencyFilesAccurate");
            HashSet<string> result = new HashSet<string>();
            GetDependencyFilesAccurateImpl(pathName, recursive, ref result);
            List<string> resultList = new List<string>(result);
            var rt = resultList.ToArray();
            Profiler.EndSample();
            return rt;

        }
        static void GetDependencyFilesAccurateImpl(string pathName, bool recursive, ref HashSet<string> result)
        {
            result.Add(pathName);

            if (pathName.EndsWith(".mat"))
            {
                // 排除残留历史引用
                var mat = AssetDatabase.LoadAssetAtPath<Material>(pathName);
                var matDepArr = EditorUtility.CollectDependencies(new Object[] { mat });
                foreach (var dep in matDepArr)
                {
                    var p = AssetDatabase.GetAssetOrScenePath(dep);
                    if (!p.StartsWith("Resource") && !p.StartsWith("Library"))
                        result.Add(p);
                }
                // 只依赖shader和texture，不用再递归
            }
            else
            {
                var deps = AssetDatabase.GetDependencies(pathName, false);

                if (recursive)
                {
                    foreach (var v in deps)
                        if (!result.Contains(v))
                            GetDependencyFilesAccurateImpl(v, true, ref result);
                }
                else
                {
                    foreach (var v in deps)
                        result.Add(v);
                }
            }
        }
    }

}
