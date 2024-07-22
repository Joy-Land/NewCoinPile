using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using System;
using System.Reflection;
using UnityEditor;
#endif 

namespace ThinRL.Core
{
    // 方便创建配置文件，并在projectSetting里编辑，每个类对应一个配置文件，并且不能重名
    // 子类实现public static SubClassType Instance属性才能在projectSetting里编辑
    // 可以用这里的LoadOrCreateHelper方法辅助并统一加载、创建、和保存，也可以完全独立实现Instance
    // 会在Assets下创建配置序列化文件，需要加入到使用者的仓库中进行版本管理，可以跟公共库代码分离开
    public class ThinRLSettingBase : ScriptableObject
    {
        const string k_SaveDir = "Assets/Plugins/ThinRLSettings";

        static string GetAssetPath(string fileName, bool runtimeAsset)
        {
            var subDir = runtimeAsset ? "Resources" : "Editor";
            return $"{k_SaveDir}/{subDir}/{fileName}.asset";
        }

        protected static T LoadOrCreateHelper<T>(bool runtimeAsset) where T : ScriptableObject
        {
            ScriptableObject obj = null;
            string assetPath = "";
            if (runtimeAsset)
            {
                assetPath = typeof(T).Name;
                obj = Resources.Load<T>(assetPath);
            }
#if UNITY_EDITOR
            else
            {
                assetPath = GetAssetPath(typeof(T).Name, runtimeAsset);
                obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
                
            if (obj == null)
            {
                // 有可能文件存在但加载失败，比如切分支编译内容有变化，被AssetSettingsProvider调用时，
                // 为了不改变文件内容返回空
                if (System.IO.File.Exists(assetPath))
                {
                    Debug.LogError($"{assetPath}, exists but loading failed");
                    return null;
                }
                obj = CreateInstance<T>();
                // runtimeAsset的create和load要用2个不一样的路径                
                assetPath = GetAssetPath(typeof(T).Name, runtimeAsset);
                {
                    string assetDictionary = System.IO.Path.GetDirectoryName(assetPath);
                    if (!System.IO.Directory.Exists(assetDictionary))
                    {
                        System.IO.Directory.CreateDirectory(assetDictionary);
                    }
                }
                AssetDatabase.CreateAsset(obj, assetPath);
                AssetDatabase.SaveAssets();
            }
#endif
            return obj as T;
        }


    }

#if UNITY_EDITOR

    // Register a SettingsProvider using IMGUI for the drawing framework:
    // 反射找到所有ThinRLSettingBase子类，在ProjectSetting中显示出编辑界面
    static class MyCustomSettingsIMGUIRegister
    {
        // Register a SettingsProvider using IMGUI for the drawing framework:
        [SettingsProviderGroup]
        public static SettingsProvider[] CreateMyCustomSettingsProvider()
        {
            // 所有dll中ThinRLSettingBase的子类
            var subclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             from type in assembly.GetTypes()
                             where type.IsSubclassOf(typeof(ThinRLSettingBase))
                             select type;
            List<SettingsProvider> providers = new List<SettingsProvider>();

            HashSet<String> uniqueNames = new HashSet<string>();

            const string propName = "Instance";
            foreach (var subClass in subclasses)
            {
                // 查找Instance属性，创建AssetSettingsProvider进行ProjectSetting中的显示
                // Debug.LogError(subClass);
                var property = subClass.GetProperty(propName, BindingFlags.Public | BindingFlags.Static);
                if (property == null)
                {
                    Debug.LogError($"{propName} property not found in {subClass}, can't add to ProjectSetting");
                    continue;
                }

                var settingAsset = property.GetValue(null) as ScriptableObject;
                if (settingAsset == null)
                {
                    Debug.LogError($"asset for {subClass} not found, can't add to project Setting");
                    continue;
                }

                var name = settingAsset.GetType().Name;
                if (uniqueNames.Contains(name))
                {
                    Debug.LogError($"duplicated class name for {subClass}, can't add to project Setting");
                    continue;
                }

                var provider = AssetSettingsProvider.CreateProviderFromObject($"Project/ThinRL/{name}", settingAsset);
                provider.keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(new SerializedObject(settingAsset));

                providers.Add(provider);
                uniqueNames.Add(name);
            }

            return providers.ToArray();
        }

    }
#endif


}
