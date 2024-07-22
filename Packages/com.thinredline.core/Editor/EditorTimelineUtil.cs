using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace ThinRL.Core.Editor
{
    static public class EditorTimelineUtil
    {
        // 检查或修复PlayableDirector的binding信息，使不残留对当前timeline asset之外的binding信息，以免产生错误
        public static bool CheckFixPlayableDirecotrBindings(GameObject prefab, bool fixAsset = false, bool recursive = true)
        {
            if (prefab == null) return true;

            PlayableDirector[] allPlayDirectorArr = prefab.GetComponentsInChildren<PlayableDirector>(true);
            bool allOk = true;
            foreach (var d in allPlayDirectorArr)
            {
                var so = new SerializedObject(d);
                var bindingSo = so.FindProperty("m_SceneBindings");
                var tlPath = AssetDatabase.GetAssetOrScenePath(d.playableAsset);

                var directorOk = true;
                for (int i = bindingSo.arraySize - 1; i >= 0; --i)
                {
                    // key在文件中对应key: {fileID: -2138553689410654287, guid: ab1e4fb7a6c7ec94fa58da05e84a4734, type: 2}
                    // 可能因为guid对应的timeline中，没有fileId对应的track，而使keyObj为空，但还会依赖guid对应的timeline资源
                    var keyObj = bindingSo.GetArrayElementAtIndex(i).FindPropertyRelative("key").objectReferenceValue;
                    string objAssetPath = AssetDatabase.GetAssetOrScenePath(keyObj);
                    if (keyObj == null || objAssetPath != tlPath)
                    {
                        // 不是当前使用的tl的binding，需要移除，以免产生错误
                        if (fixAsset)
                        {
                            bindingSo.DeleteArrayElementAtIndex(i);
                        }
                        directorOk = false;
                    }
                }
                if (!directorOk)
                {
                    if (!fixAsset)
                    {
                        var directorPath = AssetDatabase.GetAssetOrScenePath(prefab);
                        Debug.LogError($"{directorPath}的PlayableDirector {d.name} 残留其他timeline引用，需要移除引用，执行导出修复", prefab);
                        allOk = false;
                    }
                    else
                    {
                        // 如果prefab中有missing script会AssetDatabase:SaveAssets()保存失败
                        // You are trying to replace or create a Prefab from the instance 'BattleCamera' that references a missing script. This is not allowed.
                        // Please change the script or remove it from the GameObject.
                        so.ApplyModifiedProperties();
                        // 不用执行AssetDatabase:SaveAssets()，会在导出时执行，否则执行太频繁，影响效率
                    }
                }
            }

            if (recursive)
            {
                // 查依赖
                string[] deps = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab), true);
                foreach (var d in deps)
                {
                    if (d.EndsWith(".prefab", true, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        var depPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(d);
                        if (depPrefab != null)
                            allOk &= CheckFixPlayableDirecotrBindings(depPrefab, fixAsset, false);
                    }
                }
            }

            return allOk;
        }
    }
}